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
        string Text { get; set; }

        /// <summary>
        /// Normal integer
        /// </summary>
        int Number { get; set; }

        /// <summary>
        /// Default implementation for Guid
        /// </summary>
        Guid ConcurrencyStamp => Guid.NewGuid();

        public string GetValue()
        {
            throw new NotImplementedException("Not implemented on interface");
        }

    }
}
