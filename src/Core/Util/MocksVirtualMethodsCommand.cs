using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using EnsureThat;
using Moq;
using Moq.Language;

namespace AutoFixture.Extensions
{
    public class MocksVirtualMethodsCommand : ISpecimenCommand
    {
        private readonly MockVirtualMethodsCommand _mockVirtualMethodsCommand = new();
        private readonly Type _mockVirtualMethodsCommandType = typeof(MockVirtualMethodsCommand);

        public void Execute(object specimen, ISpecimenContext context)
        {
            Ensure.Any.IsNotNull(context);

            if (!(specimen is Mock mock))
                return;
            Type mockedType = mock.GetType().GetMockedType();
            foreach (MethodInfo configurableMethod in GetConfigurableMethods(mockedType))
            {
                Type returnType = configurableMethod.ReturnType;
                var expression = _mockVirtualMethodsCommandType.GetMethod("MakeMethodInvocationLambda", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(_mockVirtualMethodsCommand, new object[]
                {
                    mockedType,
                    configurableMethod,
                    context
                });

                if (expression != null)
                {
                    if (configurableMethod.IsVoid())
                    {
                        _mockVirtualMethodsCommandType.GetMethod("SetupVoidMethod", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(mockedType).Invoke(_mockVirtualMethodsCommand, new[]
                        {
                            mock,
                            expression
                        });
                    }
                    else
                    {
                        GetType().GetMethod("SetupMethod", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(mockedType, returnType).Invoke(this, new[]
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
            var specification = (DelegateSpecification)_mockVirtualMethodsCommandType.GetField("DelegateSpecification", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(_mockVirtualMethodsCommand)!;

            if (!specification.IsSatisfiedBy(type))
            {
                methodInfos = type.GetAllMethods();
            }
            else
            {
                methodInfos = new[]
                {
                    type.GetTypeInfo().GetMethod("Invoke")!
                };
            }

            IEnumerable<MethodInfo> methods = methodInfos;
            //var canBeConfigured = _mockVirtualMethodsCommandType.GetMethod("DelegateSpecification", BindingFlags.Static | BindingFlags.NonPublic)!;
            var skipWritablePropertyGetters = _mockVirtualMethodsCommandType.GetMethod("SkipWritablePropertyGetters", BindingFlags.Static | BindingFlags.NonPublic)!;
            var results = ((IEnumerable<MethodInfo>)skipWritablePropertyGetters.Invoke(_mockVirtualMethodsCommand, new object[] { type, methods })!)
                .Where(CanBeConfigured);

            return results;
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

        #endregion
    }
}
