using System;
using System.Collections.Generic;

namespace AutoFixture.Extensions.Tests
{
    public class ComplexChild : IHasProperties
    {
        #region ctor

        public ComplexChild()
        {
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

        public readonly string PrivateString = "fieldString";

        /// <inheritdoc />
        public string Name { get; set; } = nameof(ComplexChild);

        /// <inheritdoc />
        public int Number { get; set; }

        /// <inheritdoc />
        public Guid ConcurrencyStamp { get; set; }

        public bool? Boolean { get; set; }

        public uint? Nullable { get; set; }

        public ICollection<string> StringCollection { get; set; } = new List<string>();

        public IDictionary<string, SimpleChild> DictionaryCollection { get; set; } =
            new Dictionary<string, SimpleChild>();

        public Func<string, string> Function { get; set; } = s => s;
        
        public string ReturnMethod()
        {
            throw new NotImplementedException("Not implemented on class");
        }

        public void VoidMethod()
        {
            throw new NotImplementedException("Not implemented on class");
        }
    }
}
