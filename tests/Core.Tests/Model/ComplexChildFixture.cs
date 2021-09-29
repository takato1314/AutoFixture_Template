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

        /// <inheritdoc />
        public ComplexChildFixture(
            IFixture fixture,
            Func<AssignmentOptions<ComplexChild>, AssignmentOptions<ComplexChild>> config) : base(fixture, config)
        {
        }

        public sealed override void Setup()
        {
            // We use mapper to do one-to-one mapping for unit testing.
            // Do use object assignments and set Moq expectations to override their default values.
            var mapper = new Mapper(new MapperConfiguration(m => { m.CreateMap<ComplexChild, ComplexChild>(); }));
            mapper.Map(Instance, Object);
        }

        #region Properties

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
                { nameof(Tests.SimpleChild), SimpleChild }
            }
        };

        #endregion
    }

    public class ComplexChildFixtureTest
    {
        [Theory, AutoMoqData]
        public Task FixtureSetup_TestEquivalency(IFixture fixture)
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
        public Task FixtureSetup_Default_ShouldReturnDefaultSetupValues(IFixture fixture)
        {
            // Arrange
            // Act
            var sut = new ComplexChildFixture(fixture);

            // Assert
            var instances = new List<ComplexChild>{ sut.Object, fixture.Create<ComplexChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().BeEquivalentTo(ComplexChildFixture.Instance);
                instance.Name.Should().NotBeNullOrEmpty();
                instance.Number.Should().Be(5566);
                instance.ConcurrencyStamp.Should().NotBe(default(Guid));
                instance.Nullable.Should().NotBeNull();
                instance.Boolean.Should().BeFalse();
                instance.StringCollection.Should().NotBeNullOrEmpty();
                instance.StringCollection.All(_ => !_.IsNullOrEmpty()).Should().BeTrue();
                instance.DictionaryCollection.Should().NotBeNullOrEmpty();
                instance.DictionaryCollection.Should().HaveCount(1);
                instance.DictionaryCollection[nameof(SimpleChild)].Should().Be(ComplexChildFixture.SimpleChild);
                instance.Function(string.Empty).Should().Be("FixtureSetupFunction");
                instance.Invoking(_ => _.GetValue())
                    .Should()
                    .Throw<NotImplementedException>()
                    .WithMessage("Not implemented on class");
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectItem_ShouldReturnInjectedObject(IFixture fixture)
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
                instance.Should().NotBeEquivalentTo(ComplexChildFixture.Instance);
                instance.Should().BeSameAs(injected.Object);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
                instance.Nullable.Should().BeNull(); // not being setup
                instance.Boolean.Should().BeTrue();
                instance.StringCollection.Should().BeNull(); // not being setup
                instance.DictionaryCollection.Should().BeNull(); // not being setup
                instance.Function(string.Empty).Should().Be("default");
                instance.GetValue().Should().Be("No longer throws exception");
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectOptions_ShouldReturnInjectedValues(IFixture fixture)
        {
            // Arrange
            // Act
            var sut = new ComplexChildFixture(fixture, options => options
                .Setup(_ => _.Name, "OverridenText")
                .Setup(_ => _.Number, 111)
                .Setup(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"))
                .Setup(_ => _.Boolean, true)
                .Setup(_ => _.Function, _ => "default")
            );

            // Assert
            var instances = new List<ComplexChild> { sut.Object, fixture.Create<ComplexChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(ComplexChildFixture.Instance);
                instance.Name.Should().Be("OverridenText");
                instance.Number.Should().Be(111);
                instance.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
                instance.Nullable.Should().NotBeNull();
                instance.Boolean.Should().BeTrue();
                instance.StringCollection.Should().NotBeNullOrEmpty();
                instance.StringCollection.All(_ => !_.IsNullOrEmpty()).Should().BeTrue();
                instance.DictionaryCollection.Should().NotBeNullOrEmpty();
                instance.Function(string.Empty).Should().Be("default");
                //instance.GetValue().Should().Be("No longer throws exception");
            }

            return Task.CompletedTask;
        }
    }
}