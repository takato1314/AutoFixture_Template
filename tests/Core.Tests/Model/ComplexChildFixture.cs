using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Internal;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class ComplexChildFixture : BaseFixtureSetup<ComplexChild>
    {
        /// <inheritdoc />
        public ComplexChildFixture(IFixture fixture) : base(fixture)
        {
        }

        /// <inheritdoc />
        public ComplexChildFixture(IFixture fixture, ComplexChild item) : base(fixture, item)
        {
        }

        public override void Setup()
        {
            // We use mapper to do one-to-one mapping for unit testing.
            // Do use object assignments and set Moq expectations to override their default values.
            var mapper = new Mapper(new MapperConfiguration(m =>
            {
                m.CreateMap<ComplexChild, ComplexChild>();
            }));
            mapper.Map(Instance, Object);

            // Use assignments or mock setups as follows
            Object.Number = Number;
        }

        #region Properties

        // Additional properties are introduced to allow direct configurations on Mock/Object values for different test cases.
        // Eg. Fixture for HttpRequest may have same 'User' and 'Host' values, but may need to return different 'Content' and 'StatusCode'.
        public int Number { get; set; } = 5566;

        internal static readonly SimpleChild SimpleChild = new(nameof(Tests.SimpleChild), 100);

        internal static readonly ComplexChild Instance = new()
        {
            Name = "FixtureSetup",
            Number = 5566,
            Nullable = 5566,
            ConcurrencyStamp = new Guid("ac8fd90c-f84e-45ff-88b7-0a971db1ddff"),
            Boolean = false,
            Function = _ => "FixtureSetupFunction",
            StringCollection = new List<string>
            {
                "FixtureSetupStringCollection"
            },
            DictionaryCollection = new Dictionary<string, SimpleChild>
            {
                {nameof(Tests.SimpleChild), SimpleChild}
            }
        };

        #endregion
    }


    public class ComplexChildFixtureTest
    {
        [Theory, AutoMoqData]
        public Task CreateFixtures_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexChildFixture(fixture);

            // Act
            var i0 = new ComplexChild();
            var i1 = sut.Object;
            var i2 = fixture.Create<ComplexChild>();

            // Assert
            // Should not be equivalent, because fixture will use a different instance
            i1.Should().NotBeNull();
            i1.Should().NotBeEquivalentTo(i0);
            i2.Should().NotBeEquivalentTo(i0);

            // Should be same, because the fixture share the same instance.
            i1.Should().BeSameAs(i2);
            i1.Should().BeEquivalentTo(ComplexChildFixture.Instance);

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
            var sut = new ComplexChildFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            i1.IsMockType().Should().BeTrue();
            i1.Name.Should().NotBeNullOrEmpty();
            i1.Number.Should().Be(5566);
            i1.ConcurrencyStamp.Should().NotBe(default(Guid));
            i1.Nullable.Should().NotBeNull();
            i1.Boolean.Should().BeFalse();
            i1.StringCollection.Should().NotBeNullOrEmpty();
            i1.StringCollection.All(_ => !_.IsNullOrEmpty()).Should().BeTrue();
            i1.DictionaryCollection.Should().NotBeNullOrEmpty();
            i1.DictionaryCollection.Should().HaveCount(1);
            i1.DictionaryCollection[nameof(SimpleChild)].Should().Be(ComplexChildFixture.SimpleChild);

            // All instances should be same as fixture
            var instances = fixture.CreateMany<ComplexChild>();
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.Should().BeSameAs(i1);
                instance.Invoking(_ => _.GetValue())
                    .Should()
                    .Throw<NotImplementedException>()
                    .WithMessage("Not implemented on class");
            }
            
            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_Ext_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new Mock<ComplexChild>();
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            injected.SetupProperty(_ => _.Boolean, true);
            injected.SetupProperty(_ => _.Function, _ => "default");
            injected.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            var sut = new ComplexChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(injected.Object);

            // Assert
            oldObject.Should().NotBeNull();
            oldObject.Invoking(_ => _.GetValue())
                .Should()
                .Throw<NotImplementedException>()
                .WithMessage("Not implemented on class");

            var instances = new List<ComplexChild> { sut.Object, fixture.Create<ComplexChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
                instance.Boolean.Should().BeTrue();
                instance.Function(string.Empty).Should().Be("default");
                instance.GetValue().Should().Be("No longer throws exception");
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_FixtureInject_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new Mock<ComplexChild>();
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            injected.SetupProperty(_ => _.Boolean, true);
            injected.SetupProperty(_ => _.Function, _ => "default");
            injected.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            var sut = new ComplexChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, injected.Object);

            // Assert
            oldObject.Should().NotBeNull();
            oldObject.Invoking(_ => _.GetValue())
                .Should()
                .Throw<NotImplementedException>()
                .WithMessage("Not implemented on class");

            var instances = new List<ComplexChild> { sut.Object, fixture.Create<ComplexChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(oldObject);
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
                instance.Boolean.Should().BeTrue();
                instance.Function(string.Empty).Should().Be("default");
                instance.GetValue().Should().Be("No longer throws exception");
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_Ctor_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new Mock<ComplexChild>();
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            injected.SetupProperty(_ => _.Boolean, true);
            injected.SetupProperty(_ => _.Function, _ => "default");
            injected.Setup(_ => _.GetValue()).Returns("No longer throws exception");

            // Act
            var sut = new ComplexChildFixture(fixture, injected.Object);

            // Assert
            var instances = new List<ComplexChild> { sut.Object, fixture.Create<ComplexChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
                instance.Boolean.Should().BeTrue();
                instance.Function(string.Empty).Should().Be("default");
                instance.GetValue().Should().Be("No longer throws exception");
            }

            return Task.CompletedTask;
        }

    }
}