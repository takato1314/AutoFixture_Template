using AutoFixture.AutoMoq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Additional customizations for Moq on top of the default <see cref="AutoMoqCustomization"/>
    /// </summary>
    public class AutoPopulatedMoqCustomization : AutoMoqCustomization
    {
        public AutoPopulatedMoqCustomization()
        {
            Relay = new MocksRelay();
            ConfigureMembers = true;
            GenerateDelegates = true;
        }
    }
}
