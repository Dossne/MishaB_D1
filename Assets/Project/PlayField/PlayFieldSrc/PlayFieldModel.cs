using System.Collections.Generic;
using TetrisTactic.Treasure;
using TetrisTactic.Units;

namespace TetrisTactic.PlayField
{
    public enum CellContentType
    {
        Empty = 0,
        Player = 1,
        Enemy = 2,
        Treasure = 3,
        Obstacle = 4,
    }

    public sealed class PlayFieldModel
    {
        private readonly Dictionary<GridPosition, UnitRuntimeModel> unitsByPosition = new();
        private readonly Dictionary<GridPosition, TreasureData> treasuresByPosition = new();
        private readonly HashSet<GridPosition> obstacles = new();
        private readonly List<UnitRuntimeModel> enemies = new();
        private readonly List<TreasureData> treasures = new();

        public PlayFieldModel(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
        }

        public int Columns { get; }
        public int Rows { get; }
        public UnitRuntimeModel PlayerUnit { get; private set; }
        public IReadOnlyList<UnitRuntimeModel> EnemyUnits => enemies;
        public IReadOnlyList<TreasureData> Treasures => treasures;
        public IReadOnlyCollection<GridPosition> Obstacles => obstacles;

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0 && position.X < Columns && position.Y >= 0 && position.Y < Rows;
        }

        public CellContentType GetCell(GridPosition position)
        {
            if (!IsInside(position))
            {
                return CellContentType.Empty;
            }

            if (IsActiveUnit(PlayerUnit) && PlayerUnit.Position == position)
            {
                return CellContentType.Player;
            }

            if (TryGetUnitAt(position, out var unit))
            {
                return unit.TeamType == TeamType.Enemy
                    ? CellContentType.Enemy
                    : CellContentType.Player;
            }

            if (treasuresByPosition.ContainsKey(position))
            {
                return CellContentType.Treasure;
            }

            return obstacles.Contains(position)
                ? CellContentType.Obstacle
                : CellContentType.Empty;
        }

        public bool HasUnitAt(GridPosition position)
        {
            return unitsByPosition.TryGetValue(position, out var unit) && IsActiveUnit(unit);
        }

        public bool TryGetUnitAt(GridPosition position, out UnitRuntimeModel unit)
        {
            unit = null;
            if (!unitsByPosition.TryGetValue(position, out var candidate))
            {
                return false;
            }

            if (!IsActiveUnit(candidate))
            {
                return false;
            }

            unit = candidate;
            return true;
        }

        public bool HasTreasureAt(GridPosition position)
        {
            return treasuresByPosition.ContainsKey(position);
        }

        public bool TryTakeTreasureAt(GridPosition position, out TreasureData treasure)
        {
            treasure = null;
            if (!treasuresByPosition.TryGetValue(position, out var existingTreasure) || existingTreasure == null)
            {
                return false;
            }

            treasuresByPosition.Remove(position);
            treasures.Remove(existingTreasure);
            treasure = existingTreasure;
            return true;
        }

        public bool IsObstacle(GridPosition position)
        {
            return obstacles.Contains(position);
        }

        public bool IsBlocked(GridPosition position)
        {
            return !IsInside(position) || IsObstacle(position);
        }

        public bool IsOccupied(GridPosition position)
        {
            return !IsInside(position) || HasUnitAt(position) || HasTreasureAt(position) || IsObstacle(position);
        }

        public bool IsEmpty(GridPosition position)
        {
            if (!IsInside(position))
            {
                return false;
            }

            return !HasUnitAt(position) && !HasTreasureAt(position) && !IsObstacle(position);
        }

        public bool TrySetPlayer(UnitRuntimeModel playerUnit)
        {
            if (playerUnit == null || playerUnit.TeamType != TeamType.Player)
            {
                return false;
            }

            if (!CanPlaceOn(playerUnit.Position))
            {
                return false;
            }

            PlayerUnit = playerUnit;
            unitsByPosition[playerUnit.Position] = playerUnit;
            return true;
        }

        public bool TryAddEnemy(UnitRuntimeModel enemyUnit)
        {
            if (enemyUnit == null || enemyUnit.TeamType != TeamType.Enemy)
            {
                return false;
            }

            if (!CanPlaceOn(enemyUnit.Position))
            {
                return false;
            }

            enemies.Add(enemyUnit);
            unitsByPosition[enemyUnit.Position] = enemyUnit;
            return true;
        }

        public bool TryAddTreasure(TreasureData treasureData)
        {
            if (treasureData == null)
            {
                return false;
            }

            if (!CanPlaceOn(treasureData.Position))
            {
                return false;
            }

            treasures.Add(treasureData);
            treasuresByPosition[treasureData.Position] = treasureData;
            return true;
        }

        public bool TryAddObstacle(GridPosition obstaclePosition)
        {
            if (!CanPlaceOn(obstaclePosition))
            {
                return false;
            }

            return obstacles.Add(obstaclePosition);
        }

        public bool RemoveObstacle(GridPosition obstaclePosition)
        {
            return obstacles.Remove(obstaclePosition);
        }

        public bool TryMoveUnit(UnitRuntimeModel unit, GridPosition destination)
        {
            if (unit == null || !IsActiveUnit(unit) || !IsInside(destination))
            {
                return false;
            }

            if (!unitsByPosition.TryGetValue(unit.Position, out var existingUnit) || existingUnit != unit)
            {
                return false;
            }

            if (IsObstacle(destination) || HasUnitAt(destination))
            {
                return false;
            }

            if (unit.TeamType != TeamType.Player && HasTreasureAt(destination))
            {
                return false;
            }

            unitsByPosition.Remove(unit.Position);
            unit.Position = destination;
            unitsByPosition[unit.Position] = unit;

            if (unit.TeamType == TeamType.Player)
            {
                PlayerUnit = unit;
            }

            return true;
        }

        public bool RemoveUnit(UnitRuntimeModel unit)
        {
            if (unit == null)
            {
                return false;
            }

            if (!unitsByPosition.TryGetValue(unit.Position, out var existingUnit) || existingUnit != unit)
            {
                return false;
            }

            unitsByPosition.Remove(unit.Position);

            if (unit.TeamType == TeamType.Player)
            {
                if (PlayerUnit == unit)
                {
                    PlayerUnit = null;
                }

                return true;
            }

            enemies.Remove(unit);
            return true;
        }

        public IEnumerable<UnitRuntimeModel> GetAllUnits()
        {
            if (IsActiveUnit(PlayerUnit))
            {
                yield return PlayerUnit;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (IsActiveUnit(enemy))
                {
                    yield return enemy;
                }
            }
        }

        public void ClearAll()
        {
            unitsByPosition.Clear();
            treasuresByPosition.Clear();
            obstacles.Clear();
            enemies.Clear();
            treasures.Clear();
            PlayerUnit = null;
        }

        private bool CanPlaceOn(GridPosition position)
        {
            return IsInside(position) && IsEmpty(position);
        }

        private static bool IsActiveUnit(UnitRuntimeModel unit)
        {
            return unit != null && unit.Health != null && unit.Health.IsAlive;
        }
    }
}
