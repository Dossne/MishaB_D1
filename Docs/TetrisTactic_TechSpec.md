# TetrisTactic Technical Specification

## Purpose

Develop a mobile 2D turn-based tactical prototype in Unity where the player moves on a grid, uses shape-based abilities inspired by Tetris pieces, defeats enemies, and collects treasure.

This specification is written for AI-assisted implementation by Codex 5.3 and is intended to be the main source of truth for staged development.

## Mandatory execution rules for Codex

1. Implement only one stage at a time.
2. Stop after every stage.
3. After every stage:
   - summarize what was implemented;
   - list created or changed files, prefabs, ScriptableObjects, scenes, and folders;
   - describe the currently available gameplay or interaction loop;
   - state what was not verified if Unity/editor execution was not available;
   - ask the user to test the stage and report bugs.
4. Do not start the next stage until the user explicitly asks to continue.
5. If the user reports bugs for the current stage, fix them before moving to the next stage.
6. Keep architecture simple, readable, and safe for Unity assets.

## Project goals

Primary goal:
- quickly validate the core gameplay loop.

Secondary goals:
- keep the prototype easy to iterate on;
- avoid unnecessary complexity;
- preserve Unity project stability.

## Core game overview

The game is a turn-based 2D tactical game on a rectangular grid. The player moves cell-by-cell, attacks using directional abilities with Tetris-like patterns, kills enemies, and collects treasures. Between levels, the player spends resources on upgrades.

## Play field requirements

- Base field size: 6 columns x 8 rows.
- Base cell size: 180x180 pixels at reference resolution 1920x1080.
- Field size and cell size must be configurable.
- The screen must reserve space above the field for HUD.
- The screen must reserve space below the field for player action buttons.
- At level start, the field contains:
  - player character;
  - enemy characters;
  - treasures;
  - obstacles.
- Level generation must ensure passability:
  - the player must be able to reach every enemy;
  - the player must be able to reach every treasure.

## Turn rules

Each unit has three possible actions on its turn:
- move;
- attack;
- wait.

Units have HP. Abilities deal damage and may later support extra effects.

### Move

- Move only to passable cells.
- A passable cell cannot contain an obstacle or another unit.
- Move range: 1 cell.
- Allowed directions: up, down, left, right.

### Attack

- The acting unit targets an adjacent cell.
- Allowed directions: up, down, left, right.
- The ability area is defined by a shape.
- One cell of the shape is the base cell.
- The base cell must be placed on the targeted adjacent board cell.
- The rest of the shape must not overlap the acting unit cell.
- Ability effects travel as a wave through the shape cells with a short delay.
- Damage and effects are applied when the wave reaches each cell.

### Wait

- The unit skips the turn.
- A unit cannot wait twice in a row.
- Player wait:
  - if the player currently has no more than 2 available abilities, gain 1 random additional ability.
- Enemy wait:
  - regenerate part of missing HP.

## Player rules

- Base damage: 1.
- Base HP: 1.
- The player can have up to 3 available abilities.
- The first ability is always available.
- The second and third abilities are accumulated and consumed.

Ability consumption rules:
- If the player has more than 1 available ability:
  - the used ability is consumed and removed from the list;
  - the remaining abilities shift toward the start of the list.
- If the player has only 1 available ability:
  - the used ability is applied;
  - it is not consumed.

The player can obtain abilities with Tetris-like shapes:
- O;
- T;
- L;
- S;
- I.

The base cell may have different positions and orientations as long as the resulting cast pattern does not hit the caster.

## Enemy rules

Enemy types:

### Warrior
- Base damage: 3
- Base HP: 2
- Ability: straight line, 2 cells away from the attacker

### Archer
- Base damage: 2
- Base HP: 1
- Ability: straight line, 4 cells away from the attacker

### Mage
- Base damage: 2
- Base HP: 3
- Ability: T-shape with range 4, oriented away from the attacker

Enemy AI priorities:

