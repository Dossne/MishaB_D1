using System.Collections.Generic;
using TetrisTactic.Abilities;
using TetrisTactic.PlayField;
using TetrisTactic.Units;

namespace TetrisTactic.EnemyTurn
{
    public sealed class EnemyThreatAnalyzer
    {
        private static readonly GridPosition[] DirectionOffsets =
        {
            new GridPosition(0, 1),
            new GridPosition(1, 0),
            new GridPosition(0, -1),
            new GridPosition(-1, 0),
        };

        private readonly PlayFieldController playFieldController;

        public EnemyThreatAnalyzer(PlayFieldController playFieldController)
        {
            this.playFieldController = playFieldController;
        }

        public bool CanAttackTarget(UnitRuntimeModel enemy, GridPosition targetPosition)
        {
            return TryBuildAttackWave(enemy, targetPosition, out _);
        }

        public bool TryBuildAttackWave(UnitRuntimeModel enemy, GridPosition targetPosition, out List<List<GridPosition>> waveSteps)
        {
            waveSteps = null;
            if (enemy == null || !enemy.Health.IsAlive)
            {
                return false;
            }

            for (var directionIndex = 0; directionIndex < DirectionOffsets.Length; directionIndex++)
            {
                if (!TryBuildDirectionalAttack(enemy, directionIndex, out var cells, out var directionWave))
                {
                    continue;
                }

                if (!ContainsCell(cells, targetPosition))
                {
                    continue;
                }

                waveSteps = directionWave;
                return true;
            }

            return false;
        }

        public IReadOnlyList<ZoneHighlightData> GetAllDangerZones(IReadOnlyList<UnitRuntimeModel> enemies)
        {
            var result = new List<ZoneHighlightData>();
            if (enemies == null)
            {
                return result;
            }

            var player = playFieldController.GetPlayerUnit();
            var bestByCell = new Dictionary<GridPosition, ZoneHighlightData>();

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                var priority = i + 1;
                for (var directionIndex = 0; directionIndex < DirectionOffsets.Length; directionIndex++)
                {
                    if (!TryBuildDirectionalAttack(enemy, directionIndex, out var cells, out _))
                    {
                        continue;
                    }

                    for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                    {
                        var cell = cells[cellIndex];
                        if (!IsThreatCellRelevantForPlayer(cell, player))
                        {
                            continue;
                        }

                        var candidate = new ZoneHighlightData(cell, enemy.UnitType, priority);
                        if (!bestByCell.TryGetValue(cell, out var existing) || candidate.Priority < existing.Priority)
                        {
                            bestByCell[cell] = candidate;
                        }
                    }
                }
            }

            foreach (var zone in bestByCell.Values)
            {
                result.Add(zone);
            }

            return result;
        }

        public int GetDistanceToNearestThreatPosition(UnitRuntimeModel enemy, GridPosition startPosition, GridPosition targetPosition)
        {
            if (enemy == null || !enemy.Health.IsAlive)
            {
                return int.MaxValue;
            }

            var targets = BuildThreatStandPositions(enemy, targetPosition);
            if (targets.Count == 0)
            {
                return int.MaxValue;
            }

            var visited = new HashSet<GridPosition>();
            var queue = new Queue<(GridPosition position, int distance)>();
            visited.Add(startPosition);
            queue.Enqueue((startPosition, 0));

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (targets.Contains(node.position))
                {
                    return node.distance;
                }

                for (var i = 0; i < DirectionOffsets.Length; i++)
                {
                    var offset = DirectionOffsets[i];
                    var next = new GridPosition(node.position.X + offset.X, node.position.Y + offset.Y);
                    if (visited.Contains(next))
                    {
                        continue;
                    }

                    if (!playFieldController.IsCellPassableForMovement(next, enemy))
                    {
                        continue;
                    }

                    visited.Add(next);
                    queue.Enqueue((next, node.distance + 1));
                }
            }

            return int.MaxValue;
        }

        private bool IsThreatCellRelevantForPlayer(GridPosition cell, UnitRuntimeModel player)
        {
            _ = player;
            return playFieldController.IsInside(cell);
        }

        private HashSet<GridPosition> BuildThreatStandPositions(UnitRuntimeModel enemy, GridPosition targetPosition)
        {
            var result = new HashSet<GridPosition>();
            var columns = playFieldController.Columns;
            var rows = playFieldController.Rows;

            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var stand = new GridPosition(x, y);
                    if (!playFieldController.IsCellPassableForMovement(stand, enemy))
                    {
                        continue;
                    }

                    if (CanAttackFromPosition(enemy, stand, targetPosition))
                    {
                        result.Add(stand);
                    }
                }
            }

            return result;
        }

        private bool CanAttackFromPosition(UnitRuntimeModel enemy, GridPosition attackerPosition, GridPosition targetPosition)
        {
            var definition = enemy?.AbilityDefinition ?? GetDefaultDefinition(enemy?.UnitType ?? UnitType.Warrior);
            if (definition == null)
            {
                return false;
            }

            for (var directionIndex = 0; directionIndex < DirectionOffsets.Length; directionIndex++)
            {
                var baseCell = new GridPosition(
                    attackerPosition.X + DirectionOffsets[directionIndex].X,
                    attackerPosition.Y + DirectionOffsets[directionIndex].Y);

                if (!AbilityResolver.TryResolveCast(definition, attackerPosition, baseCell, playFieldController, out var cells, out _))
                {
                    continue;
                }

                if (ContainsCell(cells, targetPosition))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryBuildDirectionalAttack(UnitRuntimeModel enemy, int directionIndex, out List<GridPosition> cells, out List<List<GridPosition>> waveSteps)
        {
            cells = new List<GridPosition>();
            waveSteps = new List<List<GridPosition>>();

            var definition = enemy.AbilityDefinition ?? GetDefaultDefinition(enemy.UnitType);
            if (definition == null)
            {
                return false;
            }

            var baseCell = new GridPosition(
                enemy.Position.X + DirectionOffsets[directionIndex].X,
                enemy.Position.Y + DirectionOffsets[directionIndex].Y);

            return AbilityResolver.TryResolveCast(definition, enemy.Position, baseCell, playFieldController, out cells, out waveSteps);
        }

        private static AbilityDefinition GetDefaultDefinition(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Warrior => AbilityDefinition.CreatePreset(AbilityDefinitionId.OLeft),
                UnitType.Archer => AbilityDefinition.CreatePreset(AbilityDefinitionId.I),
                UnitType.Mage => AbilityDefinition.CreatePreset(AbilityDefinitionId.LLeft),
                _ => null,
            };
        }

        private static bool ContainsCell(List<GridPosition> cells, GridPosition target)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i] == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
