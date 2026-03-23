using UnityEngine;

namespace TetrisTactic.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private ServiceLocator serviceLocator;

        private readonly System.Collections.Generic.List<IInitializableController> initializableControllers = new();
        private readonly System.Collections.Generic.List<IUpdatableController> updatableControllers = new();
        private readonly System.Collections.Generic.List<IDisposableController> disposableControllers = new();

        private void Awake()
        {
            if (serviceLocator == null)
            {
                Debug.LogError("GameManager requires a ServiceLocator reference.");
                enabled = false;
                return;
            }

            // Stage 0 bootstrap order: register GameManager first, then ConfigurationProvider.
            serviceLocator.RegisterGameManager(this);
            serviceLocator.RegisterConfigurationProvider();

            InitializeControllers();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            for (var i = 0; i < updatableControllers.Count; i++)
            {
                updatableControllers[i].Tick(deltaTime);
            }
        }

        private void OnDestroy()
        {
            for (var i = disposableControllers.Count - 1; i >= 0; i--)
            {
                disposableControllers[i].Dispose();
            }

            disposableControllers.Clear();
            updatableControllers.Clear();
            initializableControllers.Clear();
        }

        private void InitializeControllers()
        {
            for (var i = 0; i < initializableControllers.Count; i++)
            {
                initializableControllers[i].Initialize();
            }
        }
    }
}