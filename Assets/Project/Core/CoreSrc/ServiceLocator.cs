using TetrisTactic.MainUi;
using TetrisTactic.PlayField;
using UnityEngine;

namespace TetrisTactic.Core
{
    public sealed class ServiceLocator : MonoBehaviour
    {
        [SerializeField] private ConfigurationProvider configurationProvider;
        [SerializeField] private MainUiProvider mainUiProvider;
        [SerializeField] private PlayFieldView playFieldView;

        private GameManager gameManager;
        private bool configurationRegistered;

        public GameManager GameManager => gameManager;
        public ConfigurationProvider ConfigurationProvider => configurationProvider;
        public MainUiProvider MainUiProvider => mainUiProvider;
        public PlayFieldView PlayFieldView => playFieldView;

        public void RegisterGameManager(GameManager manager)
        {
            gameManager = manager;
        }

        public void RegisterConfigurationProvider()
        {
            if (configurationRegistered)
            {
                return;
            }

            if (configurationProvider == null)
            {
                Debug.LogError("ServiceLocator requires a ConfigurationProvider reference.");
                return;
            }

            configurationRegistered = true;
        }

        public void RegisterMainUiProvider(MainUiProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            mainUiProvider = provider;
        }

        public void RegisterPlayFieldView(PlayFieldView view)
        {
            if (view == null)
            {
                return;
            }

            playFieldView = view;
        }
    }
}