1. If the player is in attack range, attack.
2. Otherwise move to get closer to a position where the player can be attacked.
3. If the enemy is damaged, is not in the player's ability range, and the player cannot get into this enemy's attack range within the next 2 turns, the enemy may wait to regenerate if wait is available.
4. If none of the above is possible, the enemy may move in any available direction or wait if available.

## Victory and defeat

- Victory: all enemies are dead.
- Defeat: player HP reaches 0.
- Victory reward:
  - collected treasure resource;
  - additional victory bonus resource.
- Defeat reward:
  - only collected treasure resource.

## Progression

- Levels are played sequentially.
- If the player loses, the same level is replayed.
- Before level 1 and between levels, show a progression popup.
- On the first progression popup, grant enough resource for exactly one upgrade.

Upgrade options:
- +1 ability damage;
- +1 player HP.

Level scaling:
- enemy HP and damage increase with level;
- enemy count gradually grows from 1 to 5;
- level 1: only Warrior;
- level 2: Warrior and Archer;
- level 3 and later: Warrior, Archer, Mage.

The player may choose between:
- killing all enemies quickly to get the victory bonus sooner;
- collecting treasure first and risking defeat for more upgrade income.

## UX and feedback requirements

### All units
- Damage must be emphasized with animation, sound, and FX.
- Attacks must be emphasized with sound and animation.
- Optional floating emoji or emotion text is allowed.

### Enemies
- Their ability danger zones in all possible directions should be softly highlighted.
- Enemy damage should cause a small temporary slowdown for impact feel.

### Player
- If an ability is selected, show the possible cast zones in all valid directions.
- If no ability is selected, highlight possible movement cells.
- Player damage should trigger a small screen shake.

### Controls

- If no ability is selected:
  - tapping a highlighted movement cell moves the player.
- If an ability is selected:
  - tapping a valid target cell casts the ability in that direction;
  - tapping outside valid cast cells clears the ability selection.
- The selected ability button must be highlighted.
- The wait button must be active only when waiting is allowed.

### Turn feel

- Enemy turns must include a small delay so their actions are visible.
- Turn handoff to the next unit must happen only after all ability wave visuals are fully completed.

## Architecture requirements

### GameManager : MonoBehaviour

Responsibilities:
- application bootstrap;
- create and initialize manager/controller classes in the correct order;
- call update logic where needed;
- deinitialize systems.

Requirements:
- placed in the main scene;
- has a serialized reference to ServiceLocator;
- registers itself in ServiceLocator first.

### ServiceLocator : MonoBehaviour

Responsibilities:
- central dependency access;
- access to serialized MonoBehaviours;
- access to configuration through ConfigurationProvider;
- access to created manager/controller classes.

Requirements:
- placed in the main scene;
- has a serialized reference to ConfigurationProvider;
- registers ConfigurationProvider second.

### ConfigurationProvider : ScriptableObject

Responsibilities:
- store references to configuration ScriptableObjects;
- provide centralized config access.

## Dependency rules

1. Manager/controller classes are created by GameManager.
2. Dependencies must be passed explicitly through constructors or Initialize methods.
3. Do not use FindObjectOfType for production dependencies.
4. Access UI through MainUiProvider.
5. Access configs through ConfigurationProvider.
6. Keep public API surface small and intentional.

## Feature folder structure

All feature folders must be placed inside:
- `Assets/Project`

Rule:
- one feature = one folder.

Folder format:

```text
Assets/Project/
  FeatureName/
    FeatureNameSrc/
    FeatureNamePfs/
    FeatureNameArt/
    FeatureNameCfg/
```

Rules:
- `FeatureNameSrc`: all scripts for the feature;
- `FeatureNamePfs`: all prefabs for the feature;
- `FeatureNameArt`: sprites, animations, animator controllers;
- `FeatureNameCfg`: only `.asset` ScriptableObjects;
- ScriptableObject C# definitions must stay in `FeatureNameSrc`.

## Required main feature folders

```text
Assets/Project/
  Core/
  MainUi/
  PlayField/
  LevelFlow/
  FinishFlow/
  Resource/
  Units/
  PlayerTurn/
  EnemyTurn/
  Abilities/
  Progression/
  Treasure/
  Feedback/
  CameraFx/
  Audio/
```

