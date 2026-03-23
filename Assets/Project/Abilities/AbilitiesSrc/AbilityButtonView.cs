using TetrisTactic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.Abilities
{
    public sealed class AbilityButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Text label;

        private static Sprite fallbackSprite;

        public void BindView(Button boundButton, Image boundBackground, Text boundLabel)
        {
            button = boundButton;
            background = boundBackground;
            label = boundLabel;
        }

        public void Bind(string labelText, System.Action onPressed, bool interactable, bool selected)
        {
            if (button == null || background == null || label == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (onPressed != null)
            {
                button.onClick.AddListener(() => onPressed());
            }

            button.interactable = interactable;
            GameTextStyling.SetUiText(label, labelText);
            background.color = selected
                ? new Color(0.78f, 0.47f, 0.17f, 1f)
                : new Color(0.24f, 0.31f, 0.44f, 1f);
        }

        public static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.SetPixel(1, 0, Color.white);
            texture.SetPixel(0, 1, Color.white);
            texture.SetPixel(1, 1, Color.white);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
            return fallbackSprite;
        }
    }
}

