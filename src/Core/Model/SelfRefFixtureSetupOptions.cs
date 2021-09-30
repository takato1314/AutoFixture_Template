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

        /// <inheritdoc cref="IFixtureSetupOptions{T}.Object" />
        public T Object { get; internal set; } = null!;

        /// <inheritdoc cref="IFixtureSetupOptions{T}.Mock" />
        public Mock<T>? Mock { get; internal set; }

        #endregion

        T IFixtureSetupOptions<T>.Object
        {
            get => Object;
            set => Object = value;
        }

        Mock<T>? IFixtureSetupOptions<T>.Mock
        {
            get => Mock;
            set => Mock = value;
        }
    }
}
