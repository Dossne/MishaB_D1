using TetrisTactic.Audio;
using TetrisTactic.CameraFx;
using TetrisTactic.Core;
using TetrisTactic.PlayField;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Feedback
{
    public sealed class HitFeedbackPlayer : IInitializableController, IDisposableController
    {
        private readonly PlayFieldController playFieldController;
        private readonly FloatingTextController floatingTextController;
        private readonly AudioController audioController;
        private readonly CameraShakeController cameraShakeController;
        private readonly TimeImpactController timeImpactController;

        public HitFeedbackPlayer(
            PlayFieldController playFieldController,
            FloatingTextController floatingTextController,
            AudioController audioController,
            CameraShakeController cameraShakeController,
            TimeImpactController timeImpactController)
        {
            this.playFieldController = playFieldController;
            this.floatingTextController = floatingTextController;
            this.audioController = audioController;
            this.cameraShakeController = cameraShakeController;
            this.timeImpactController = timeImpactController;
        }

        public void Initialize()
        {
            playFieldController.DamageApplied -= OnDamageApplied;
            playFieldController.DamageApplied += OnDamageApplied;
        }

        public void Dispose()
        {
            playFieldController.DamageApplied -= OnDamageApplied;
        }

        public void PlayAttackFeedback(UnitRuntimeModel attacker)
        {
            if (attacker == null)
            {
                return;
            }

            var worldPosition = ResolveUnitWorldPosition(attacker);
            floatingTextController.ShowCirclePulse(worldPosition, new Color(1f, 0.85f, 0.25f, 0.86f), 0.18f, 0.64f);
            SpawnEmojiBubble(worldPosition + (Vector3.up * 0.35f), "?", new Color(1f, 0.96f, 0.75f, 1f), 0.35f);
            audioController.PlayCue(AudioCue.AttackCast, worldPosition);
        }

        public void PlayWaveCellFeedback(Vector3 worldPosition, bool wasHit)
        {
            if (wasHit)
            {
                floatingTextController.ShowCirclePulse(worldPosition, new Color(1f, 0.36f, 0.24f, 0.95f), 0.12f, 0.5f);
                return;
            }
        }

        private void OnDamageApplied(DamageEventData damageEvent)
        {
            if (damageEvent == null)
            {
                return;
            }

            var targetUnitWorld = ResolveUnitWorldPosition(damageEvent.TargetUnit);
            if (!playFieldController.TryGetCellWorldPosition(damageEvent.TargetPosition, out var worldPosition))
            {
                worldPosition = new Vector3(damageEvent.TargetPosition.X, damageEvent.TargetPosition.Y, 0f);
            }

            var damageColor = damageEvent.TargetUnit != null && damageEvent.TargetUnit.TeamType == TeamType.Player
                ? new Color(1f, 0.85f, 0.32f, 1f)
                : new Color(1f, 0.42f, 0.42f, 1f);

            var textAnchor = targetUnitWorld != Vector3.zero ? targetUnitWorld : worldPosition;
            floatingTextController.ShowWorldText(textAnchor + (Vector3.up * 0.46f), $"-{damageEvent.DamageAmount}", damageColor);
            floatingTextController.ShowCirclePulse(worldPosition, new Color(1f, 0.26f, 0.2f, 0.9f), 0.14f, 0.58f);

            if (damageEvent.WasFatal)
            {
                SpawnEmojiBubble(worldPosition + (Vector3.up * 0.42f), "??", Color.white, 0.5f);
                floatingTextController.ShowWorldText(worldPosition + (Vector3.up * 0.55f), "KO", new Color(1f, 0.94f, 0.7f, 1f), 0.6f, 0.35f, 44);
                audioController.PlayCue(AudioCue.UnitDefeated, worldPosition);
            }

            if (damageEvent.TargetUnit != null && damageEvent.TargetUnit.TeamType == TeamType.Player)
            {
                cameraShakeController.PlayPlayerDamageShake();
                audioController.PlayCue(AudioCue.PlayerDamaged, worldPosition);
                return;
            }

            if (damageEvent.TargetUnit != null && damageEvent.TargetUnit.TeamType == TeamType.Enemy)
            {
                audioController.PlayCue(AudioCue.EnemyDamaged, worldPosition);
                if (damageEvent.SourceUnit != null && damageEvent.SourceUnit.TeamType == TeamType.Player)
                {
                    timeImpactController.PlayEnemyDamageImpact();
                }
            }
        }

        private Vector3 ResolveUnitWorldPosition(UnitRuntimeModel unit)
        {
            if (unit != null && playFieldController.TryGetCellWorldPosition(unit.Position, out var worldPosition))
            {
                return worldPosition;
            }

            if (unit == null)
            {
                return Vector3.zero;
            }

            return new Vector3(unit.Position.X, unit.Position.Y, 0f);
        }

        private static void SpawnEmojiBubble(Vector3 worldPosition, string emoji, Color color, float lifetime)
        {
            var bubbleObject = new GameObject("EmojiBubble", typeof(EmojiBubbleView));
            bubbleObject.transform.position = worldPosition;
            var bubbleView = bubbleObject.GetComponent<EmojiBubbleView>();
            bubbleView.Show(emoji, color, lifetime);
        }
    }
}
