using System;
using System.Collections.Generic;
using TetrisTactic.Core;
using TetrisTactic.Feedback;
using TetrisTactic.PlayField;
using TetrisTactic.PlayerTurn;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Abilities
{
    public sealed class AbilityController : IInitializableController, IDisposableController
    {
        private sealed class CastOption
        {
            public GridPosition BaseCell;
            public List<GridPosition> AffectedCells;
            public List<List<GridPosition>> WaveSteps;
        }

        public event Action SelectionChanged;

        private readonly ServiceLocator serviceLocator;
        private readonly PlayFieldController playFieldController;
        private readonly HitFeedbackPlayer hitFeedbackPlayer;

        private readonly List<AbilityRuntime> abilities = new();
        private readonly List<GridPosition> previewCells = new();
        private readonly List<CastOption> castOptions = new();
        private readonly System.Random random = new();

        private AbilityConfig abilityConfig;
        private AbilityWavePlayer wavePlayer;
        private PlayerActionPanel actionPanel;
        private UnitRuntimeModel currentCaster;
        private int selectedAbilityIndex = -1;
        private bool isResolving;

        public AbilityController(ServiceLocator serviceLocator, PlayFieldController playFieldController, HitFeedbackPlayer hitFeedbackPlayer)
        {
            this.serviceLocator = serviceLocator;
            this.playFieldController = playFieldController;
            this.hitFeedbackPlayer = hitFeedbackPlayer;
        }

        public bool HasSelectedAbility => selectedAbilityIndex >= 0 && selectedAbilityIndex < abilities.Count;
        public bool IsResolving => isResolving;
        public int AvailableAbilityCount => abilities.Count;

        public void Initialize()
        {
            EnsureConfig();
            EnsureWavePlayer();
            EnsureStartingAbilities();
        }

        public void Dispose()
        {
            ClearSelection();
        }

        public void BindActionPanel(PlayerActionPanel panel)
        {
            actionPanel = panel;
            RefreshButtons();
        }

        public void BeginTurn(UnitRuntimeModel caster)
        {
            currentCaster = caster;
            isResolving = false;
            EnsureStartingAbilities();
            ClearSelection();
            RefreshButtons();
        }

        public IReadOnlyList<GridPosition> GetPotentialImpactCells(UnitRuntimeModel caster)
        {
            var result = new List<GridPosition>();
            if (caster == null)
            {
                return result;
            }

            var unique = new HashSet<GridPosition>();
            for (var abilityIndex = 0; abilityIndex < abilities.Count; abilityIndex++)
            {
                var definition = abilities[abilityIndex].Definition;
                var casterPosition = caster.Position;
                var neighbors = new[]
                {
                    new GridPosition(casterPosition.X, casterPosition.Y + 1),
                    new GridPosition(casterPosition.X + 1, casterPosition.Y),
                    new GridPosition(casterPosition.X, casterPosition.Y - 1),
                    new GridPosition(casterPosition.X - 1, casterPosition.Y),
                };

                for (var i = 0; i < neighbors.Length; i++)
                {
                    var baseCell = neighbors[i];
                    if (!AbilityResolver.TryResolveCast(definition, casterPosition, baseCell, playFieldController, out var affectedCells, out _))
                    {
                        continue;
                    }

                    for (var cellIndex = 0; cellIndex < affectedCells.Count; cellIndex++)
                    {
                        var affected = affectedCells[cellIndex];
                        if (unique.Add(affected))
                        {
                            result.Add(affected);
                        }
                    }
                }
            }

            return result;
        }

        public void ClearSelection()
        {
            selectedAbilityIndex = -1;
            castOptions.Clear();
            previewCells.Clear();
            playFieldController.ClearAbilityHighlights();
            RefreshButtons();
            SelectionChanged?.Invoke();
        }

        public bool TryHandleCellTap(GridPosition tappedCell, Action onCastStarted, Action onCastCompleted)
        {
            if (isResolving || !HasSelectedAbility || currentCaster == null)
            {
                return false;
            }

            var castOption = FindCastOptionByTappedCell(tappedCell);
            if (castOption == null)
            {
                ClearSelection();
                return true;
            }

            var consumedAbilityIndex = selectedAbilityIndex;

            isResolving = true;
            selectedAbilityIndex = -1;
            castOptions.Clear();
            previewCells.Clear();
            playFieldController.ClearAbilityHighlights();
            RefreshButtons();
            SelectionChanged?.Invoke();
            onCastStarted?.Invoke();
            hitFeedbackPlayer?.PlayAttackFeedback(currentCaster);

            wavePlayer.PlayWave(
                castOption.WaveSteps,
                currentCaster.UnitType,
                ResolveWorldPosition,
                cell =>
                {
                    var wasHit = playFieldController.TryApplyDamageAt(cell, currentCaster.BaseDamage, currentCaster);
                    hitFeedbackPlayer?.PlayWaveCellFeedback(ResolveWorldPosition(cell), wasHit);
                },
                () =>
                {
                    if (abilities.Count > 1 && consumedAbilityIndex >= 0 && consumedAbilityIndex < abilities.Count)
                    {
                        abilities.RemoveAt(consumedAbilityIndex);
                    }

                    EnsureStartingAbilities();
                    EnforceAbilityBounds();
                    LogAbilityQueue("cast");
                    isResolving = false;
                    currentCaster = null;
                    RefreshButtons();
                    onCastCompleted?.Invoke();
                });

            return true;
        }

        public bool TryGainRandomAbilityOnWait(UnitRuntimeModel caster)
        {
            if (caster == null || abilities.Count > abilityConfig.WaitAbilityGainThreshold || abilities.Count >= abilityConfig.MaxAvailableAbilities)
            {
                return false;
            }

            var gainPool = abilityConfig.RandomGainPool;
            var validPool = new List<AbilityDefinitionId>(gainPool.Count);

            for (var i = 0; i < gainPool.Count; i++)
            {
                var candidateDefinitionId = gainPool[i];
                var candidateDefinition = AbilityDefinition.CreatePreset(candidateDefinitionId);
                if (!HasAnyValidCastWithoutSelfHit(candidateDefinition, caster.Position))
                {
                    continue;
                }

                validPool.Add(candidateDefinitionId);
            }

            if (validPool.Count == 0)
            {
                return false;
            }

            var selectedDefinitionId = validPool[random.Next(validPool.Count)];
            abilities.Add(new AbilityRuntime(AbilityDefinition.CreatePreset(selectedDefinitionId)));
            EnforceAbilityBounds();
            LogAbilityQueue("wait gain");
            RefreshButtons();
            SelectionChanged?.Invoke();
            return true;
        }

        private void EnsureConfig()
        {
            if (abilityConfig != null)
            {
                return;
            }

            try
            {
                abilityConfig = serviceLocator.ConfigurationProvider.GetConfig<AbilityConfig>();
            }
            catch
            {
                abilityConfig = AbilityConfig.CreateDefault();
            }
        }

        private void EnsureWavePlayer()
        {
            wavePlayer ??= new AbilityWavePlayer(abilityConfig.WaveCellDelay, abilityConfig.ImpactDuration);
        }

        private void EnsureStartingAbilities()
        {
            if (abilities.Count > 0)
            {
                return;
            }

            var configured = abilityConfig.StartingAbilities;
            if (configured != null)
            {
                for (var i = 0; i < configured.Count; i++)
                {
                    abilities.Add(new AbilityRuntime(AbilityDefinition.CreatePreset(configured[i])));
                }
            }

            if (abilities.Count == 0)
            {
                abilities.Add(new AbilityRuntime(AbilityDefinition.CreatePreset(AbilityDefinitionId.OLeft)));
            }

            EnforceAbilityBounds();
            LogAbilityQueue("initialization");
        }

        private void EnforceAbilityBounds()
        {
            if (abilities.Count == 0)
            {
                abilities.Add(new AbilityRuntime(AbilityDefinition.CreatePreset(AbilityDefinitionId.OLeft)));
            }

            while (abilities.Count > abilityConfig.MaxAvailableAbilities)
            {
                abilities.RemoveAt(abilities.Count - 1);
            }
        }

        private bool HasAnyValidCastWithoutSelfHit(AbilityDefinition definition, GridPosition casterPosition)
        {
            var baseCells = new[]
            {
                new GridPosition(casterPosition.X, casterPosition.Y + 1),
                new GridPosition(casterPosition.X + 1, casterPosition.Y),
                new GridPosition(casterPosition.X, casterPosition.Y - 1),
                new GridPosition(casterPosition.X - 1, casterPosition.Y),
            };

            for (var i = 0; i < baseCells.Length; i++)
            {
                if (AbilityResolver.TryResolveCast(definition, casterPosition, baseCells[i], playFieldController, out _, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshButtons()
        {
            if (actionPanel == null)
            {
                return;
            }

            actionPanel.EnsureAbilityButtonCount(abilities.Count);

            for (var i = 0; i < abilities.Count; i++)
            {
                var index = i;
                var definition = abilities[i].Definition;
                var abilityIcon = AbilityButtonIconResolver.Resolve(abilityConfig, definition.DefinitionId);
                var abilityLabel = definition.DisplayName;
                var isSelected = index == selectedAbilityIndex;
                actionPanel.ConfigureAbilityButton(
                    i,
                    abilityIcon,
                    abilityLabel,
                    isSelected,
                    !isResolving,
                    () => OnAbilityButtonPressed(index));
            }
        }

        private void OnAbilityButtonPressed(int abilityIndex)
        {
            if (isResolving || abilityIndex < 0 || abilityIndex >= abilities.Count || currentCaster == null)
            {
                return;
            }

            if (selectedAbilityIndex == abilityIndex)
            {
                ClearSelection();
                return;
            }

            selectedAbilityIndex = abilityIndex;
            RebuildCastOptionsAndPreview();
            RefreshButtons();
            SelectionChanged?.Invoke();
        }

        private void RebuildCastOptionsAndPreview()
        {
            castOptions.Clear();
            previewCells.Clear();

            if (!HasSelectedAbility || currentCaster == null)
            {
                playFieldController.ClearAbilityHighlights();
                return;
            }

            var caster = currentCaster.Position;
            var neighbors = new[]
            {
                new GridPosition(caster.X, caster.Y + 1),
                new GridPosition(caster.X + 1, caster.Y),
                new GridPosition(caster.X, caster.Y - 1),
                new GridPosition(caster.X - 1, caster.Y),
            };

            var previewUnique = new HashSet<GridPosition>();
            var definition = abilities[selectedAbilityIndex].Definition;
            for (var i = 0; i < neighbors.Length; i++)
            {
                var baseCell = neighbors[i];
                if (!AbilityResolver.TryResolveCast(definition, caster, baseCell, playFieldController, out var affectedCells, out var waveSteps))
                {
                    continue;
                }

                castOptions.Add(new CastOption
                {
                    BaseCell = baseCell,
                    AffectedCells = affectedCells,
                    WaveSteps = waveSteps,
                });

                for (var cellIndex = 0; cellIndex < affectedCells.Count; cellIndex++)
                {
                    var cell = affectedCells[cellIndex];
                    if (previewUnique.Add(cell))
                    {
                        previewCells.Add(cell);
                    }
                }
            }

            playFieldController.SetAbilityHighlights(previewCells);
        }

        private CastOption FindCastOptionByTappedCell(GridPosition tappedCell)
        {
            for (var i = 0; i < castOptions.Count; i++)
            {
                var option = castOptions[i];
                for (var cellIndex = 0; cellIndex < option.AffectedCells.Count; cellIndex++)
                {
                    if (option.AffectedCells[cellIndex] == tappedCell)
                    {
                        return option;
                    }
                }
            }

            return null;
        }

        private void LogAbilityQueue(string reason)
        {
            if (abilityConfig == null || !abilityConfig.EnableAbilityQueueDebugLogs)
            {
                return;
            }

            var labels = new string[abilities.Count];
            for (var i = 0; i < abilities.Count; i++)
            {
                labels[i] = abilities[i].Definition.DefinitionId.ToString();
            }

            Debug.Log($"Ability queue ({reason}): [{string.Join(", ", labels)}]");
        }

        private Vector3 ResolveWorldPosition(GridPosition position)
        {
            if (playFieldController.TryGetCellWorldPosition(position, out var worldPosition))
            {
                return worldPosition;
            }

            return new Vector3(position.X, position.Y, 0f);
        }
    }
}
