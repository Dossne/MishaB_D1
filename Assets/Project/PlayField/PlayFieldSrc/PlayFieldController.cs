using System;
using System.Collections.Generic;
using TetrisTactic.Core;
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

        private readonly ServiceLocator serviceLocator;
        private readonly System.Random random = new();

        private PlayFieldView playFieldView;
        private PlayFieldModel playFieldModel;
        private PlayFieldConfig playFieldConfig;
        private UnitConfig unitConfig;
        private TreasureConfig treasureConfig;
        private UnitFactory unitFactory;

        public PlayFieldController(ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public bool HasActiveField => playFieldModel != null;

        public void CreateField()
        {
            EnsureConfigs();
            EnsureView();

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

            return playFieldModel.IsInside(destination) && playFieldModel.IsEmpty(destination);
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
            _ = model.TrySetPlayer(unitFactory.CreatePlayer(playerPosition));

            var maxSpawnSlots = Math.Max(0, allPositions.Count - 1);
            var enemyCount = unitConfig.GetEnemyCount(random, maxSpawnSlots);
            var remainingAfterEnemies = Math.Max(0, maxSpawnSlots - enemyCount);
            var treasureCount = treasureConfig.GetTreasureCount(random, remainingAfterEnemies);

            var nextIndex = 1;

            for (var i = 0; i < enemyCount && nextIndex < allPositions.Count; i++)
            {
                var spawnPosition = allPositions[nextIndex++];
                var enemyType = unitFactory.GetRandomEnemyType(random);
                _ = model.TryAddEnemy(unitFactory.CreateEnemy(enemyType, spawnPosition));
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
