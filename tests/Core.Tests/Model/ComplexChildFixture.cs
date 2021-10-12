using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Func<FixtureSetupOptions<ComplexChild>, FixtureSetupOptions<ComplexChild>> options) : base(fixture, options)
        {
        }
        
        #region Properties
        
        /// <summary>
        /// Additional properties are introduced to allow direct configurations on Mock/Object values for different test cases. <br/>
        /// <b>E.g.</b> Fixture for HttpRequest may have same 'User' and 'Host' values, but may need to return different 'Content' and 'StatusCode'
        /// on different use cases.
        /// </summary>
        public int MyNumber { get; set; }
        
        // For unit test comparisons
        internal static readonly SimpleChild SimpleChild = new(nameof(Tests.SimpleChild), 100);
        internal static readonly ComplexChild Instance = new()
        {
            Name = "FixtureSetup",
            Number = 5566,
            Nullable = 5566,
            ConcurrencyStamp = new Guid("ac8fd90c-f84e-45ff-88b7-0a971db1ddff"),
            Boolean = false,
            Function = _ => "FixtureSetupFunction",
            DictionaryCollection = new Dictionary<string, SimpleChild>
            {
                { nameof(Tests.SimpleChild), SimpleChild }
            }
        };

        protected override Func<FixtureSetupOptions<ComplexChild>, FixtureSetupOptions<ComplexChild>> Setups =>
            options => options
                .Setup(_ => _.Name, Instance.Name)
                .Setup(_ => _.Number, MyNumber)
                .Setup(_ => _.Nullable, Instance.Nullable)
                .Setup(_ => _.ConcurrencyStamp, Instance.ConcurrencyStamp)
                .Setup(_ => _.Boolean, Instance.Boolean)
                .Setup(_ => _.Function, Instance.Function)
                .Setup(_ => _.DictionaryCollection, Instance.DictionaryCollection);

        #endregion
    }

    public class ComplexChildFixtureTest
    {
        [Theory, AutoMoqData(false)]
        public Task FixtureSetup_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexChildFixture(fixture)
            {
                MyNumber = 5566
            };

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
            i1.StringCollection.Should().BeEmpty();

            // Should be a mock
            Mock.Get(i1).Should().NotBeNull();
            Mock.Get(i2).Should().NotBeNull();
            i1.IsMockType().Should().BeTrue();
            sut.Mock.Should().BeSameAs(Mock.Get(i1));

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_Default_ShouldReturnDefaultSetupValues(IFixture fixture)
        {
            // Arrange
            // Act
            var sut = new ComplexChildFixture(fixture)
            {
                MyNumber = 5566
            };

            // Assert
            var instances = new List<ComplexChild>{ sut.Object, fixture.Create<ComplexChild>() };
            foreach (var instance in instances)
            {
                instance.Should().NotBeNull();
                instance.IsMockType().Should().BeTrue();
                instance.Should().NotBeEquivalentTo(ComplexChildFixture.Instance);
                instance.Name.Should().NotBeNullOrEmpty();
                instance.Number.Should().Be(5566);
                instance.ConcurrencyStamp.Should().NotBe(default(Guid));
                instance.Nullable.Should().NotBeNull();
                instance.Boolean.Should().BeFalse();
                instance.StringCollection.Should().NotBeNullOrEmpty();
                instance.DictionaryCollection.Should().NotBeNullOrEmpty();
                instance.DictionaryCollection.Should().HaveCount(1);
                instance.DictionaryCollection[nameof(SimpleChild)].Should().Be(ComplexChildFixture.SimpleChild);
                instance.Function(string.Empty).Should().Be("FixtureSetupFunction");
                instance.Invoking(_ => _.ReturnMethod())
                    .Should()
                    .Throw<NotImplementedException>()
                    .WithMessage("Not implemented on class");
                instance.Invoking(_ => _.VoidMethod())
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
            var hasCalledVoidMethod = false;
            var injected = new Mock<ComplexChild>();
            injected.SetupProperty(_ => _.Name, "OverridenText");
            injected.SetupProperty(_ => _.Number, 111);
            injected.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            injected.SetupProperty(_ => _.Boolean, true);
            injected.SetupProperty(_ => _.Function, _ => "default");
            injected.Setup(_ => _.ReturnMethod()).Returns("No longer throws exception");
            injected.Setup(_ => _.VoidMethod()).Callback(() => { hasCalledVoidMethod = true; });

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
                instance.ReturnMethod().Should().Be("No longer throws exception");

                // Ensure void method is called and reset the callback value for next iteration.
                instance.Invoking(_ => _.VoidMethod()).Should().NotThrow();
                hasCalledVoidMethod.Should().BeTrue();
                hasCalledVoidMethod = false;
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task FixtureSetup_InjectOptions_ShouldReturnInjectedValues(IFixture fixture)
        {
            // Arrange
            var hasCalledVoidMethod = false;

            // Act
            var sut = new ComplexChildFixture(fixture, options => options
                .Setup(_ => _.Name, "OverridenText")
                .Setup(_ => _.Number, 111)
                .Setup(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"))
                .Setup(_ => _.Boolean, true)
                .Setup(_ => _.Function, _ => "default")
                .Setup(_ => _.ReturnMethod(), "No longer throws exception")
                .Setup(_ => _.VoidMethod(), () => { hasCalledVoidMethod = true; })
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
                instance.ReturnMethod().Should().Be("No longer throws exception");

                // Ensure void method is called and reset the callback value for next iteration.
                instance.Invoking(_ => _.VoidMethod()).Should().NotThrow();
                hasCalledVoidMethod.Should().BeTrue();
                hasCalledVoidMethod = false;
            }

            return Task.CompletedTask;
        }
    }
}