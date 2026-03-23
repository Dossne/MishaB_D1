using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetrisTactic.Audio
{
    [CreateAssetMenu(menuName = "Project/Audio/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [Serializable]
        public sealed class AudioCueEntry
        {
            [SerializeField] private AudioCue cue;
            [SerializeField] private AudioClip clip;
            [SerializeField, Range(0f, 1f)] private float volume = 1f;
            [SerializeField, Range(0.5f, 1.5f)] private float pitch = 1f;

            public AudioCue Cue => cue;
            public AudioClip Clip => clip;
            public float Volume => Mathf.Clamp01(volume);
            public float Pitch => Mathf.Clamp(pitch, 0.5f, 1.5f);
        }

        [SerializeField] private List<AudioCueEntry> cues = new();
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] private bool logMissingCueOnce = true;

        public float MasterVolume => Mathf.Clamp01(masterVolume);
        public bool LogMissingCueOnce => logMissingCueOnce;

        public bool TryGetCue(AudioCue cue, out AudioCueEntry entry)
        {
            for (var i = 0; i < cues.Count; i++)
            {
                var current = cues[i];
                if (current != null && current.Cue == cue)
                {
                    entry = current;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        public static AudioConfig CreateDefault()
        {
            return CreateInstance<AudioConfig>();
        }
    }
}
