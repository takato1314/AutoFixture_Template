using System;

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesGeneric<TValue> : IHasProperties where TValue : IHasProperties, new()
    {
        /// <inheritdoc />
        public virtual string Text { get; set; } = "DefaultGenericString";

        /// <inheritdoc />
        public virtual int Number { get; set; } = 0;

        public virtual TValue GenericValue { get; set; } = default!;

        public virtual Guid ConcurrencyStamp { get; set; } = default;

        public virtual string GetValue()
        {
            return Number.ToString();
        }
    }
}
