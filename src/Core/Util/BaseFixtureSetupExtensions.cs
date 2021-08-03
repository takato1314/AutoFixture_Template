namespace AutoFixture.Extensions
{
    /// <summary>
    /// Extension class for <see cref="IFixture"/> for interfacing with <see cref="BaseFixtureSetup{TFixture}"/>
    /// </summary>
    public static class BaseFixtureSetupExtensions
    {
        /// <inheritdoc cref="BaseFixtureSetup{TFixture}.Inject{T}"/>
        public static void Inject<T>(
            this IFixture fixture,
            IFixtureSetup fixtureSetup,
            T item)
        {
            fixtureSetup.Inject(fixture, item);
        }
    }
}