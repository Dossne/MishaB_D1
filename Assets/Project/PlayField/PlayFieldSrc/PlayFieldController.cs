using System;
using System.Collections.Generic;
using TetrisTactic.Core;
using TetrisTactic.Progression;
using TetrisTactic.Treasure;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.PlayField
{
    public sealed class PlayFieldController : IDisposableController
    {
        private static readonly GridPosition[] NeighborOffsets =
        {
            new GridPosition(0, 1),
            new GridPosition(0, -1),
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
        };

        public event Action<GridPosition> CellTapped;
        public event Action<UnitRuntimeModel> UnitDied;
        public event Action<DamageEventData> DamageApplied;
        public event Action<int> TreasureCollected;

        private readonly ServiceLocator serviceLocator;
        private readonly System.Random random = new();

        private PlayFieldView playFieldView;
        private PlayFieldModel playFieldModel;
        private PlayFieldConfig playFieldConfig;
        private UnitConfig unitConfig;
        private TreasureConfig treasureConfig;
        private UnitFactory unitFactory;
        private LevelDefinition activeLevelDefinition;
        private PlayerUpgradeState activePlayerUpgradeState;

        public PlayFieldController(ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public bool HasActiveField => playFieldModel != null;
        public int Columns => playFieldModel?.Columns ?? 0;
        public int Rows => playFieldModel?.Rows ?? 0;
        public bool IsPlayerAlive => playFieldModel != null && playFieldModel.PlayerUnit != null && playFieldModel.PlayerUnit.Health.IsAlive;
        public bool HasLivingEnemies => playFieldModel != null && playFieldModel.EnemyUnits.Count > 0;

        public void CreateField()
        {
            CreateField(null, null);
        }

        public void CreateField(LevelDefinition levelDefinition, PlayerUpgradeState playerUpgradeState)
        {
            EnsureConfigs();
            EnsureView();

            activeLevelDefinition = levelDefinition;
            activePlayerUpgradeState = playerUpgradeState;

            playFieldModel = new PlayFieldModel(playFieldConfig.Columns, playFieldConfig.Rows);
            GenerateLevelContent(playFieldModel);
            UpdateView();
        }

        public void ClearField()
        {
            playFieldModel = null;

            if (playFieldView != null)
            {
                playFieldView.ClearMoveHighlights();
                playFieldView.ClearAbilityHighlights();
                playFieldView.ClearEnemyDangerHighlights();
                playFieldView.Clear();
            }
        }

        public void UpdateView()
        {
            if (playFieldModel == null || playFieldView == null)
            {
                return;
            }

            playFieldView.Render(playFieldModel);
        }

        public UnitRuntimeModel GetPlayerUnit()
        {
            return playFieldModel?.PlayerUnit;
        }

        public IReadOnlyList<UnitRuntimeModel> GetEnemyUnits()
        {
            if (playFieldModel == null)
            {
                return Array.Empty<UnitRuntimeModel>();
            }

            var enemies = playFieldModel.EnemyUnits;
            var result = new List<UnitRuntimeModel>(enemies.Count);
            for (var i = 0; i < enemies.Count; i++)
            {
                result.Add(enemies[i]);
            }

            return result;
        }

        public IReadOnlyList<GridPosition> GetLegalPlayerMoveCells()
        {
            var result = new List<GridPosition>(4);
            if (playFieldModel == null || playFieldModel.PlayerUnit == null)
            {
                return result;
            }

            var playerPosition = playFieldModel.PlayerUnit.Position;

            for (var i = 0; i < NeighborOffsets.Length; i++)
            {
                var offset = NeighborOffsets[i];
                var target = new GridPosition(playerPosition.X + offset.X, playerPosition.Y + offset.Y);
                if (CanMovePlayerTo(target))
                {
                    result.Add(target);
                }
            }

            return result;
        }

        public bool TryMovePlayerTo(GridPosition destination)
        {
            if (playFieldModel == null || playFieldModel.PlayerUnit == null)
            {
                return false;
            }

            if (!CanMovePlayerTo(destination))
            {
                return false;
            }

            var moved = playFieldModel.TryMoveUnit(playFieldModel.PlayerUnit, destination);
            if (!moved)
            {
                return false;
            }

            if (playFieldModel.TryTakeTreasureAt(destination, out var treasureData) && treasureData != null)
            {
                var amount = Mathf.Max(0, treasureData.ResourceAmount);
                if (amount > 0)
                {
                    TreasureCollected?.Invoke(amount);
                }
            }

            UpdateView();
            return true;
        }

        public IReadOnlyList<GridPosition> GetLegalUnitMoveCells(UnitRuntimeModel unit)
        {
            var result = new List<GridPosition>(4);
            if (playFieldModel == null || unit == null || !unit.Health.IsAlive)
            {
                return result;
            }

            var unitPosition = unit.Position;
            for (var i = 0; i < NeighborOffsets.Length; i++)
            {
                var offset = NeighborOffsets[i];
                var destination = new GridPosition(unitPosition.X + offset.X, unitPosition.Y + offset.Y);
                if (CanMoveUnitTo(unit, destination))
                {
                    result.Add(destination);
                }
            }

            return result;
        }

        public bool TryMoveUnit(UnitRuntimeModel unit, GridPosition destination)
        {
            if (playFieldModel == null || unit == null || !unit.Health.IsAlive)
            {
                return false;
            }

            if (!CanMoveUnitTo(unit, destination))
            {
                return false;
            }

            var moved = playFieldModel.TryMoveUnit(unit, destination);
            if (moved)
            {
                UpdateView();
            }

            return moved;
        }

        public bool CanMovePlayerTo(GridPosition destination)
        {
            if (playFieldModel == null || playFieldModel.PlayerUnit == null)
            {
                return false;
            }

            var playerPosition = playFieldModel.PlayerUnit.Position;
            var distance = Mathf.Abs(playerPosition.X - destination.X) + Mathf.Abs(playerPosition.Y - destination.Y);
            if (distance != 1)
            {
                return false;
            }

            if (!playFieldModel.IsInside(destination))
            {
                return false;
            }

            return !playFieldModel.IsObstacle(destination) && !playFieldModel.HasUnitAt(destination);
        }

        public bool CanMoveUnitTo(UnitRuntimeModel unit, GridPosition destination)
        {
            if (playFieldModel == null || unit == null || !unit.Health.IsAlive)
            {
                return false;
            }

            var unitPosition = unit.Position;
            var distance = Mathf.Abs(unitPosition.X - destination.X) + Mathf.Abs(unitPosition.Y - destination.Y);
            if (distance != 1)
            {
                return false;
            }

            return playFieldModel.IsInside(destination) && playFieldModel.IsEmpty(destination);
        }

        public bool IsCellPassableForMovement(GridPosition position, UnitRuntimeModel movingUnit)
        {
            if (playFieldModel == null)
            {
                return false;
            }

            if (!playFieldModel.IsInside(position))
            {
                return false;
            }

            if (movingUnit != null && movingUnit.Position == position)
            {
                return true;
            }

            return playFieldModel.IsEmpty(position);
        }

        public bool IsInside(GridPosition position)
        {
            return playFieldModel != null && playFieldModel.IsInside(position);
        }

        public bool TryGetUnitAt(GridPosition position, out UnitRuntimeModel unit)
        {
            unit = null;
            return playFieldModel != null && playFieldModel.TryGetUnitAt(position, out unit);
        }

        public bool TryApplyDamageAt(GridPosition position, int damage, UnitRuntimeModel sourceUnit)
        {
            if (playFieldModel == null || damage <= 0)
            {
                return false;
            }

            if (!playFieldModel.TryGetUnitAt(position, out var targetUnit) || targetUnit == null)
            {
                return false;
            }

            if (sourceUnit != null && targetUnit == sourceUnit)
            {
                return false;
            }

            var damaged = targetUnit.Health.TryApplyDamage(damage);
            if (!damaged)
            {
                return false;
            }

            var wasFatal = !targetUnit.Health.IsAlive;
            DamageApplied?.Invoke(new DamageEventData(sourceUnit, targetUnit, position, damage, wasFatal));

            if (wasFatal)
            {
                _ = playFieldModel.RemoveUnit(targetUnit);
                UnitDied?.Invoke(targetUnit);
            }

            UpdateView();
            return true;
        }

        public bool TryGetCellWorldPosition(GridPosition position, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            return playFieldView != null && playFieldView.TryGetCellWorldPosition(position, out worldPosition);
        }

        public void SetMoveHighlights(IReadOnlyList<GridPosition> positions)
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.SetMoveHighlights(positions);
        }

        public void ClearMoveHighlights()
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.ClearMoveHighlights();
        }

        public void SetAbilityHighlights(IReadOnlyList<GridPosition> positions)
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.SetAbilityHighlights(positions);
        }

        public void SetAbilityHighlights(IReadOnlyList<ZoneHighlightData> zones)
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.SetAbilityHighlights(zones);
        }

        public void ClearAbilityHighlights()
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.ClearAbilityHighlights();
        }

        public void SetEnemyDangerHighlights(IReadOnlyList<GridPosition> positions)
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.SetEnemyDangerHighlights(positions);
        }

        public void SetEnemyDangerHighlights(IReadOnlyList<ZoneHighlightData> zones)
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.SetEnemyDangerHighlights(zones);
        }

        public void ClearEnemyDangerHighlights()
        {
            if (playFieldView == null)
            {
                return;
            }

            playFieldView.ClearEnemyDangerHighlights();
        }

        public void Dispose()
        {
            if (playFieldView != null)
            {
                playFieldView.CellTapped -= OnViewCellTapped;
            }
        }

        private void EnsureConfigs()
        {
            if (playFieldConfig == null)
            {
                try
                {
                    playFieldConfig = serviceLocator.ConfigurationProvider.GetConfig<PlayFieldConfig>();
                }
                catch (Exception)
                {
                    playFieldConfig = PlayFieldConfig.CreateDefault();
                }
            }

            if (unitConfig == null)
            {
                try
                {
                    unitConfig = serviceLocator.ConfigurationProvider.GetConfig<UnitConfig>();
                }
                catch (Exception)
                {
                    unitConfig = UnitConfig.CreateDefault();
                }
            }

            if (treasureConfig == null)
            {
                try
                {
                    treasureConfig = serviceLocator.ConfigurationProvider.GetConfig<TreasureConfig>();
                }
                catch (Exception)
                {
                    treasureConfig = TreasureConfig.CreateDefault();
                }
            }

            unitFactory ??= new UnitFactory(unitConfig);
        }

        private void EnsureView()
        {
            if (playFieldView == null)
            {
                playFieldView = serviceLocator.PlayFieldView;
            }

            if (playFieldView == null)
            {
                var viewObject = new GameObject("PlayFieldView", typeof(PlayFieldView));
                playFieldView = viewObject.GetComponent<PlayFieldView>();
                serviceLocator.RegisterPlayFieldView(playFieldView);
            }

            playFieldView.Initialize(playFieldConfig);
            playFieldView.CellTapped -= OnViewCellTapped;
            playFieldView.CellTapped += OnViewCellTapped;
        }

        private void GenerateLevelContent(PlayFieldModel model)
        {
            model.ClearAll();

            var allPositions = BuildAllPositions(model.Columns, model.Rows);
            if (allPositions.Count == 0)
            {
                return;
            }

            Shuffle(allPositions);

            var playerPosition = allPositions[0];
            var playerUpgradeState = activePlayerUpgradeState ?? PlayerUpgradeState.CreateDefault();
            _ = model.TrySetPlayer(unitFactory.CreatePlayer(playerPosition, playerUpgradeState.DamageBonus, playerUpgradeState.HpBonus));

            var maxSpawnSlots = Math.Max(0, allPositions.Count - 1);
            var enemyCount = GetEnemyCount(maxSpawnSlots);
            var remainingAfterEnemies = Math.Max(0, maxSpawnSlots - enemyCount);
            var treasureCount = treasureConfig.GetTreasureCount(random, remainingAfterEnemies);

            var nextIndex = 1;

            for (var i = 0; i < enemyCount && nextIndex < allPositions.Count; i++)
            {
                var spawnPosition = allPositions[nextIndex++];
                var enemyType = GetEnemyTypeForSpawn();
                var damageBonus = activeLevelDefinition?.EnemyDamageBonus ?? 0;
                var hpBonus = activeLevelDefinition?.EnemyHpBonus ?? 0;
                _ = model.TryAddEnemy(unitFactory.CreateEnemy(enemyType, spawnPosition, damageBonus, hpBonus));
            }

            for (var i = 0; i < treasureCount && nextIndex < allPositions.Count; i++)
            {
                var spawnPosition = allPositions[nextIndex++];
                _ = model.TryAddTreasure(new TreasureData(spawnPosition, treasureConfig.TreasureValue));
            }

            var obstacleBudget = Math.Max(0, allPositions.Count - nextIndex);
            var obstacleCount = unitConfig.GetObstacleCount(random, obstacleBudget);
            var candidates = BuildObstacleCandidates(model, playerPosition);

            var placedObstacles = 0;
            for (var i = 0; i < candidates.Count && placedObstacles < obstacleCount; i++)
            {
                var candidate = candidates[i];
                if (!model.TryAddObstacle(candidate))
                {
                    continue;
                }

                if (!AreAllTargetsReachable(model, playerPosition))
                {
                    _ = model.RemoveObstacle(candidate);
                    continue;
                }

                placedObstacles++;
            }
        }

        private int GetEnemyCount(int maxSpawnSlots)
        {
            if (activeLevelDefinition == null)
            {
                return unitConfig.GetEnemyCount(random, maxSpawnSlots);
            }

            return Mathf.Clamp(activeLevelDefinition.EnemyCount, 0, maxSpawnSlots);
        }

        private UnitType GetEnemyTypeForSpawn()
        {
            if (activeLevelDefinition == null)
            {
                return unitFactory.GetRandomEnemyType(random);
            }

            return activeLevelDefinition.GetRandomEnemyType(random);
        }

        private List<GridPosition> BuildObstacleCandidates(PlayFieldModel model, GridPosition playerPosition)
        {
            var candidates = new List<GridPosition>();

            for (var x = 0; x < model.Columns; x++)
            {
                for (var y = 0; y < model.Rows; y++)
                {
                    var position = new GridPosition(x, y);
                    if (position == playerPosition)
                    {
                        continue;
                    }

                    if (model.IsEmpty(position))
                    {
                        candidates.Add(position);
                    }
                }
            }

            Shuffle(candidates);
            return candidates;
        }

        private bool AreAllTargetsReachable(PlayFieldModel model, GridPosition start)
        {
            var enemies = model.EnemyUnits;
            for (var i = 0; i < enemies.Count; i++)
            {
                if (!IsReachable(model, start, enemies[i].Position))
                {
                    return false;
                }
            }

            var treasures = model.Treasures;
            for (var i = 0; i < treasures.Count; i++)
            {
                if (!IsReachable(model, start, treasures[i].Position))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsReachable(PlayFieldModel model, GridPosition start, GridPosition target)
        {
            if (start == target)
            {
                return true;
            }

            var visited = new HashSet<GridPosition> { start };
            var queue = new Queue<GridPosition>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = 0; i < NeighborOffsets.Length; i++)
                {
                    var offset = NeighborOffsets[i];
                    var next = new GridPosition(current.X + offset.X, current.Y + offset.Y);
                    if (!model.IsInside(next) || visited.Contains(next) || model.IsObstacle(next))
                    {
                        continue;
                    }

                    if (next == target)
                    {
                        return true;
                    }

                    if (model.HasUnitAt(next))
                    {
                        continue;
                    }

                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }

            return false;
        }

        private static List<GridPosition> BuildAllPositions(int columns, int rows)
        {
            var positions = new List<GridPosition>(columns * rows);
            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    positions.Add(new GridPosition(x, y));
                }
            }

            return positions;
        }

        private void Shuffle(List<GridPosition> positions)
        {
            for (var i = positions.Count - 1; i > 0; i--)
            {
                var swapIndex = random.Next(0, i + 1);
                (positions[i], positions[swapIndex]) = (positions[swapIndex], positions[i]);
            }
        }

        private void OnViewCellTapped(GridPosition position)
        {
            CellTapped?.Invoke(position);
        }
    }
}



