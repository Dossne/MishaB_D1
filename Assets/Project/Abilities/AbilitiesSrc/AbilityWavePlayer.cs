using System.Collections;
using System.Collections.Generic;
using TetrisTactic.PlayField;
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
            System.Func<GridPosition, Vector3> worldPositionResolver,
            System.Action<GridPosition> onWaveCellReached,
            System.Action onCompleted)
        {
            coroutineRunner.StartCoroutine(PlayWaveRoutine(waveSteps, worldPositionResolver, onWaveCellReached, onCompleted));
        }

        private IEnumerator PlayWaveRoutine(
            IReadOnlyList<List<GridPosition>> waveSteps,
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
                    SpawnImpact(worldPositionResolver != null ? worldPositionResolver(cell) : Vector3.zero);
                }

                if (stepIndex < waveSteps.Count - 1)
                {
                    yield return new WaitForSeconds(stepDelay);
                }
            }

            yield return new WaitForSeconds(impactDuration);
            onCompleted?.Invoke();
        }

        private void SpawnImpact(Vector3 worldPosition)
        {
            var impact = new GameObject("AbilityImpactCell", typeof(SpriteRenderer));
            impact.transform.position = worldPosition;
            impact.transform.localScale = Vector3.one * 0.62f;

            var renderer = impact.GetComponent<SpriteRenderer>();
            renderer.sprite = AbilityButtonView.GetFallbackSprite();
            renderer.color = new Color(1f, 0.67f, 0.2f, 0.84f);
            renderer.sortingOrder = 20;

            Object.Destroy(impact, impactDuration);
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