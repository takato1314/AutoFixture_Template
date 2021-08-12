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

        //protected override ComplexChild CreateObject(IFixture fixture)
        //{
        //    // Mock is useful especially for scenario that uses HttpClients for connection or has heavy operations.
        //    // Otherwise, use actual class whenever possible.
        //    var mock = new Mock<ComplexChild> { CallBase = true, DefaultValue = DefaultValue.Mock };
        //    var hasProperties = mock.Object;

        //    return hasProperties;
        //}
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
            var i3 = fixture.Create<IHasProperties>();

            // Assert
            i1.Should().NotBeNull();
            i0.Should().NotBeEquivalentTo(i1);
            i0.Should().NotBeEquivalentTo(i2);
            i0.Should().NotBeEquivalentTo(i3);
            i1.Should().NotBeEquivalentTo(i3);
            i1.Should().NotBeEquivalentTo(i2);
            i2.Should().NotBeEquivalentTo(i3);

            Mock.Get(i1).Should().NotBeNull();
            Mock.Get(i2).Should().NotBeNull();
            i2.IsMock().Should().BeTrue();
            i3.IsMock().Should().BeTrue();

            return Task.CompletedTask;
        }
        
        [Theory, AutoMoqData]
        public Task GetObject_ShouldReturnDifferentObjects(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexChildFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            Mock.Get(i1).Should().NotBeNull();
            i1.Name.Should().NotBeNullOrEmpty();
            i1.Number.Should().NotBe(default);
            i1.ConcurrencyStamp.Should().NotBe(default);
            i1.Nullable.Should().NotBeNull();
            i1.Boolean.Should().NotBeNull();
            i1.StringCollection.Should().NotBeNullOrEmpty();
            i1.StringCollection.All(_ => !_.IsNullOrEmpty()).Should().BeTrue();

            var hasPropertiesList = new List<IHasProperties>
            {
                fixture.Create<IHasProperties>(),
                fixture.Create<ComplexChild>(),
            };
            foreach (var instance in hasPropertiesList)
            {
                instance.Should().NotBeNull();
                instance.Should().NotBeEquivalentTo(i1);

                instance.Name.Should().NotBe(i1.Name);
                instance.Number.Should().NotBe(i1.Number);
                instance.ConcurrencyStamp.Should().NotBe(i1.ConcurrencyStamp);
                instance.Invoking(_ => _.GetValue())
                    .Should()
                    .Throw<NotImplementedException>()
                    .WithMessage("Not implemented on *");
            }
            
            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<ComplexChild>();
            mock.SetupProperty(_ => _.Name, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            mock.SetupProperty(_ => _.Boolean, true);
            mock.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            var sut = new ComplexChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(fixture, mock.Object);
            var newObject = sut.Object;

            // Assert
            oldObject.Should().NotBeNull();
            oldObject.Invoking(_ => _.GetValue())
                .Should()
                .Throw<NotImplementedException>()
                .WithMessage("Not implemented on class");

            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeEquivalentTo(mock.Object);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            newObject.Boolean.Should().BeTrue();
            newObject.GetValue().Should().Be("No longer throws exception");

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
        public Task TestInject_AnotherWay_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<ComplexChild>();
            mock.SetupProperty(_ => _.Name, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            mock.SetupProperty(_ => _.Boolean, true);
            mock.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            var sut = new ComplexChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, mock.Object);
            var newObject = sut.Object;

            // Assert
            oldObject.Should().NotBeNull();
            oldObject.Invoking(_ => _.GetValue())
                .Should()
                .Throw<NotImplementedException>()
                .WithMessage("Not implemented on class");

            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeEquivalentTo(mock.Object);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            newObject.Boolean.Should().BeTrue();
            newObject.GetValue().Should().Be("No longer throws exception");

            return Task.CompletedTask;
        }

    }
}