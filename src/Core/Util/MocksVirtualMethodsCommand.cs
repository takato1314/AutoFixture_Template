using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoFixture.Kernel;
using EnsureThat;
using Moq;
using Moq.Language;

namespace AutoFixture.Extensions
{
    public class MocksVirtualMethodsCommand : ISpecimenCommand
    {
        private static readonly DelegateSpecification DelegateSpecification = new();

        public void Execute(object specimen, ISpecimenContext context)
        {
            Ensure.Any.IsNotNull(context);

            if (!(specimen is Mock mock))
                return;
            Type mockedType = mock.GetType().GetMockedType();
            foreach (MethodInfo configurableMethod in GetConfigurableMethods(mockedType))
            {
                Type returnType = configurableMethod.ReturnType;
                var expression = MakeMethodInvocationLambda(mockedType, configurableMethod, context);

                if (expression != null)
                {
                    if (configurableMethod.IsVoid())
                    {
                        GetType().GetMethod("SetupVoidMethod", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(mockedType).Invoke(this, new object[]
                        {
                            mock,
                            expression
                        });
                    }
                    else
                    {
                        GetType().GetMethod("SetupMethod", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(mockedType, returnType).Invoke(this, new object[]
                        {
                            mock,
                            expression,
                            context
                        });
                    }
                }
            }
        }

        #region Private

        /// <summary>Sets up a void method.</summary>
        /// <typeparam name="TMock">The type of the object being mocked.</typeparam>
        /// <param name="mock">The mock being set up.</param>
        /// <param name="methodCallExpression">An expression representing a call to the method being set up.</param>
        private static void SetupVoidMethod<TMock>(
            Mock<TMock> mock,
            Expression<Action<TMock>> methodCallExpression)
            where TMock : class
        {
            mock.Setup(methodCallExpression);
        }

        /// <summary>Sets up a non-void method.</summary>
        /// <typeparam name="TMock">The type of the object being mocked.</typeparam>
        /// <typeparam name="TResult">The return type of the method being set up.</typeparam>
        /// <param name="mock">The mock being set up.</param>
        /// <param name="methodCallExpression">An expression representing a call to the method being set up.</param>
        /// <param name="context">The context that will be used to resolve the method's return value.</param>
        private static void SetupMethod<TMock, TResult>(
            Mock<TMock> mock,
            Expression<Func<TMock, TResult>> methodCallExpression,
            ISpecimenContext context)
            where TMock : class
        {
            ((IReturns<TMock, TResult>)mock.Setup<TResult>(methodCallExpression)).ReturnsUsingContext<TMock, TResult>(context);
        }

        /// <summary>Gets a list of methods to configure.</summary>
        /// <param name="type">The type being mocked and whose methods need to be configured.</param>
        private IEnumerable<MethodInfo> GetConfigurableMethods(Type type)
        {
            IEnumerable<MethodInfo> methodInfos;
            if (!DelegateSpecification.IsSatisfiedBy(type))
            {
                methodInfos = type.GetAllProperties()
                    .Where(p => p.GetGetMethod() != null)
                    .Select((p => p.GetGetMethod()!));
            }
            else
            {
                methodInfos = new[]
                {
                    type.GetTypeInfo().GetMethod("Invoke")!
                };
            }

            IEnumerable<MethodInfo> methods = methodInfos;
            var results = SkipWritablePropertyGetters(type, methods)
                .Where(CanBeConfigured);

            return results;
        }

        /// <summary>
        /// Skip writable properties
        /// </summary>
        private static IEnumerable<MethodInfo> SkipWritablePropertyGetters(
            Type type,
            IEnumerable<MethodInfo> methods)
        {
            IEnumerable<MethodInfo> second = type.GetAllProperties()
                .Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null)
                .Select((p => p.GetGetMethod()!));
            return methods.Except(second);
        }

        /// <summary>Determines whether a method can be mocked.</summary>
        /// <param name="method">The candidate method.</param>
        /// <returns>Whether <paramref name="method" /> can be configured.</returns>
        private static bool CanBeConfigured(MethodInfo method)
        {
            if (!method.IsOverridable() || method.IsGenericMethod || method.HasRefParameters())
                return false;
            return !method.IsVoid() || method.HasOutParameters();
        }


        /// <summary>
        /// Returns a lambda expression that represents an invocation of a mocked type's method.
        /// E.g.,. <![CDATA[ x => x.Method(It.IsAny<string>(), out parameter) ]]>
        /// </summary>
        private static Expression? MakeMethodInvocationLambda(
            Type mockedType,
            MethodInfo method,
            ISpecimenContext context)
        {
            ParameterExpression parameterExpression = Expression.Parameter(mockedType, "x");
            var list = method.GetParameters()
                .Select(param => MakeParameterExpression(param, context))
                .ToList();

            if (list.Any(exp => exp is null))
                return null;

            return Expression.Lambda(!DelegateSpecification.IsSatisfiedBy(mockedType) ? 
                Expression.Call(parameterExpression, method, list!) : 
                Expression.Invoke(parameterExpression, list!), parameterExpression);
        }

        private static Expression? MakeParameterExpression(
            ParameterInfo parameter,
            ISpecimenContext context)
        {
            if (parameter.IsOut)
            {
                var elementType = parameter.ParameterType.GetElementType()!;
                var obj = context.Resolve(elementType);
                return obj is OmitSpecimen ? null : Expression.Constant(obj, elementType);
            }
            return Expression.Call(typeof(It).GetMethod("IsAny")!.MakeGenericMethod(parameter.ParameterType));
        }

        #endregion
    }
}
