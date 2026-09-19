namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>Whether a registered key is still declared by a client application.</summary>
public enum KeyStatus
{
    /// <summary>Declared by the most recent registration.</summary>
    Active = 1,

    /// <summary>
    /// No longer declared. The key and its value are kept — an older deployment still running may well
    /// be reading it — but it cannot be changed until a registration brings it back.
    /// </summary>
    Obsolete = 2,
}
