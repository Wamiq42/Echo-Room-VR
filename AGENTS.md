# Echo Room VR — Agent Instructions

## Persistent project memory

Before changing this project, read `Docs/PROJECT_MEMORY.md`.

After every completed change, update `Docs/PROJECT_MEMORY.md` in the same work session. The entry must record:

- the date and a concise change ID;
- the goal and resulting behavior;
- every created, modified, moved, or deleted file using a project-relative path;
- every affected Unity object using `SceneOrPrefabAssetPath :: Root/Child/Object`;
- important component, asset, setting, and dependency references;
- decisions, assumptions, known limitations, and follow-up work;
- verification performed and its result.

Never invent a Unity object path. Inspect the scene or prefab through the Unity Editor connection before recording it. If Unity is unavailable, write `Not inspected — Unity connection unavailable` and update the entry later.

Treat the memory as an append-only project journal. Correct inaccurate older information with a new correction entry; do not silently rewrite history. The `Current Project Index` section may be maintained in place because it represents current state.

Do not record passwords, API keys, access tokens, personal data, or other secrets.

