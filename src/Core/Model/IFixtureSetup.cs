﻿namespace AutoFixture.Extensions
{
    /// <summary>
    /// Create test fixture objects (aka. <see cref="IFixture"/>) for GREEN and RED cases.
    /// See:
    /// <list type="bullet">
    ///     <item>https://blog.ploeh.dk/2009/03/22/AnnouncingAutoFixture/</item>
    ///     <item>https://blogs.msmvps.com/bsonnino/2020/10/04/mocking-non-virtual-methods-of-a-class/</item>
    /// </list>
    /// </summary>
    public interface IFixtureSetup<T>
    {
        /// <summary>
        /// Retrieves the current fixture's object
        /// </summary>
        public T Object { get; internal set; }

        /// <summary>
        /// Inject an instance of <typeparam name="T">object</typeparam> into the current fixture and overrides the <see cref="Object"/> instance. <br/>
        /// Also see <see cref="FixtureRegistrar.Inject{T}"/>.
        /// </summary>
        public void Inject(T item);
    }
}