## Scene requirements

Main scene must contain at minimum:
- GameManager;
- ServiceLocator;
- PlayFieldView;
- MainUiProvider;
- Camera;
- EventSystem.

## Required staged development plan

### Stage 0. Core bootstrap architecture

Goal:
- prepare the base project architecture.

Implement:
- `GameManager`
- `ServiceLocator`
- `ConfigurationProvider`

Create:

```text
Assets/Project/Core/
  CoreSrc/
    GameManager.cs
    ServiceLocator.cs
    ConfigurationProvider.cs
    IInitializableController.cs
    IUpdatableController.cs
    IDisposableController.cs
  CoreCfg/
    ConfigurationProvider.asset
  CorePfs/
  CoreArt/
```

Resulting vertical slice:
- the app launches;
- core bootstrap initializes successfully;
- no expected startup errors.

User-available interaction loop:
- launch app;
- verify successful initialization.

### Stage 1. Base UI

Goal:
- create the initial UI shell and progression popup.

Implement:
- `MainUiProvider`
- `ProgressionPopup`
- `ResourceCounter`

Requirements:
- Main canvas in overlay mode.
- CanvasScaler reference resolution 1920x1080.
- MainUiProvider holds references to:
  - `FloatingTextParent`
  - `HudParent`
  - `PopupParent`
  - `ProgressionPopup`
  - HUD `ResourceCounter`
- ProgressionPopup visible on startup.
- It contains:
  - top-left resource plate;
  - centered `Level N` text;
  - two upgrade plates;
  - price labels;
  - large `Start Level` button.
- Buttons can log to console on this stage.

Create:

```text
Assets/Project/MainUi/
  MainUiSrc/
    MainUiProvider.cs
    ProgressionPopup.cs
    ResourceCounter.cs
  MainUiPfs/
    ProgressionPopup.prefab
    ResourceCounter.prefab
  MainUiArt/
  MainUiCfg/
```

Resulting vertical slice:
- player opens app;
- player sees progression popup;
- player presses buttons and gets visible feedback.

User-available interaction loop:
- open app;
- inspect popup;
- press upgrade or start buttons.

### Stage 2. PlayField skeleton

Goal:
- show a real grid and connect it to level flow.

Implement:
- `PlayFieldController`
- `PlayFieldView`
- `LevelFlowController`

Requirements:
- LevelFlowController shows ProgressionPopup on startup.
- On Start Level:
  - close popup;
  - create field through PlayFieldController.
- PlayFieldController supports:
  - create field;
  - clear field;
  - generate initial content placeholders;
  - update PlayFieldView;
  - emit cell tap event.
- On field cell tap:
  - LevelFlowController clears field;
  - shows ProgressionPopup again.

Create:

```text
Assets/Project/PlayField/
  PlayFieldSrc/
    PlayFieldController.cs
    PlayFieldView.cs
    CellView.cs
    PlayFieldModel.cs
    GridPosition.cs
    PlayFieldConfig.cs
  PlayFieldPfs/
    PlayFieldView.prefab
    CellView.prefab
  PlayFieldArt/
  PlayFieldCfg/
    PlayFieldConfig.asset

Assets/Project/LevelFlow/
  LevelFlowSrc/
    LevelFlowController.cs
  LevelFlowPfs/
  LevelFlowArt/
  LevelFlowCfg/
```

Resulting vertical slice:
- player opens popup;
- presses Start Level;
- sees the field;
- taps a cell;
- returns to popup.

User-available interaction loop:
- popup -> start level -> view field -> tap cell -> back to popup.

### Stage 3. Finish popup flow

Goal:
- add a base level-completion popup.

Implement:
- `FinishPopup`

Requirements:
- Popup contains:
  - Victory/Defeat title;
  - resource icon and amount;
  - victory bonus block;
  - Continue button.
- On this stage, tapping a field cell counts as defeat.
- LevelFlowController:
  - clears field;
  - opens FinishPopup;
  - on Continue closes FinishPopup and opens ProgressionPopup.

Create:

