using System;
using System.Collections.Generic;
using System.IO;
using Moq;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Describes the other nested <see cref="IFixtureSetup"/> dependencies for this instance of <see cref="IFixtureSetup"/>.
    /// </summary>
    public class FixtureDependencies
    {
        /// <inheritdoc cref="FixtureDependencies"/>
        internal FixtureDependencies(IFixtureSetup parent, IFixture fixture)
        {
            _parent = parent;
            Fixture = fixture;
        }

        #region Properties

        private readonly IFixtureSetup _parent;

        internal readonly IDictionary<Type, IFixtureSetup> FixtureDictionary = new Dictionary<Type, IFixtureSetup>();

        internal IFixture Fixture { get; set; }

        #endregion

        /// <inheritdoc cref="Get"/>
        public IFixtureSetup this[Type type] => FixtureDictionary[type];

        /// <summary>
        /// Register the type dependency for the specified <paramref name="fixtureSetup"/>
        /// </summary>
        public void Register(IFixtureSetup fixtureSetup)
        {
            var fixtureType = fixtureSetup?.GetType();
            var genericType = fixtureType?.BaseType?.GenericTypeArguments;
            if (fixtureSetup == null ||
                fixtureType?.BaseType == null ||
                !fixtureType.BaseType.ToString().Contains("BaseFixtureSetup") ||
                genericType == null || 
                genericType.Length == 0)
            {
                throw new ArgumentException($"{nameof(fixtureSetup)} is not a valid implementation of {typeof(BaseFixtureSetup<>)}!");
            }

            // Skip customize on parent fixtureSetup
            if (fixtureSetup != _parent)
            {
                Fixture.Customize(fixtureSetup);

                // Add nested dependencies to dictionary as well
                foreach (var kvp in fixtureSetup.Dependencies.FixtureDictionary)
                {
                    AddOrUpdateFixtureDependency(kvp.Key, kvp.Value);
                }
            }

            // Add current fixtureSetup to dictionary
            AddOrUpdateFixtureDependency(genericType[0], fixtureSetup);
        }

        /// <inheritdoc cref="Get"/>
        public dynamic Get<T>()
        {
            return Get(typeof(T));
        }

        /// <summary>
        /// Get the specified <see cref="IFixtureSetup"/> dependency for this <see cref="IFixtureSetup"/> instance.
        /// </summary>
        public dynamic Get(Type fixtureType)
        {
            var fixtureSetup = FixtureDictionary[fixtureType];
            if (fixtureSetup == null)
            {
                throw new DirectoryNotFoundException($"Requested fixture '{fixtureType}' is not a valid dependency for fixture '{_parent}'");
            }

            return fixtureSetup;
        }

        #region Private

        internal void Update<T>(T item)
        {
            var isMockObject = typeof(T).GetInterface(nameof(IMocked)) != null;
            var fixtureType = isMockObject ? typeof(T).BaseType : typeof(T);
            if (fixtureType == null ||
                item == null ||
                !FixtureDictionary.ContainsKey(fixtureType))
            {
                throw new DirectoryNotFoundException($"Requested fixture '{typeof(T).Name}' is not a valid dependency for fixture '{_parent}'");
            }

            Fixture.Inject(item);
            FixtureDictionary[fixtureType].Object = item;
        }

        internal void AddOrUpdateFixtureDependency(Type key, IFixtureSetup value)
        {
            if (!FixtureDictionary.ContainsKey(key))
            {
                FixtureDictionary.Add(key, value);
            }
            else
            {
                FixtureDictionary[key] = value;
            }
        }

        #endregion
    }
}
