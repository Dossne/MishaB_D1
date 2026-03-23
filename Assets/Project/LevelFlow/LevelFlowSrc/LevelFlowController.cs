using TetrisTactic.Core;
using TetrisTactic.EnemyTurn;
using TetrisTactic.FinishFlow;
using TetrisTactic.MainUi;
using TetrisTactic.PlayField;
using TetrisTactic.PlayerTurn;
using TetrisTactic.Resource;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.LevelFlow
{
    public sealed class LevelFlowController : IInitializableController, IDisposableController
    {
        private const int BaseFinishReward = 1;
        private const int VictoryBonusReward = 1;

        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly ResourceController resourceController;
        private readonly PlayerTurnController playerTurnController;
        private readonly EnemyTurnController enemyTurnController;

        private ProgressionPopup progressionPopup;
        private FinishPopup finishPopup;
        private ResourceCounter[] resourceCounters;
        private int currentLevel = 1;
        private bool isBattleActive;
        private int pendingReward;

        public LevelFlowController(
            ServiceLocator serviceLocator,
            PlayFieldController playFieldController,
            ResourceController resourceController,
            PlayerTurnController playerTurnController,
            EnemyTurnController enemyTurnController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.resourceController = resourceController;
            this.playerTurnController = playerTurnController;
            this.enemyTurnController = enemyTurnController;
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

            playFieldController.UnitDied -= OnUnitDied;
            playFieldController.UnitDied += OnUnitDied;

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
            playFieldController.UnitDied -= OnUnitDied;
        }

        private void OnStartLevelRequested()
        {
            pendingReward = 0;
            isBattleActive = true;

            finishPopup.Hide();
            progressionPopup.Hide();
            playFieldController.CreateField();

            if (TryResolveBattleOutcome())
            {
                return;
            }

            enemyTurnController.RefreshDangerHighlights();
            playerTurnController.BeginTurn();
        }

        private void OnPlayerTurnEnded(PlayerTurnActionType action)
        {
            if (!playFieldController.HasActiveField || !isBattleActive)
            {
                return;
            }

            if (TryResolveBattleOutcome())
            {
                return;
            }

            Debug.Log($"Player turn ended with action: {action}");
            enemyTurnController.BeginEnemyTurn(OnEnemyTurnCompleted);
        }

        private void OnEnemyTurnCompleted()
        {
            if (!playFieldController.HasActiveField || !isBattleActive)
            {
                return;
            }

            if (TryResolveBattleOutcome())
            {
                return;
            }

            enemyTurnController.RefreshDangerHighlights();
            playerTurnController.BeginTurn();
        }

        private void OnUnitDied(UnitRuntimeModel deadUnit)
        {
            if (!isBattleActive || deadUnit == null)
            {
                return;
            }

            if (deadUnit.TeamType == TeamType.Player)
            {
                HandleBattleCompleted(false);
            }
        }

        private bool TryResolveBattleOutcome()
        {
            if (!isBattleActive || !playFieldController.HasActiveField)
            {
                return false;
            }

            if (!playFieldController.IsPlayerAlive)
            {
                HandleBattleCompleted(false);
                return true;
            }

            if (!playFieldController.HasLivingEnemies)
            {
                HandleBattleCompleted(true);
                return true;
            }

            return false;
        }
        private void HandleBattleCompleted(bool isVictory)
        {
            if (!isBattleActive)
            {
                return;
            }

            isBattleActive = false;
            pendingReward = BaseFinishReward + (isVictory ? VictoryBonusReward : 0);

            playerTurnController.CancelTurn();
            enemyTurnController.CancelEnemyTurn();

            playFieldController.ClearEnemyDangerHighlights();
            playFieldController.ClearField();

            finishPopup.Show(isVictory, BaseFinishReward, isVictory ? VictoryBonusReward : 0);
        }

        private void OnFinishContinueRequested()
        {
            if (pendingReward > 0)
            {
                resourceController.Add(pendingReward);
            }

            pendingReward = 0;
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