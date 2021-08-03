using System;
using System.Collections.Generic;
using AutoFixture.Kernel;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Use this relay to replace the default ConstructorInvoker pattern that invokes concrete types using
    /// its constructor. Instead, we use mocked types.
    /// See https://blog.ploeh.dk/2010/08/25/ChangingthebehaviorofAutoFixtureauto-mockingwithMoq/.
    /// </summary>
    public class FilteringRelays : DefaultEngineParts
    {
        private readonly Func<ISpecimenBuilder, bool> _spec;

        /// <inheritdoc cref="FilteringRelays"/>
        public FilteringRelays(Func<ISpecimenBuilder, bool> specification)
        {
            _spec = specification ?? throw new ArgumentNullException(nameof(specification));
        }

        /// <inheritdoc />
        public override IEnumerator<ISpecimenBuilder> GetEnumerator()
        {
            using var enumerator = base.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (_spec(enumerator.Current))
                {
                    yield return enumerator.Current;
                }
            }
        }
    }
}
