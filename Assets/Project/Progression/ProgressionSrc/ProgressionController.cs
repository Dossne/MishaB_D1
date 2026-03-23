using TetrisTactic.Core;
using TetrisTactic.Resource;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Progression
{
    public sealed class ProgressionController : IInitializableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly ResourceController resourceController;

        private LevelProgressionConfig progressionConfig;
        private UnitConfig unitConfig;
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
            if (progressionConfig != null && unitConfig != null)
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

            try
            {
                unitConfig = serviceLocator.ConfigurationProvider.GetConfig<UnitConfig>();
            }
            catch
            {
                unitConfig = UnitConfig.CreateDefault();
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

        public int GetUpgradeCost()
        {
            EnsureInitialized();
            return progressionConfig.UpgradeCost;
        }

        public int GetCurrentPlayerDamageValue()
        {
            EnsureInitialized();
            var data = unitConfig.CreateUnitData(UnitType.Player, playerUpgradeState.DamageBonus, playerUpgradeState.HpBonus);
            return Mathf.Max(1, data.BaseDamage);
        }

        public int GetCurrentPlayerHpValue()
        {
            EnsureInitialized();
            var data = unitConfig.CreateUnitData(UnitType.Player, playerUpgradeState.DamageBonus, playerUpgradeState.HpBonus);
            return Mathf.Max(1, data.MaxHp);
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
            if (progressionConfig == null || unitConfig == null)
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
