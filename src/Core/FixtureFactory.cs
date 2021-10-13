using System;
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
        #region Properties

        /// <summary>
        /// Gets the IFixture instance
        /// </summary>
        public static IFixture Instance { get; private set; } = CreateFixture();

        #endregion

        /// <summary>
        /// Creates a customized <see cref="IFixture"/> instance for IFixture.
        /// </summary>
        public static IFixture CreateFixture(
            bool generateMembers = true,
            bool generateDelegates = true)
        {
            // Setup mock for all types to use mocks
            bool ConcreteFilter(ISpecimenBuilder sb) => sb is not MethodInvoker;
            var defaultEngine = new FilteringRelays(ConcreteFilter);

            // Customizations
            var fixture = new Fixture(defaultEngine).Customize(new CompositeCustomization(new AutoPopulatedMoqCustomization
            {
                ConfigureMembers = generateMembers,
                GenerateDelegates = generateDelegates
            }));
            var postprocessor = fixture.Customizations.SingleOrDefault(f => f is Postprocessor) as Postprocessor;
            if (postprocessor?.Command is CompositeSpecimenCommand command)
            {
                // Since we may have classes with virtual method or be using Virtuosity, we need to exclude virtual methods to be mock by default. 
                var commands = ((ISpecimenCommand[])command.Commands);
                var index = Array.IndexOf(commands, commands.Single(c => c is MockVirtualMethodsCommand));
                commands[index] = new MocksVirtualMethodsCommand();
            }

            // Behaviors
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Instance = fixture;
            return fixture;
        }
    }
}
