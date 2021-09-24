using System;

namespace AutoFixture.Extensions.Tests
{
    public class ComplexParent : IHasProperties
    {
        #region ctor
        
        public ComplexParent(ComplexChild complexChild)
        {
            ComplexChild = complexChild;
        }

        public ComplexParent(ComplexChild complexChild, SimpleChild simpleChild)
        {
            ComplexChild = complexChild;
            SimpleChild = simpleChild;
        }

        #endregion

        public ComplexChild ComplexChild { get; }

        public SimpleChild SimpleChild { get; set; } = null!;

        public StructChild StructChild { get; set; }

        /// <inheritdoc />
        public string Name { get; set; } = nameof(ComplexChild);

        /// <inheritdoc />
        public int Number { get; set; }

        /// <inheritdoc />
        public Guid ConcurrencyStamp { get; set; }
    }
}
