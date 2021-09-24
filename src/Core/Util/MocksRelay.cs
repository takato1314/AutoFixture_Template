using System;
using System.Linq;
using System.Reflection;
using AutoFixture.Kernel;
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
        public MocksRelay() : this(new IsMockableSpecification())
        {
        }

        /// <inheritdoc cref="MocksRelay"/>
        public MocksRelay(IRequestSpecification mockableSpecification) => MockableSpecification = mockableSpecification ?? throw new ArgumentNullException(nameof(mockableSpecification));

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
        public IRequestSpecification MockableSpecification { get; }

        #endregion

        public object Create(object request, ISpecimenContext context)
        {
            Ensure.Any.IsNotNull(context);

            if (!MockableSpecification.IsSatisfiedBy(request))
                return new NoSpecimen();

            if (!(request is Type t))
                return new NoSpecimen();

            object obj = ResolveMock(t, context);
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

        private static object ResolveMock(Type t, ISpecimenContext context)
        {
            if (t.IsValueType)
            {
                if (t.IsGenericType)
                {
                    // Nullable<type>
                    var paramType = t.GetTypeInfo().GetGenericArguments().Single();
                    return context.Resolve(paramType);
                }
                
                return context.Resolve(t);
            }

            var mockType = typeof(Mock<>).MakeGenericType(t);
            return context.Resolve(mockType);
        }

        private class IsMockableSpecification : IRequestSpecification
        {
            public bool IsSatisfiedBy(object request)
            {
                return request is Type;
            }
        }
    }
}
