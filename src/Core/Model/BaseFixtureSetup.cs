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
            Mock = Object.IsMockType() ? Moq.Mock.Get(Object) : null;

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
            Func<FixtureSetupOptions<T>, FixtureSetupOptions<T>> options)
        {
            Fixture = fixture;
            Object = CreateObject();
            Mock = Object.IsMockType() ? Moq.Mock.Get(Object) : null;

            options(FixtureSetupOptions<T>.CloneDefaults(Object));
            _shouldRunSetup = false;
        }
        
        #region Properties

        private bool _shouldRunSetup;
        private T _object = null!;

        protected IFixture Fixture { get; private set; }

        IFixture IFixtureSetupOptions<T>.Fixture
        {
            get => Fixture;
            set => Fixture = value;
        }

        /// <inheritdoc cref="IFixtureSetupOptions{T}.Object" />
        public T Object {
            get
            {
                // Post-construction
                // See https://stackoverflow.com/a/46140327
                if (_shouldRunSetup && Setups != null)
                {
                    _shouldRunSetup = false;
                    Setups(FixtureSetupOptions<T>.CloneDefaults(Object));
                }
                
                return _object;
            }
            private set => _object = value;
        }

        T IFixtureSetupOptions<T>.Object
        {
            get => Object;
            set => Object = value;
        }

        /// <inheritdoc cref="IFixtureSetupOptions{T}.Mock"/>
        public Mock<T>? Mock { get; private set; }

        Mock<T>? IFixtureSetupOptions<T>.Mock
        {
            get => Mock;
            set => Mock = value;
        }

        /// <inheritdoc cref="IFixtureSetup{T}.Setups"/>
        protected virtual Func<FixtureSetupOptions<T>, FixtureSetupOptions<T>>? Setups { get; set; }

        Func<FixtureSetupOptions<T>, FixtureSetupOptions<T>>? IFixtureSetup<T>.Setups
        {
            get => Setups;
            set => Setups = value;
        }

        #endregion

        #region Private

        /// <summary>
        /// Inject an instance of <typeparam name="T">object</typeparam> into the current fixture and overrides the <see cref="Object"/> instance. <br/>
        /// Also see <see cref="FixtureRegistrar.Inject{T}"/>.
        /// </summary>
        protected void Inject(T item)
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
