using System.Collections;
using TetrisTactic.Core;
using UnityEngine;

namespace TetrisTactic.CameraFx
{
    public sealed class CameraShakeController : IInitializableController, IDisposableController
    {
        private const float DefaultShakeDuration = 0.16f;
        private const float DefaultShakeMagnitude = 0.14f;

        private MonoBehaviour coroutineRunner;
        private Coroutine shakeCoroutine;
        private float remainingShakeTime;
        private float currentShakeMagnitude;

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

            shakeCoroutine = null;
            remainingShakeTime = 0f;
            currentShakeMagnitude = 0f;
        }

        public void PlayPlayerDamageShake()
        {
            PlayShake(DefaultShakeDuration, DefaultShakeMagnitude);
        }

        public void PlayShake(float duration, float magnitude)
        {
            Initialize();

            remainingShakeTime = Mathf.Max(remainingShakeTime, Mathf.Max(0.05f, duration));
            currentShakeMagnitude = Mathf.Max(currentShakeMagnitude, Mathf.Max(0.02f, magnitude));

            if (shakeCoroutine == null)
            {
                shakeCoroutine = coroutineRunner.StartCoroutine(ShakeRoutine());
            }
        }

        private IEnumerator ShakeRoutine()
        {
            var basePosition = Vector3.zero;
            while (remainingShakeTime > 0f)
            {
                var camera = Camera.main;
                if (camera == null)
                {
                    shakeCoroutine = null;
                    remainingShakeTime = 0f;
                    yield break;
                }

                var cameraTransform = camera.transform;
                basePosition = cameraTransform.position;

                while (remainingShakeTime > 0f)
                {
                    remainingShakeTime -= Time.unscaledDeltaTime;
                    var damping = Mathf.Clamp01(remainingShakeTime / DefaultShakeDuration);
                    var strength = currentShakeMagnitude * Mathf.Lerp(0.4f, 1f, damping);
                    var offset = Random.insideUnitCircle * strength;
                    cameraTransform.position = new Vector3(basePosition.x + offset.x, basePosition.y + offset.y, basePosition.z);
                    yield return null;
                }

                cameraTransform.position = basePosition;
            }

            currentShakeMagnitude = 0f;
            shakeCoroutine = null;
        }

        private static MonoBehaviour CreateRunner()
        {
            var runnerObject = new GameObject("CameraShakeRunner", typeof(CameraShakeRunnerBehaviour));
            Object.DontDestroyOnLoad(runnerObject);
            return runnerObject.GetComponent<CameraShakeRunnerBehaviour>();
        }

        private sealed class CameraShakeRunnerBehaviour : MonoBehaviour
        {
        }
    }
}
