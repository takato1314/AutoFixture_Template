using System;

namespace AutoFixture.Extensions.Tests
{
    public class SimpleChild : IHasProperties
    {
        #region ctor

        public SimpleChild()
        {
        }

        public SimpleChild(string name)
        {
            Name = name;
        }

        public SimpleChild(string name, int number)
        {
            Name = name;
            Number = number;
        }

        #endregion

        /// <inheritdoc />
        public string Name { get; set; } = nameof(SimpleChild);

        /// <inheritdoc />
        public int Number { get; set; } = 123;

        /// <inheritdoc />
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}
