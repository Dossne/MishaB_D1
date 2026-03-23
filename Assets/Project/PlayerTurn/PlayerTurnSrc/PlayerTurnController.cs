using System.Collections.Generic;
using TetrisTactic.Core;
using TetrisTactic.PlayField;
using UnityEngine;

namespace TetrisTactic.PlayerTurn
{
    public enum PlayerTurnActionType
    {
        Moved = 0,
        Waited = 1,
    }

    public sealed class PlayerTurnController : IInitializableController, IDisposableController
    {
        public event System.Action<PlayerTurnActionType> TurnEnded;

        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;

        private readonly List<GridPosition> legalMoveCache = new(4);

        private MoveHighlighter moveHighlighter;
        private PlayerActionPanel actionPanel;

        private bool isTurnActive;
        private bool usedWaitOnPreviousTurn;

        public PlayerTurnController(ServiceLocator serviceLocator, PlayFieldController playFieldController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
        }

        public void Initialize()
        {
            moveHighlighter = new MoveHighlighter(playFieldController);
            EnsureActionPanel();

            playFieldController.CellTapped -= OnCellTapped;
            playFieldController.CellTapped += OnCellTapped;

            if (actionPanel?.WaitButtonView != null)
            {
                actionPanel.WaitButtonView.Pressed -= OnWaitPressed;
                actionPanel.WaitButtonView.Pressed += OnWaitPressed;
            }
        }

        public void Dispose()
        {
            playFieldController.CellTapped -= OnCellTapped;

            if (actionPanel?.WaitButtonView != null)
            {
                actionPanel.WaitButtonView.Pressed -= OnWaitPressed;
            }
        }

        public void BeginTurn()
        {
            if (!playFieldController.HasActiveField || playFieldController.GetPlayerUnit() == null)
            {
                return;
            }

            isTurnActive = true;
            RefreshTurnPresentation();
        }

        private void EnsureActionPanel()
        {
            if (actionPanel != null)
            {
                return;
            }

            var uiProvider = serviceLocator.MainUiProvider;
            if (uiProvider == null)
            {
                Debug.LogError("PlayerTurnController requires MainUiProvider.");
                return;
            }

            var panelObject = new GameObject("PlayerActionPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(PlayerActionPanel));
            actionPanel = panelObject.GetComponent<PlayerActionPanel>();
            actionPanel.Initialize(uiProvider.HudParent);
        }

        private void RefreshTurnPresentation()
        {
            legalMoveCache.Clear();
            var moves = playFieldController.GetLegalPlayerMoveCells();
            for (var i = 0; i < moves.Count; i++)
            {
                legalMoveCache.Add(moves[i]);
            }

            moveHighlighter.HighlightLegalMoves();

            if (actionPanel != null)
            {
                actionPanel.Show();
                actionPanel.SetWaitInteractable(!usedWaitOnPreviousTurn);
            }
        }

        private void OnCellTapped(GridPosition tappedCell)
        {
            if (!isTurnActive)
            {
                return;
            }

            // Stage 6: no ability selection yet, so taps only attempt movement.
            if (!IsLegalMoveCell(tappedCell))
            {
                return;
            }

            if (!playFieldController.TryMovePlayerTo(tappedCell))
            {
                return;
            }

            usedWaitOnPreviousTurn = false;
            EndTurn(PlayerTurnActionType.Moved);
        }

        private void OnWaitPressed()
        {
            if (!isTurnActive || usedWaitOnPreviousTurn)
            {
                return;
            }

            usedWaitOnPreviousTurn = true;
            EndTurn(PlayerTurnActionType.Waited);
        }

        private bool IsLegalMoveCell(GridPosition position)
        {
            for (var i = 0; i < legalMoveCache.Count; i++)
            {
                if (legalMoveCache[i] == position)
                {
                    return true;
                }
            }

            return false;
        }

        private void EndTurn(PlayerTurnActionType actionType)
        {
            isTurnActive = false;
            moveHighlighter.Clear();

            if (actionPanel != null)
            {
                actionPanel.Hide();
            }

            TurnEnded?.Invoke(actionType);
        }
    }
}
