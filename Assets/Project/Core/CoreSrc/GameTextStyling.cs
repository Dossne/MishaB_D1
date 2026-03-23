using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TetrisTactic.Core
{
    public static class GameTextStyling
    {
        private const float UiFontScale = 1.2f;
        private const char ThinSpace = '\u200A';

        public static int ScaleUiFontSize(int baseSize)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseSize * UiFontScale));
        }

        public static string ApplyTracking(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
            {
                return value;
            }

            var builder = new StringBuilder(value.Length * 2);
            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (i > 0)
                {
                    var previous = value[i - 1];
                    if (!char.IsWhiteSpace(previous) && !char.IsWhiteSpace(current))
                    {
                        builder.Append(ThinSpace);
                    }
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        public static void SetUiText(Text text, string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = ApplyTracking(value);
        }

        public static void SetWorldText(TextMesh textMesh, string value)
        {
            if (textMesh == null)
            {
                return;
            }

            textMesh.text = ApplyTracking(value);
        }
    }
}
