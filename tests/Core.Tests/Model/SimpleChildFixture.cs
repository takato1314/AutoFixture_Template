using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class SimpleChildFixture : BaseFixtureSetup<SimpleChild>
    {
        /// <inheritdoc />
        public SimpleChildFixture(IFixture fixture) : base(fixture)
        {
        }
    }


    public class SimpleChildFixtureTest
    {
        [Theory, AutoMoqData]
        public Task CreateFixtures_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var sut = new SimpleChildFixture(fixture);

            // Act
            var i0 = new SimpleChild();
            var i1 = sut.Object;
            var i2 = fixture.Create<SimpleChild>();

            // Assert
            // Should not be equivalent, because fixture will use a different instance
            i1.Should().NotBeNull();
            i1.Should().NotBeEquivalentTo(i0);
            i2.Should().NotBeEquivalentTo(i0);
            
            // Should be same, because the fixture share the same instance.
            i1.Should().BeSameAs(i2);

            // Should be a mock
            Mock.Get(i1).Should().NotBeNull();
            Mock.Get(i2).Should().NotBeNull();
            i2.IsMockType().Should().BeTrue();
            sut.Mock.Should().BeSameAs(Mock.Get(i1));

            return Task.CompletedTask;
        }
        
        [Theory, AutoMoqData]
        public Task GetObject_ShouldReturnSameObjects(IFixture fixture)
        {
            // Arrange
            var sut = new SimpleChildFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            i1.IsMockType().Should().BeTrue();
            i1.Name.Should().NotBeNullOrEmpty();
            i1.Number.Should().NotBe(default);
            i1.ConcurrencyStamp.Should().NotBe(default(Guid));

            // All instances should be same as fixture
            var instances = fixture.CreateMany<SimpleChild>();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.Should().BeSameAs(i1);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new SimpleChild("OverridenText", 111)
            {
                ConcurrencyStamp = new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889")
            };
            var sut = new SimpleChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(injected);

            // Assert
            oldObject.Should().NotBeNull();

            var instances = new List<SimpleChild> {sut.Object, fixture.Create<SimpleChild>()};
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeFalse();
                instance.Should().BeOfType<SimpleChild>();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(injected);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_AnotherWay_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new SimpleChild("OverridenText", 111)
            {
                ConcurrencyStamp = new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889")
            };
            var sut = new SimpleChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, injected);

            // Assert
            oldObject.Should().NotBeNull();

            var instances = new List<SimpleChild> { sut.Object, fixture.Create<SimpleChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeFalse();
                instance.Should().BeOfType<SimpleChild>();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(injected);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            }

            return Task.CompletedTask;
        }
    }
}