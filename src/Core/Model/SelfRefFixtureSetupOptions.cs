namespace AutoFixture.Extensions
{
    /// <summary>
    /// Represents the run-time behavior of <see cref="IFixtureSetup{T}"/> operations
    /// </summary>
    public abstract class SelfRefFixtureSetupOptions<TSelf> : IFixtureSetupOptions 
        where TSelf : SelfRefFixtureSetupOptions<TSelf>
    {
        protected SelfRefFixtureSetupOptions()
        {
        }

        protected SelfRefFixtureSetupOptions(IFixtureSetupOptions defaults)
        {
            Instance = defaults.Instance;
        }

        #region Properties

        /// <inheritdoc />
        public object Instance { get; set; } = null!;

        #endregion
    }
}
