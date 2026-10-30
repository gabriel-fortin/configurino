using org.g14.Configurino.Application.Config.RegisterKeys;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tests.Config;

public class RegisterKeysTests
{
    private static RegisterKeysCommand Declare(params KeyRegistration[] keys) =>
        new(World.Billing, keys, World.Client);

    [Fact]
    public async Task Registering_keys_logs_the_registration()
    {
        var world = await World.WithBillingNode();

        await world.RegisterKeys.Handle(Declare(World.Declare("timeout", ValueKind.Integer)));

        Assert.Single(world.Store.AllChanges);
    }

    [Fact]
    public async Task Registering_the_same_keys_again_saves_nothing()
    {
        var world = await World.WithBillingNode();
        var command = Declare(World.Declare("timeout", ValueKind.Integer));
        var version = await world.RegisterKeys.Handle(command);
        var savesBefore = world.Store.SaveCalls;

        var versionAgain = await world.RegisterKeys.Handle(command);

        Assert.Equal(version, versionAgain);
        Assert.Equal(savesBefore, world.Store.SaveCalls);
    }

    [Fact]
    public async Task A_conflict_is_retried_from_a_fresh_load_and_the_registration_goes_through()
    {
        var world = await World.WithBillingNode();
        var repository = new FreshNodeRepository(conflicts: RegisterKeysHandler.MaxAttempts - 1);

        await HandlerOver(world, repository).Handle(Declare(World.Declare("timeout", ValueKind.Integer)));

        Assert.Equal(1, repository.SavedCount);
    }

    [Fact]
    public async Task Conflicts_beyond_the_attempt_limit_are_reported()
    {
        var world = await World.WithBillingNode();
        var repository = new FreshNodeRepository(conflicts: RegisterKeysHandler.MaxAttempts);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            HandlerOver(world, repository).Handle(Declare(World.Declare("timeout", ValueKind.Integer))));

        Assert.Equal(0, repository.SavedCount);
    }

    [Fact]
    public async Task Registering_on_a_config_node_that_does_not_exist_fails_instead_of_creating_it()
    {
        var world = await World.WithBillingNode();

        await Assert.ThrowsAsync<ConfigNodeNotFoundException>(() =>
            world.RegisterKeys.Handle(
                new RegisterKeysCommand(
                    World.Path("/prod/ghost"),
                    [World.Declare("timeout", ValueKind.Integer)],
                    World.Client)));
    }

    private static RegisterKeysHandler HandlerOver(World world, IConfigNodeRepository repository) =>
        new(new NodeLocator(world.Store, world.Store, repository), repository, world.Time);

    /// <summary>
    /// Hands out a node freshly loaded on every call, as a real database would, and throws a conflict on
    /// the first few saves.
    /// </summary>
    private sealed class FreshNodeRepository(int conflicts) : IConfigNodeRepository
    {
        private int _remainingConflicts = conflicts;

        public int SavedCount { get; private set; }

        public Task<ConfigNode?> Find(NodeId id, CancellationToken cancellationToken = default) =>
            Task.FromResult<ConfigNode?>(
                GroupingNode.CreateRoot(World.Human, World.Start)
                    .CreateChildConfig(World.Name("billing"), World.Human, World.Start));

        public Task Add(ConfigNode node, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Save(ConfigNode node, CancellationToken cancellationToken = default)
        {
            if (_remainingConflicts > 0)
            {
                _remainingConflicts--;

                throw new ConcurrencyConflictException(node.Id, node.Version - 1, node.Version);
            }

            SavedCount++;

            return Task.CompletedTask;
        }
    }
}
