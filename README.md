A remote configuration system.

# How configuration is organised

Configurations are kept in a tree where they can be grouped. 'Grouping' nodes are used for
keeping together different sets of config that are related; for example:

```
ROOT
 |– website A [grouping]
 |  ` – service 1 [grouping]
 |      |– Logging [object]
 |      |  `– Level = "Info" [leaf]
 |      `– Feature Alpha = true [leaf]
 `– website B [grouping]
```

The following node types can exist in the config tree:

- tree grouping – for organising related configs (for example for grouping permissions)
- config object – an object existing in some `IConfiguration`/appsettings file
- config array - an array existing in some `IConfiguration`/appsettings file
- config leaf – a primitive value existing in some `IConfiguration`/appsettings file
