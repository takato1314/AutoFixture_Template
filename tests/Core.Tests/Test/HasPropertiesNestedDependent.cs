namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesNestedDependent
    {
        /// <summary>
        /// Dependency demo for <see cref="HasPropertiesDependent"/>.
        /// This should change if it is replaced/injected with another instance
        /// </summary>
        public HasPropertiesDependent HasPropertiesDependent1 { get; set; } = null!;

        /// <summary>
        /// Dependency demo for <see cref="HasPropertiesDependent"/>.
        /// This should NOT change even when replaced/injected with another instance
        /// </summary>
        public HasPropertiesDependent HasPropertiesDependent2 { get; set; } = null!;
    }
}
