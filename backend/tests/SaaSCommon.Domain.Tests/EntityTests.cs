using FluentAssertions;
using SaaSCommon.Domain;

namespace SaaSCommon.Domain.Tests;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public TestEntity() { }
    }

    private record TestDomainEvent : DomainEvent { }

    [Fact]
    public void Entity_ShouldHaveGeneratedId()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Entity_ShouldHaveEmptyTenantIdByDefault()
    {
        var entity = new TestEntity();

        entity.TenantId.Should().Be(TenantId.Empty);
    }

    [Fact]
    public void Entity_ShouldHaveCreatedAtSet()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entity = new TestEntity();
        var after = DateTime.UtcNow.AddSeconds(1);

        entity.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEvent()
    {
        var entity = new TestEntity();
        var evt = new TestDomainEvent();

        entity.AddDomainEvent(evt);

        entity.DomainEvents.Should().ContainSingle().Which.Should().Be(evt);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());

        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<DomainEvent>>();
    }
}
