using UnityEngine;

namespace TetrisTactic.Core
{
    public sealed class ServiceLocator : MonoBehaviour
    {
        [SerializeField] private ConfigurationProvider configurationProvider;

        private GameManager gameManager;
        private bool configurationRegistered;

        public GameManager GameManager => gameManager;
        public ConfigurationProvider ConfigurationProvider => configurationProvider;

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
    }
}