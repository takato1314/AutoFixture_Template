using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
#pragma warning disable 414

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesComplex : IHasProperties
    {
        public readonly string _privateString = "fieldString";
        
        /// <inheritdoc />
        public virtual string Text { get; set; } = "DefaultString";

        /// <inheritdoc />
        public virtual int Number { get; set; } = 0;

        public virtual bool Boolean { get; set; } = true;

        public virtual uint? Nullable { get; set; }

        public virtual Guid ConcurrencyStamp { get; set; } = default;

        //public virtual ICollection<string> StringCollection { get; set; } = new HashSet<string>();

        public virtual string GetValue()
        {
            throw new NotImplementedException("Not implemented on class");
        }
    }
}
