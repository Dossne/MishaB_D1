using TetrisTactic.Core;
using TetrisTactic.EnemyTurn;
using TetrisTactic.FinishFlow;
using TetrisTactic.MainUi;
using TetrisTactic.PlayField;
using TetrisTactic.PlayerTurn;
using TetrisTactic.Progression;
using TetrisTactic.Resource;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.LevelFlow
{
    public sealed class LevelFlowController : IInitializableController, IDisposableController
    {
        private const int EnemyKillResourceReward = 1;
        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly ResourceController resourceController;
        private readonly ProgressionController progressionController;
        private readonly PlayerTurnController playerTurnController;
        private readonly EnemyTurnController enemyTurnController;

        private ProgressionPopup progressionPopup;
        private FinishPopup finishPopup;
        private ResourceCounter[] resourceCounters;
        private bool isBattleActive;
        private int pendingReward;
        private int collectedTreasureReward;
        private bool lastBattleWasVictory;

        public LevelFlowController(
            ServiceLocator serviceLocator,
            PlayFieldController playFieldController,
            ResourceController resourceController,
            ProgressionController progressionController,
            PlayerTurnController playerTurnController,
            EnemyTurnController enemyTurnController)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.resourceController = resourceController;
            this.progressionController = progressionController;
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
            progressionPopup.Initialize();

            progressionPopup.StartLevelRequested -= OnStartLevelRequested;
            progressionPopup.StartLevelRequested += OnStartLevelRequested;

            progressionPopup.UpgradeRequested -= OnUpgradeRequested;
            progressionPopup.UpgradeRequested += OnUpgradeRequested;

            finishPopup.ContinueRequested -= OnFinishContinueRequested;
            finishPopup.ContinueRequested += OnFinishContinueRequested;

            resourceController.BalanceChanged -= OnResourceBalanceChanged;
            resourceController.BalanceChanged += OnResourceBalanceChanged;

            playerTurnController.TurnEnded -= OnPlayerTurnEnded;
            playerTurnController.TurnEnded += OnPlayerTurnEnded;

            playFieldController.UnitDied -= OnUnitDied;
            playFieldController.UnitDied += OnUnitDied;

            playFieldController.TreasureCollected -= OnTreasureCollected;
            playFieldController.TreasureCollected += OnTreasureCollected;

            RefreshResourceCounters(resourceController.GetCurrentAmount());

            finishPopup.Hide();
            progressionPopup.Show(progressionController.CurrentLevel);
        }

        public void Dispose()
        {
            if (progressionPopup != null)
            {
                progressionPopup.StartLevelRequested -= OnStartLevelRequested;
                progressionPopup.UpgradeRequested -= OnUpgradeRequested;
            }

            if (finishPopup != null)
            {
                finishPopup.ContinueRequested -= OnFinishContinueRequested;
            }

            resourceController.BalanceChanged -= OnResourceBalanceChanged;
            playerTurnController.TurnEnded -= OnPlayerTurnEnded;
            playFieldController.UnitDied -= OnUnitDied;
            playFieldController.TreasureCollected -= OnTreasureCollected;
        }

        private void OnStartLevelRequested()
        {
            pendingReward = 0;
            collectedTreasureReward = 0;
            lastBattleWasVictory = false;
            isBattleActive = true;

            finishPopup.Hide();
            progressionPopup.Hide();

            var levelDefinition = progressionController.BuildCurrentLevelDefinition();
            playFieldController.CreateField(levelDefinition, progressionController.PlayerUpgradeState);

            if (TryResolveBattleOutcome())
            {
                return;
            }

            enemyTurnController.RefreshDangerHighlights();
            playerTurnController.BeginTurn();
        }

        private void OnUpgradeRequested(PlayerUpgradeType upgradeType)
        {
            var purchased = progressionController.TryPurchaseUpgrade(upgradeType);
            if (!purchased)
            {
                Debug.Log("ProgressionPopup: Not enough resource for selected upgrade.");
                return;
            }

            Debug.Log($"ProgressionPopup: Purchased {upgradeType} upgrade.");
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

            if (deadUnit.TeamType == TeamType.Enemy)
            {
                resourceController.Add(EnemyKillResourceReward);
                return;
            }

            if (deadUnit.TeamType == TeamType.Player)
            {
                HandleBattleCompleted(false);
            }
        }

        private void OnTreasureCollected(int resourceAmount)
        {
            if (!isBattleActive || resourceAmount <= 0)
            {
                return;
            }

            collectedTreasureReward += resourceAmount;
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
            lastBattleWasVictory = isVictory;

            var victoryBonus = isVictory ? progressionController.GetVictoryBonusResource() : 0;
            pendingReward = collectedTreasureReward + victoryBonus;

            playerTurnController.CancelTurn();
            enemyTurnController.CancelEnemyTurn();

            playFieldController.ClearEnemyDangerHighlights();
            playFieldController.ClearField();

            finishPopup.Show(isVictory, collectedTreasureReward, victoryBonus);
        }

        private void OnFinishContinueRequested()
        {
            if (pendingReward > 0)
            {
                resourceController.Add(pendingReward);
            }

            progressionController.RegisterBattleResult(lastBattleWasVictory);

            pendingReward = 0;
            collectedTreasureReward = 0;
            lastBattleWasVictory = false;

            finishPopup.Hide();
            progressionPopup.Show(progressionController.CurrentLevel);
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


