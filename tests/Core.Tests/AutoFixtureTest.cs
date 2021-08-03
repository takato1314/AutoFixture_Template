using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using Xunit;
// ReSharper disable PossibleMultipleEnumeration

#pragma warning disable 618

namespace AutoFixture.Extensions.Tests
{
    /// <summary>
    /// This test is used to features for AutoFixture is working as expected, for sanity checking purpose.
    /// See https://blog.ploeh.dk/2013/04/08/how-to-automatically-populate-properties-with-automoq/
    /// </summary>
    public class AutoFixtureTest
    {
        [Fact]
        public void GetFixture_SingleInstance_ShouldReturnSameInstance()
        {
            // Should be same instance only when accessing its Factory Getter
            var i1 = FixtureFactory.Instance;
            var i2 = FixtureFactory.GetFixture();

            i1.Should().BeSameAs(i2);
        }

        [Theory, AutoDomainData]
        public void GetFixture_MultipleInstances_ShouldReturnDifferentInstances(IFixture fixture)
        {
            // Different instances on other accessors
            // See https://github.com/AutoFixture/AutoFixture/issues/1064#issuecomment-409619359
            var i1 = new AutoFixture.Fixture();
            var i2 = FixtureFactory.Instance;
            var i3 = FixtureFactory.CreateFixture();
            var i4 = new AutoDomainDataAttribute().Fixture;

            fixture.Should().NotBeSameAs(i1);
            fixture.Should().NotBeSameAs(i2);
            fixture.Should().NotBeSameAs(i3);
            fixture.Should().NotBeSameAs(i4);
        }
        
        [Fact]
        public void CustomizeFixture_WithNull_ShouldThrowNullArgumentException()
        {
            var sut = new AutoPopulatedMoqPropertiesCustomization();
            sut.Invoking(_ => _.Customize(null!))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Theory, AutoDomainData]
        public void CustomizeFixture_ShouldHaveMoreCustomizationsCount(IFixture fixture)
        {
            var before = fixture.Customizations.Count;
            fixture.Customize(new HasPropertiesComplexFixture(fixture));
            var after = fixture.Customizations.Count;

            after.Should().BeGreaterThan(before);
        }

        [Theory, AutoDomainData]
        public void Create_OnMockedInterfaceType_ShouldReturnMockType(IFixture fixture)
        {
            // AutoMock should mock and return mock types for interfaces and abstract classes by default
            // See https://blog.ploeh.dk/2010/08/25/ChangingthebehaviorofAutoFixtureauto-mockingwithMoq/
            var i1 = fixture.Create<IHasProperties>();

            i1.Text.Should().NotBe("foo");
            i1.Number.Should().NotBe(42);
            i1.GetValue().Should().NotBe(i1.Text);

            var mock = Mock.Get(i1);
            mock.SetupAllProperties();
            mock.Setup(_ => _.GetValue()).Returns("SomeValue");
            i1.Text = "foo";
            i1.Number = 42;

            mock.Should().NotBeNull();
            i1.Text.Should().Be("foo");
            i1.Number.Should().Be(42);
            i1.GetValue().Should().Be("SomeValue");
        }

        [Theory, AutoDomainData]
        public void Create_OnMockedInterfaceType_ShouldNotBeNull(IFixture fixture)
        {
            var i1 = fixture.Create<IHasProperties>();

            Mock.Get(i1).Should().NotBeNull();
            i1.Text.Should().NotBe(default);
            i1.Number.Should().NotBe(default);
        }

        [Theory, AutoDomainData]
        public void Create_OnMockedConcreteType_ShouldNotBeNull(IFixture fixture)
        {
            var i1 = fixture.Create<HasPropertiesSimple>();

            // Concrete type should return concrete type
            Mock.Get(i1).Should().NotBeNull();
            i1.Text.Should().NotBe(default);
            i1.Number.Should().NotBe(default);
        }

        [Theory, AutoDomainData]
        public void Freeze_OnInterfaceTypeEnumerable_ShouldReturnSameInstances(IFixture fixture)
        {
            // AutoMock can control specific items generated in a list
            // See https://blog.ploeh.dk/2011/02/07/CreatingspecificpopulatedlistswithAutoFixture/
            var i0 = fixture.Freeze<IHasProperties>();
            i0.Number = 42;
            i0.Text = "RandomText42";
            Mock.Get(i0).Object.Should().Be(i0);

            // Create should generate same instance
            var i1 = fixture.Create<IHasProperties>();
            i1.Should().BeSameAs(i0);

            // CreateMany should generate same instances
            var i2 = fixture.CreateMany<IHasProperties>();
            foreach (var i in i2)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IEnumerable<T>> should generate same instances
            var i3 = fixture.Create<IEnumerable<IHasProperties>>();
            foreach (var i in i3)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IList<T>> should generate same instances
            var i4 = fixture.Create<IList<IHasProperties>>();
            foreach (var i in i4)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IList<T>> should generate same instances
            var i5 = fixture.Create<List<IHasProperties>>();
            foreach (var i in i5)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<ICollection<T>> should generate same instances
            var i6 = fixture.Create<ICollection<IHasProperties>>();
            foreach (var i in i6)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<Collection<T>> should generate same instances
            var i7 = fixture.Create<Collection<IHasProperties>>();
            foreach (var i in i7)
            {
                i.Should().BeSameAs(i0);
            }
            
            i0.Number.Should().Be(42);
            i0.Text.Should().Be("RandomText42");
            i7[0].Number.Should().Be(42);
            i7[0].Text.Should().Be("RandomText42");
        }

