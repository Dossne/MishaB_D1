using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.PlayerTurn
{
    public sealed class WaitButtonView : MonoBehaviour
    {
        public event System.Action Pressed;

        [SerializeField] private Button button;

        public void Bind(Button sourceButton)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }

            button = sourceButton;

            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
                button.onClick.AddListener(OnClicked);
            }
        }

        public void SetInteractable(bool canUse)
        {
            if (button != null)
            {
                button.interactable = canUse;
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            Pressed?.Invoke();
        }
    }
}
