A remote configuration system.

# How configuration is organised

Configurations are kept in a tree where they can be grouped. 'Grouping' nodes are used for
keeping together different sets of config that are related; for example:

```
ROOT
 ├── website A [grouping]
 │   └── service 1 [config]
 │        – Logging:Level = "Info"
 │        – Feature_Alpha = true
 └── website B [grouping]
```

The following node types can exist in the tree:

- grouping – for organising related configs (for example for grouping permissions)
- config – a set of configuration values (like an appsettings file)




# [tech] File organisation in ConfigTree/
```
  ConfigTree/
  ├── GroupingNode.cs          the two aggregate roots stay at the top,
  ├── ConfigNode.cs            where you can't miss them
  ├── ConfigSnapshot.cs        what ConfigNode hands a client (+ ConfigKeyView)
  │
  ├── Nodes/                   identity and addressing, shared by both aggregates
  │   └ ...
  │
  ├── Keys/                    what a config node is made of
  │   ├── KeyDeclaration.cs    registration input
  │   ├── KeyAssignment.cs     edit input
  │   ├── KeyReadResult.cs     read output
  │   └ ...
  │
  ├── Changes/                 the vocabulary of "something changed"
      └ ...
```


# [tech] Known gaps

Things the domain and application layers deliberately leave open for now:

- **No way to rebuild a `ConfigNode` or a `NodeAccessList` from stored state.** Their constructors are private
  and always start at version 0, so Infrastructure will need a rehydration path (an internal factory or a
  persistence-state type). The in-memory test store keeps live instances meanwhile.
- **Permissions are deliberately simple.** They are given to single actors (no groups), always reach every
  node below, cannot be blocked further down, and there are no denials.
- **Whoever uses a fresh installation first owns it.** Creating the root needs no permission, and the first
  caller is made `Owner` of the whole tree. The same goes for whoever next ensures a root left without an
  access list. A deployment should make that first call itself, before anyone else can reach it.
- **`ManagePermissions` is as good as owning the node.** Whoever holds it may set any role, including roles
  with rights they do not hold themselves.
- **A refusal reveals that a node exists.** A path is resolved before access is checked, so a missing node
  answers "not found" and an existing one "access denied" (or "wrong kind"), whoever asks.
- **No read model for the permission history.** `RoleSet` events are raised and the
  repository records them, but nothing reads them back yet the way `IConfigChangeLog` does for config changes.
- **No cheap "version only" read.** `GetConfigSnapshot` with a known version still loads the whole aggregate.
  Add a dedicated read port only if polling load shows it matters.
- **No expected version on person edits.** `SetValues`/`ClearValues` are checked against the stored version
  at save time, but not against the version the person was looking at, so an edit made from a stale screen
  is applied rather than rejected.
- **A rollback mixing restored and cleared keys produces two change sets.** The domain has no operation that
  sets some keys and clears others as one change, so both are applied and saved atomically in one `Save`,
  but they appear as two history entries sharing the same rollback reason.