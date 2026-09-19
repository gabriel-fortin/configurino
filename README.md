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