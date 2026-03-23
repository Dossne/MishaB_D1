using System.Collections.Generic;

namespace TetrisTactic.Abilities
{
    public sealed class AbilityDefinition
    {
        private readonly AbilityShapeCell[] shapeCells;

        public AbilityDefinition(AbilityShapeType shapeType, string displayName, AbilityShapeCell[] shapeCells)
        {
            ShapeType = shapeType;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? shapeType.ToString() : displayName;
            this.shapeCells = shapeCells ?? System.Array.Empty<AbilityShapeCell>();
        }

        public AbilityShapeType ShapeType { get; }
        public string DisplayName { get; }
        public IReadOnlyList<AbilityShapeCell> ShapeCells => shapeCells;

        public static AbilityDefinition CreateDefault(AbilityShapeType shapeType)
        {
            return shapeType switch
            {
                AbilityShapeType.O => new AbilityDefinition(shapeType, "O", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(1, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(1, 1),
                }),
                AbilityShapeType.T => new AbilityDefinition(shapeType, "T", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(-1, 1),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(1, 1),
                }),
                AbilityShapeType.L => new AbilityDefinition(shapeType, "L", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(0, 2),
                    new AbilityShapeCell(1, 2),
                }),
                AbilityShapeType.S => new AbilityDefinition(shapeType, "S", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(1, 0),
                    new AbilityShapeCell(-1, 1),
                    new AbilityShapeCell(0, 1),
                }),
                AbilityShapeType.I => new AbilityDefinition(shapeType, "I", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(0, 2),
                    new AbilityShapeCell(0, 3),
                }),
                _ => new AbilityDefinition(AbilityShapeType.O, "O", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(1, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(1, 1),
                }),
            };
        }
    }
}