# Smash Fest - Coding Standard

## Language
All identifiers, comments and log messages are written in English.

## Member order (every script follows this order)
1. Constants (`const`, `static readonly`)
2. Events
3. Properties
4. Serialized fields (grouped with `[Header]`)
5. Private runtime fields
6. Unity messages (Awake, OnEnable, Start, FixedUpdate, Update, collision callbacks, OnDisable, OnDestroy)
7. Public methods
8. Protected methods
9. Private methods
10. Editor only members inside `#if UNITY_EDITOR`

## References
Component references are assigned through `[SerializeField]` in the inspector.
Runtime `GetComponent` is only allowed when the target is discovered at runtime
(physics queries, raycasts) and the result is used immediately.

## Null checks
A null check is written only when the reference can legitimately be null:
- C# events (`StateChanged?.Invoke(...)`) - an event without subscribers is null.
- Singleton guards.
- Editor time code where the designer has not assigned the field yet.
Serialized references that are always filled in the prefab are never null checked.
