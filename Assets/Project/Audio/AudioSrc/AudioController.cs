using System.Collections.Generic;
using TetrisTactic.Core;
using UnityEngine;

namespace TetrisTactic.Audio
{
    public sealed class AudioController : IInitializableController, IDisposableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly HashSet<AudioCue> loggedMissingCues = new();

        private AudioConfig audioConfig;
        private AudioSource audioSource;

        public AudioController(ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void Initialize()
        {
            EnsureConfig();
            EnsureAudioSource();
        }

        public void Dispose()
        {
            if (audioSource != null)
            {
                Object.Destroy(audioSource.gameObject);
                audioSource = null;
            }
        }

        public void PlayCue(AudioCue cue, Vector3 worldPosition)
        {
            EnsureConfig();
            EnsureAudioSource();

            if (!audioConfig.TryGetCue(cue, out var entry) || entry == null || entry.Clip == null)
            {
                if (!audioConfig.LogMissingCueOnce || loggedMissingCues.Add(cue))
                {
                    Debug.Log($"Audio cue missing clip: {cue}");
                }

                return;
            }

            audioSource.transform.position = worldPosition;
            audioSource.pitch = entry.Pitch;
            audioSource.PlayOneShot(entry.Clip, entry.Volume * audioConfig.MasterVolume);
        }

        private void EnsureConfig()
        {
            if (audioConfig != null)
            {
                return;
            }

            try
            {
                audioConfig = serviceLocator.ConfigurationProvider.GetConfig<AudioConfig>();
            }
            catch
            {
                audioConfig = AudioConfig.CreateDefault();
            }
        }

        private void EnsureAudioSource()
        {
            if (audioSource != null)
            {
                return;
            }

            var audioObject = new GameObject("AudioControllerSource", typeof(AudioSource));
            Object.DontDestroyOnLoad(audioObject);
            audioSource = audioObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }
}
