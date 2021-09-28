using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
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

        /// <inheritdoc />
        public ComplexParentFixture(IFixture fixture, ComplexParent item) : base(fixture, item)
        {
        }

        public override void Setup()
        {
            // Use mock setup for getter only properties.
            Mock!.Setup(m => m.ComplexChild).Returns(new ComplexChildFixture(Fixture).Object);
            Mock.Setup(m => m.StructChild).Returns(new StructChild("testHost", 80));
        }
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
            i2.IsMockType().Should().BeTrue();
            sut.Mock.Should().BeSameAs(Mock.Get(i1));

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task GetObject_ShouldReturnSameObjects(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexParentFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            i1.IsMockType().Should().BeTrue();
            i1.Name.Should().NotBeNullOrEmpty();
            i1.Number.Should().NotBe(default);
            i1.ConcurrencyStamp.Should().NotBe(default(Guid));
            i1.ComplexChild.Should().NotBe(default);
            i1.SimpleChild.Should().NotBe(default);
            i1.StructChild.Should().NotBe(default);

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
        public Task MultipleFixtures_ShouldReturnSameNestedFixtures(IFixture fixture)
        {
            // Act
            var i1 = new ComplexParentFixture(fixture).Object;
            var i2 = fixture.Create<ComplexParent>();
            var i3 = new ComplexParentFixture(fixture).Object;

            // Assert
            i1.Should().BeSameAs(i2);
            i1.SimpleChild.Should().BeSameAs(i2.SimpleChild);
            i1.ComplexChild.Should().BeSameAs(i2.ComplexChild);
            i1.StructChild.Should().BeEquivalentTo(i2.StructChild);

            i1.Should().BeSameAs(i3);
            i1.SimpleChild.Should().BeSameAs(i3.SimpleChild);
            i1.ComplexChild.Should().BeSameAs(i3.ComplexChild);
            i1.StructChild.Should().BeEquivalentTo(i3.StructChild);

            i2.Should().BeSameAs(i3);
            i2.SimpleChild.Should().BeSameAs(i3.SimpleChild);
            i2.ComplexChild.Should().BeSameAs(i3.ComplexChild);
            i2.StructChild.Should().BeEquivalentTo(i3.StructChild);

            i1.ComplexChild.Should().BeEquivalentTo(ComplexChildFixture.Instance);
            i2.ComplexChild.Should().BeEquivalentTo(ComplexChildFixture.Instance);
            i3.ComplexChild.Should().BeEquivalentTo(ComplexChildFixture.Instance);

            // Should share same references
            i1.SimpleChild.Number = 1234;
            i1.SimpleChild.Number.Should().Be(1234);
            i2.SimpleChild.Number.Should().Be(1234);
            i3.SimpleChild.Number.Should().Be(1234);

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task GetObject_ChildShouldBeSameFixture(IFixture fixture)
        {
            // Arrange
            var structChild = new StructChild("testHost", 80);
            var simpleChild = new SimpleChildFixture(fixture).Object;
            var complexChildFixture = new ComplexChildFixture(fixture);
            var complexChild = complexChildFixture.Object;
            var sut = new ComplexParentFixture(fixture);

            // Act
            var i1 = sut.Object;
            var i2 = fixture.Create<ComplexParent>();
            var i3 = new ComplexParentFixture(fixture).Object;
            var instances = new List<ComplexParent> { i1, i2, i3 };

            // Assert
            simpleChild.Should().NotBeNull();
            complexChild.Should().NotBeNull();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();

                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild); // Same instance for created fixture
                instance.ComplexChild.Should()
                    .BeEquivalentTo(ComplexChildFixture.Instance); // Similar to the setup object. 

                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMockType().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild);

                instance.StructChild.Should().NotBeNull();
                instance.StructChild.Should().BeEquivalentTo(structChild);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task GetObject_DI_ChildShouldBeSameFixture(
            IFixture fixture,
            [Frozen] SimpleChild simpleChild,
            [Frozen] ComplexChild complexChild,
            ComplexParent sut
        )
        {
            // Act
            var i1 = sut;
            var i2 = fixture.Create<ComplexParent>();
            var i3 = new ComplexParentFixture(fixture).Object;
            var instances = new List<ComplexParent> { i1, i2, i3 };

            // Assert
            simpleChild.Should().NotBeNull();
            complexChild.Should().NotBeNull();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();

                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild); // Same instance for created fixture
                instance.ComplexChild.Should()
                    .BeEquivalentTo(ComplexChildFixture.Instance); // Similar to the setup object. 

                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMockType().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild);

                instance.StructChild.Should().NotBeNull();
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_Ext_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var simpleChild = new SimpleChildFixture(fixture).Object;
            var complexChild = new ComplexChildFixture(fixture).Object;
            var injected = new Mock<ComplexParent>(complexChild, simpleChild)
                { CallBase = true, DefaultValue = DefaultValue.Mock };
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            var sut = new ComplexParentFixture(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(injected.Object);

            // Assert
            oldObject.Should().NotBeNull();

            var instances = new List<ComplexParent> { sut.Object, fixture.Create<ComplexParent>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

                // ComplexChild is injected via constructor, should be the same
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                // SimpleChild is injected via constructor, should be the same
                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMockType().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_FixtureInject_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var complexChild = new ComplexChildFixture(fixture).Object;
            var injected = new Mock<ComplexParent>(complexChild) { CallBase = true, DefaultValue = DefaultValue.Mock };
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            var sut = new ComplexParentFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, injected.Object);

            // Assert
            oldObject.Should().NotBeNull();

            var instances = new List<ComplexParent> { sut.Object, fixture.Create<ComplexParent>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

                // ComplexChild is injected via constructor, should be the same
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                // SimpleChild is NOT injected via constructor, should be null
                instance.SimpleChild.Should().BeNull();
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_Ctor_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var complexChild = new ComplexChildFixture(fixture).Object;
            var injected = new Mock<ComplexParent>(complexChild) { CallBase = true, DefaultValue = DefaultValue.Mock };
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));

            // Act
            var sut = new ComplexParentFixture(fixture, injected.Object);

            // Assert
            var instances = new List<ComplexParent> { sut.Object, fixture.Create<ComplexParent>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

                // ComplexChild is injected via constructor, should be the same
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                // SimpleChild is NOT injected via constructor, should be null
                instance.SimpleChild.Should().BeNull();
            }

            return Task.CompletedTask;
        }
    }
}