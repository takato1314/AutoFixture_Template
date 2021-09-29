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

        /// <inheritdoc />
        public ComplexParentFixture(
            IFixture fixture,
            Func<FixtureSetupOptions<ComplexParent>, FixtureSetupOptions<ComplexParent>> config) : base(fixture, config)
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
        public Task FixtureSetup_TestEquivalency(IFixture fixture)
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
        public Task FixtureSetup_Default_ShouldReturnDefaultSetupValues(IFixture fixture)
        {
            // Arrange
            var structChild = new StructChild("testHost", 80);
            var simpleChild = new SimpleChildFixture(fixture).Object;
            var complexChildFixture = new ComplexChildFixture(fixture);
            var complexChild = complexChildFixture.Object;
            var sut = new ComplexParentFixture(fixture);

            // Act
            // Note: All instances here should be same,
            // because we override with our own fixture
            var instances = new List<ComplexParent>
            {
                sut.Object, 
                fixture.Create<ComplexParent>(), 
                new ComplexParentFixture(fixture).Object
            };

            // Assert
            simpleChild.Should().NotBeNull();
            complexChild.Should().NotBeNull();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Name.Should().NotBeNullOrEmpty();
                instance.Number.Should().BeGreaterThan(0);
                instance.ConcurrencyStamp.ToString().Should().NotBeNullOrEmpty();

                // ComplexChild is injected
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild); // Same instance for created fixture
                instance.ComplexChild.Should().BeEquivalentTo(ComplexChildFixture.Instance); // Similar to the setup object. 

                // SimpleChild is injected
                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMockType().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild);

                // StructChild is injected
                instance.StructChild.Should().NotBeNull();
                instance.StructChild.Should().BeEquivalentTo(structChild);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_DI_ShouldReturnDifferentSetupValues(
            IFixture fixture,
            [Frozen] SimpleChild simpleChild,
            [Frozen] ComplexChild complexChild,
            [Frozen] StructChild structChild,
            ComplexParent sut
        )
        {
            // Act
            // Note: All instances here should be different,
            // because AutoFixture will create an instance before entering this function
            var instances = new List<ComplexParent>
            {
                sut,
                fixture.Create<ComplexParent>(),
                new ComplexParentFixture(fixture).Object
            };

            // Assert
            simpleChild.Should().NotBeNull();
            complexChild.Should().NotBeNull();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Name.Should().NotBeNullOrEmpty();
                instance.Number.Should().BeGreaterThan(0);
                instance.ConcurrencyStamp.ToString().Should().NotBeNullOrEmpty();

                // ComplexChild is injected
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild); // Same instance for created fixture
                instance.ComplexChild.Should().BeEquivalentTo(ComplexChildFixture.Instance); // Equivalent to the setup object. 

                // SimpleChild is injected
                instance.SimpleChild.Should().NotBeNull();
                instance.SimpleChild!.IsMockType().Should().BeTrue();
                instance.SimpleChild.Should().BeSameAs(simpleChild); // Same instance for created fixture

                // StructChild has its own setup in fixture
                instance.StructChild.Should().NotBeNull();
                instance.StructChild.Should().NotBeSameAs(structChild);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectItem_ShouldReturnInjectedObject(IFixture fixture)
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

                // SimpleChild is 'null' in injected, should be 'null'
                instance.SimpleChild.Should().BeNull();

                // StructChild is 'default' in injected, should be 'default'
                instance.StructChild.Should().NotBeNull();
                instance.StructChild.Should().Be(default(StructChild));
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectOptions_ShouldReturnInjectedValues(IFixture fixture)
        {
            // Arrange
            var complexChild = new ComplexChildFixture(fixture).Object;
            
            // Act
            var sut = new ComplexParentFixture(fixture, options => options
                //.Setup(_ => _.ComplexChild, complexChild)
                .Setup(_ => _.Name, "OverridenText")
                .Setup(_ => _.Number, 111)
                .Setup(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"))
            );

            // Assert
            var instances = new List<ComplexParent> { sut.Object, fixture.Create<ComplexParent>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

                // ComplexChild is injected via constructor, should be the same
                instance.ComplexChild.Should().NotBeNull();
                instance.ComplexChild.IsMockType().Should().BeTrue();
                instance.ComplexChild.Should().BeSameAs(complexChild);

                // SimpleChild is auto generated by AutoFixture
                instance.SimpleChild.Should().NotBeNull();

                // StructChild is auto generated by AutoFixture
                instance.StructChild.Should().NotBeNull();
            }

            return Task.CompletedTask;
        }
    }
}