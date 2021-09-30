using System;

namespace AutoFixture.Extensions.Tests
{
    /// <summary>
    /// A simple interface used to test AutoFixture features to ensure there are no breaking changes.
    /// </summary>
    public interface IHasProperties
    {
        /// <summary>
        /// Normal string
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Normal integer
        /// </summary>
        int Number { get; set; }

        /// <summary>
        /// Default implementation for Guid
        /// </summary>
        Guid ConcurrencyStamp { get; set; }

        public string ReturnMethod()
        {
            throw new NotImplementedException("Not implemented on interface");
        }

        public void VoidMethod()
        {
            throw new NotImplementedException("Not implemented on interface");
        }

    }
}
