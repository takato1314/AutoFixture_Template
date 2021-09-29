using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoFixture.Extensions
{
    /// <inheritdoc />
    public class AssignmentOptions<T> : SelfReferenceAssignmentOptions<AssignmentOptions<T>>
    {
        public AssignmentOptions()
        {
        }

        public AssignmentOptions(IAssignmentOptions defaults) : base(defaults)
        {
        }
        
        /// <summary>
        /// Setup expression for assigning expectations into <see cref="IFixtureSetup{T}.Object"/>
        /// </summary>
        public AssignmentOptions<T> Setup<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
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

        internal static AssignmentOptions<T> CloneDefaults(T instance)
        {
            return new(new AssignmentOptions(instance!));
        }
    }

    /// <inheritdoc />
    public class AssignmentOptions : SelfReferenceAssignmentOptions<AssignmentOptions>
    {
        public AssignmentOptions(object instance)
        {
            Instance = instance;
        }
    }
}
