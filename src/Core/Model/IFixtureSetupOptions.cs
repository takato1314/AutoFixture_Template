using Moq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Provides the run-time details of the <see cref="IFixtureSetup{T}" />.
    /// </summary>
    public interface IFixtureSetupOptions<T> where T : class
    {
        
        /// <summary>
        /// The current <see cref="IFixture"/> instance referenced.
        /// </summary>
        internal IFixture Fixture { get; set; }

        /// <summary>
        /// Retrieves the current fixture's object
        /// </summary>
        public T Object { get; internal set; }

        /// <summary>
        /// The <see cref="Moq.Mock"/> for the <see cref="Object"/> object if it's available.
        /// </summary>
        public Mock<T> Mock { get; internal set; }
    }
}
