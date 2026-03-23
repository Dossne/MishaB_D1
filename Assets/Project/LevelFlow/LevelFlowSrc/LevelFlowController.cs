using TetrisTactic.Core;
using TetrisTactic.FinishFlow;
using TetrisTactic.MainUi;
using TetrisTactic.PlayField;
using UnityEngine;

namespace TetrisTactic.LevelFlow
{
    public sealed class LevelFlowController : IInitializableController, IDisposableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;

        private ProgressionPopup progressionPopup;
        private FinishPopup finishPopup;
        private int currentLevel = 1;

        public LevelFlowController(ServiceLocator serviceLocator, PlayFieldController playFieldController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
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

            progressionPopup.StartLevelRequested -= OnStartLevelRequested;
            progressionPopup.StartLevelRequested += OnStartLevelRequested;

            finishPopup.ContinueRequested -= OnFinishContinueRequested;
            finishPopup.ContinueRequested += OnFinishContinueRequested;

            playFieldController.CellTapped -= OnFieldCellTapped;
            playFieldController.CellTapped += OnFieldCellTapped;

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
            finishPopup.Show(isVictory: false, resourceAmount: 0, victoryBonusAmount: 1);
        }

        private void OnFinishContinueRequested()
        {
            finishPopup.Hide();
            progressionPopup.Show(currentLevel);
        }
    }
}
