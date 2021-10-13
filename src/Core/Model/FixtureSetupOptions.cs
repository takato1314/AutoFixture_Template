using System;
using System.Linq.Expressions;

namespace AutoFixture.Extensions
{
    /// <inheritdoc />
    public class FixtureSetupOptions<T> : SelfRefFixtureSetupOptions<FixtureSetupOptions<T>, T> 
        where T : class
    {
        public FixtureSetupOptions(IFixture fixture, T instance) : base(fixture, instance)
        {
        }

        public FixtureSetupOptions(IFixtureSetupOptions<T> defaults) : base(defaults)
        {
        }

        #region Public
        
        /// <summary>
        /// Specifies a setup for a call to a <see langword="void"/> method.
        /// See https://stackoverflow.com/q/5780232.
        /// </summary>
        public FixtureSetupOptions<T> Setup(Expression<Action<T>> expression, Action action)
        {
            Mock?.Setup(expression).Callback(action);
            return this;
        }

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a non-<see langword="void"/> (value-returning) method.
        /// </summary>
        public FixtureSetupOptions<T> Setup<TResult>(Expression<Func<T, TResult>> expression, TResult value)
        {
            Mock?.Setup(expression).Returns(value);
            return this;
        }

        #endregion

        #region Private
        
        internal static FixtureSetupOptions<T> CloneDefaults(IFixture fixture, T instance)
        {
            // Create default options.
            return new FixtureSetupOptions<T>(new FixtureSetupOptions<T>(fixture, instance));
        }

        #endregion
    }
}
