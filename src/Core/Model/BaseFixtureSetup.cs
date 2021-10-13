using System;
using AutoFixture.AutoMoq;
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
            Ensure.Any.IsNotNull(item);
            Fixture = fixture;
            Object = item;
            Mock = item.IsMockType() ? Moq.Mock.Get(item) : null!;
            Fixture.Inject(item);

            _shouldRunSetup = false;
        }

        /// <inheritdoc cref="BaseFixtureSetup{TFixture}"/>
        protected BaseFixtureSetup(
            IFixture fixture, 
            Func<FixtureSetupOptions<T>, FixtureSetupOptions<T>> options)
        {
            Fixture = fixture;
            Object = CreateObject();
            Mock = Moq.Mock.Get(Object);

            options(FixtureSetupOptions<T>.CloneDefaults(Fixture, Object));
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
                    Setups(FixtureSetupOptions<T>.CloneDefaults(Fixture, Object));
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
        public Mock<T> Mock { get; private set; }

        Mock<T> IFixtureSetupOptions<T>.Mock
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
        
        /// <inheritdoc cref="AutoMoqCustomization.ConfigureMembers"/>
        public bool GenerateMembers { get; set; } = true;

        /// <inheritdoc cref="AutoMoqCustomization.GenerateDelegates"/>
        public bool GenerateDelegates { get; set; } = true;

        #endregion

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
