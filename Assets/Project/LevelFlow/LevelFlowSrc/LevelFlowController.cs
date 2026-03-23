using TetrisTactic.Core;
using TetrisTactic.FinishFlow;
using TetrisTactic.MainUi;
using TetrisTactic.PlayField;
using TetrisTactic.PlayerTurn;
using TetrisTactic.Resource;
using UnityEngine;

namespace TetrisTactic.LevelFlow
{
    public sealed class LevelFlowController : IInitializableController, IDisposableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly ResourceController resourceController;
        private readonly PlayerTurnController playerTurnController;

        private ProgressionPopup progressionPopup;
        private FinishPopup finishPopup;
        private ResourceCounter[] resourceCounters;
        private int currentLevel = 1;

        public LevelFlowController(
            ServiceLocator serviceLocator,
            PlayFieldController playFieldController,
            ResourceController resourceController,
            PlayerTurnController playerTurnController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.resourceController = resourceController;
            this.playerTurnController = playerTurnController;
        }

        public void Initialize()
        {
            var uiProvider = serviceLocator.MainUiProvider;
            if (uiProvider == null)
            {
                Debug.LogError("LevelFlowController requires MainUiProvider.");
                return;
            }

            progressionPopup = uiProvider.ProgressionPopup;
            if (progressionPopup == null)
            {
                Debug.LogError("LevelFlowController requires ProgressionPopup.");
                return;
            }

            finishPopup = uiProvider.FinishPopup;
            if (finishPopup == null)
            {
                Debug.LogError("LevelFlowController requires FinishPopup.");
                return;
            }

            resourceCounters = uiProvider.GetResourceCounters();
            progressionPopup.Initialize(resourceController);

            progressionPopup.StartLevelRequested -= OnStartLevelRequested;
            progressionPopup.StartLevelRequested += OnStartLevelRequested;

            finishPopup.ContinueRequested -= OnFinishContinueRequested;
            finishPopup.ContinueRequested += OnFinishContinueRequested;

            resourceController.BalanceChanged -= OnResourceBalanceChanged;
            resourceController.BalanceChanged += OnResourceBalanceChanged;

            playerTurnController.TurnEnded -= OnPlayerTurnEnded;
            playerTurnController.TurnEnded += OnPlayerTurnEnded;

            RefreshResourceCounters(resourceController.GetCurrentAmount());

            finishPopup.Hide();
            progressionPopup.Show(currentLevel);
        }

        public void Dispose()
        {
            if (progressionPopup != null)
            {
                progressionPopup.StartLevelRequested -= OnStartLevelRequested;
            }

            if (finishPopup != null)
            {
                finishPopup.ContinueRequested -= OnFinishContinueRequested;
            }

            resourceController.BalanceChanged -= OnResourceBalanceChanged;
            playerTurnController.TurnEnded -= OnPlayerTurnEnded;
        }

        private void OnStartLevelRequested()
        {
            finishPopup.Hide();
            progressionPopup.Hide();
            playFieldController.CreateField();
            playerTurnController.BeginTurn();
        }

        private void OnPlayerTurnEnded(PlayerTurnActionType action)
        {
            if (!playFieldController.HasActiveField)
            {
                return;
            }

            Debug.Log($"Player turn ended with action: {action}");

            // Stage 6 has no enemy turn yet, so we immediately hand control back.
            playerTurnController.BeginTurn();
        }

        private void OnFinishContinueRequested()
        {
            finishPopup.Hide();
            progressionPopup.Show(currentLevel);
        }

        private void OnResourceBalanceChanged(int amount)
        {
            RefreshResourceCounters(amount);
        }

        private void RefreshResourceCounters(int amount)
        {
            if (resourceCounters == null)
            {
                return;
            }

            for (var i = 0; i < resourceCounters.Length; i++)
            {
                if (resourceCounters[i] == null)
                {
                    continue;
                }

                resourceCounters[i].Refresh(amount);
            }
        }
    }
}
