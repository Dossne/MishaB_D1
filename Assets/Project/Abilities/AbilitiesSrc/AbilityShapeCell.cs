using UnityEngine;

namespace TetrisTactic.Abilities
{
    [System.Serializable]
    public struct AbilityShapeCell
    {
        public AbilityShapeCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        [field: SerializeField] public int X { get; private set; }
        [field: SerializeField] public int Y { get; private set; }
    }
}