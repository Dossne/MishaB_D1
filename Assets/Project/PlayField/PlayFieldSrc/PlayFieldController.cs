using System;
using TetrisTactic.Core;
using UnityEngine;

namespace TetrisTactic.PlayField
{
    public sealed class PlayFieldController : IDisposableController
    {
        public event Action<GridPosition> CellTapped;

        private readonly ServiceLocator serviceLocator;

        private PlayFieldView playFieldView;
        private PlayFieldModel playFieldModel;
        private PlayFieldConfig playFieldConfig;

        public PlayFieldController(ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void CreateField()
        {
            EnsureConfig();
            EnsureView();

            playFieldModel = new PlayFieldModel(playFieldConfig.Columns, playFieldConfig.Rows);
            GenerateInitialContentPlaceholders(playFieldModel);
            UpdateView();
        }

        public void ClearField()
        {
            playFieldModel = null;

            if (playFieldView != null)
            {
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

        public void Dispose()
        {
            if (playFieldView != null)
            {
                playFieldView.CellTapped -= OnViewCellTapped;
            }
        }

        private void EnsureConfig()
        {
            if (playFieldConfig != null)
            {
                return;
            }

            try
            {
                playFieldConfig = serviceLocator.ConfigurationProvider.GetConfig<PlayFieldConfig>();
            }
            catch (Exception)
            {
                playFieldConfig = PlayFieldConfig.CreateDefault();
            }
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

        private void GenerateInitialContentPlaceholders(PlayFieldModel model)
        {
            model.ClearAll();

            var centerX = model.Columns / 2;
            var centerY = model.Rows / 2;

            model.SetCell(new GridPosition(centerX, model.Rows - 2), CellContentType.Player);
            model.SetCell(new GridPosition(centerX - 1, 1), CellContentType.Enemy);
            model.SetCell(new GridPosition(centerX + 1, 1), CellContentType.Enemy);
            model.SetCell(new GridPosition(centerX, centerY), CellContentType.Treasure);

            model.SetCell(new GridPosition(1, centerY), CellContentType.Obstacle);
            model.SetCell(new GridPosition(model.Columns - 2, centerY), CellContentType.Obstacle);
        }

        private void OnViewCellTapped(GridPosition position)
        {
            CellTapped?.Invoke(position);
        }
    }
}
