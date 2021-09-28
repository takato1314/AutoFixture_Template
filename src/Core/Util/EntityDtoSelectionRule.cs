using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Equivalency;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Exclude <see cref="DateTime"/> from assertion selection.
    /// </summary>
    public class EntityDtoSelectionRule : IMemberSelectionRule
    {
        public IEnumerable<IMember> SelectMembers(
            INode currentNode, 
            IEnumerable<IMember> selectedMembers, 
            MemberSelectionContext context)
        {
            var selectedMembersList = selectedMembers.ToList();
            var filteredMembersList = new List<IMember>(selectedMembersList);

            foreach (var selectedMemberInfo in selectedMembersList)
            {
                var currentMemberType = selectedMemberInfo.Type;
                if (selectedMemberInfo.Name.EndsWith("Id") ||
                    selectedMemberInfo.Type == typeof(DateTime) ||
                    selectedMemberInfo.Type == typeof(DateTimeOffset) ||
                    (currentMemberType.GenericTypeArguments.FirstOrDefault()?.GetProperties().Select(_ => _.PropertyType.Name).Contains(context.Type.Name) ?? false) ||
                    currentMemberType.Name.EndsWith("Dto") && currentMemberType.GetProperties().Select(_ => _.PropertyType.Name).Contains(context.Type.Name))
                {
                    filteredMembersList.Remove(selectedMemberInfo);
                }
            }

            return filteredMembersList;
        }

        /// <inheritdoc />
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool IncludesMembers { get; }
    }
}
