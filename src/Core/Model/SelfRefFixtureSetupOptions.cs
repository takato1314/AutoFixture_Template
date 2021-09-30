using Moq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Represents the run-time behavior of <see cref="IFixtureSetup{T}"/> operations
    /// </summary>
    public abstract class SelfRefFixtureSetupOptions<TSelf, T> : IFixtureSetupOptions<T>
        where TSelf : SelfRefFixtureSetupOptions<TSelf, T>
        where T: class
    {
        protected SelfRefFixtureSetupOptions()
        {
        }

        protected SelfRefFixtureSetupOptions(IFixtureSetupOptions<T> defaults)
        {
            Object = defaults.Object;
            Mock = Object.IsMockType() ? Moq.Mock.Get(Object) : null;
        }

        #region Properties

        /// <inheritdoc />
        public T Object { get; set; } = null!;

        public Mock<T>? Mock { get; set; }

        #endregion
    }
}
