using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoFixture.Extensions
{
    /// <inheritdoc />
    public class FixtureSetupOptions<T> : SelfRefFixtureSetupOptions<FixtureSetupOptions<T>>
    {
        public FixtureSetupOptions(IFixtureSetupOptions defaults) : base(defaults)
        {
        }
        
        /// <summary>
        /// Setup expression for assigning expectations into <see cref="IFixtureSetup{T}.Object"/>
        /// </summary>
        public FixtureSetupOptions<T> Setup<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            AssignValue(expression, value);
            return this;
        }

        /// <summary>
        /// Assign the expected value to the <see cref="IFixtureSetup{T}.Object"/>.
        /// See https://stackoverflow.com/q/5780232.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        private void AssignValue<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            var body = (MemberExpression)expression.Body;
            var propertyInfo = (PropertyInfo)body.Member;
            propertyInfo.SetValue(Instance, value, null);
        }

        internal static FixtureSetupOptions<T> CloneDefaults(T instance)
        {
            // Create default options.
            return new FixtureSetupOptions<T>(new FixtureSetupOptions(instance!));
        }
    }

    /// <inheritdoc />
    public class FixtureSetupOptions : SelfRefFixtureSetupOptions<FixtureSetupOptions>
    {
        public FixtureSetupOptions(object instance)
        {
            Instance = instance;
        }
    }
}
