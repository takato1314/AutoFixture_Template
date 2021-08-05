using System;

namespace AutoFixture.Extensions.Tests
{
    public class SimpleChild : IHasProperties
    {
        #region ctor

        public SimpleChild()
        {
            Name = string.Empty;
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
        public string Name { get; set; }

        /// <inheritdoc />
        public int Number { get; set; }

        /// <inheritdoc />
        public Guid ConcurrencyStamp { get; set; }
    }
}
