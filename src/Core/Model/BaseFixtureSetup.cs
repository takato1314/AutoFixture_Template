using System;
using System.Diagnostics.CodeAnalysis;
using EnsureThat;

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
            fixture.Customize(this);
        }

        #region Properties

        /// <inheritdoc cref="IFixtureSetup{T}.Object" />
        public T Object { get; protected set; } = null!;

        T IFixtureSetup<T>.Object
        {
            get => Object;
            set => Object = value;
        }

        #endregion

        /// <inheritdoc />
        public virtual void Customize(IFixture fixture)
        {
            Ensure.Any.IsNotNull(fixture);
            Object = CreateObject(fixture);
        }

        /// <inheritdoc />
        public void Inject(IFixture fixture, T item)
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }
            
            Object = item;
        }

        #region Private
        
        /// <summary>
        /// Defines the expected <see cref="Object"/> instance of current fixture.
        /// See examples for various ways for implementing this.
        /// </summary>
        protected virtual T CreateObject(IFixture fixture)
        {
            Object = fixture.Create<T>();
            return Object;
        }

        #endregion
    }
}
