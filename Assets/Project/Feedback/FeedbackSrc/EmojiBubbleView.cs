using System.Collections;
using UnityEngine;

namespace TetrisTactic.Feedback
{
    public sealed class EmojiBubbleView : MonoBehaviour
    {
        private const float DamageTextCharacterSize = 0.11f;
        private const int DamageTextFontSize = 26;

        private TextMesh textMesh;

        public void Show(string value, Color color, float lifetime)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Destroy(gameObject);
                return;
            }

            EnsureTextMesh();
            textMesh.text = value;
            textMesh.color = color;
            StartCoroutine(PlayRoutine(Mathf.Max(0.12f, lifetime)));
        }

        private void EnsureTextMesh()
        {
            if (textMesh != null)
            {
                return;
            }

            var labelObject = new GameObject("EmojiLabel", typeof(TextMesh));
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.zero;

            textMesh = labelObject.GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = DamageTextCharacterSize;
            textMesh.fontSize = DamageTextFontSize;
            textMesh.font = LoadGameFont();
            textMesh.richText = false;
            textMesh.lineSpacing = 1f;
            textMesh.tabSize = 4f;
            textMesh.color = Color.white;

            var renderer = textMesh.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 45;
                if (textMesh.font != null && textMesh.font.material != null)
                {
                    renderer.sharedMaterial = textMesh.font.material;
                }
            }
        }

        private static Font LoadGameFont()
        {
            var font = Resources.Load<Font>("bangerscyrillic");
            if (font != null)
            {
                return font;
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private IEnumerator PlayRoutine(float lifetime)
        {
            var elapsed = 0f;
            var startPosition = transform.position;
            var riseDistance = 0.28f;
            var initialScale = 0.5f;
            var finalScale = 1f;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / lifetime);
                transform.position = startPosition + (Vector3.up * riseDistance * t);
                var scale = Mathf.Lerp(initialScale, finalScale, t);
                transform.localScale = new Vector3(scale, scale, 1f);

                if (textMesh != null)
                {
                    var color = textMesh.color;
                    color.a = 1f - t;
                    textMesh.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