```text
Assets/Project/FinishFlow/
  FinishFlowSrc/
    FinishPopup.cs
  FinishFlowPfs/
    FinishPopup.prefab
  FinishFlowArt/
  FinishFlowCfg/
```

Resulting vertical slice:
- player starts level;
- taps field;
- sees defeat popup;
- returns to progression popup.

User-available interaction loop:
- popup -> start -> field -> defeat popup -> continue -> popup.

### Stage 4. Resource system

Goal:
- make the shared resource real.

Implement:
- `ResourceController`

Requirements:
- Supports:
  - add resource;
  - spend resource;
  - query amount;
  - check affordability;
  - balance changed event.
- All ResourceCounter views update from ResourceController event.
- On game start, grant 1 resource.
- ProgressionPopup upgrade buttons spend 1 resource.
- FinishPopup shows reward of 1 resource.
- On Continue, reward is added through ResourceController.

Create:

```text
Assets/Project/Resource/
  ResourceSrc/
    ResourceController.cs
  ResourcePfs/
  ResourceArt/
  ResourceCfg/
```

Resulting vertical slice:
- player sees and spends resource;
- player completes mock run;
- player gains reward;
- resource amount updates in UI.

User-available interaction loop:
- spend initial resource in popup;
- start level;
- finish run;
- receive resource;
- return to popup.

### Stage 5. Units and level content foundation

Goal:
- replace placeholder field with real entities.

Implement:
- unit runtime data;
- player placement;
- enemy placement;
- treasure placement;
- obstacle placement;
- passable generation.

Create:

```text
Assets/Project/Units/
  UnitsSrc/
    UnitData.cs
    UnitRuntimeModel.cs
    UnitView.cs
    UnitFactory.cs
    UnitType.cs
    TeamType.cs
    HealthComponent.cs
    UnitConfig.cs
  UnitsPfs/
    PlayerUnit.prefab
    WarriorUnit.prefab
    ArcherUnit.prefab
    MageUnit.prefab
    ObstacleView.prefab
  UnitsArt/
  UnitsCfg/
    UnitConfig.asset

Assets/Project/Treasure/
  TreasureSrc/
    TreasureView.cs
    TreasureData.cs
    TreasureConfig.cs
  TreasurePfs/
    TreasureView.prefab
  TreasureArt/
  TreasureCfg/
    TreasureConfig.asset
```

Resulting vertical slice:
- player sees a proper level layout with unit, enemies, treasure, and obstacles.

User-available interaction loop:
- start level;
- inspect generated combat board with level content.

### Stage 6. Player turn: movement and wait

Goal:
- create the first real player turn.

Implement:
- `PlayerTurnController`
- movement highlight
- wait action
- lower action panel

Requirements:
- If no ability is selected:
  - highlight legal movement cells;
  - tapping one moves the player.
- Add wait button.
- Wait cannot be used twice in a row.
- Turn passes onward after move or wait.

Create:

```text
Assets/Project/PlayerTurn/
  PlayerTurnSrc/
    PlayerTurnController.cs
    PlayerActionPanel.cs
    MoveHighlighter.cs
    WaitButtonView.cs
  PlayerTurnPfs/
    PlayerActionPanel.prefab
    WaitButton.prefab
  PlayerTurnArt/
  PlayerTurnCfg/
```

Resulting vertical slice:
- player can move one cell or wait.

User-available interaction loop:
- start level;
- inspect move options;
- move or wait;
- end turn.

### Stage 7. Ability system and player attack

Goal:
- implement real player abilities with shape previews and wave execution.

Implement:
- `AbilityController`
- `AbilityDefinition`
- ability preview
- cast validation
- damage wave execution
- ability buttons

Requirements:
- Player starts with at least one ability.
- Support shapes:
  - O;
  - T;
  - L;
  - S;
  - I.
- If ability selected:
  - highlight valid cast targets;
  - valid tap casts;
  - invalid tap clears selection.
- Ability wave resolves over time.
- Damage is applied when wave reaches each affected cell.
- If more than one ability is available:
  - the used one is consumed;
  - the rest shift left.
- If only one ability is available:
  - it is reused and remains.
