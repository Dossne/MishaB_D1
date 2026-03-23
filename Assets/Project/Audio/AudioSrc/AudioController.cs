using System.Collections.Generic;
using TetrisTactic.Core;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Audio
{
    public sealed class AudioController : IInitializableController, IDisposableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly HashSet<AudioCue> loggedMissingCues = new();
        private readonly System.Random random = new();
        private AudioCue? lastCoinCue;

        private AudioConfig audioConfig;
        private AudioSource oneShotSource;
        private AudioSource ambientSource;

        public AudioController(ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void Initialize()
        {
            EnsureConfig();
            EnsureAudioSources();
            StartAmbientLoop();
        }

        public void Dispose()
        {
            if (oneShotSource != null)
            {
                Object.Destroy(oneShotSource.gameObject);
                oneShotSource = null;
                ambientSource = null;
            }
        }

        public void PlayCue(AudioCue cue, Vector3 worldPosition)
        {
            EnsureConfig();
            EnsureAudioSources();

            if (!TryResolveClip(cue, out var entry) || entry == null || entry.Clip == null)
            {
                LogMissingCue(cue);
                return;
            }

            oneShotSource.transform.position = worldPosition;
            oneShotSource.pitch = entry.Pitch;
            oneShotSource.PlayOneShot(entry.Clip, entry.Volume * audioConfig.MasterVolume);
        }

        public void PlayCastCue(UnitType casterType, Vector3 worldPosition)
        {
            switch (casterType)
            {
                case UnitType.Player:
                    PlayRandomCue(new[] { AudioCue.PlayerCast1, AudioCue.PlayerCast2 }, worldPosition);
                    break;
                case UnitType.Warrior:
                    PlayRandomCue(new[] { AudioCue.WarriorCast1, AudioCue.WarriorCast2 }, worldPosition);
                    break;
                case UnitType.Archer:
                    PlayRandomCue(new[] { AudioCue.ArcherCast1, AudioCue.ArcherCast2 }, worldPosition);
                    break;
                case UnitType.Mage:
                    PlayRandomCue(new[] { AudioCue.MageCast1, AudioCue.MageCast2 }, worldPosition);
                    break;
                default:
                    PlayCue(AudioCue.AttackCast, worldPosition);
                    break;
            }
        }

        public void PlayCoinCue(Vector3 worldPosition)
        {
            var coinCues = new[] { AudioCue.Coin1, AudioCue.Coin2, AudioCue.Coin3 };
            var selectedCue = SelectRandomCue(coinCues, lastCoinCue);
            lastCoinCue = selectedCue;
            PlayCue(selectedCue, worldPosition);
        }

        public void PlayWalkCue(Vector3 worldPosition)
        {
            PlayRandomCue(new[] { AudioCue.Walk1, AudioCue.Walk2, AudioCue.Walk3, AudioCue.Walk4, AudioCue.Walk5 }, worldPosition);
        }

        public void PlayFinishCue(bool isVictory)
        {
            PlayCue(isVictory ? AudioCue.Win : AudioCue.Lose, Vector3.zero);
        }

        private void PlayRandomCue(IReadOnlyList<AudioCue> cues, Vector3 worldPosition)
        {
            if (cues == null || cues.Count == 0)
            {
                return;
            }

            var startIndex = random.Next(cues.Count);
            for (var i = 0; i < cues.Count; i++)
            {
                var cue = cues[(startIndex + i) % cues.Count];
                if (TryResolveClip(cue, out var entry) && entry != null && entry.Clip != null)
                {
                    oneShotSource.transform.position = worldPosition;
                    oneShotSource.pitch = entry.Pitch;
                    oneShotSource.PlayOneShot(entry.Clip, entry.Volume * audioConfig.MasterVolume);
                    return;
                }
            }

            LogMissingCue(cues[startIndex]);
        }

        private AudioCue SelectRandomCue(IReadOnlyList<AudioCue> cues, AudioCue? excludedCue)
        {
            if (cues == null || cues.Count == 0)
            {
                return AudioCue.Coin1;
            }

            if (cues.Count == 1 || excludedCue == null)
            {
                return cues[random.Next(cues.Count)];
            }

            var available = new List<AudioCue>(cues.Count);
            for (var i = 0; i < cues.Count; i++)
            {
                if (cues[i] != excludedCue.Value)
                {
                    available.Add(cues[i]);
                }
            }

            if (available.Count == 0)
            {
                return cues[random.Next(cues.Count)];
            }

            return available[random.Next(available.Count)];
        }

        private bool TryResolveClip(AudioCue cue, out AudioConfig.AudioCueEntry entry)
        {
            EnsureConfig();
            return audioConfig.TryGetCue(cue, out entry);
        }

        private void StartAmbientLoop()
        {
            if (!TryResolveClip(AudioCue.AmbientAction, out var entry) || entry == null || entry.Clip == null)
            {
                LogMissingCue(AudioCue.AmbientAction);
                return;
            }

            ambientSource.clip = entry.Clip;
            ambientSource.volume = entry.Volume * audioConfig.MasterVolume;
            ambientSource.pitch = entry.Pitch;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        private void LogMissingCue(AudioCue cue)
        {
            if (!audioConfig.LogMissingCueOnce || loggedMissingCues.Add(cue))
            {
                Debug.Log($"Audio cue missing clip: {cue}");
            }
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

        private void EnsureAudioSources()
        {
            if (oneShotSource != null && ambientSource != null)
            {
                return;
            }

            var audioObject = new GameObject("AudioControllerSource", typeof(AudioSource));
            Object.DontDestroyOnLoad(audioObject);

            oneShotSource = audioObject.GetComponent<AudioSource>();
            oneShotSource.playOnAwake = false;
            oneShotSource.loop = false;
            oneShotSource.spatialBlend = 0f;

            ambientSource = audioObject.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
        }
    }
}



