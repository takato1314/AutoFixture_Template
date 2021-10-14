using System;
using System.Reflection;
using AutoFixture.Kernel;
using AutoMapper.Internal;
using EnsureThat;
using Moq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Relays a request for an interface or an abstract class to a request for a
    /// <see cref="T:Moq.Mock`1" /> of that class.
    /// </summary>
    public class MocksRelay : ISpecimenBuilder
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AutoFixture.AutoMoq.MockRelay" /> class.
        /// </summary>
        public MocksRelay() : this(new IsTypeSpecification())
        {
        }

        /// <inheritdoc cref="MocksRelay"/>
        public MocksRelay(IRequestSpecification specification) => Specification = specification ?? throw new ArgumentNullException(nameof(specification));

        #endregion

        #region Properties

        /// <summary>
        /// Gets a specification that determines whether a given request should
        /// be mocked.
        /// </summary>
        /// <value>The specification.</value>
        /// <remarks>
        /// <para>
        /// This specification determines whether a given type should be
        /// relayed as a request for a mock of the same type. By default it
        /// only returns <see langword="true" /> for interfaces and abstract
        /// classes, but a different specification can be supplied by using the
        /// overloaded constructor that takes an
        /// <see cref="T:AutoFixture.Kernel.IRequestSpecification" /> as input. In that case, this
        /// property returns the specification supplied to the constructor.
        /// </para>
        /// </remarks>
        /// <seealso cref="M:AutoFixture.AutoMoq.MockRelay.#ctor(AutoFixture.Kernel.IRequestSpecification)" />
        public IRequestSpecification Specification { get; }

        #endregion

        public object Create(object request, ISpecimenContext context)
        {
            Ensure.Any.IsNotNull(context);

            if (!Specification.IsSatisfiedBy(request))
                return new NoSpecimen();

            if (request is not Type t)
                return new NoSpecimen();

            object obj = ResolveObject(t, context);
            switch (obj)
            {
                case NoSpecimen _:
                case OmitSpecimen _:
                case null:
                    return obj!;
                case Mock mock:
                    return mock.Object;
                default:
                    return obj;
            }
        }

        #region Private

        private object ResolveObject(Type t, ISpecimenContext context)
        {
            if (t.IsGenericType)
            {
                // Generic types
                Type[] genericArguments = t.GetTypeInfo().GetGenericArguments();
                if (t.IsNullableType())
                {
                    return context.Resolve(genericArguments[0]);
                }
            }

            if (t.IsValueType)
            {
                return ResolveValueTypes(t, context);
            }

            // Try resolve the object itself
            var mockType = typeof(Mock<>).MakeGenericType(t);
            return context.Resolve(mockType);
        }

        private static object ResolveValueTypes(Type t, ISpecimenContext context)
        {
            var ctor = t.GetConstructors()[^1];
            var ctorParams = ctor.GetParameters();
            object specimen;

            if (t.HasWritableProperties())
            {
                specimen = Activator.CreateInstance(t)!;
                foreach (PropertyInfo property in t.GetProperties())
                {
                    object obj = context.Resolve(property);
                    if (obj is not OmitSpecimen && property.CanWrite)
                        property.SetValue(specimen, obj);
                }
            }
            else
            {
                // Get and resolve the first ctor parameters
                var parameters = new object[ctorParams.Length];
                for (var i = 0; i < ctorParams.Length; i++)
                {
                    object obj = context.Resolve(ctorParams[i]);
                    parameters[i] = obj is not OmitSpecimen ? obj : null!;
                }

                try
                {
                    specimen = Activator.CreateInstance(t, parameters)!;
                }
                catch (Exception)
                {
                    // Unable to create object due to exceptions thrown from ctor
                    specimen = Activator.CreateInstance(t)!;
                }
            }

            return specimen;
        }

        private class IsTypeSpecification : IRequestSpecification
        {
            public bool IsSatisfiedBy(object request)
            {
                return request is Type;
            }
        }

        #endregion
    }
}
