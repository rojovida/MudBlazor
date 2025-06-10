﻿using Microsoft.AspNetCore.Components;
using MudBlazor.State;
using MudBlazor.Utilities;

namespace MudBlazor
{
#nullable enable

    /// <summary>
    /// A small popup which provides more information.
    /// </summary>
    public partial class MudTooltip : MudComponentBase
    {
        private readonly ParameterState<bool> _visibleState;
        private Origin _anchorOrigin;
        private Origin _transformOrigin;
        public MudTooltip()
        {
            using var registerScope = CreateRegisterScope();
            _visibleState = registerScope.RegisterParameter<bool>(nameof(Visible))
                .WithParameter(() => Visible)
                .WithEventCallback(() => VisibleChanged);
        }

        protected string ContainerClass => new CssBuilder("mud-tooltip-root")
            .AddClass("mud-tooltip-inline", Inline)
            .AddClass(RootClass)
            .Build();

        protected string Classname => new CssBuilder("mud-tooltip")
            .AddClass("d-flex")
            .AddClass("mud-tooltip-default", Color == Color.Default)
            .AddClass($"mud-tooltip-{ConvertPlacement().ToDescriptionString()}")
            .AddClass("mud-tooltip-arrow", Arrow)
            .AddClass($"mud-border-{Color.ToDescriptionString()}", Arrow && Color != Color.Default)
            .AddClass($"mud-theme-{Color.ToDescriptionString()}", Color != Color.Default)
            .AddClass(Class)
            .Build();

        /// <summary>
        /// Displays content right-to-left.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <summary>
        /// The tooltip color.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// The tooltip text.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Behavior)]
        public string? Text { get; set; } = string.Empty;

        /// <summary>
        /// Displays an arrow pointing towards the tooltip content.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public bool Arrow { get; set; } = false;

        /// <summary>
        /// The length of time to animate the opening transition.
        /// </summary>
        /// <remarks>
        /// Defaults to 251ms in <see cref="MudGlobal.TooltipDefaults.Duration"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public double Duration { get; set; } = MudGlobal.TooltipDefaults.Duration.TotalMilliseconds;

        /// <summary>
        /// The amount of time, in milliseconds, to wait from opening the popover before performing the transition. 
        /// </summary>
        /// <remarks>
        /// Defaults to 0ms in <see cref="MudGlobal.TooltipDefaults.Delay"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public double Delay { get; set; } = MudGlobal.TooltipDefaults.Delay.TotalMilliseconds;

        /// <summary>
        /// The location of the tooltip relative to its content.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Placement.Bottom"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public Placement Placement { get; set; } = Placement.Bottom;

        /// <summary>
        /// The content described by this tooltip.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The content of the tooltip.
        /// </summary>
        /// <remarks>
        /// Can contain any valid HTML.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Behavior)]
        public RenderFragment? TooltipContent { get; set; }

        /// <summary>
        /// Displays this tooltip inline with its container.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When <c>false</c>, the content will display as a block element.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public bool Inline { get; set; } = true;

        /// <summary>
        /// Any CSS styles applied to the tooltip.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public string? RootStyle { get; set; }

        /// <summary>
        /// Any CSS classes applied to the tooltip.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public string? RootClass { get; set; }

        /// <summary>
        /// Shows this tooltip when hovering over its content.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public bool ShowOnHover { get; set; } = true;

        /// <summary>
        /// Shows this tooltip when its content is focused.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public bool ShowOnFocus { get; set; } = true;

        /// <summary>
        /// Shows this tooltip when its content is clicked.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tooltip.Appearance)]
        public bool ShowOnClick { get; set; } = false;

        /// <summary>
        /// Shows this tooltip.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Visible { get; set; }

        /// <summary>
        /// Occurs when <see cref="Visible"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public EventCallback<bool> VisibleChanged { get; set; }

        /// <summary>
        /// Prevents this tooltip from being displayed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets whether the tooltip should be shown.
        /// </summary>
        /// <remarks>
        /// The tooltip will be displayed if not disabled, not already visible, and either <see cref="TooltipContent"/> or <see cref="Text"/> is specified.
        /// </remarks>
        internal bool ShowToolTip()
        {
            return !Disabled && (TooltipContent is not null || !string.IsNullOrEmpty(Text));
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            ConvertPlacement();
        }

        internal Task HandlePointerEnterAsync() => ShowOnHover ? _visibleState.SetValueAsync(true) : Task.CompletedTask;

        internal Task HandlePointerLeaveAsync() => ShowOnHover ? _visibleState.SetValueAsync(false) : Task.CompletedTask;

        private Task HandleFocusInAsync()
        {
            return ShowOnFocus ? _visibleState.SetValueAsync(true) : Task.CompletedTask;
        }

        private Task HandleFocusOutAsync()
        {
            return ShowOnFocus ? _visibleState.SetValueAsync(false) : Task.CompletedTask;
        }

        private Task HandlePointerUpAsync()
        {
            return ShowOnClick ? _visibleState.SetValueAsync(!_visibleState.Value) : Task.CompletedTask;
        }

        private Origin ConvertPlacement()
        {
            if (Placement == Placement.Bottom)
            {
                _anchorOrigin = Origin.BottomCenter;
                _transformOrigin = Origin.TopCenter;

                return Origin.BottomCenter;
            }

            if (Placement == Placement.Top)
            {
                _anchorOrigin = Origin.TopCenter;
                _transformOrigin = Origin.BottomCenter;

                return Origin.TopCenter;
            }

            if (Placement == Placement.Left || (Placement == Placement.Start && !RightToLeft) || (Placement == Placement.End && RightToLeft))
            {
                _anchorOrigin = Origin.CenterLeft;
                _transformOrigin = Origin.CenterRight;

                return Origin.CenterLeft;
            }

            if (Placement == Placement.Right || (Placement == Placement.End && !RightToLeft) || (Placement == Placement.Start && RightToLeft))
            {
                _anchorOrigin = Origin.CenterRight;
                _transformOrigin = Origin.CenterLeft;

                return Origin.CenterRight;
            }

            return Origin.BottomCenter;
        }
    }
}
