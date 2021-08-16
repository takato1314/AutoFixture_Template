using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class ComplexParentFixture : BaseFixtureSetup<ComplexParent>
    {
        /// <inheritdoc />
        public ComplexParentFixture(IFixture fixture) : base(fixture)
        {
        }

        //protected override ComplexParent CreateObject(IFixture fixture)
        //{
        //    // Mock is useful especially for scenario that uses HttpClients for connection or has heavy operations.
        //    // Otherwise, use actual class whenever possible.
        //    var mock = new Mock<ComplexParent> { CallBase = true, DefaultValue = DefaultValue.Mock };
        //    var hasProperties = mock.Object;

        //    return hasProperties;
        //}
    }


    public class ComplexParentFixtureTest
    {
        [Theory, AutoMoqData]
        public Task CreateFixtures_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var complexChild = new ComplexChildFixture(fixture).Object;
            var sut = new ComplexParentFixture(fixture);

            // Act
            var i0 = new ComplexParent(complexChild);
            var i1 = sut.Object;
            var i2 = fixture.Create<ComplexParent>();

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
            i2.IsMock().Should().BeTrue();
            sut.Mock.Should().BeSameAs(Mock.Get(i1));

            return Task.CompletedTask;
        }
        
        [Theory, AutoMoqData]
        public Task GetObject_ShouldReturnDifferentObjects(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexParentFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            i1.IsMock().Should().BeTrue();
            i1.Name.Should().NotBeNullOrEmpty();
            i1.Number.Should().NotBe(default);
            i1.ConcurrencyStamp.Should().NotBe(default);
            i1.ComplexChild.Should().NotBe(default);
            i1.SimpleChild.Should().NotBe(default);

            // All instances should be same as fixture
            var instances = fixture.CreateMany<ComplexParent>();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.Should().BeSameAs(i1);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task GetObject_ChildShouldBeSameFixture(IFixture fixture)
        {
            // Arrange
            var simpleChild = new SimpleChildFixture(fixture).Object;
            var complexChild = new ComplexChildFixture(fixture).Object;
            var sut = new ComplexParentFixture(fixture);

            // Act
            var i1 = sut.Object;
            var i2 = fixture.Create<ComplexParent>();
            var instances = new List<ComplexParent> {i1, i2};

            // Assert
            complexChild.Should().NotBeNull();
            foreach (var instance in instances)
            {
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMock().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMock().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var simpleChild = new SimpleChildFixture(fixture).Object;
            var complexChild = new ComplexChildFixture(fixture).Object;
            var mock = new Mock<ComplexParent>(complexChild, simpleChild) {CallBase = true, DefaultValue = DefaultValue.Mock};
            mock.SetupProperty(_ => _.Name, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            var sut = new ComplexParentFixture(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(mock.Object);

            // Assert
            oldObject.Should().NotBeNull();
            
            var instances = new List<ComplexParent> { sut.Object, fixture.Create<ComplexParent>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMock().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(mock.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

                // ComplexChild is injected via constructor, should be the same
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMock().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                // SimpleChild is injected via constructor, should be the same
                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMock().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_AnotherWay_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var complexChild = new ComplexChildFixture(fixture).Object;
            var mock = new Mock<ComplexParent>(complexChild) { CallBase = true, DefaultValue = DefaultValue.Mock };
            mock.SetupProperty(_ => _.Name, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            var sut = new ComplexParentFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, mock.Object);

            // Assert
            oldObject.Should().NotBeNull();

            var instances = new List<ComplexParent> { sut.Object, fixture.Create<ComplexParent>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMock().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(mock.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

                // ComplexChild is injected via constructor, should be the same
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMock().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                // SimpleChild is NOT injected via constructor, should be null
                instance.SimpleChild.Should().BeNull();
            }

            return Task.CompletedTask;
        }

    }
}