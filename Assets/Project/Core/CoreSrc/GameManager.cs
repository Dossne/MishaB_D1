using TetrisTactic.Abilities;
using TetrisTactic.Audio;
using TetrisTactic.CameraFx;
using TetrisTactic.EnemyTurn;
using TetrisTactic.Feedback;
using TetrisTactic.LevelFlow;
using TetrisTactic.PlayField;
using TetrisTactic.PlayerTurn;
using TetrisTactic.Progression;
using TetrisTactic.Resource;
using UnityEngine;

namespace TetrisTactic.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private ServiceLocator serviceLocator;

        private readonly System.Collections.Generic.List<IInitializableController> initializableControllers = new();
        private readonly System.Collections.Generic.List<IUpdatableController> updatableControllers = new();
        private readonly System.Collections.Generic.List<IDisposableController> disposableControllers = new();

        private PlayFieldController playFieldController;
        private ResourceController resourceController;
        private AudioController audioController;
        private CameraShakeController cameraShakeController;
        private TimeImpactController timeImpactController;
        private FloatingTextController floatingTextController;
        private HitFeedbackPlayer hitFeedbackPlayer;
        private AbilityController abilityController;
        private PlayerTurnController playerTurnController;
        private EnemyTurnController enemyTurnController;
        private ProgressionController progressionController;
        private LevelFlowController levelFlowController;

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
        }

        private void Start()
        {
            BuildControllers();
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

        private void BuildControllers()
        {
            playFieldController = new PlayFieldController(serviceLocator);
            RegisterController(playFieldController);

            resourceController = new ResourceController();
            serviceLocator.RegisterResourceController(resourceController);
            RegisterController(resourceController);

            progressionController = new ProgressionController(serviceLocator, resourceController);
            RegisterController(progressionController);

            audioController = new AudioController(serviceLocator);
            RegisterController(audioController);

            cameraShakeController = new CameraShakeController();
            RegisterController(cameraShakeController);

            timeImpactController = new TimeImpactController();
            RegisterController(timeImpactController);

            floatingTextController = new FloatingTextController();
            RegisterController(floatingTextController);

            hitFeedbackPlayer = new HitFeedbackPlayer(
                playFieldController,
                floatingTextController,
                audioController,
                cameraShakeController,
                timeImpactController);
            RegisterController(hitFeedbackPlayer);

            abilityController = new AbilityController(serviceLocator, playFieldController, hitFeedbackPlayer);
            RegisterController(abilityController);

            playerTurnController = new PlayerTurnController(serviceLocator, playFieldController, abilityController);
            RegisterController(playerTurnController);

            enemyTurnController = new EnemyTurnController(playFieldController, abilityController, hitFeedbackPlayer);
            RegisterController(enemyTurnController);

            levelFlowController = new LevelFlowController(
                serviceLocator,
                playFieldController,
                resourceController,
                progressionController,
                audioController,
                playerTurnController,
                enemyTurnController);
            RegisterController(levelFlowController);
        }

        private void RegisterController(object controller)
        {
            if (controller is IInitializableController initializable)
            {
                initializableControllers.Add(initializable);
            }

            if (controller is IUpdatableController updatable)
            {
                updatableControllers.Add(updatable);
            }

            if (controller is IDisposableController disposable)
            {
                disposableControllers.Add(disposable);
            }
        }
    }
}



