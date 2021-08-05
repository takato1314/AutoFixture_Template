using System;
using System.Reflection;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using EnsureThat;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Additional customizations for Moq on top of the default <see cref="AutoMoqCustomization"/>
    /// </summary>
    public class AutoPopulatedMoqCustomization : ICustomization
    {
        /// <inheritdoc cref="IFixture.Customize"/>
        public void Customize(IFixture fixture)
        {
            Ensure.Any.IsNotNull(fixture);

            // Postprocessor for mocks
            var relay = new Postprocessor(
                new MockRelay(new MockableSpecification()),
                new CompositeSpecimenCommand(
                    new StubPropertiesCommand(),
                    new MockVirtualMethodsCommand(),
                    new AutoMockPropertiesCommand(),
                    new ReadonlyCollectionPropertiesCommand())
            );
            
            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true,
                Relay = relay
            });
        }
        
        private class MockableSpecification : IRequestSpecification
        {
            /// <inheritdoc />
            public bool IsSatisfiedBy(object request)
            {
                return request is Type type && type.GetTypeInfo().IsPublic;
            }
        }
    }
}
