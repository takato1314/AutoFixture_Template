namespace AutoFixture.Extensions
{
    /// <summary>
    /// Provides the run-time details of the <see cref="FixtureSetupOptions{T}" /> class.
    /// </summary>
    public interface IFixtureSetupOptions
    {
        /// <summary>
        /// The current pointer to the <see cref="IFixtureSetup{T}.Object"/> instance.
        /// </summary>
        public object Instance { get; set; }
    }
}
