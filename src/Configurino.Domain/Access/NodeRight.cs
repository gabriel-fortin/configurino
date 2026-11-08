namespace org.g14.Configurino.Domain.Access;

/// <summary>
/// What can be done on a node. Combined as flags: a <see cref="Permission"/> holds a set of them, given
/// by setting a <see cref="Role"/>.
/// </summary>
/// <remarks>
/// A right held on a node holds on everything below it too, and nothing below can take it away.
/// </remarks>
[Flags]
public enum NodeRight
{
    None = 0,

    /// <summary>See a node and list what sits under it.</summary>
    Browse = 1 << 0,

    /// <summary>Read the values of a config node.</summary>
    ReadConfig = 1 << 1,

    /// <summary>Read the change history of a config node.</summary>
    ReadHistory = 1 << 2,

    /// <summary>Declare the keys of a config node — what a client application does.</summary>
    RegisterKeys = 1 << 3,

    /// <summary>Set, clear and roll back values of a config node.</summary>
    EditValues = 1 << 4,

    /// <summary>Create grouping and config nodes under a grouping node.</summary>
    CreateNodes = 1 << 5,

    /// <summary>Decide who holds which rights on a node.</summary>
    ManagePermissions = 1 << 6,
}
