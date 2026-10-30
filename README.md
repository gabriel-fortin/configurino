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

The following node types can exist in the config tree:

- tree grouping – for organising related configs (for example for grouping permissions)
- config object – an object existing in some `IConfiguration`/appsettings file
- config array - an array existing in some `IConfiguration`/appsettings file
- config leaf – a primitive value existing in some `IConfiguration`/appsettings file


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

- **No way to rebuild a `ConfigNode` from stored state.** Its constructor is private and always starts at
  version 0, so Infrastructure will need a rehydration path (an internal factory or a persistence-state type).
  The in-memory test store keeps live instances meanwhile.
- **No cheap "version only" read.** `GetConfigSnapshot` with a known version still loads the whole aggregate.
  Add a dedicated read port only if polling load shows it matters.
- **No expected version on person edits.** `SetValues`/`ClearValues` are checked against the stored version
  at save time, but not against the version the person was looking at, so an edit made from a stale screen
  is applied rather than rejected.
- **A rollback mixing restored and cleared keys produces two change sets.** The domain has no operation that
  sets some keys and clears others as one change, so both are applied and saved atomically in one `Save`,
  but they appear as two history entries sharing the same rollback reason.