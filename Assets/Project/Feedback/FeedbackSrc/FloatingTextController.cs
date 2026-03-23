using System.Collections;
using TetrisTactic.Abilities;
using TetrisTactic.Core;
using UnityEngine;

namespace TetrisTactic.Feedback
{
    public sealed class FloatingTextController : IInitializableController, IDisposableController
    {
        private const float DefaultLifetime = 0.55f;
        private const float DefaultRiseDistance = 0.38f;
        private const int DefaultSortingOrder = 40;

        private MonoBehaviour coroutineRunner;

        public void Initialize()
        {
            coroutineRunner ??= CreateRunner();
        }

        public void Dispose()
        {
            if (coroutineRunner == null)
            {
                return;
            }

            coroutineRunner.StopAllCoroutines();
        }

        public void ShowWorldText(Vector3 worldPosition, string text, Color color)
        {
            ShowWorldText(worldPosition, text, color, DefaultLifetime, DefaultRiseDistance, DefaultSortingOrder);
        }

        public void ShowWorldText(
            Vector3 worldPosition,
            string text,
            Color color,
            float lifetime,
            float riseDistance,
            int sortingOrder)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Initialize();

            var textObject = new GameObject("FloatingText", typeof(TextMesh));
            textObject.transform.position = worldPosition;

            var textMesh = textObject.GetComponent<TextMesh>();
            GameTextStyling.SetWorldText(textMesh, text);
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.18f;
            textMesh.fontSize = 56;
            textMesh.font = LoadGameFont();
            textMesh.color = color;

            var renderer = textMesh.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }

            coroutineRunner.StartCoroutine(AnimateFloatingTextRoutine(textObject.transform, textMesh, Mathf.Max(0.1f, lifetime), riseDistance));
        }

        public void ShowCirclePulse(Vector3 worldPosition, Color color, float lifetime = 0.16f, float maxScale = 0.55f)
        {
            Initialize();

            var pulseObject = new GameObject("HitPulse", typeof(SpriteRenderer));
            pulseObject.transform.position = worldPosition;
            pulseObject.transform.localScale = Vector3.one * 0.05f;

            var renderer = pulseObject.GetComponent<SpriteRenderer>();
            renderer.sprite = AbilityButtonView.GetFallbackSprite();
            renderer.color = color;
            renderer.sortingOrder = 30;

            coroutineRunner.StartCoroutine(AnimatePulseRoutine(pulseObject.transform, renderer, Mathf.Max(0.05f, lifetime), Mathf.Max(0.1f, maxScale)));
        }

        private IEnumerator AnimateFloatingTextRoutine(Transform textTransform, TextMesh textMesh, float lifetime, float riseDistance)
        {
            var elapsed = 0f;
            var startPosition = textTransform.position;
            var rise = Mathf.Max(0.1f, riseDistance);

            while (elapsed < lifetime)
            {
                if (textTransform == null || textMesh == null)
                {
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / lifetime);
                var eased = 1f - ((1f - t) * (1f - t));
                textTransform.position = startPosition + (Vector3.up * rise * eased);

                var color = textMesh.color;
                color.a = 1f - t;
                textMesh.color = color;

                yield return null;
            }

            if (textTransform != null)
            {
                Object.Destroy(textTransform.gameObject);
            }
        }

        private IEnumerator AnimatePulseRoutine(Transform pulseTransform, SpriteRenderer spriteRenderer, float lifetime, float maxScale)
        {
            var elapsed = 0f;
            var startColor = spriteRenderer.color;

            while (elapsed < lifetime)
            {
                if (pulseTransform == null || spriteRenderer == null)
                {
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / lifetime);
                pulseTransform.localScale = Vector3.one * Mathf.Lerp(0.05f, maxScale, t);

                var color = startColor;
                color.a = 1f - t;
                spriteRenderer.color = color;

                yield return null;
            }

            if (pulseTransform != null)
            {
                Object.Destroy(pulseTransform.gameObject);
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

        private static MonoBehaviour CreateRunner()
        {
            var runnerObject = new GameObject("FloatingTextRunner", typeof(FloatingTextRunnerBehaviour));
            Object.DontDestroyOnLoad(runnerObject);
            return runnerObject.GetComponent<FloatingTextRunnerBehaviour>();
        }

        private sealed class FloatingTextRunnerBehaviour : MonoBehaviour
        {
        }
    }
}

