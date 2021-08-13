using System;
using System.Globalization;
using System.Reflection;
using Moq;
using System.Linq;
using AutoFixture.Kernel;
using Moq.Language;
using Moq.Language.Flow;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Mock"/>
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Determines whether the current object <paramref name="obj"/> is a <see cref="Mock"/> or an instance of <see cref="Mock"/>
        /// </summary>
        public static bool IsMock<T>(this T obj) where T : class
        {
            var result = IsMockObject(typeof(T));
            if (!result)
            {
                result = IsMocked(obj);
            }

            return result;
        }

        /// <summary>
        /// Get the underlying mocked for the <see cref="Mock{T}"/> mocked.
        /// </summary>
        public static Type GetMockedType(this Type type)
        {
            return type.GetTypeInfo().GetGenericArguments().Single();
        }

        #region Private

        internal static IReturnsResult<TMock> ReturnsUsingContext<TMock, TResult>(
            this IReturns<TMock, TResult> setup,
            ISpecimenContext context)
            where TMock : class
        {
            return setup.Returns((() =>
            {
                object obj = context.Resolve(typeof(TResult));
                if (obj == null && default(TResult) != null)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, $"Tried to setup a member with a return type of {typeof(TResult)}, but null was found instead."));
                var result = obj is null or TResult ?
                    (TResult)obj! :
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, $"Tried to setup a member with a return type of {typeof(TResult)}, but an instance of {obj.GetType()} was found instead."));
                setup.Returns(result);
                return result;
            }));
        }

        /// <summary>
        /// Determines if specified <see cref="mocked"/> is an object created from <see cref="Mock"/>.
        /// </summary>
        private static bool IsMocked<T>(T mocked) where T : class
        {
            bool result;
            try
            {
                var mockObj = Mock.Get(mocked);
                result = mockObj != null;
            }
            catch (Exception)
            {
                // Not a mocked object
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Determines if the specified <paramref name="type"/> is instance of <see cref="Mock"/>.
        /// </summary>
        private static bool IsMockObject(this Type type)
        {
            return type.GetTypeInfo().IsGenericType &&
                   typeof(Mock<>).IsAssignableFrom(type.GetGenericTypeDefinition()) &&
                   !type.GetMockedType().IsGenericParameter;
        }
        
        #endregion
    }
}
