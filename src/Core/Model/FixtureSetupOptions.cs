using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoFixture.Extensions
{
    /// <inheritdoc />
    public class FixtureSetupOptions<T> : SelfRefFixtureSetupOptions<FixtureSetupOptions<T>, T> 
        where T : class
    {
        public FixtureSetupOptions(T instance)
        {
            Object = instance;
        }

        public FixtureSetupOptions(IFixtureSetupOptions<T> defaults) : base(defaults)
        {
        }

        /// <summary>
        /// Specifies a setup for a call to a <see langword="void"/> method.
        /// </summary>
        public FixtureSetupOptions<T> Setup(Expression<Action<T>> expression, Action action)
        {
            Mock!.Setup(expression).Callback(action);
            return this;
        }

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a non-<see langword="void"/> (value-returning) method.
        /// </summary>
        public FixtureSetupOptions<T> Setup<TResult>(Expression<Func<T, TResult>> expression, TResult value)
        {
            Mock!.Setup(expression).Returns(value);
            return this;
        }

        #region Private

        /// <summary>
        /// Assign the expected value to the <see cref="IFixtureSetup{T}.Object"/>.
        /// See https://stackoverflow.com/q/5780232.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        private void AssignSetterValue<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            var body = (MemberExpression)expression.Body;
            var propertyInfo = (PropertyInfo)body.Member;
            propertyInfo.SetValue(Object, value, null);
        }

        internal static FixtureSetupOptions<T> CloneDefaults(T instance)
        {
            // Create default options.
            return new FixtureSetupOptions<T>(new FixtureSetupOptions<T>(instance!));
        }

        #endregion
    }
}
