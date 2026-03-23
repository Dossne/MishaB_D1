using UnityEngine;
namespace TetrisTactic.Units
{
    public sealed class HealthComponent
    {
        public HealthComponent(int maxHp)
        {
            MaxHp = Mathf.Max(1, maxHp);
            CurrentHp = MaxHp;
        }
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public bool IsAlive => CurrentHp > 0;
        public bool TryApplyDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return false;
            }
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            return true;
        }
        public bool TryHeal(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return false;
            }
            var newHp = Mathf.Min(MaxHp, CurrentHp + amount);
            if (newHp == CurrentHp)
            {
                return false;
            }
            CurrentHp = newHp;
            return true;
        }
    }
}
