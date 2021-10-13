using AutoFixture.AutoMoq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Additional customizations for Moq on top of the default <see cref="AutoMoqCustomization"/>.<br/>
    /// See https://stackoverflow.com/questions/4769928/using-moq-to-mock-only-some-methods/37952383#37952383. <br/>
    /// See https://stackoverflow.com/questions/59660284/creating-mock-with-moq-around-existing-instance.
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
