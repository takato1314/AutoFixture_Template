using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;

[assembly: InternalsVisibleTo("AutoFixture.Extensions.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace AutoFixture.Extensions
{
    /// <summary>
    /// Factory method to declare a single <see cref="IFixture"/> for unit tests applications. <br/>
    /// See https://autofixture.github.io.
    /// </summary>
    public static class FixtureFactory
    {
        /// <summary>
        /// Gets the IFixture instance
        /// </summary>
        public static readonly IFixture Instance = CreateFixture();

        internal static IFixture GetFixture()
        {
            return Instance;
        }

        /// <summary>
        /// Creates a customized <see cref="IFixture"/> instance for IFixture.
        /// </summary>
        public static IFixture CreateFixture()
        {
            // Setup mock for all types to use mocks
            bool ConcreteFilter(ISpecimenBuilder sb) => !(sb is MethodInvoker);
            var defaultEngine = new FilteringRelays(ConcreteFilter);

            // Customizations
            var fixture = new AutoFixture.Fixture(defaultEngine)
                .Customize(new AutoPopulatedMoqCustomization());
            var postprocessor = (Postprocessor)fixture.Customizations.Single(f => f is Postprocessor);
            if (postprocessor.Command is CompositeSpecimenCommand command)
            {
                // Since we are using Virtuosity, we need to exclude virtual methods to be mock by default. 
                var commands = ((ISpecimenCommand[])command.Commands);
                var index = Array.IndexOf(commands, commands.Single(c => c is MockVirtualMethodsCommand));
                commands[index] = new MocksVirtualMethodsCommand();
            }

            // Residue Collectors
            //fixture.Customizations.Add(new DictionaryRelay());
            //fixture.Customizations.Add(new TypeRelay(typeof(IDictionary<,>), typeof(IDictionary<,>)));
            //fixture.Customizations.Add(new TypeRelay(typeof(KeyValuePair<,>), typeof(KeyValuePair<,>)));
            //fixture.ResidueCollectors.Add(new TypeRelay(typeof(IDictionary<,>), typeof(IDictionary<,>)));
            //fixture.ResidueCollectors.Add(new TypeRelay(typeof(KeyValuePair<,>), typeof(KeyValuePair<,>)));

            // Behaviors
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            return fixture;
        }
    }
}
