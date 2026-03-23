using System.Collections;
using System.Collections.Generic;
using TetrisTactic.PlayField;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Abilities
{
    public sealed class AbilityWavePlayer
    {
        private readonly MonoBehaviour coroutineRunner;
        private readonly float stepDelay;
        private readonly float impactDuration;

        public AbilityWavePlayer(float stepDelay, float impactDuration)
        {
            this.stepDelay = Mathf.Max(0.01f, stepDelay);
            this.impactDuration = Mathf.Max(0.01f, impactDuration);
            coroutineRunner = CreateRunner();
        }

        public void PlayWave(
            IReadOnlyList<List<GridPosition>> waveSteps,
            UnitType ownerUnitType,
            System.Func<GridPosition, Vector3> worldPositionResolver,
            System.Action<GridPosition> onWaveCellReached,
            System.Action onCompleted)
        {
            coroutineRunner.StartCoroutine(PlayWaveRoutine(waveSteps, ownerUnitType, worldPositionResolver, onWaveCellReached, onCompleted));
        }

        private IEnumerator PlayWaveRoutine(
            IReadOnlyList<List<GridPosition>> waveSteps,
            UnitType ownerUnitType,
            System.Func<GridPosition, Vector3> worldPositionResolver,
            System.Action<GridPosition> onWaveCellReached,
            System.Action onCompleted)
        {
            if (waveSteps == null || waveSteps.Count == 0)
            {
                onCompleted?.Invoke();
                yield break;
            }

            for (var stepIndex = 0; stepIndex < waveSteps.Count; stepIndex++)
            {
                var step = waveSteps[stepIndex];
                for (var i = 0; i < step.Count; i++)
                {
                    var cell = step[i];
                    onWaveCellReached?.Invoke(cell);
                    SpawnImpact(ownerUnitType, worldPositionResolver != null ? worldPositionResolver(cell) : Vector3.zero);
                }

                if (stepIndex < waveSteps.Count - 1)
                {
                    yield return new WaitForSeconds(stepDelay);
                }
            }

            yield return new WaitForSeconds(impactDuration);
            onCompleted?.Invoke();
        }

        private void SpawnImpact(UnitType ownerUnitType, Vector3 worldPosition)
        {
            var impact = new GameObject("AbilityImpactCell", typeof(SpriteRenderer));
            impact.transform.position = worldPosition;

            var renderer = impact.GetComponent<SpriteRenderer>();
            var icon = AbilityIconResolver.GetAbilityIcon(ownerUnitType);
            renderer.sprite = icon != null ? icon : AbilityButtonView.GetFallbackSprite();
            renderer.color = Color.white;
            renderer.sortingOrder = 20;

            var scale = CalculateImpactScale(renderer.sprite);
            impact.transform.localScale = Vector3.one * (scale * 0.65f);
            coroutineRunner.StartCoroutine(AnimateImpactRoutine(impact.transform, renderer, scale));
        }

        private IEnumerator AnimateImpactRoutine(Transform impactTransform, SpriteRenderer renderer, float finalScale)
        {
            var elapsed = 0f;
            var duration = impactDuration;
            var startScale = finalScale * 0.65f;
            var bounceScale = finalScale * 1.08f;
            var baseColor = Color.white;

            while (elapsed < duration)
            {
                if (impactTransform == null || renderer == null)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);

                if (t < 0.35f)
                {
                    var localT = t / 0.35f;
                    impactTransform.localScale = Vector3.one * Mathf.Lerp(startScale, bounceScale, localT);
                }
                else
                {
                    var localT = (t - 0.35f) / 0.65f;
                    impactTransform.localScale = Vector3.one * Mathf.Lerp(bounceScale, finalScale, localT);
                }

                var color = baseColor;
                color.a = 1f - t;
                renderer.color = color;

                yield return null;
            }

            if (impactTransform != null)
            {
                Object.Destroy(impactTransform.gameObject);
            }
        }

        private static float CalculateImpactScale(Sprite sprite)
        {
            const float targetSize = 0.62f;
            if (sprite == null)
            {
                return targetSize;
            }

            var bounds = sprite.bounds.size;
            var maxSize = Mathf.Max(bounds.x, bounds.y);
            if (maxSize <= 0.0001f)
            {
                return targetSize;
            }

            return targetSize / maxSize;
        }

        private static MonoBehaviour CreateRunner()
        {
            var runnerObject = new GameObject("AbilityWaveRunner", typeof(AbilityWaveRunnerBehaviour));
            Object.DontDestroyOnLoad(runnerObject);
            return runnerObject.GetComponent<AbilityWaveRunnerBehaviour>();
        }

        private sealed class AbilityWaveRunnerBehaviour : MonoBehaviour
        {
        }
    }
}
