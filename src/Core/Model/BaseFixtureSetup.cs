using System;
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
            
            _shouldRunSetup = true;
        }

        /// <inheritdoc cref="BaseFixtureSetup{TFixture}"/>
        protected BaseFixtureSetup(
            IFixture fixture,
            T item)
        {
            Fixture = fixture;
            Inject(item);

            _shouldRunSetup = false;
        }

        /// <inheritdoc cref="BaseFixtureSetup{TFixture}"/>
        protected BaseFixtureSetup(
            IFixture fixture, 
            Func<FixtureSetupOptions<T>, FixtureSetupOptions<T>> config)
        {
            Fixture = fixture;
            Object = CreateObject();
            Mock = Moq.Mock.Get(Object);
            
            config(FixtureSetupOptions<T>.CloneDefaults(Object));
            _shouldRunSetup = false;
        }

        #region Properties

        private bool _shouldRunSetup;
        private T _object = null!;

        protected IFixture Fixture { get; }

        /// <inheritdoc cref="IFixtureSetup{T}.Object" />
        public T Object {
            get
            {
                // Post-construction
                // See https://stackoverflow.com/a/46140327
                if (_shouldRunSetup)
                {
                    _shouldRunSetup = false;
                    Setup();
                }
                
                return _object;
            }
            set => _object = value;
        }

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
        public virtual void Setup()
        {
        }

        #region Private
        
        /// <summary>
        /// Inject an instance of <typeparam name="T">object</typeparam> into the current fixture and overrides the <see cref="Object"/> instance. <br/>
        /// Also see <see cref="FixtureRegistrar.Inject{T}"/>.
        /// </summary>
        private void Inject(T item)
        {
            Ensure.Any.IsNotNull(item);

            Object = item;
            Mock = item.IsMockType() ? Moq.Mock.Get(item) : null;
            Fixture.Inject(item);
        }

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
