using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Equivalency;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace AutoFixture.Extensions
{
    /// <summary>
    /// Exclude <see cref="DateTime"/> from assertion selection.
    /// </summary>
    public class EntityDtoSelectionRule : IMemberSelectionRule
    {
        /// <inheritdoc />
        public IEnumerable<SelectedMemberInfo> SelectMembers(
            IEnumerable<SelectedMemberInfo> selectedMembers,
            IMemberInfo context,
            IEquivalencyAssertionOptions config)
        {
            var selectedMembersList = selectedMembers.ToList();
            var filteredMembersList = new List<SelectedMemberInfo>(selectedMembersList);

            foreach (var selectedMemberInfo in selectedMembersList)
            {
                var currentMemberType = selectedMemberInfo.MemberType;
                if (selectedMemberInfo.Name.EndsWith("Id") ||
                    selectedMemberInfo.MemberType == typeof(DateTime) ||
                    selectedMemberInfo.MemberType == typeof(DateTimeOffset) ||
                    (currentMemberType.GenericTypeArguments.FirstOrDefault()?.GetProperties().Select(_ => _.PropertyType.Name).Contains(context.CompileTimeType.Name) ?? false) ||
                    currentMemberType.Name.EndsWith("Dto") && currentMemberType.GetProperties().Select(_ => _.PropertyType.Name).Contains(context.CompileTimeType.Name))
                {
                    filteredMembersList.Remove(selectedMemberInfo);
                }
            }

            return filteredMembersList;
        }

        /// <inheritdoc />
        public bool IncludesMembers { get; }
    }
}
