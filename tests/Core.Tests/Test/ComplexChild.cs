using System;

// ReSharper disable InconsistentNaming
#pragma warning disable 414

namespace AutoFixture.Extensions.Tests
{
    public class ComplexChild : IHasProperties
    {
        #region ctor

        public ComplexChild()
        {
            Name = string.Empty;
        }

        public ComplexChild(string name)
        {
            Name = name;
        }
        
        public ComplexChild(string name, int number)
        {
            Name = name;
            Number = number;
        }

        #endregion

        public readonly string _privateString = "fieldString";
        
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public int Number { get; set; }

        public bool Boolean { get; set; } = true;

        public uint? Nullable { get; set; } = null!;

        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();


        public virtual string GetValue()
        {
            throw new NotImplementedException("Not implemented on class");
        }
    }
}
