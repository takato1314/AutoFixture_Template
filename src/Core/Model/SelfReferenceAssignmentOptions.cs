namespace AutoFixture.Extensions
{
    /// <summary>
    /// Represents the run-time behavior of <see cref="IFixtureSetup{T}"/> operations
    /// </summary>
    public class SelfReferenceAssignmentOptions<TSelf> : IAssignmentOptions 
        where TSelf : SelfReferenceAssignmentOptions<TSelf>
    {
        internal SelfReferenceAssignmentOptions()
        {
        }

        protected SelfReferenceAssignmentOptions(IAssignmentOptions defaults)
        {
            Instance = defaults.Instance;
        }

        #region Properties

        /// <inheritdoc />
        public object Instance { get; set; } = null!;

        #endregion
    }
}
