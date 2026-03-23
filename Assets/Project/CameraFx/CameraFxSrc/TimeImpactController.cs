using System.Collections;
using TetrisTactic.Core;
using UnityEngine;

namespace TetrisTactic.CameraFx
{
    public sealed class TimeImpactController : IInitializableController, IDisposableController
    {
        private const float EnemyDamageImpactScale = 0.08f;
        private const float EnemyDamageImpactDuration = 0.06f;

        private MonoBehaviour coroutineRunner;
        private Coroutine impactCoroutine;
        private float restoreTimeScale = 1f;

        public void Initialize()
        {
            coroutineRunner ??= CreateRunner();
        }

        public void Dispose()
        {
            if (coroutineRunner != null)
            {
                coroutineRunner.StopAllCoroutines();
            }

            impactCoroutine = null;
            Time.timeScale = restoreTimeScale;
        }

        public void PlayEnemyDamageImpact()
        {
            Initialize();

            if (impactCoroutine != null)
            {
                coroutineRunner.StopCoroutine(impactCoroutine);
            }

            impactCoroutine = coroutineRunner.StartCoroutine(EnemyDamageImpactRoutine());
        }

        private IEnumerator EnemyDamageImpactRoutine()
        {
            restoreTimeScale = Mathf.Max(0.01f, Time.timeScale);
            Time.timeScale = EnemyDamageImpactScale;
            yield return new WaitForSecondsRealtime(EnemyDamageImpactDuration);
            Time.timeScale = restoreTimeScale;
            impactCoroutine = null;
        }

        private static MonoBehaviour CreateRunner()
        {
            var runnerObject = new GameObject("TimeImpactRunner", typeof(TimeImpactRunnerBehaviour));
            Object.DontDestroyOnLoad(runnerObject);
            return runnerObject.GetComponent<TimeImpactRunnerBehaviour>();
        }

        private sealed class TimeImpactRunnerBehaviour : MonoBehaviour
        {
        }
    }
}
