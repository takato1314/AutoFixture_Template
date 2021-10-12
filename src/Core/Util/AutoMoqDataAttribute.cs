using AutoFixture.Xunit2;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// A AutoFixture-xUnit attribute used for decorating unit test Theories.
    /// See https://blog.ploeh.dk/2011/03/18/EncapsulatingAutoFixtureCustomizations/
    /// </summary>
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        /// <inheritdoc cref="AutoMoqDataAttribute"/>
        public AutoMoqDataAttribute(bool generateMembers = true, bool generateDelegates = true)
            : base(() => FixtureFactory.CreateFixture(generateMembers, generateDelegates))
        {
        }
    }
}