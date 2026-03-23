using System.Collections.Generic;
using TetrisTactic.Abilities;
using TetrisTactic.PlayField;
using TetrisTactic.Units;

namespace TetrisTactic.EnemyTurn
{
    public sealed class EnemyWaitLogic
    {
        private static readonly GridPosition[] DirectionOffsets =
        {
            new GridPosition(0, 1),
            new GridPosition(1, 0),
            new GridPosition(0, -1),
            new GridPosition(-1, 0),
        };

        private readonly PlayFieldController playFieldController;
        private readonly AbilityController abilityController;
        private readonly EnemyThreatAnalyzer threatAnalyzer;

        public EnemyWaitLogic(
            PlayFieldController playFieldController,
            AbilityController abilityController,
            EnemyThreatAnalyzer threatAnalyzer)
        {
            this.playFieldController = playFieldController;
            this.abilityController = abilityController;
            this.threatAnalyzer = threatAnalyzer;
        }

        public bool ShouldWait(UnitRuntimeModel enemy, System.Func<UnitRuntimeModel, bool> canWait)
        {
            if (enemy == null || !enemy.Health.IsAlive)
            {
                return false;
            }

            if (canWait == null || !canWait(enemy))
            {
                return false;
            }

            if (enemy.Health.CurrentHp >= enemy.Health.MaxHp)
            {
                return false;
            }

            var player = playFieldController.GetPlayerUnit();
            if (player == null || !player.Health.IsAlive)
            {
                return false;
            }

            var playerThreatCells = abilityController.GetPotentialImpactCells(player);
            for (var i = 0; i < playerThreatCells.Count; i++)
            {
                if (playerThreatCells[i] == enemy.Position)
                {
                    return false;
                }
            }

            var reachablePlayerPositions = BuildReachablePlayerPositions(player.Position, 2, player);
            for (var i = 0; i < reachablePlayerPositions.Count; i++)
            {
                if (threatAnalyzer.CanAttackTarget(enemy, reachablePlayerPositions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private List<GridPosition> BuildReachablePlayerPositions(GridPosition start, int maxSteps, UnitRuntimeModel player)
        {
            var result = new List<GridPosition>();
            var visitedDepth = new Dictionary<GridPosition, int>
            {
                [start] = 0,
            };

            var queue = new Queue<GridPosition>();
            queue.Enqueue(start);
            result.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentDepth = visitedDepth[current];
                if (currentDepth >= maxSteps)
                {
                    continue;
                }

                for (var i = 0; i < DirectionOffsets.Length; i++)
                {
                    var offset = DirectionOffsets[i];
                    var next = new GridPosition(current.X + offset.X, current.Y + offset.Y);
                    if (!playFieldController.IsCellPassableForMovement(next, player))
                    {
                        continue;
                    }

                    var nextDepth = currentDepth + 1;
                    if (visitedDepth.TryGetValue(next, out var knownDepth) && knownDepth <= nextDepth)
                    {
                        continue;
                    }

                    visitedDepth[next] = nextDepth;
                    queue.Enqueue(next);
                    result.Add(next);
                }
            }

            return result;
        }
    }
}
