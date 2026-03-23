using TetrisTactic.Core;
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

            progressionPopup.StartLevelRequested -= OnStartLevelRequested;
            progressionPopup.StartLevelRequested += OnStartLevelRequested;

            playFieldController.CellTapped -= OnFieldCellTapped;
            playFieldController.CellTapped += OnFieldCellTapped;

            progressionPopup.Show(currentLevel);
        }

        public void Dispose()
        {
            if (progressionPopup != null)
            {
                progressionPopup.StartLevelRequested -= OnStartLevelRequested;
            }

            playFieldController.CellTapped -= OnFieldCellTapped;
        }

        private void OnStartLevelRequested()
        {
            progressionPopup.Hide();
            playFieldController.CreateField();
        }

        private void OnFieldCellTapped(GridPosition _)
        {
            playFieldController.ClearField();
            progressionPopup.Show(currentLevel);
        }
    }
}
