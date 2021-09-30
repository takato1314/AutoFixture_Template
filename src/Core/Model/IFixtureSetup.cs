namespace AutoFixture.Extensions
{
    /// <summary>
    /// Create test fixture objects (aka. <see cref="IFixture"/>) for GREEN and RED cases.
    /// See:
    /// <list type="bullet">
    ///     <item>https://blog.ploeh.dk/2009/03/22/AnnouncingAutoFixture/</item>
    ///     <item>https://blogs.msmvps.com/bsonnino/2020/10/04/mocking-non-virtual-methods-of-a-class/</item>
    /// </list>
    /// </summary>
    public interface IFixtureSetup<T> : IFixtureSetupOptions<T>
        where T: class
    {
        /// <summary>
        /// Setup expected values for Mock or Object.<br/>
        /// <b>Note:</b> This method will always override any previous setups done via ctor when invoked.
        /// </summary>
        public void Setup();
    }
}
