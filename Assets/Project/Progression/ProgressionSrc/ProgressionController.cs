using TetrisTactic.Core;
using TetrisTactic.Resource;
using UnityEngine;

namespace TetrisTactic.Progression
{
    public sealed class ProgressionController : IInitializableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly ResourceController resourceController;

        private LevelProgressionConfig progressionConfig;
        private PlayerUpgradeState playerUpgradeState = PlayerUpgradeState.CreateDefault();

        public ProgressionController(ServiceLocator serviceLocator, ResourceController resourceController)
        {
            this.serviceLocator = serviceLocator;
            this.resourceController = resourceController;
        }

        public int CurrentLevel { get; private set; } = 1;
        public PlayerUpgradeState PlayerUpgradeState => playerUpgradeState;

        public void Initialize()
        {
            if (progressionConfig != null)
            {
                return;
            }

            try
            {
                progressionConfig = serviceLocator.ConfigurationProvider.GetConfig<LevelProgressionConfig>();
            }
            catch
            {
                progressionConfig = LevelProgressionConfig.CreateDefault();
            }
        }

        public LevelDefinition BuildCurrentLevelDefinition()
        {
            EnsureInitialized();
            return progressionConfig.BuildLevelDefinition(CurrentLevel);
        }

        public int GetVictoryBonusResource()
        {
            EnsureInitialized();
            return progressionConfig.VictoryBonusResource;
        }

        public bool TryPurchaseUpgrade(PlayerUpgradeType upgradeType)
        {
            EnsureInitialized();

            if (!resourceController.TrySpend(progressionConfig.UpgradeCost))
            {
                return false;
            }

            playerUpgradeState = playerUpgradeState.Apply(upgradeType);
            return true;
        }

        public void RegisterBattleResult(bool isVictory)
        {
            if (isVictory)
            {
                CurrentLevel++;
            }
        }

        private void EnsureInitialized()
        {
            if (progressionConfig == null)
            {
                Initialize();
            }

            if (playerUpgradeState == null)
            {
                playerUpgradeState = PlayerUpgradeState.CreateDefault();
            }
        }
    }
}
