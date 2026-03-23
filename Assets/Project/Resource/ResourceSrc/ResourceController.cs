using System;
using TetrisTactic.Core;

namespace TetrisTactic.Resource
{
    public sealed class ResourceController : IInitializableController
    {
        private int currentAmount;

        public event Action<int> BalanceChanged;

        public void Initialize()
        {
            Add(1);
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentAmount += amount;
            BalanceChanged?.Invoke(currentAmount);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (!HasEnough(amount))
            {
                return false;
            }

            currentAmount -= amount;
            BalanceChanged?.Invoke(currentAmount);
            return true;
        }

        public bool HasEnough(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            return currentAmount >= amount;
        }

        public int GetCurrentAmount()
        {
            return currentAmount;
        }
    }
}
