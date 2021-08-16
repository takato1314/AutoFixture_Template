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
            Customize();
        }

        #region Properties

        private readonly IFixture _fixture;

        /// <inheritdoc cref="IFixtureSetup{T}.Object" />
        public T Object { get; protected set; } = null!;

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

        /// <inheritdoc cref="ICustomization.Customize"/>
        public void Customize()
        {
            Ensure.Any.IsNotNull(_fixture);
            
            Object = CreateObject();
            Mock = Moq.Mock.Get(Object);

            _fixture.Inject(Object);
        }

        /// <inheritdoc />
        public void Inject(T item)
        {
            Ensure.Any.IsNotNull(_fixture);

            Object = item;
            Mock = item.IsMock() ? Moq.Mock.Get(item) : null;

            _fixture.Inject(item);
        }

        #region Private
        
        /// <summary>
        /// Defines the expected <see cref="Object"/> instance of current fixture.
        /// See examples for various ways for implementing this.
        /// </summary>
        protected virtual T CreateObject()
        {
            return _fixture.Create<T>();
        }

        #endregion
    }
}
