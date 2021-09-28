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
        protected BaseFixtureSetup(IFixture fixture)
        {
            Fixture = fixture;
            Object = CreateObject();
            Mock = Moq.Mock.Get(Object);
            Setup();
        }

        /// <inheritdoc cref="BaseFixtureSetup{TFixture}"/>
        protected BaseFixtureSetup(
            IFixture fixture,
            T item)
        {
            Fixture = fixture;
            Inject(item);
        }

        // Factory Method Pattern
        // See https://stackoverflow.com/a/6792917
        //public static IFixtureSetup<T> Construct()
        //{
        //    var fixture = 
        //}

        #region Properties

        protected IFixture Fixture { get; }

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
        public Mock<T>? Mock { get; private set; }

        #endregion

        /// <inheritdoc />
        public void Inject(T item)
        {
            Ensure.Any.IsNotNull(item);

            Object = item;
            Mock = item.IsMockType() ? Moq.Mock.Get(item) : null;
            Fixture.Inject(item);
        }
        
        /// <inheritdoc />
        public virtual void Setup()
        {
        }

        #region Private

        /// <summary>
        /// Initialize a fixture instance for the <see cref="Object"/>.
        /// </summary>
        private T CreateObject()
        {
            return Fixture.Freeze<T>();
        }

        #endregion
    }
}
