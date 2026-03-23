using System.Collections.Generic;

namespace TetrisTactic.Abilities
{
    public sealed class AbilityDefinition
    {
        private readonly AbilityShapeCell[] shapeCells;

        public AbilityDefinition(AbilityDefinitionId definitionId, AbilityShapeType shapeType, string displayName, AbilityShapeCell[] shapeCells)
        {
            DefinitionId = definitionId;
            ShapeType = shapeType;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? shapeType.ToString() : displayName;
            this.shapeCells = shapeCells ?? System.Array.Empty<AbilityShapeCell>();
        }

        public AbilityDefinitionId DefinitionId { get; }
        public AbilityShapeType ShapeType { get; }
        public string DisplayName { get; }
        public IReadOnlyList<AbilityShapeCell> ShapeCells => shapeCells;

        public static AbilityDefinition CreateDefault(AbilityShapeType shapeType)
        {
            return shapeType switch
            {
                AbilityShapeType.O => CreatePreset(AbilityDefinitionId.OLeft),
                AbilityShapeType.T => CreatePreset(AbilityDefinitionId.T),
                AbilityShapeType.L => CreatePreset(AbilityDefinitionId.LLeft),
                AbilityShapeType.S => CreatePreset(AbilityDefinitionId.S),
                AbilityShapeType.I => CreatePreset(AbilityDefinitionId.I),
                _ => CreatePreset(AbilityDefinitionId.OLeft),
            };
        }

        public static AbilityDefinition CreatePreset(AbilityDefinitionId definitionId)
        {
            return definitionId switch
            {
                AbilityDefinitionId.OLeft => new AbilityDefinition(definitionId, AbilityShapeType.O, "O-L", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(1, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(1, 1),
                }),
                AbilityDefinitionId.ORight => new AbilityDefinition(definitionId, AbilityShapeType.O, "O-R", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(-1, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(-1, 1),
                }),
                AbilityDefinitionId.T => new AbilityDefinition(definitionId, AbilityShapeType.T, "T", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(-1, 1),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(1, 1),
                }),
                AbilityDefinitionId.LLeft => new AbilityDefinition(definitionId, AbilityShapeType.L, "L-L", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(0, 2),
                    new AbilityShapeCell(1, 2),
                }),
                AbilityDefinitionId.LRight => new AbilityDefinition(definitionId, AbilityShapeType.L, "L-R", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(0, 2),
                    new AbilityShapeCell(-1, 2),
                }),
                AbilityDefinitionId.S => new AbilityDefinition(definitionId, AbilityShapeType.S, "S", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(1, 0),
                    new AbilityShapeCell(-1, 1),
                    new AbilityShapeCell(0, 1),
                }),
                AbilityDefinitionId.I => new AbilityDefinition(definitionId, AbilityShapeType.I, "I", new[]
                {
                    new AbilityShapeCell(0, 0),
                    new AbilityShapeCell(0, 1),
                    new AbilityShapeCell(0, 2),
                    new AbilityShapeCell(0, 3),
                }),
                _ => CreatePreset(AbilityDefinitionId.OLeft),
            };
        }
    }
}
