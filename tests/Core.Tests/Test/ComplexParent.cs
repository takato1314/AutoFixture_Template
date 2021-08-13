using System;

namespace AutoFixture.Extensions.Tests
{
    public class ComplexParent : IHasProperties
    {
        #region ctor
        
        public ComplexParent(ComplexChild child)
        {
            Child = child;
        }

        #endregion

        public ComplexChild Child { get; }

        /// <inheritdoc />
        public string Name { get; set; } = nameof(ComplexChild);

        /// <inheritdoc />
        public int Number { get; set; }

        /// <inheritdoc />
        public Guid ConcurrencyStamp { get; set; }
    }
}
