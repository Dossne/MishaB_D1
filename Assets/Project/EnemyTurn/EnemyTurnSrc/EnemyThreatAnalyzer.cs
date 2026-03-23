using System.Collections.Generic;
using TetrisTactic.PlayField;
using TetrisTactic.Units;
using UnityEngine;

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

        public IReadOnlyList<GridPosition> GetAllDangerCells(IReadOnlyList<UnitRuntimeModel> enemies)
        {
            var result = new List<GridPosition>();
            var unique = new HashSet<GridPosition>();
            if (enemies == null)
            {
                return result;
            }

            var player = playFieldController.GetPlayerUnit();

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

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

                        if (unique.Add(cell))
                        {
                            result.Add(cell);
                        }
                    }
                }
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
            if (player == null)
            {
                return playFieldController.IsInside(cell);
            }

            // Highlight only cells that can matter for player positioning.
            return playFieldController.IsCellPassableForMovement(cell, player);
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

                    if (CanAttackFromPosition(enemy.UnitType, stand, targetPosition))
                    {
                        result.Add(stand);
                    }
                }
            }

            return result;
        }

        private bool CanAttackFromPosition(UnitType enemyType, GridPosition attackerPosition, GridPosition targetPosition)
        {
            for (var directionIndex = 0; directionIndex < DirectionOffsets.Length; directionIndex++)
            {
                var shapeCells = GetShapeForUnit(enemyType);
                for (var i = 0; i < shapeCells.Count; i++)
                {
                    var rotated = Rotate(shapeCells[i], directionIndex);
                    var world = new GridPosition(attackerPosition.X + rotated.X, attackerPosition.Y + rotated.Y);
                    if (world == targetPosition && playFieldController.IsInside(world))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryBuildDirectionalAttack(UnitRuntimeModel enemy, int directionIndex, out List<GridPosition> cells, out List<List<GridPosition>> waveSteps)
        {
            cells = new List<GridPosition>();
            waveSteps = new List<List<GridPosition>>();
            var shape = GetShapeForUnit(enemy.UnitType);
            if (shape.Count == 0)
            {
                return false;
            }

            var byDistance = new Dictionary<int, List<GridPosition>>();
            for (var i = 0; i < shape.Count; i++)
            {
                var rotated = Rotate(shape[i], directionIndex);
                var world = new GridPosition(enemy.Position.X + rotated.X, enemy.Position.Y + rotated.Y);
                if (!playFieldController.IsInside(world))
                {
                    continue;
                }

                cells.Add(world);
                var distance = Mathf.Max(Mathf.Abs(shape[i].X), Mathf.Abs(shape[i].Y));
                if (!byDistance.TryGetValue(distance, out var step))
                {
                    step = new List<GridPosition>();
                    byDistance[distance] = step;
                }

                step.Add(world);
            }

            if (cells.Count == 0)
            {
                return false;
            }

            var ordered = new List<int>(byDistance.Keys);
            ordered.Sort();

            for (var i = 0; i < ordered.Count; i++)
            {
                waveSteps.Add(byDistance[ordered[i]]);
            }

            return true;
        }

        private static List<GridPosition> GetShapeForUnit(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Warrior => new List<GridPosition>
                {
                    new GridPosition(0, 1),
                    new GridPosition(0, 2),
                },
                UnitType.Archer => new List<GridPosition>
                {
                    new GridPosition(0, 1),
                    new GridPosition(0, 2),
                    new GridPosition(0, 3),
                    new GridPosition(0, 4),
                },
                UnitType.Mage => new List<GridPosition>
                {
                    new GridPosition(0, 1),
                    new GridPosition(0, 2),
                    new GridPosition(0, 3),
                    new GridPosition(0, 4),
                    new GridPosition(-1, 4),
                    new GridPosition(1, 4),
                },
                _ => new List<GridPosition>(),
            };
        }

        private static GridPosition Rotate(GridPosition source, int directionIndex)
        {
            return directionIndex switch
            {
                0 => new GridPosition(source.X, source.Y),
                1 => new GridPosition(source.Y, -source.X),
                2 => new GridPosition(-source.X, -source.Y),
                3 => new GridPosition(-source.Y, source.X),
                _ => new GridPosition(source.X, source.Y),
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
