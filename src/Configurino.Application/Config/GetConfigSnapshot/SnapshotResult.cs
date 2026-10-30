using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Application.Config.GetConfigSnapshot;

/// <summary>Either the snapshot, or the news that the caller's copy is still current.</summary>
public abstract record SnapshotResult
{
    private SnapshotResult()
    {
    }

    /// <summary>The configuration is newer than the caller's version, or the caller had none.</summary>
    public sealed record Changed(ConfigSnapshot Snapshot) : SnapshotResult;

    /// <summary>Nothing has changed since <see cref="Version"/>.</summary>
    public sealed record NotModified(int Version) : SnapshotResult;
}
