namespace AutoFixture.Extensions
{
    /// <summary>
    /// Create test fixture objects (aka. <see cref="IFixture"/>) for GREEN and RED cases.
    /// See https://blog.ploeh.dk/2009/03/22/AnnouncingAutoFixture/.
    /// </summary>
    public interface IFixtureSetup : ICustomization
    {
        /// <summary>
        /// Contains fixture object that contains the desired state of that type.
        /// </summary>
        public dynamic Object { get; internal set; }

        /// <summary>
        /// Specifies the current <see cref="IFixtureSetup"/> instance dependencies, which is also another <see cref="IFixtureSetup"/> type.
        /// Current instance of <see cref="IFixtureSetup"/> will also have dependency reference to itself.
        /// </summary>
        public FixtureDependencies Dependencies { get; set; }

        /// <summary>
        ///   <para>
        ///     Defines the default customization for this fixture.
        ///     In principle, the <see cref="Customize"/> should works the same with <see cref="ICustomization.Customize"/>.
        ///     If there are other dependencies on other fixtures, you can register them in this method.
        ///   </para>
        ///   <para>
        ///     Note: While it's possible to omit this method entirely by calling it from the constructor, I choose to remain this method call
        ///     as there are 2 ways you can customize the fixture.
        ///   </para>
        ///   <para>
        ///     See the unit tests for more details.
        ///   </para>
        /// </summary>
        public new void Customize(IFixture fixture);

        /// <summary>
        ///   <para>
        ///     In principle, the <see cref="Inject{T}"/> should works the same with <see cref="FixtureRegistrar.Inject{T}"/>,
        ///     with additional logic to ensure that the <see cref="Dependencies"/> table are updated as well.
        ///   </para>
        ///   <para>
        ///     See the unit tests for more details.
        ///   </para>
        /// </summary>
        public void Inject<T>(IFixture fixture, T item);
    }
}