- The turn ends only after the full wave finishes.

Create:

```text
Assets/Project/Abilities/
  AbilitiesSrc/
    AbilityController.cs
    AbilityDefinition.cs
    AbilityRuntime.cs
    AbilityShapeCell.cs
    AbilityShapeType.cs
    AbilityDirection.cs
    AbilityResolver.cs
    AbilityWavePlayer.cs
    AbilityButtonView.cs
    AbilityConfig.cs
  AbilitiesPfs/
    AbilityButton.prefab
    AbilityImpactCell.prefab
  AbilitiesArt/
  AbilitiesCfg/
    AbilityConfig.asset
```

Resulting vertical slice:
- player can move, select ability, preview targets, cast ability, and damage enemies.

User-available interaction loop:
- choose between move, attack, or wait;
- preview ability;
- cast and watch wave resolution.

### Stage 8. Enemy turn and AI

Goal:
- implement visible enemy turns and tactical behavior.

Implement:
- `EnemyTurnController`
- `EnemyAiController`

Requirements:
- Enemies act one by one.
- Enemy actions include visible delay.
- Enemy chooses between:
  - attack;
  - move closer;
  - wait to regenerate if conditions are met;
  - fallback move or wait.
- Enemy attack preview zones are softly highlighted.

Create:

```text
Assets/Project/EnemyTurn/
  EnemyTurnSrc/
    EnemyTurnController.cs
    EnemyAiController.cs
    EnemyDecisionModel.cs
    EnemyThreatAnalyzer.cs
    EnemyWaitLogic.cs
  EnemyTurnPfs/
  EnemyTurnArt/
  EnemyTurnCfg/
```

Resulting vertical slice:
- player takes a turn;
- enemies visibly take their turns in sequence.

User-available interaction loop:
- player action;
- enemy sequence;
- return to player.

### Stage 9. Real win and lose conditions

Goal:
- connect battle outcome to real combat resolution.

Implement:
- unit death;
- victory detection;
- defeat detection;
- finish popup with actual result.

Requirements:
- Remove units when HP reaches 0.
- Victory when all enemies are dead.
- Defeat when the player dies.

Resulting vertical slice:
- full combat now ends by real battle rules.

User-available interaction loop:
- battle until all enemies die or player dies;
- see correct finish popup.

### Stage 10. Treasures, level progression, upgrades

Goal:
- implement the real meta loop between levels.

Implement:
- treasure collection;
- level progression;
- enemy scaling;
- actual upgrade application.

Create:

```text
Assets/Project/Progression/
  ProgressionSrc/
    ProgressionController.cs
    LevelDefinition.cs
    LevelProgressionConfig.cs
    PlayerUpgradeState.cs
    PlayerUpgradeType.cs
  ProgressionPfs/
  ProgressionArt/
  ProgressionCfg/
    LevelProgressionConfig.asset
```

Requirements:
- Treasure grants resource.
- Victory grants bonus.
- Defeat grants only collected resource.
- Victory advances to next level.
- Defeat repeats the same level.
- Upgrade popup really increases player HP or damage.
- Enemy roster and count follow level rules.

Resulting vertical slice:
- real level-to-level progression loop exists.

User-available interaction loop:
- upgrade -> play level -> collect reward -> upgrade -> next level.

### Stage 11. Ability gain on wait and full player rules

Goal:
- complete the player's ability economy.

Implement:
- random ability gain on wait;
- max 3 abilities;
- proper ability queue maintenance.

Requirements:
- On wait, if the player has at most 2 abilities, add a random one.
- Do not generate invalid self-hitting shapes.
- Update UI consistently.

Resulting vertical slice:
- player can tactically bank abilities by waiting.

User-available interaction loop:
- manage movement, attacks, and ability generation through wait.

### Stage 12. Feedback and juice

Goal:
- improve readability and impact feel.

Implement:
- hit feedback;
- attack feedback;
- floating texts or emoji;
- sound hooks;
- player camera shake;
- enemy hit slow.

Create:

