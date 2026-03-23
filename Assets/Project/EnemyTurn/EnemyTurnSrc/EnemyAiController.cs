using System.Collections.Generic;
using TetrisTactic.PlayField;
using TetrisTactic.Units;

namespace TetrisTactic.EnemyTurn
{
    public sealed class EnemyAiController
    {
        private readonly PlayFieldController playFieldController;
        private readonly EnemyThreatAnalyzer threatAnalyzer;
        private readonly EnemyWaitLogic waitLogic;

        public EnemyAiController(
            PlayFieldController playFieldController,
            EnemyThreatAnalyzer threatAnalyzer,
            EnemyWaitLogic waitLogic)
        {
            this.playFieldController = playFieldController;
            this.threatAnalyzer = threatAnalyzer;
            this.waitLogic = waitLogic;
        }

        public EnemyDecisionModel BuildDecision(UnitRuntimeModel enemy, System.Func<UnitRuntimeModel, bool> canWait)
        {
            if (enemy == null || !enemy.Health.IsAlive)
            {
                return EnemyDecisionModel.CreateNone(enemy);
            }

            var player = playFieldController.GetPlayerUnit();
            if (player == null || !player.Health.IsAlive)
            {
                return EnemyDecisionModel.CreateNone(enemy);
            }

            if (threatAnalyzer.TryBuildAttackWave(enemy, player.Position, out var attackWave))
            {
                return EnemyDecisionModel.CreateAttack(enemy, attackWave);
            }

            var bestMove = TryFindThreateningMove(enemy, player.Position);
            if (bestMove.HasValue)
            {
                return EnemyDecisionModel.CreateMove(enemy, bestMove.Value);
            }

            if (waitLogic.ShouldWait(enemy, canWait))
            {
                return EnemyDecisionModel.CreateWait(enemy);
            }

            var fallbackMove = TryFindFallbackMove(enemy, player.Position);
            if (fallbackMove.HasValue)
            {
                return EnemyDecisionModel.CreateMove(enemy, fallbackMove.Value);
            }

            if (canWait != null && canWait(enemy))
            {
                return EnemyDecisionModel.CreateWait(enemy);
            }

            return EnemyDecisionModel.CreateNone(enemy);
        }

        public IReadOnlyList<ZoneHighlightData> BuildDangerCells(IReadOnlyList<UnitRuntimeModel> enemies)
        {
            return threatAnalyzer.GetAllDangerZones(enemies);
        }

        private GridPosition? TryFindThreateningMove(UnitRuntimeModel enemy, GridPosition playerPosition)
        {
            var legalMoves = playFieldController.GetLegalUnitMoveCells(enemy);
            if (legalMoves.Count == 0)
            {
                return null;
            }

            var currentDistance = threatAnalyzer.GetDistanceToNearestThreatPosition(enemy, enemy.Position, playerPosition);

            var found = false;
            var bestMove = default(GridPosition);
            var bestDistance = int.MaxValue;
            for (var i = 0; i < legalMoves.Count; i++)
            {
                var move = legalMoves[i];
                var distance = threatAnalyzer.GetDistanceToNearestThreatPosition(enemy, move, playerPosition);
                if (distance >= currentDistance)
                {
                    continue;
                }

                if (!found || distance < bestDistance)
                {
                    found = true;
                    bestMove = move;
                    bestDistance = distance;
                }
            }

            return found ? bestMove : null;
        }

        private GridPosition? TryFindFallbackMove(UnitRuntimeModel enemy, GridPosition playerPosition)
        {
            var legalMoves = playFieldController.GetLegalUnitMoveCells(enemy);
            if (legalMoves.Count == 0)
            {
                return null;
            }

            var bestMove = legalMoves[0];
            var bestDistance = GetManhattanDistance(bestMove, playerPosition);
            for (var i = 1; i < legalMoves.Count; i++)
            {
                var move = legalMoves[i];
                var distance = GetManhattanDistance(move, playerPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private static int GetManhattanDistance(GridPosition left, GridPosition right)
        {
            return System.Math.Abs(left.X - right.X) + System.Math.Abs(left.Y - right.Y);
        }
    }
}


