using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Access.GetAccessList;

/// <summary>Everything that decides who may do what on one node.</summary>
/// <param name="Version">The version of the node's own list; 0 when nothing was ever granted on it.</param>
/// <param name="Own">The permissions given on the node itself — the only ones that can be revoked here.</param>
/// <param name="Inherited">The permissions given above it, from the root down.</param>
public sealed record AccessListView(
    int Version,
    IReadOnlyList<Permission> Own,
    IReadOnlyList<InheritedPermission> Inherited);

/// <summary>A permission given on <see cref="From"/>, which reaches the node because it sits below.</summary>
public sealed record InheritedPermission(NodePath From, Permission Permission);
