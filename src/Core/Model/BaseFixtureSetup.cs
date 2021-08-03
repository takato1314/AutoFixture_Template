using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Default implementation of <see cref="IFixtureSetup"/>
    /// </summary>
    public abstract class BaseFixtureSetup<TFixture> : IFixtureSetup where TFixture : class
    {
        /// <inheritdoc cref="BaseFixtureSetup{TFixture}"/>
        protected BaseFixtureSetup(
            [DisallowNull] IFixture fixture)
        {
            Dependencies = new FixtureDependencies(this, fixture);
            ServiceCollection = new ServiceCollection();
        }

        #region Properties

        /// <summary>
        /// Service provider for the dependency types
        /// </summary>
        // TODO: Implement configure pattern to get a static instance of FixtureConfiguration so that we can inject a single instance of ServiceCollection from DI provider.
        public IServiceProvider? ServiceProvider { get; protected set; } = null;

        /// <summary>
        /// The service container registry for dependency types
        /// </summary>
        public IServiceCollection ServiceCollection { get; }

        /// <inheritdoc cref="IFixtureSetup.Object"/>
        public TFixture Object { get; protected set; } = null!;

        dynamic IFixtureSetup.Object
        {
            get => Object;
            set => Object = value;
        }

        /// <inheritdoc />
        public FixtureDependencies Dependencies { get; set; }

        #endregion

        /// <inheritdoc cref="IFixtureSetup.Customize"/>
        public void Customize(IFixture fixture)
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            Dependencies.Fixture = fixture;
            Dependencies.Register(this);
            Register(fixture);

            // Should get from ServiceProvider when desired
            if (ServiceCollection.Any())
            {
                ServiceProvider = ServiceCollection.BuildServiceProvider();
            }
            Object = CreateObject(fixture);
        }

        /// <inheritdoc />
        public virtual void Inject<T>(IFixture fixture, T item)
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            Dependencies.Fixture = fixture;
            Dependencies.Update(item);
            Object = fixture.Create<TFixture>();
        }

        #region Private

        private TFixture GetObjectFromServiceCollection()
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException($"No provided {nameof(IServiceProvider)} exists to derived from.");
            }

            var instance = ServiceProvider.GetService<TFixture>();
            return instance;
        }

        /// <summary>
        /// Register any dependencies and fixtures for this <seealso cref="IFixtureSetup"/>
        /// See examples for more details.
        /// </summary>
        protected virtual void Register(IFixture fixture)
        {
        }

        /// <summary>
        /// Defines the expected <see cref="Object"/> instance of current <see cref="IFixtureSetup"/> fixture.
        /// See examples for various ways for implementing this.
        /// </summary>
        protected virtual TFixture CreateObject(IFixture fixture)
        {
            Object = fixture.Create<TFixture>();
            return Object;
        }

        #endregion
    }
}
