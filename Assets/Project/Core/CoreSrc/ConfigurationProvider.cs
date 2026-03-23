using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetrisTactic.Core
{
    [CreateAssetMenu(menuName = "Project/Core/Configuration Provider", fileName = "ConfigurationProvider")]
    public sealed class ConfigurationProvider : ScriptableObject
    {
        [SerializeField] private List<ScriptableObject> configurations = new();

        public T GetConfig<T>() where T : ScriptableObject
        {
            for (var i = 0; i < configurations.Count; i++)
            {
                if (configurations[i] is T typedConfig)
                {
                    return typedConfig;
                }
            }

            throw new InvalidOperationException($"Configuration of type {typeof(T).Name} is not registered.");
        }
    }
}