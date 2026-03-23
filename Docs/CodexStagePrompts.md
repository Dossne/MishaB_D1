# Codex Stage Prompts

Use these prompts to run each implementation stage as a separate Codex task.

Always use them together with:
- `AGENTS.md`
- `Docs/TetrisTactic_TechSpec.md`

## Stage 0

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

You are implementing the Unity prototype TetrisTactic strictly by stages.

Mandatory workflow:
- Implement only the current stage.
- Do not start the next stage automatically.
- Stop at the end of the stage.
- After finishing the stage, report:
  - what was implemented;
  - which files, prefabs, ScriptableObjects, scenes, or folders were added or changed;
  - what gameplay loop or interaction loop is now available;
  - what was not verified if Unity/editor execution was not available.
- Then explicitly ask me to test the current stage in Unity/build, report bugs, and confirm when to continue.
- If I report bugs, fix them before moving to the next stage.

Implementation constraints:
- Follow the architecture from the spec strictly, especially GameManager, ServiceLocator, ConfigurationProvider, and explicit dependency passing.
- Keep changes minimal, readable, and safe for Unity assets.
- Do not over-engineer.
- Do not rename or move files unless necessary.
- Be careful with scene, prefab, and serialized references.
- Use the feature folder structure defined in the spec under Assets/Project.
- Use English names for classes, files, prefabs, configs, and folders.

Current task:
Implement Stage 0 from Docs/TetrisTactic_TechSpec.md:
"Core bootstrap architecture"

Required result for this run:
- Create and wire the base architecture for:
  - GameManager : MonoBehaviour
  - ServiceLocator : MonoBehaviour
  - ConfigurationProvider : ScriptableObject
- Ensure the main scene can bootstrap these objects cleanly.
- Keep the diff small and safe.

Do not implement anything from Stage 1 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 1

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Do not start the next stage automatically.
- Stop at the end of the stage.
- Report what was implemented, changed files/assets, current interaction loop, and unverified items.
- Ask me to test this stage and report bugs before continuing.

Current task:
Implement Stage 1 from Docs/TetrisTactic_TechSpec.md:
"Base UI"

Required result for this run:
- Create and wire:
  - MainUiProvider : MonoBehaviour
  - ProgressionPopup : MonoBehaviour
  - ResourceCounter : MonoBehaviour
- Configure Canvas as Overlay with CanvasScaler reference resolution 1920x1080.
- Make ProgressionPopup visible on startup.
- Add upgrade buttons and Start Level button.
- On this stage, buttons may only log to console.

Do not implement anything from Stage 2 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 2

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 2 from Docs/TetrisTactic_TechSpec.md:
"PlayField skeleton"

Required result for this run:
- Create and wire:
  - PlayFieldController
  - PlayFieldView : MonoBehaviour
  - LevelFlowController
- LevelFlowController must show ProgressionPopup on startup.
- On Start Level:
  - hide popup;
  - create the field via PlayFieldController.
- PlayFieldController must support:
  - create field;
  - clear field;
  - update view;
  - emit cell tap event.
- On cell tap:
  - LevelFlowController clears the field;
  - shows ProgressionPopup again.

Do not implement anything from Stage 3 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 3

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 3 from Docs/TetrisTactic_TechSpec.md:
"Finish popup flow"

Required result for this run:
- Create and wire:
  - FinishPopup : MonoBehaviour
- FinishPopup must contain:
  - Victory/Defeat title;
  - resource icon and amount;
  - victory bonus block;
  - Continue button.
- On this stage, tapping a field cell counts as defeat.
- LevelFlowController must:
  - clear the field;
  - show FinishPopup;
  - on Continue, close FinishPopup and reopen ProgressionPopup.

Do not implement anything from Stage 4 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 4

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 4 from Docs/TetrisTactic_TechSpec.md:
"Resource system"

Required result for this run:
- Create and wire:
  - ResourceController
- ResourceController must support:
  - Add
  - TrySpend
  - HasEnough
  - GetCurrentAmount
  - balance changed event
- All ResourceCounter instances must update from ResourceController events.
- On game start, grant 1 resource.
- Upgrade buttons in ProgressionPopup must spend 1 resource.
- FinishPopup must show reward of 1 resource.
- On Continue, reward must be added through ResourceController.

Do not implement anything from Stage 5 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 5

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current interaction loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 5 from Docs/TetrisTactic_TechSpec.md:
"Units and level content foundation"

Required result for this run:
- Add real level entities:
  - player;
  - enemies;
  - treasures;
  - obstacles.
- Add the Units and Treasure feature folders and required files from the spec.
- Create basic runtime models and views for units and treasures.
- Generate content on the board.
- Ensure generated boards remain passable so the player can reach all enemies and treasures.

Do not implement anything from Stage 6 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 6

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 6 from Docs/TetrisTactic_TechSpec.md:
"Player turn: movement and wait"