        [Theory, AutoDomainData]
        public void Freeze_OnConcreteTypeEnumerable_ShouldReturnSameInstances(IFixture fixture)
        {
            // AutoMock can control specific items generated in a list
            // See https://blog.ploeh.dk/2011/02/07/CreatingspecificpopulatedlistswithAutoFixture/
            var i0 = fixture.Freeze<HasPropertiesSimple>();
            i0.Number = 42;
            i0.Text = "RandomText42";
            Mock.Get(i0).Object.Should().Be(i0);

            // Create should generate same instance
            var i1 = fixture.Create<HasPropertiesSimple>();
            i1.Should().BeSameAs(i0);

            // CreateMany should generate same instances
            var i2 = fixture.CreateMany<HasPropertiesSimple>();
            foreach (var i in i2)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IEnumerable<T>> should generate same instances
            var i3 = fixture.Create<IEnumerable<HasPropertiesSimple>>();
            foreach (var i in i3)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IList<T>> should generate same instances
            var i4 = fixture.Create<IList<HasPropertiesSimple>>();
            foreach (var i in i4)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IList<T>> should generate same instances
            var i5 = fixture.Create<List<HasPropertiesSimple>>();
            foreach (var i in i5)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<ICollection<T>> should generate same instances
            var i6 = fixture.Create<ICollection<HasPropertiesSimple>>();
            foreach (var i in i6)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<Collection<T>> should generate same instances
            var i7 = fixture.Create<Collection<HasPropertiesSimple>>();
            foreach (var i in i7)
            {
                i.Should().BeSameAs(i0);
            }

            i0.Number.Should().Be(42);
            i0.Text.Should().Be("RandomText42");
            i7[0].Number.Should().Be(42);
            i7[0].Text.Should().Be("RandomText42");
        }

        [Theory, AutoDomainData]
        public void Inject_SameFixture_ShouldReturnSameInstances(IFixture fixture)
        {
            var original = new HasPropertiesComplex();
            fixture.Inject(original);

            var i1 = fixture.Create<HasPropertiesComplex>();
            var i2 = fixture.Create<HasPropertiesComplex>();

            // i1 and i2 are equal, and equal to original
            i1.Should().BeSameAs(i2);
            i1.Should().BeSameAs(original);
            i2.Should().BeSameAs(original);
        }

        [Fact]
        public void Inject_DifferentFixtures_ShouldReturnSameInstances()
        {
            var fixture1 = FixtureFactory.CreateFixture();
            var fixture2 = FixtureFactory.Instance;
            var original = new HasPropertiesComplex();
            fixture1.Inject(original);
            fixture2.Inject(original);

            var i1 = fixture1.Create<HasPropertiesComplex>();
            var i2 = fixture1.Create<HasPropertiesComplex>();
            var j1 = fixture2.Create<HasPropertiesComplex>();
            var j2 = fixture2.Create<HasPropertiesComplex>();

            // i1 and i2 are equal, and equal to original
            i1.Should().BeSameAs(i2);
            i1.Should().BeSameAs(original);
            i2.Should().BeSameAs(original);
            j1.Should().BeSameAs(j2);
            j1.Should().BeSameAs(original);
            j2.Should().BeSameAs(original);
        }

        [Theory, AutoDomainData]
        public void Inject_OnMockedConcreteType_ShouldNotBeNull(IFixture fixture)
        {
            fixture.Inject(Mock.Of<HasPropertiesComplex>());
            var i1 = fixture.Create<HasPropertiesComplex>();

            // Mock type should return mock type with default values
            Mock.Get(i1).Should().NotBeNull();
            i1.Text.Should().Be(default);
            i1.Number.Should().Be(0);
        }

        [Theory, AutoDomainData]
        public void Inject_OnMockedConcreteType_ShouldReturnMockType(IFixture fixture)
        {
            // AutoMock can mock concrete types if you explicitly inject the mock of concrete types
            // See https://github.com/AutoFixture/AutoFixture/issues/1078
            var mockObj = new Mock<HasPropertiesComplex>().Object;
            fixture.Inject(mockObj);

            var i1 = fixture.Create<HasPropertiesComplex>();
            i1.Text.Should().Be(mockObj.Text);
            i1.Number.Should().Be(mockObj.Number);
            i1.GetValue().Should().Be(mockObj.GetValue());

            var mock = Mock.Get(i1);
            mock.SetupAllProperties();
            mock.Setup(_ => _.GetValue()).Returns("SomeValue");
            i1.Text = "foo";
            i1.Number = 42;

            i1.Text.Should().Be("foo");
            i1.Number.Should().Be(42);
            i1.GetValue().Should().Be("SomeValue");

            var i2 = fixture.Freeze<HasPropertiesComplex>();
            i2.Should().BeSameAs(i1);
        }

