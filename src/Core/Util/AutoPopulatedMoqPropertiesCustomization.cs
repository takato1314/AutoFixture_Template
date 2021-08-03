using System;
using System.Reflection;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Additional customizations for Moq on top of the default <see cref="AutoMoqCustomization"/>
    /// </summary>
    public class AutoPopulatedMoqPropertiesCustomization : ICustomization
    {
        /// <inheritdoc cref="IFixture.Customize"/>
        public void Customize(IFixture fixture)
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            // Postprocessor for mocks
            var relay = new Postprocessor(
                new MockRelay(new MockableSpecification()),
                new AutoPropertiesCommand(
                    new PropertiesOnlySpecification()));

            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                Relay = relay
            });
        }

        private class PropertiesOnlySpecification : IRequestSpecification
        {
            public bool IsSatisfiedBy(object request)
            {
                return request is PropertyInfo;
            }
        }

        private class MockableSpecification : IRequestSpecification
        {
            /// <inheritdoc />
            public bool IsSatisfiedBy(object request) => true;
        }
    }
}
