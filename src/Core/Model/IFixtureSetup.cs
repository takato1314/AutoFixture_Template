namespace AutoFixture.Extensions
{
    /// <summary>
    /// Create test fixture objects (aka. <see cref="IFixture"/>) for GREEN and RED cases.
    /// See https://blog.ploeh.dk/2009/03/22/AnnouncingAutoFixture/.
    /// </summary>
    public interface IFixtureSetup<T> : ICustomization
    {
        /// <summary>
        /// Retrieves the current fixture's object
        /// </summary>
        public T Object { get; internal set; }
        
        /// <summary>
        /// Inject an instance of <typeparam name="T">object</typeparam> into the current fixture.
        /// </summary>
        public void Inject(IFixture fixture, T item);
    }
}
