using TetrisTactic.Core;
using TetrisTactic.FinishFlow;
using TetrisTactic.MainUi;
using TetrisTactic.PlayField;
using TetrisTactic.Resource;
using UnityEngine;

namespace TetrisTactic.LevelFlow
{
    public sealed class LevelFlowController : IInitializableController, IDisposableController
    {
        private const int StageRewardAmount = 1;

        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly ResourceController resourceController;

        private ProgressionPopup progressionPopup;
        private FinishPopup finishPopup;
        private ResourceCounter[] resourceCounters;
        private int currentLevel = 1;
        private int pendingReward;

        public LevelFlowController(ServiceLocator serviceLocator, PlayFieldController playFieldController, ResourceController resourceController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.resourceController = resourceController;
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

            playFieldController.CellTapped -= OnFieldCellTapped;
            playFieldController.CellTapped += OnFieldCellTapped;

            resourceController.BalanceChanged -= OnResourceBalanceChanged;
            resourceController.BalanceChanged += OnResourceBalanceChanged;

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

            playFieldController.CellTapped -= OnFieldCellTapped;
            resourceController.BalanceChanged -= OnResourceBalanceChanged;
        }

        private void OnStartLevelRequested()
        {
            finishPopup.Hide();
            progressionPopup.Hide();
            playFieldController.CreateField();
        }

        private void OnFieldCellTapped(GridPosition _)
        {
            playFieldController.ClearField();
            pendingReward = StageRewardAmount;
            finishPopup.Show(isVictory: false, resourceAmount: StageRewardAmount, victoryBonusAmount: 1);
        }

        private void OnFinishContinueRequested()
        {
            if (pendingReward > 0)
            {
                resourceController.Add(pendingReward);
                pendingReward = 0;
            }

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