Required result for this run:
- Create and wire:
  - PlayerTurnController
  - lower action panel
  - movement highlight
  - wait button
- If no ability is selected:
  - highlight legal move cells;
  - tapping one moves the player by 1 cell orthogonally.
- Movement must respect blocked and occupied cells.
- Add Wait action.
- Wait cannot be used twice in a row.
- End the player turn after move or wait.

Do not implement anything from Stage 7 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 7

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 7 from Docs/TetrisTactic_TechSpec.md:
"Ability system and player attack"

Required result for this run:
- Create and wire the Abilities feature from the spec.
- Implement player ability selection, preview, validation, and cast.
- Support Tetris-like shapes:
  - O
  - T
  - L
  - S
  - I
- Enforce base-cell targeting rules.
- Prevent ability patterns from hitting the caster.
- Implement wave-based ability resolution with per-cell delay.
- Apply damage when the wave reaches each affected cell.
- If the player has more than 1 ability, the used ability is consumed and the rest shift left.
- If the player has only 1 ability, it remains after use.
- End the turn only after the full wave visual is complete.

Do not implement anything from Stage 8 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 8

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 8 from Docs/TetrisTactic_TechSpec.md:
"Enemy turn and AI"

Required result for this run:
- Create and wire:
  - EnemyTurnController
  - EnemyAiController
- Enemies must act one by one with a visible delay.
- Enemy AI priorities:
  - attack if player is in range;
  - otherwise move closer to threaten attack;
  - wait to regenerate if the spec conditions are met;
  - otherwise make a fallback move or wait.
- Use the correct enemy attack shapes and ranges for Warrior, Archer, and Mage.
- Softly highlight enemy danger zones in all possible directions.

Do not implement anything from Stage 9 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 9

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 9 from Docs/TetrisTactic_TechSpec.md:
"Real win and lose conditions"

Required result for this run:
- Implement real combat death handling for units.
- Remove units from the board when HP reaches 0.
- Trigger victory when all enemies are dead.
- Trigger defeat when the player dies.
- Show FinishPopup with the correct result based on actual combat.

Do not implement anything from Stage 10 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 10

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 10 from Docs/TetrisTactic_TechSpec.md:
"Treasures, level progression, upgrades"

Required result for this run:
- Create and wire the Progression feature from the spec.
- Treasure collection must grant resource.
- Victory must grant bonus resource.
- Defeat must grant only collected resource.
- Victory advances to the next level.
- Defeat repeats the same level.
- ProgressionPopup upgrades must actually increase:
  - player HP;
  - player damage.
- Enemy roster, count, and scaling must follow the stage rules from the spec.

Do not implement anything from Stage 11 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 11

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 11 from Docs/TetrisTactic_TechSpec.md:
"Ability gain on wait and full player rules"

Required result for this run:
- On player wait, if the player has at most 2 abilities, add 1 random ability.
- Enforce a maximum of 3 available abilities.
- Maintain correct ability queue behavior after gain and use.
- Do not generate invalid self-hitting shapes.
- Update the ability UI consistently.

Do not implement anything from Stage 12 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 12

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 12 from Docs/TetrisTactic_TechSpec.md:
"Feedback and juice"

Required result for this run:
- Add readable hit feedback.
- Add attack feedback.
- Add floating text and/or emoji feedback where appropriate.
- Add sound hooks or placeholder audio integration.
- Add player damage camera shake.
- Add enemy damage time slow / hit stop.
- Keep the battle readable and do not break turn flow timing.

Do not implement anything from Stage 13 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 13

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 13 from Docs/TetrisTactic_TechSpec.md:
"Balancing and polish"

Required result for this run:
- Move key balancing values into ScriptableObject configs.
- Remove dangerous hardcodes where they block balancing or maintainability.
- Clean up dependency flow where needed without broad refactors.
- Add only small useful debug support.
- Keep the MVP stable, tunable, and aligned with the spec.

Do not add speculative post-MVP systems or Stage 14 changes in this run.

When done, stop and wait for my feedback.
```

## Stage 14

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Continue staged implementation of TetrisTactic.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.

Current task:
Implement Stage 14 from Docs/TetrisTactic_TechSpec.md:
"Denser play field layout tuning"

Required result for this run:
- Reduce the base cell size by about 1.5x, from roughly 180x180 to roughly 120x120.
- Increase the board size proportionally from 6x8 to roughly 9x12.
- Keep the overall field footprint on screen approximately the same size and in the same position as before.
- Preserve HUD space above the field and action panel space below it.
- Make the final values configurable through config assets rather than gameplay hardcodes.
- Update any grid generation, pathing, input, highlights, previews, and board layout logic that still assumes 6x8.
- Keep the diff minimal and focused on adapting the existing systems to the denser board.

Do not implement new gameplay systems in this run.

When done, stop and wait for my feedback.
```