```text
Assets/Project/Feedback/
  FeedbackSrc/
    FloatingTextController.cs
    HitFeedbackPlayer.cs
    EmojiBubbleView.cs
  FeedbackPfs/
    FloatingText.prefab
    EmojiBubble.prefab
  FeedbackArt/
  FeedbackCfg/

Assets/Project/CameraFx/
  CameraFxSrc/
    CameraShakeController.cs
    TimeImpactController.cs
  CameraFxPfs/
  CameraFxArt/
  CameraFxCfg/

Assets/Project/Audio/
  AudioSrc/
    AudioController.cs
    AudioCue.cs
    AudioConfig.cs
  AudioPfs/
  AudioArt/
  AudioCfg/
    AudioConfig.asset
```

Resulting vertical slice:
- the combat loop feels readable and impactful.

User-available interaction loop:
- full combat with readable feedback on attacks, damage, and turn pacing.

### Stage 13. Balancing and polish

Goal:
- finish the core prototype in a stable and tunable state.

Implement:
- move balancing values into configs;
- remove dangerous hardcodes;
- final cleanup of dependency flow;
- small debug support where useful.

Required config assets by the end:
- `ConfigurationProvider.asset`
- `PlayFieldConfig.asset`
- `UnitConfig.asset`
- `AbilityConfig.asset`
- `LevelProgressionConfig.asset`
- `TreasureConfig.asset`
- `AudioConfig.asset` when audio is added

Resulting vertical slice:
- a complete MVP loop across multiple levels with progression, combat, and upgrades.

User-available interaction loop:
- play several consecutive levels with stable progression and tunable balance.

## Coding guidelines

- Use English for class names, file names, folder names, prefab names, and config names.
- Prefer explicit readable code over clever abstractions.
- Keep classes small when possible.
- Keep public API small.
- Do not introduce large frameworks unless explicitly required.
- Separate gameplay logic from presentation when practical.
- Use explicit methods such as:
  - `Initialize`
  - `Show`
  - `Hide`
  - `Refresh`
  - `TrySpend`
  - `CreateLevel`
  - `ClearLevel`
- Unsubscribe from events explicitly where needed.

## Risks Codex must keep in mind

1. Do not break scene or prefab references.
2. Do not create unnecessary coupling between controllers.
3. Do not over-engineer prototype systems.
4. Do not make enemy turns too fast to read.
5. Do not allow ability patterns that hit the caster.
6. Do not allow unpassable generated boards.
7. Do not continue to the next stage without stopping for user validation.

### Stage 14. Denser play field layout tuning

Goal:
- reduce cell size by about 1.5x and increase board density while preserving the total on-screen field footprint.

Implement:
- updated play field size configuration;
- updated cell size configuration;
- view/layout adjustments required to keep the field in the same screen position and pixel footprint;
- validation of generation, selection, movement, attacks, and highlights on the denser board.

Requirements:
- Keep the overall field rectangle on screen approximately the same size and in the same position as before this stage.
- Reduce the base cell size from 180x180 to approximately 120x120.
- Increase the board size proportionally from 6x8 to approximately 9x12.
- Final exact values must remain configurable through ScriptableObject config and must not be hardcoded into gameplay logic.
- PlayFieldView, input handling, highlights, unit placement, pathing, generation, and ability previews must work correctly with the new grid density.
- Preserve top HUD space and bottom action panel space.
- Keep cells large enough for touch interaction on the target device; if the exact 1.5x reduction causes usability problems, use the closest practical values while keeping the overall field footprint unchanged.
- If any existing generation or UI assumptions depend on 6x8, refactor only the minimal code required to make them data-driven.

Files and assets expected to change:
- `Assets/Project/PlayField/PlayFieldSrc/*` where grid sizing or layout is assumed;
- `Assets/Project/PlayField/PlayFieldCfg/PlayFieldConfig.asset`;
- any related config or UI layout files that assume the previous 6x8 board.

Resulting vertical slice:
- the full existing game loop remains playable on a denser board with smaller cells, while the field keeps the same overall footprint on screen.

User-available interaction loop:
- play the same combat and progression loop on the resized board;
- verify comfort of tapping, readability, spacing, and gameplay pacing on the target device.

