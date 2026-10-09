# Goals

- [ ]  Client library
  + [ ]  Get an entire config from a location in the tree
  + [ ]  integrate with `IConfiguration`
- [ ]  Core logic for tree of groups and configs
- [ ]  Web app for editing configs
- [ ]  Authentication using AD (for humans) and API keys (for client applications)
- [ ]  Access to each tree location is ACL-protected (with inheritance but maybe not all rights are inherited)
- [ ]  Conventions for how to organise groupings
- [ ]  Audit of every change (what changed, when, and by whom)

# Out of scope

- Config values can be encrypted to keep secrets – too risky, creates a single point of secret leakage.
  Use something like Azure Key Vault instead

# Later Goals

- Allow to move/rename nodes. This will need a way to have two functioning locations having "the same" config. 
Otherwise, that would break client applications until they get updated. The "two parallel locations" could be 
solved in many ways. It could be a clone of the original config. Or the original location could become an alias 
for the new location. 
- Allow to delete nodes. Can a Config Node be always removed or only if it was not used in some time? 
Can a Grouping Node be always removed or only if there are no descendant Config Nodes? 

# Notes and thoughts

(maybe) Having rights in the root doesn't mean having rights in children. In other words, which
scopes or situations disable inheritance? (e.g. groupings)

(maybe) Project names: Composition Root, Application/Use cases, Infrastructure, Domain

Rights:
- register config – register a configuration tree / a set of keys (and their types) for which values can be set; for clients
- read – read a specific grouping node or config node; for humans and clients
- read config node – read a specific config node; for clients
- read node – read a specific grouping node or config node; for humans
- set values – set values of keys in a config node; for humans
- set permissions – decide who has which rights in a node; for humans/admins
- create nodes – create child grouping or config nodes – for humans/admins


===========

we're building the domain layer of a DDD project. The project's goal is to hold configurations for multiple services.                                                                                                    
  configurations for services can be organised in a tree-like structure. There should be two types of nodes in the tree:                                                                                                    
  - config nodes - contain entire configurations for a service, the aggregate root is the root of the config and the aggregate                                                                                             
  contains the entire configuration for that service                                                                                                                                                                       
  - grouping nodes - allow to group nodes of any types, the aggregate is just this node (figure out how to handle children                                                                                                 
  nodes)
    There are a few main ways for interaction:                                                                                                                                                                             
  - A client application, by giving a tree location it wants to use (as a path), registers the config keys that can be used.                                                                                               
  - A human user can set keys' values in a configuration. The user can browse nodes (grouping and config) and read their content                                                                                           
  - what children nodes it contains. Probably using tree paths for the location will make sense too.                                                                                                                       
  - A client application reads keys' values from a config node.                                                                                                                                                            
  - A human user can create grouping nodes and config nodes. A created config node is only a container to be populated by a                                                                                                
  client application and cannot have children. A grouping node can have children. A root grouping node exists by default.                                                                                                  
    Ask me for anything unclear

todo (maybe): check that nodes have a 'fullpath'; check that newly created nodes get the 'fullpath' set  
todo: We might want Grouping Nodes to have a collection of child nodes' ids. Or might find them by parentId. To be seen
todo: Investigate the 'outbox', probably not needed
