﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using Bunit.Web.AngleSharp;
using FluentAssertions;
using MudBlazor.UnitTests.TestComponents.Collapse;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class CollapseTests : BunitTest
    {
        [Test]
        public void Collapse_TwoWayBinding_Test1()
        {
            var comp = Context.RenderComponent<CollapseBindingTest>();
            var collapse = comp.FindComponent<MudCollapse>();

            collapse.Markup.Should().Contain("mud-collapse-entered");

            IElement Button() => comp.Find("#outside_btn");

            IRenderedComponent<MudSwitch<bool>> MudSwitch() => comp.FindComponent<MudSwitch<bool>>();
            // Initial state is expanded
            MudSwitch().Find("input").HasAttribute("checked").Should().BeTrue();

            // Collapse via button
            Button().Click();
            MudSwitch().Find("input").HasAttribute("checked").Should().BeFalse();

            // Expand via button
            Button().Click();
            MudSwitch().Find("input").HasAttribute("checked").Should().BeTrue();

            // Collapse via switch
            MudSwitch().Find("input").Change(false);
            MudSwitch().Find("input").HasAttribute("checked").Should().BeFalse();

            // Expand via switch
            MudSwitch().Find("input").Change(true);
            MudSwitch().Find("input").HasAttribute("checked").Should().BeTrue();
        }
    }
}
