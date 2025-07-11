﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace MudBlazor.UnitTests.UserAttributes
{
    [TestFixture]
    public sealed class UserAttributesTests
    {
        static UserAttributesTests()
        {
            Exclude(typeof(MudBooleanInput<>)); // This is the base class of Switch and CheckBox and should be skipped
            Exclude(typeof(MudHidden));         // No need to test
            Exclude(typeof(MudBreakpointProvider)); // just exposing a cascading value, no layout implications
            Exclude(typeof(MudPicker<>));       // Internal component, skip
            Exclude(typeof(MudRadioGroup<>));   // Wrapping component, skip
            Exclude(typeof(MudOverlay));        // Sectioned component, skip
            Exclude(typeof(DataGridGroupRow<>));  // Internal component, skip
            Exclude(typeof(DataGridVirtualizeRow<>)); // Internal component, skip
        }

        [Test]
        public void AllMudComponents_ShouldForwardUserAttributes()
        {
            // Arrange
            using var testContext = new TestContext();
            testContext.AddTestServices();
            testContext.Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            var componentFactory = new MudComponentFactory
            {
                UserAttributes = new Dictionary<string, object> { { "data-testid", "test-123" } },
            };

            // Act & Assert
            var mudComponentTypes = GetMudComponentTypes();

            mudComponentTypes.Should().NotBeEmpty();

            // these components do not need to have user attributes
            var excludedComponents = new HashSet<string>()
            {
                nameof(MudPopover), nameof(MudStep), nameof(MudContextualActionBar), nameof(MudHeatMapCell),
                "Column`1", "FooterCell`1", "HeaderCell`1", "FilterHeaderCell`1", "SelectColumn`1",
                "HierarchyColumn`1", "PropertyColumn`2", "TemplateColumn`1", "MudToggleItem`1",
            };

            foreach (var componentType in mudComponentTypes)
            {
                if (excludedComponents.Contains(componentType.Name))
                    continue;
                var component = componentFactory.Create(componentType, testContext);
                component.Markup.Should()
                    .NotBeEmpty(because: $"the component {componentType.Name} should at least contain one element");

                var elementsWithUserAttributes = component.FindAll("[data-testid='test-123']");
                elementsWithUserAttributes.Should()
                    .NotBeEmpty(because: $"UserAttributes should be forwarded by component {componentType.Name}");
            }
        }

        private Type[] GetMudComponentTypes()
        {
            return typeof(MudElement).Assembly
                .GetTypes()
                .Where(type => type.IsAssignableTo(typeof(MudComponentBase)) && !type.IsAbstract)
                .Select(type => type.IsGenericType ? type.GetGenericTypeDefinition() : type)
                .Except(_excludedComponents)
                .ToArray();
        }

        private static ConcurrentBag<Type> _excludedComponents = [];
        private static void Exclude(Type componentType) => _excludedComponents.Add(componentType);
    }
}
