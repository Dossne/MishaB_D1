using System.Collections.Generic;
using TetrisTactic.Abilities;
using TetrisTactic.Core;
using TetrisTactic.PlayField;
using UnityEngine;

namespace TetrisTactic.PlayerTurn
{
    public enum PlayerTurnActionType
    {
        Moved = 0,
        Waited = 1,
        Attacked = 2,
    }

    public sealed class PlayerTurnController : IInitializableController, IDisposableController
    {
        public event System.Action<PlayerTurnActionType> TurnEnded;

        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly AbilityController abilityController;

        private readonly List<GridPosition> legalMoveCache = new(4);

        private MoveHighlighter moveHighlighter;
        private PlayerActionPanel actionPanel;

        private bool isTurnActive;
        private bool usedWaitOnPreviousTurn;
        private bool isResolvingAbility;

        public PlayerTurnController(ServiceLocator serviceLocator, PlayFieldController playFieldController, AbilityController abilityController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.abilityController = abilityController;
        }

        public void Initialize()
        {
            moveHighlighter = new MoveHighlighter(playFieldController);
            EnsureActionPanel();

            playFieldController.CellTapped -= OnCellTapped;
            playFieldController.CellTapped += OnCellTapped;

            abilityController.BindActionPanel(actionPanel);
            abilityController.SelectionChanged -= OnAbilitySelectionChanged;
            abilityController.SelectionChanged += OnAbilitySelectionChanged;

            if (actionPanel?.WaitButtonView != null)
            {
                actionPanel.WaitButtonView.Pressed -= OnWaitPressed;
                actionPanel.WaitButtonView.Pressed += OnWaitPressed;
            }
        }

        public void Dispose()
        {
            playFieldController.CellTapped -= OnCellTapped;
            abilityController.SelectionChanged -= OnAbilitySelectionChanged;

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
            isResolvingAbility = false;
            abilityController.BeginTurn(playFieldController.GetPlayerUnit());
            RefreshTurnPresentation();
        }

        public void CancelTurn()
        {
            isTurnActive = false;
            isResolvingAbility = false;
            moveHighlighter?.Clear();
            playFieldController.ClearAbilityHighlights();
            abilityController.ClearSelection();

            if (actionPanel != null)
            {
                actionPanel.Hide();
            }
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

            if (abilityController.HasSelectedAbility)
            {
                moveHighlighter.Clear();
            }
            else
            {
                moveHighlighter.HighlightLegalMoves();
            }

            if (actionPanel != null)
            {
                actionPanel.Show();
                actionPanel.SetWaitInteractable(!usedWaitOnPreviousTurn && !isResolvingAbility);
            }
        }

        private void OnCellTapped(GridPosition tappedCell)
        {
            if (!isTurnActive || isResolvingAbility)
            {
                return;
            }

            if (abilityController.HasSelectedAbility)
            {
                var handledAbilityTap = abilityController.TryHandleCellTap(tappedCell, OnAbilityCastStarted, OnAbilityCastCompleted);
                if (handledAbilityTap)
                {
                    return;
                }
            }

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
            if (!isTurnActive || usedWaitOnPreviousTurn || isResolvingAbility)
            {
                return;
            }

            usedWaitOnPreviousTurn = true;
            EndTurn(PlayerTurnActionType.Waited);
        }

        private void OnAbilityCastStarted()
        {
            isResolvingAbility = true;
            RefreshTurnPresentation();
        }

        private void OnAbilityCastCompleted()
        {
            isResolvingAbility = false;
            usedWaitOnPreviousTurn = false;
            EndTurn(PlayerTurnActionType.Attacked);
        }

        private void OnAbilitySelectionChanged()
        {
            if (!isTurnActive || isResolvingAbility)
            {
                return;
            }

            RefreshTurnPresentation();
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
            isResolvingAbility = false;
            moveHighlighter.Clear();
            playFieldController.ClearAbilityHighlights();
            abilityController.ClearSelection();

            if (actionPanel != null)
            {
                actionPanel.Hide();
            }

            TurnEnded?.Invoke(actionType);
        }
    }
}