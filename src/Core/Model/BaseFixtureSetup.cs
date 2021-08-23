using System.Diagnostics.CodeAnalysis;
using EnsureThat;
using Moq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Default implementation of <see cref="IFixtureSetup{T}"/>
    /// </summary>
    public abstract class BaseFixtureSetup<T> : IFixtureSetup<T> where T : class
    {
        /// <inheritdoc cref="BaseFixtureSetup{TFixture}"/>
        protected BaseFixtureSetup(
            [DisallowNull] IFixture fixture)
        {
            _fixture = fixture;

            Object = CreateObject();
            Mock = Moq.Mock.Get(Object);
        }

        #region Properties

        private readonly IFixture _fixture;

        /// <inheritdoc cref="IFixtureSetup{T}.Object" />
        public T Object { get; protected set; }

        T IFixtureSetup<T>.Object
        {
            get => Object;
            set => Object = value;
        }

        /// <summary>
        /// The <see cref="Mock"/> instance for <see cref="Object"/>.
        /// </summary>
        internal Mock<T>? Mock { get; private set; }

        #endregion

        /// <inheritdoc />
        public void Inject(T item)
        {
            Ensure.Any.IsNotNull(item);

            Object = item;
            Mock = item.IsMock() ? Moq.Mock.Get(item) : null;

            _fixture.Inject(item);
        }

        #region Private

        /// <summary>
        /// Initialize a fixture instance for the <see cref="Object"/>.
        /// </summary>
        private T CreateObject()
        {
            return _fixture.Freeze<T>();
        }

        #endregion
    }
}
