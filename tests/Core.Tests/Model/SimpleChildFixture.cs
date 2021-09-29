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

        /// <inheritdoc />
        public SimpleChildFixture(IFixture fixture, SimpleChild item) : base(fixture, item)
        {
        }

        /// <inheritdoc />
        public SimpleChildFixture(
            IFixture fixture,
            Func<AssignmentOptions<SimpleChild>, AssignmentOptions<SimpleChild>> config) : base(fixture, config)
        {
        }
    }

    public class SimpleChildFixtureTest
    {
        [Theory, AutoMoqData]
        public Task FixtureSetup_TestEquivalency(IFixture fixture)
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
        public Task FixtureSetup_Default_ShouldReturnDefaultSetupValues(IFixture fixture)
        {
            // Arrange
            var i0 = new SimpleChild();

            // Act
            var sut = new SimpleChildFixture(fixture);

            // Assert
            var instances = new List<SimpleChild> { sut.Object, fixture.Create<SimpleChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(i0);
                instance.Name.Should().NotBeNullOrEmpty();
                instance.Number.Should().NotBe(default);
                instance.ConcurrencyStamp.Should().NotBe(default(Guid));
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectItem_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var i0 = new SimpleChild();
            var injected = new SimpleChild("OverridenText", 111)
            {
                ConcurrencyStamp = new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889")
            };

            // Act
            var sut = new SimpleChildFixture(fixture, injected);

            // Assert
            var instances = new List<SimpleChild> { sut.Object, fixture.Create<SimpleChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeFalse();
                instance.Should().BeOfType<SimpleChild>();
                instance.Should().NotBeEquivalentTo(i0);
                instance.Should().BeSameAs(injected);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectOptions_ShouldReturnInjectedValues(IFixture fixture)
        {
            // Arrange
            var i0 = new SimpleChild();

            // Act
            var sut = new SimpleChildFixture(fixture, options => options
                .Setup(_ => _.Name, "OverridenText")
                .Setup(_ => _.Number, 111)
                .Setup(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"))
            );

            // Assert
            var instances = new List<SimpleChild> { sut.Object, fixture.Create<SimpleChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(i0);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            }

            return Task.CompletedTask;
        }
    }
}