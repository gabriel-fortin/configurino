namespace org.g14.Configurino.Domain.ConfigTree.Changes;

/// <summary>What a registration did to one key.</summary>
public enum KeyChangeKind
{
    /// <summary>The key had never been registered here before.</summary>
    Added = 1,

    /// <summary>A key that had gone obsolete is declared again, keeping the value it had.</summary>
    Reactivated = 2,

    /// <summary>
    /// The key is declared with a different type than before. The new type wins and the value is
    /// cleared, so a value of the wrong type can never be served.
    /// </summary>
    KindChanged = 3,

    /// <summary>The key was not declared any more, so it is kept but marked obsolete.</summary>
    MarkedObsolete = 4,
}