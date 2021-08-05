using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        protected override void Register(IFixture fixture)
        {
            // See https://github.com/AutoFixture/AutoFixture/issues/731
            //fixture.Customize<ComplexChild>(
            //    composer =>
            //    {
            //        var postProcess = composer
            //            .Without(_ => _.Nullable);

            //        return postProcess.Do(
            //            item =>
            //            {
            //                item.Nullable = 100;
            //            });
            //    });

            // Override default creation function above for these instances.
            //fixture.Register<IFixture, IHasProperties>(CreateObject);
            //fixture.Register<IFixture, ComplexChild>(CreateObject);
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
        [Theory, AutoDomainData]
        public Task CreateFixtures_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexChildFixture(fixture);

            // Act
            var i1 = sut.Object;
            var i2 = fixture.Create<ComplexChild>();
            var i3 = fixture.Create<IHasProperties>();

            // Assert
            i1.Should().NotBeNull();
            Mock.Get(i1).Should().NotBeNull();
            i1.Should().NotBeEquivalentTo(i2);
            i1.Should().NotBeEquivalentTo(i3);
            i2.Should().NotBeEquivalentTo(i3);

            var actualType = i1.GetType();
            i2.Should().BeOfType(actualType);
            i3.Should().BeOfType(actualType);

            return Task.CompletedTask;
        }
        
        [Theory, AutoDomainData]
        public Task GetObject_ShouldReturnObjects(IFixture fixture)
        {
            // Arrange
            var sut = new ComplexChildFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            Mock.Get(i1).Should().NotBeNull();
            i1.Name.Should().Be("DefaultString");
            i1.Number.Should().Be(default);
            i1.ConcurrencyStamp.Should().Be(default(Guid));
            i1.Nullable.Should().BeNull();

            var hasPropertiesList = new List<IHasProperties>
            {
                fixture.Create<IHasProperties>(),
                fixture.Create<ComplexChild>(),
            };
            foreach (var instance in hasPropertiesList)
            {
                instance.Should().NotBeNull();
                instance.Name.Should().NotBe(i1.Name);
                instance.Number.Should().NotBe(i1.Number);
                instance.ConcurrencyStamp.Should().NotBe(i1.ConcurrencyStamp);
                instance.Invoking(_ => _.GetValue())
                    .Should()
                    .Throw<NotImplementedException>();
            }
            
            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<ComplexChild>();
            mock.SetupProperty(_ => _.Name, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            mock.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            var sut = new ComplexChildFixture(fixture);
            sut.Customize(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(fixture, mock.Object);
            var newObject = sut.Object;

            // Assert
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeEquivalentTo(mock.Object);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            oldObject.Invoking(_ => _.GetValue())
                .Should()
                .Throw<NotImplementedException>()
                .WithMessage("Not implemented on class");
            newObject.GetValue().Should().Be("No longer throws exception");

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_AnotherWay_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<ComplexChild>();
            mock.SetupProperty(_ => _.Name, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            mock.Setup(_ => _.GetValue()).Returns("No longer throws exception");

            var sut = new ComplexChildFixture(fixture);
            fixture.Customize(sut);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, mock.Object);
            var newObject = sut.Object;

            // Assert
            Assert.NotNull(oldObject);
            Assert.NotNull(newObject);
            Assert.NotEqual(oldObject, newObject);
            Assert.Equal<ComplexChild>(mock.Object, newObject);
            Assert.Equal("OverridenText", newObject.Name);
            Assert.Equal<int>(111, newObject.Number);
            Assert.Equal("6f55a677-c447-45f0-8e71-95c7b73fa889", newObject.ConcurrencyStamp.ToString());
            Assert.Throws<NotImplementedException>(() => oldObject.GetValue());
            Assert.Equal("No longer throws exception", newObject.GetValue());

            return Task.CompletedTask;
        }

    }
}