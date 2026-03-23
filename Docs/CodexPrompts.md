# Codex Prompt Templates

This file contains reusable prompt templates for staged implementation of `TetrisTactic`.

Use them together with:
- `AGENTS.md`
- `Docs/TetrisTactic_TechSpec.md`

## 1. Start Stage 0

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

## 2. Continue To Next Stage

```text
Continue development strictly according to:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

The current stage is accepted.

Now implement the next stage only.
Do not touch later stages.
Keep the diff minimal and safe.

After completion:
- stop;
- summarize what was implemented;
- list changed files/prefabs/assets/folders;
- describe the currently available gameplay or interaction loop;
- state what was not verified;
- ask me to test and report bugs.
```

## 3. Fix Current Stage Bugs

```text
Do not continue to the next stage.

Fix only the bugs of the current stage according to:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Keep the implementation within the scope of the current stage unless a very small supporting fix is required for stability.

After fixing:
- stop;
- list what was changed;
- explain what bug was fixed;
- mention any remaining risks or unverified parts;
- ask me to re-test the current stage.
```

## 4. Prevent Scope Creep

```text
Important:
Do not implement anything from future stages.
Do not add speculative architecture for later stages unless it is strictly required for the current stage to work.

Work only within the scope of the active stage from:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

If something belongs to a later stage, leave it out and stop after finishing the current stage.
```

## 5. Verify Stage Workflow Compliance

```text
Before making further changes, verify your work against:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Answer briefly:
1. What is the current active stage?
2. Which exact requirements of that stage are still incomplete?
3. Did you implement anything from later stages? If yes, list it explicitly.
4. What should be tested now before continuing?

Do not start new implementation in this response.
```

## 6. Force Stage Boundary Check Before New Work

```text
Before you write code, restate the active stage from Docs/TetrisTactic_TechSpec.md and list only the requirements that belong to this stage.

Then implement only those requirements.

Do not include improvements, refactors, or systems from later stages unless they are strictly required for the current stage to run.

At the end, stop and wait for validation.
```

## 7. Ask For Minimal Safe Fixes Only

```text
Make only the minimal safe changes required to complete the current stage.

Avoid:
- speculative extensibility;
- future-stage hooks that are not needed yet;
- broad refactors;
- renames or moves unless unavoidable.

If you think something should be deferred to a later stage, defer it and mention it in the final summary instead of implementing it now.
```

## 8. Re-Check Against Source Of Truth

```text
Re-read and follow:
- AGENTS.md
- Docs/TetrisTactic_TechSpec.md

Treat them as the source of truth for architecture, folder structure, and stage order.

If your current plan conflicts with those files, adjust your plan to match them before changing code.
```
