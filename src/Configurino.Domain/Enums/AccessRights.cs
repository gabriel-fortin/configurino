namespace org.g14.Configurino.Domain.Enums;

[Flags]
public enum AccessRights
{
    None = 0,
    Read = 1,
    CreateChildren = 2, // create groupings, objects, arrays, leafs
    SetAccessRights = 4,
    AddTypeHints = 8,
}

/*
- read – read a specific value or an entire config; for humans and clients
   - set values – for humans
   - set key and type hints – for clients, to indicate expected types and key names, to minimise human error
   - set values against type hint – for humans that want to be naughty
   - set permissions, set node type – for humans/admins
   - create grouping – for humans/admins
   */