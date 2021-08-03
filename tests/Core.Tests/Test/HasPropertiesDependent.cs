namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesDependent
    {
        /// <summary>
        /// Dependency demo for <see cref="HasPropertiesSimple"/>
        /// </summary>
        public HasPropertiesSimple HasPropertiesSimple { get; set; } = null!;

        /// <summary>
        /// Dependency demo for <see cref="HasPropertiesComplex"/>
        /// </summary>
        public HasPropertiesComplex HasPropertiesComplex { get; set; } = null!;
    }
}
