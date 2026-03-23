namespace TetrisTactic.Abilities
{
    public sealed class AbilityRuntime
    {
        public AbilityRuntime(AbilityDefinition definition)
        {
            Definition = definition;
        }

        public AbilityDefinition Definition { get; }
    }
}