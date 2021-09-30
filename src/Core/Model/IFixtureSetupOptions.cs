using Moq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Provides the run-time details of the <see cref="FixtureSetupOptions{T}" /> class.
    /// </summary>
    public interface IFixtureSetupOptions<T> where T : class
    {
        /// <summary>
        /// The current pointer to the <see cref="IFixtureSetup{T}.Object"/> instance.
        /// </summary>
        public T Object { get; set; }

        /// <summary>
        /// The <see cref="Moq.Mock"/> for the <see cref="Object"/> object if it's available.
        /// </summary>
        public Mock<T>? Mock { get; set; }
    }
}
