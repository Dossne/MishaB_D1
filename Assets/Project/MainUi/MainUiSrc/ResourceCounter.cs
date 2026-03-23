using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.MainUi
{
    public sealed class ResourceCounter : MonoBehaviour
    {
        [SerializeField] private Text amountText;

        public void BindAmountText(Text targetText)
        {
            amountText = targetText;
        }

        public void Refresh(int amount)
        {
            if (amountText == null)
            {
                return;
            }

            amountText.text = $"Resource: {amount}";
        }
    }
}