        [Theory, AutoDomainData]
        public void Inject_OnConcreteTypeEnumerable_ShouldReturnSameInstances(IFixture fixture)
        {
            // AutoMock can control specific items generated in a list
            // See https://blog.ploeh.dk/2011/02/07/CreatingspecificpopulatedlistswithAutoFixture/
            fixture.Inject(Mock.Of<HasPropertiesComplex>());
            var i0 = fixture.Freeze<HasPropertiesComplex>();
            i0.Number = 42;
            i0.Text = "RandomText42";
            Mock.Get(i0).Object.Should().Be(i0);

            // Create should generate same instance
            var i1 = fixture.Create<HasPropertiesComplex>();
            i1.Should().BeSameAs(i0);

            // CreateMany should generate same instances
            var i2 = fixture.CreateMany<HasPropertiesComplex>();
            foreach (var i in i2)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IEnumerable<T>> should generate same instances
            var i3 = fixture.Create<IEnumerable<HasPropertiesComplex>>();
            foreach (var i in i3)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IList<T>> should generate same instances
            var i4 = fixture.Create<IList<HasPropertiesComplex>>();
            foreach (var i in i4)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<IList<T>> should generate same instances
            var i5 = fixture.Create<List<HasPropertiesComplex>>();
            foreach (var i in i5)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<ICollection<T>> should generate same instances
            var i6 = fixture.Create<ICollection<HasPropertiesComplex>>();
            foreach (var i in i6)
            {
                i.Should().BeSameAs(i0);
            }

            // Create<Collection<T>> should generate same instances
            var i7 = fixture.Create<Collection<HasPropertiesComplex>>();
            foreach (var i in i7)
            {
                i.Should().BeSameAs(i0);
            }

            i0.Number.Should().Be(42);
            i0.Text.Should().Be("RandomText42");
            i7[0].Number.Should().Be(42);
            i7[0].Text.Should().Be("RandomText42");
        }

        [Theory, AutoDomainData]
        public void FreezeAndInject_ShouldReturnCorrectInstances(IFixture fixture)
        {
            var i1 = new HasPropertiesSimple
            {
                Number = 10,
                Text = "RandomText10"
            };
            var i2 = fixture.Freeze<HasPropertiesSimple>();
            var i3 = fixture.Create<HasPropertiesSimple>();
            
            // Before injection 
            i1.Should().NotBeSameAs(i2);
            i1.Should().NotBeSameAs(i3);
            i2.Should().BeSameAs(i3);

            // After injection
            fixture.Inject(i1);
            var i4 = fixture.Freeze<HasPropertiesSimple>();
            var i5 = fixture.Create<HasPropertiesSimple>();
            i1.Should().NotBeSameAs(i2);
            i1.Should().NotBeSameAs(i3);
            i2.Should().NotBeSameAs(i4);
            i2.Should().NotBeSameAs(i5);
            i3.Should().NotBeSameAs(i4);
            i3.Should().NotBeSameAs(i5);
            i1.Should().BeSameAs(i4);
            i1.Should().BeSameAs(i5);
            i2.Should().BeSameAs(i3);
            i4.Should().BeSameAs(i5);

            // Since i6 == i1, changing its properties also changes other references (After injection)
            // Since i2 and i3 are create separately (Before injection), they should not be affected
            var i6 = fixture.Create<HasPropertiesSimple>();
            i4.Should().BeSameAs(i6);
            i5.Should().BeSameAs(i6);
            i6.Number = 20;
            i6.Text = "RandomText20";
            i2.Should().NotBeSameAs(i6);
            i3.Should().NotBeSameAs(i6);
            i1.Should().BeSameAs(i6);
            i4.Should().BeSameAs(i6);
            i5.Should().BeSameAs(i6);
            
            // Since i7 is now injected with custom object, it should differ from all of the previously created objects
            fixture.Inject(new HasPropertiesSimple
            {
                Number = 30,
                Text = "RandomText30"
            });
            var i7 = fixture.Create<HasPropertiesSimple>();
            i1.Should().NotBeSameAs(i7);
            i2.Should().NotBeSameAs(i7);
            i3.Should().NotBeSameAs(i7);
            i4.Should().NotBeSameAs(i7);
            i5.Should().NotBeSameAs(i7);
            i6.Should().NotBeSameAs(i7);
        }

        [Theory, AutoDomainData]
        public void FreezeAndCreateSequences_ShouldReturnSameInstances(IFixture fixture)
        {
            var seq = fixture.Freeze<IEnumerable<int>>();
            var list = fixture.Create<List<int>>();
            var iList = fixture.Create<IList<int>>();
            var collection = fixture.Create<Collection<int>>();

            seq.Should().NotBeEmpty();
            seq.Should().BeEquivalentTo(list);
            seq.Should().BeEquivalentTo(iList);
            seq.Should().BeEquivalentTo(collection);
        }
    }
}
