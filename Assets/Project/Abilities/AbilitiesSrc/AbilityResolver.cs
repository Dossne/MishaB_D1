using System.Collections.Generic;
using TetrisTactic.PlayField;

namespace TetrisTactic.Abilities
{
    public static class AbilityResolver
    {
        public static bool TryGetDirectionFromBaseCell(GridPosition casterPosition, GridPosition baseCell, out AbilityDirection direction)
        {
            var deltaX = baseCell.X - casterPosition.X;
            var deltaY = baseCell.Y - casterPosition.Y;

            if (deltaX == 0 && deltaY == 1)
            {
                direction = AbilityDirection.Up;
                return true;
            }

            if (deltaX == 1 && deltaY == 0)
            {
                direction = AbilityDirection.Right;
                return true;
            }

            if (deltaX == 0 && deltaY == -1)
            {
                direction = AbilityDirection.Down;
                return true;
            }

            if (deltaX == -1 && deltaY == 0)
            {
                direction = AbilityDirection.Left;
                return true;
            }

            direction = AbilityDirection.Up;
            return false;
        }

        public static bool TryResolveCast(
            AbilityDefinition ability,
            GridPosition casterPosition,
            GridPosition baseCell,
            PlayFieldController playFieldController,
            out List<GridPosition> affectedCells,
            out List<List<GridPosition>> waveSteps)
        {
            affectedCells = null;
            waveSteps = null;

            if (ability == null || playFieldController == null)
            {
                return false;
            }

            if (!TryGetDirectionFromBaseCell(casterPosition, baseCell, out var direction))
            {
                return false;
            }

            if (!playFieldController.IsInside(baseCell))
            {
                return false;
            }

            var uniqueCells = new HashSet<GridPosition>();
            var rawCells = new List<GridPosition>(ability.ShapeCells.Count);
            var distanceByCell = new Dictionary<GridPosition, int>();

            for (var i = 0; i < ability.ShapeCells.Count; i++)
            {
                var cell = ability.ShapeCells[i];
                var rotated = Rotate(cell, direction);
                var worldCell = new GridPosition(baseCell.X + rotated.X, baseCell.Y + rotated.Y);

                if (worldCell == casterPosition)
                {
                    return false;
                }

                // Stage 7 rule update: clip non-fitting part of the shape instead of blocking cast.
                if (!playFieldController.IsInside(worldCell))
                {
                    continue;
                }

                if (uniqueCells.Add(worldCell))
                {
                    rawCells.Add(worldCell);
                    distanceByCell[worldCell] = System.Math.Abs(rotated.X) + System.Math.Abs(rotated.Y);
                }
            }

            if (rawCells.Count == 0)
            {
                return false;
            }

            rawCells.Sort((left, right) =>
            {
                var distanceCompare = distanceByCell[left].CompareTo(distanceByCell[right]);
                if (distanceCompare != 0)
                {
                    return distanceCompare;
                }

                var yCompare = left.Y.CompareTo(right.Y);
                if (yCompare != 0)
                {
                    return yCompare;
                }

                return left.X.CompareTo(right.X);
            });

            var stepsByDistance = new Dictionary<int, List<GridPosition>>();
            var orderedDistances = new List<int>();

            for (var i = 0; i < rawCells.Count; i++)
            {
                var castCell = rawCells[i];
                var distance = distanceByCell[castCell];
                if (!stepsByDistance.TryGetValue(distance, out var step))
                {
                    step = new List<GridPosition>();
                    stepsByDistance[distance] = step;
                    orderedDistances.Add(distance);
                }

                step.Add(castCell);
            }

            orderedDistances.Sort();

            waveSteps = new List<List<GridPosition>>(orderedDistances.Count);
            for (var i = 0; i < orderedDistances.Count; i++)
            {
                var stepCells = stepsByDistance[orderedDistances[i]];
                stepCells.Sort((left, right) =>
                {
                    var yCompare = left.Y.CompareTo(right.Y);
                    return yCompare != 0 ? yCompare : left.X.CompareTo(right.X);
                });
                waveSteps.Add(stepCells);
            }

            affectedCells = new List<GridPosition>(rawCells);
            return true;
        }

        private static AbilityShapeCell Rotate(AbilityShapeCell source, AbilityDirection direction)
        {
            return direction switch
            {
                AbilityDirection.Up => new AbilityShapeCell(source.X, source.Y),
                AbilityDirection.Right => new AbilityShapeCell(source.Y, -source.X),
                AbilityDirection.Down => new AbilityShapeCell(-source.X, -source.Y),
                AbilityDirection.Left => new AbilityShapeCell(-source.Y, source.X),
                _ => new AbilityShapeCell(source.X, source.Y),
            };
        }
    }
}