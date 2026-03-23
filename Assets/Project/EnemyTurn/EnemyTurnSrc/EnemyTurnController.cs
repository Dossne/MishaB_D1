using System.Collections;
using System.Collections.Generic;
using TetrisTactic.Abilities;
using TetrisTactic.Core;
using TetrisTactic.Feedback;
using TetrisTactic.PlayField;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.EnemyTurn
{
    public sealed class EnemyTurnController : IInitializableController, IDisposableController
    {
        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly AbilityController abilityController;
        private readonly HitFeedbackPlayer hitFeedbackPlayer;

        private readonly HashSet<UnitRuntimeModel> waitedLastTurn = new();
        private readonly HashSet<UnitRuntimeModel> waitedThisTurn = new();

        private EnemyAiController enemyAiController;
        private AbilityWavePlayer enemyWavePlayer;
        private MonoBehaviour coroutineRunner;
        private UnitConfig unitConfig;
        private bool isEnemySequenceActive;

        public EnemyTurnController(
            ServiceLocator serviceLocator,
            PlayFieldController playFieldController,
            AbilityController abilityController,
            HitFeedbackPlayer hitFeedbackPlayer)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.abilityController = abilityController;
            this.hitFeedbackPlayer = hitFeedbackPlayer;
        }

        public void Initialize()
        {
            coroutineRunner ??= CreateRunner();
            unitConfig ??= ResolveUnitConfig();

            var threatAnalyzer = new EnemyThreatAnalyzer(playFieldController);
            var waitLogic = new EnemyWaitLogic(playFieldController, abilityController, threatAnalyzer);
            enemyAiController = new EnemyAiController(playFieldController, threatAnalyzer, waitLogic);
            enemyWavePlayer = new AbilityWavePlayer(unitConfig.EnemyWaveCellDelay, unitConfig.EnemyImpactDuration);
        }

        public void Dispose()
        {
            CancelEnemyTurn();
            waitedLastTurn.Clear();
            waitedThisTurn.Clear();
            playFieldController.ClearEnemyDangerHighlights();
        }

        public void BeginEnemyTurn(System.Action onCompleted)
        {
            if (!playFieldController.HasActiveField)
            {
                onCompleted?.Invoke();
                return;
            }

            if (isEnemySequenceActive)
            {
                return;
            }

            playFieldController.ClearMoveHighlights();
            playFieldController.ClearAbilityHighlights();
            playFieldController.ClearEnemyDangerHighlights();

            isEnemySequenceActive = true;
            coroutineRunner.StartCoroutine(ExecuteEnemyTurnRoutine(onCompleted));
        }

        public void RefreshDangerHighlights()
        {
            if (enemyAiController == null || !playFieldController.HasActiveField)
            {
                playFieldController.ClearEnemyDangerHighlights();
                return;
            }

            var enemies = playFieldController.GetEnemyUnits();
            var dangerCells = enemyAiController.BuildDangerCells(enemies);
            playFieldController.SetEnemyDangerHighlights(dangerCells);
        }

        public void CancelEnemyTurn()
        {
            if (coroutineRunner != null)
            {
                coroutineRunner.StopAllCoroutines();
            }

            isEnemySequenceActive = false;
        }

        private IEnumerator ExecuteEnemyTurnRoutine(System.Action onCompleted)
        {
            waitedThisTurn.Clear();

            var enemies = playFieldController.GetEnemyUnits();
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                var preActionDelay = Random.Range(unitConfig.EnemyMinPreActionDelay, unitConfig.EnemyMaxPreActionDelay);
                yield return new WaitForSeconds(preActionDelay);

                var decision = enemyAiController.BuildDecision(enemy, CanEnemyWait);
                yield return ExecuteDecisionRoutine(decision);

                yield return new WaitForSeconds(unitConfig.EnemyPostActionDelay);
            }

            waitedLastTurn.Clear();
            foreach (var waited in waitedThisTurn)
            {
                waitedLastTurn.Add(waited);
            }

            isEnemySequenceActive = false;
            onCompleted?.Invoke();
        }

        private IEnumerator ExecuteDecisionRoutine(EnemyDecisionModel decision)
        {
            if (decision == null || decision.Enemy == null)
            {
                yield break;
            }

            switch (decision.ActionType)
            {
                case EnemyActionType.Move:
                {
                    var moved = playFieldController.TryMoveUnit(decision.Enemy, decision.MoveDestination);
                    if (moved)
                    {
                        waitedThisTurn.Remove(decision.Enemy);
                    }

                    break;
                }
                case EnemyActionType.Attack:
                {
                    waitedThisTurn.Remove(decision.Enemy);
                    hitFeedbackPlayer?.PlayAttackFeedback(decision.Enemy);

                    var attackFinished = false;
                    enemyWavePlayer.PlayWave(
                        decision.AttackWaveSteps,
                        decision.Enemy.UnitType,
                        ResolveWorldPosition,
                        cell =>
                        {
                            var wasHit = playFieldController.TryApplyDamageAt(cell, decision.Enemy.BaseDamage, decision.Enemy);
                            hitFeedbackPlayer?.PlayWaveCellFeedback(ResolveWorldPosition(cell), wasHit);
                        },
                        () => attackFinished = true);

                    while (!attackFinished)
                    {
                        yield return null;
                    }

                    break;
                }
                case EnemyActionType.Wait:
                {
                    if (decision.Enemy.Health.TryHeal(unitConfig.EnemyWaitHealAmount))
                    {
                        playFieldController.UpdateView();
                    }

                    waitedThisTurn.Add(decision.Enemy);
                    break;
                }
                default:
                {
                    waitedThisTurn.Remove(decision.Enemy);
                    break;
                }
            }
        }

        private bool CanEnemyWait(UnitRuntimeModel enemy)
        {
            return enemy != null && !waitedLastTurn.Contains(enemy);
        }

        private Vector3 ResolveWorldPosition(GridPosition position)
        {
            if (playFieldController.TryGetCellWorldPosition(position, out var worldPosition))
            {
                return worldPosition;
            }

            return new Vector3(position.X, position.Y, 0f);
        }

        private MonoBehaviour CreateRunner()
        {
            var runnerObject = new GameObject("EnemyTurnRunner", typeof(EnemyTurnRunnerBehaviour));
            Object.DontDestroyOnLoad(runnerObject);
            return runnerObject.GetComponent<EnemyTurnRunnerBehaviour>();
        }

        private UnitConfig ResolveUnitConfig()
        {
            try
            {
                return serviceLocator.ConfigurationProvider.GetConfig<UnitConfig>();
            }
            catch
            {
                return UnitConfig.CreateDefault();
            }
        }

        private sealed class EnemyTurnRunnerBehaviour : MonoBehaviour
        {
        }
    }
}
