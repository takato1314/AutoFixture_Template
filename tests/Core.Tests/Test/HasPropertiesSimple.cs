using System;

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesSimple : IHasProperties
    {
        /// <inheritdoc />
        public virtual string Text { get; set; } = string.Empty;

        /// <inheritdoc />
        public virtual int Number { get; set; }

        /// <inheritdoc />
        public virtual Guid ConcurrencyStamp { get; set; }
    }
}
