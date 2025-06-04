﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Interfaces;
using MudBlazor.State;
using MudBlazor.Utilities;
using MudBlazor.Utilities.Debounce;

namespace MudBlazor
{
#nullable enable
    /// <summary>
    /// An interactive menu that displays a list of options.
    /// </summary>
    /// <seealso cref="MudMenuItem" />
    public partial class MudMenu : MudComponentBase, IActivatable, IDisposable
    {
        private readonly ParameterState<bool> _openState;
        private readonly List<MudMenu> _subMenus = [];
        private (double Top, double Left) _openPosition;
        private bool _isPointerOver;
        private bool _isTransient;
        internal DebounceDispatcher _showDebouncer;
        internal DebounceDispatcher _hideDebouncer;

        public MudMenu()
        {
            _showDebouncer = new DebounceDispatcher(MudGlobal.MenuDefaults.HoverDelay);
            _hideDebouncer = new DebounceDispatcher(MudGlobal.MenuDefaults.HoverDelay);

            using var registerScope = CreateRegisterScope();
            _openState = registerScope.RegisterParameter<bool>(nameof(Open))
                .WithParameter(() => Open)
                .WithEventCallback(() => OpenChanged)
                .WithChangeHandler(OnOpenChanged);
        }

        /// <summary>
        /// The CSS class for the root menu container.
        /// </summary>
        protected string Classname =>
            new CssBuilder("mud-menu")
                .AddClass("mud-menu-button-hidden", GetActivatorHidden())
                .AddClass(Class)
                .Build();

        /// <summary>
        /// The CSS class for the menu's popover container.
        /// </summary>
        protected string PopoverClassname =>
            new CssBuilder()
                .AddClass(PopoverClass)
                .AddClass("mud-popover-nested", ParentMenu is not null)
                .AddClass("mud-popover-position-override", PositionAtCursor)
                .Build();

        /// <summary>
        /// The CSS class for the list containing menu items.
        /// </summary>
        protected string ListClassname =>
            new CssBuilder("mud-menu-list")
                .AddClass(ListClass)
                .Build();

        /// <summary>
        /// The CSS class for the activator element (button or custom content).
        /// </summary>
        protected string ActivatorClassname =>
            new CssBuilder("mud-menu-activator")
                .AddClass("mud-disabled", Disabled)
                .Build();

        /// <summary>
        /// Inline styles for positioning the menu at the cursor's location.
        /// </summary>
        [ExcludeFromCodeCoverage]
        [Obsolete($"Will be removed in future, replaced by {nameof(PositionAttributes)}.")]
        protected string Stylename =>
            new StyleBuilder()
                .AddStyle("top", _openPosition.Top.ToPx(), PositionAtCursor)
                .AddStyle("left", _openPosition.Left.ToPx(), PositionAtCursor)
                .Build();

        /// <summary>
        /// Inline data attributes for positioning the menu at the cursor's location.
        /// </summary>
        private Dictionary<string, object> PositionAttributes => new()
        {
            { "data-pc-x", _openPosition.Left.ToString(CultureInfo.InvariantCulture) },
            { "data-pc-y", _openPosition.Top.ToString(CultureInfo.InvariantCulture) }
        };

        /// <summary>
        /// The text shown for this menu.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public string? Label { get; set; }

        /// <summary>
        /// The <c>aria-label</c> for the menu button when <see cref="ActivatorContent"/> is not set.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public string? AriaLabel { get; set; }

        /// <summary>
        /// The CSS classes applied to items in this menu.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public string? ListClass { get; set; }

        /// <summary>
        /// The CSS classes applied to the popover for this menu.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public string? PopoverClass { get; set; }

        /// <summary>
        /// The icon displayed for this menu.
        /// </summary>
        /// <remarks>
        /// When set, this menu will display a <see cref="MudIconButton" />.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public string? Icon { get; set; }

        /// <summary>
        /// The color of the icon when <see cref="Icon"/> is set.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Inherit"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Appearance)]
        public Color IconColor { get; set; } = Color.Inherit;

        /// <summary>
        /// The icon displayed before the text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public string? StartIcon { get; set; }

        /// <summary>
        /// The icon displayed after the text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public string? EndIcon { get; set; }

        /// <summary>
        /// The color of this menu's button when <see cref="Icon"/> is not set.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// The size of this menu's button when <see cref="Icon" /> is not set.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Menu.Appearance)]
        public Size Size { get; set; } = Size.Medium;

        /// <summary>
        /// The display variant to use.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Variant.Text"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Appearance)]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// Applies compact vertical padding to all menu items.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// Expands this menu to the same width as its parent.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public bool FullWidth { get; set; }

        /// <summary>
        /// Sets the maximum allowed height for this menu, when open.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Opens this menu at the cursor's location instead of the button's location.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  Typically used for larger-sized activators.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public bool PositionAtCursor { get; set; }

        /// <summary>
        /// Overrides the default button with a custom component.
        /// </summary>
        /// <remarks>
        /// Can be a <see cref="MudButton"/>, <see cref="MudIconButton"/>, or any other component.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public RenderFragment? ActivatorContent { get; set; }

        /// <summary>
        /// The action which opens the menu, when <see cref="ActivatorContent"/> is set.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="MouseEvent.LeftClick"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public MouseEvent ActivationEvent { get; set; } = MouseEvent.LeftClick;

        /// <summary>
        /// The origin point for the menu's anchor. If set, overrides Nested Menus, and PositionatCursor Anchor points.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Category(CategoryTypes.Popover.Behavior)]
        [Parameter]
        public Origin? AnchorOrigin { get; set; }

        /// <summary>
        /// Sets the direction the menu will open from the anchor.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Origin.TopLeft"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public Origin TransformOrigin { get; set; } = Origin.TopLeft;

        /// <summary>
        /// The behavior of the dropdown popover menu
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DropdownSettings.Fixed" /> false
        /// Defaults to <see cref="DropdownSettings.OverflowBehavior" /> <see cref="OverflowBehavior.FlipOnOpen" />
        /// </remarks>
        [Category(CategoryTypes.Popover.Behavior)]
        [Parameter]
        public DropdownSettings DropdownSettings { get; set; } = new DropdownSettings();

        /// <summary>
        /// Determines the width of the Popover dropdown in relation the parent container.
        /// </summary>
        /// <remarks>
        /// <para>Defaults to <see cref="DropdownWidth.Ignore" />. </para>
        /// <para>When <see cref="DropdownWidth.Relative" />, restricts the max-width of the component to the width of the parent container</para>
        /// <para>When <see cref="DropdownWidth.Adaptive" />, restricts the min-width of the component to the width of the parent container</para>
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Popover.Appearance)]
        public DropdownWidth RelativeWidth { get; set; } = DropdownWidth.Ignore;

        /// <summary>
        /// Prevents the page from scrolling while this menu is open.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public bool LockScroll { get; set; }

        /// <summary>
        /// Prevents the user from interacting with this menu.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Shows a ripple animation when the user clicks the activator button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Appearance)]
        public bool Ripple { get; set; } = true;

        /// <summary>
        /// Displays a drop shadow under the activator button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.Appearance)]
        public bool DropShadow { get; set; } = true;

        /// <summary>
        /// Prevents interaction with background elements while this menu is open.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public bool Modal { get; set; } = true;

        /// <summary>
        /// The <see cref="MudMenuItem" /> components within this menu.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Whether this menu is open and the menu items are visible.
        /// </summary>
        /// <remarks>
        /// When this property changes, <see cref="OpenChanged"/> occurs.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public bool Open { get; set; }

        /// <summary>
        /// Occurs when <see cref="Open"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<bool> OpenChanged { get; set; }

        [CascadingParameter]
        protected MudMenu? ParentMenu { get; set; }

        protected bool GetActivatorHidden() => ActivatorContent is null && string.IsNullOrWhiteSpace(Label) && string.IsNullOrWhiteSpace(Icon);

        /// <summary>
        /// Walk recursively up the menu hierarchy to determine if any parent menu is dense.
        /// </summary>
        internal bool GetDense() => Dense || ParentMenu?.GetDense() == true;

        protected Origin GetAnchorOrigin()
        {
            if (AnchorOrigin is not null)
            {
                return AnchorOrigin.Value;
            }

            if (ParentMenu is not null)
            {
                return Origin.TopRight;
            }
            else if (PositionAtCursor)
            {
                return Origin.TopLeft;
            }

            return Origin.BottomLeft;
        }

        protected void RegisterChild(MudMenu child)
        {
            _subMenus.Add(child);
        }

        protected void UnregisterChild(MudMenu child)
        {
            _subMenus.Remove(child);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ParentMenu?.RegisterChild(this);
        }

        protected Task OnOpenChanged(ParameterChangedEventArgs<bool> args)
        {
            return args.Value ?
                OpenMenuAsync(EventArgs.Empty) :
                CloseMenuAsync();
        }

        /// <summary>
        /// Closes this menu and any descendants if it's a nested menu.
        /// </summary>
        public async Task CloseMenuAsync()
        {
            CancelPendingActions();

            // Recursively close all child menus.
            foreach (var child in _subMenus.Where(m => m._openState.Value))
            {
                await child.CloseMenuAsync();
            }

            await _openState.SetValueAsync(false);
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Closes all menus in the hierarchy, starting from the top-most parent.
        /// </summary>
        public async Task CloseAllMenusAsync()
        {
            // Traverse up the menu hierarchy to find the top-most parent.
            var top = this;
            while (true)
            {
                if (top.ParentMenu is null)
                {
                    break;
                }

                top = top.ParentMenu;
            }

            // Close the top-most menu, which will cascade down to close all its children.
            await top.CloseMenuAsync();
        }

        /// <summary>
        /// Opens the menu or updates its state if it's already open.
        /// </summary>
        /// <param name="args">
        /// <para>The event arguments for the activation event; <see cref="MouseEventArgs"/> or <see cref="TouchEventArgs"/>.</para>
        /// <para>When <see cref="PositionAtCursor"/> is <c>true</c>, the menu will be positioned at these coordinates.</para>
        /// </param>
        /// <param name="transient">If <c>true</c>, the menu will close automatically when the pointer leaves its bounds.</param>
        /// <remarks>
        /// Parents are not automatically opened when a child is opened.
        /// </remarks>
        public async Task OpenMenuAsync(EventArgs args, bool transient = false)
        {
            if (Disabled)
            {
                return;
            }

            _isTransient = transient;

            // Set the menu position if the event has cursor coordinates.
            if (args is MouseEventArgs mouseEventArgs)
            {
                _openPosition = (mouseEventArgs.PageY, mouseEventArgs.PageX);
            }

            await _openState.SetValueAsync(true);
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Closes siblings before opening as a "mouse over" menu.
        /// This is called in place of <see cref="OpenMenuAsync"/> if the menu activator is implicitly rendered for the submenu.
        /// </summary>
        protected async Task OpenSubMenuAsync(EventArgs args)
        {
            // Close siblings (and self) first.
            if (ParentMenu is not null)
            {
                foreach (var sibling in ParentMenu._subMenus.Where(m => m._openState.Value))
                {
                    await sibling.CloseMenuAsync();
                }
            }

            // Open transiently so it will close when the pointer leaves its bounds.
            await OpenMenuAsync(args, true);
        }

        /// <summary>
        /// Toggles the menu's open or closed state.
        /// </summary>
        /// <param name="args">
        /// <para>The event arguments for the activation event; <see cref="MouseEventArgs"/> or <see cref="TouchEventArgs"/>.</para>
        /// <para>When <see cref="PositionAtCursor"/> is <c>true</c>, the menu will be positioned at these coordinates.</para>
        /// </param>
        public Task ToggleMenuAsync(EventArgs args)
        {
            if (Disabled)
            {
                return Task.CompletedTask;
            }

            if (args is MouseEventArgs mouseEventArgs)
            {
                // Determine if the click matches the expected activation event.
                var leftClick = ActivationEvent == MouseEvent.LeftClick && mouseEventArgs.Button == 0;
                var rightClick = ActivationEvent == MouseEvent.RightClick && (mouseEventArgs.Button is -1 or 2); // oncontextmenu = -1, right click = 2.

                // Ignore invalid click types if we're using a click-based activation event.
                if (!leftClick && !rightClick && ActivationEvent != MouseEvent.MouseOver)
                {
                    return Task.CompletedTask;
                }
            }

            // Toggle the menu's state; close if open, open if closed.
            return _openState.Value
                ? CloseMenuAsync()
                : OpenMenuAsync(args);
        }

        private bool IsHoverable(PointerEventArgs args)
        {
            // If hover isn't explicitly enabled (or implicitly by being a submenu) there's no work to be done.
            if (ActivationEvent != MouseEvent.MouseOver && ParentMenu is null)
            {
                return false;
            }

            // The click event will conflict with this one on devices that can't hover so we'll return so we only handle one.
            if (args.PointerType is "touch" or "pen")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the pointer entering either the activator or the menu list.
        /// </summary>
        internal async Task PointerEnterAsync(PointerEventArgs args)
        {
            _isPointerOver = true;

            if (!IsHoverable(args))
            {
                return;
            }

            // Cancel any pending hide operation
            _hideDebouncer.Cancel();

            // Schedule the show operation with debouncing
            await _showDebouncer.DebounceAsync(async () =>
            {
                if (!_openState.Value)
                {
                    await OpenSubMenuAsync(args);
                }
            });
        }

        /// <summary>
        /// Handles the pointer leaving either the activator or the menu list.
        /// </summary>
        internal async Task PointerLeaveAsync(PointerEventArgs args)
        {
            _isPointerOver = false;
            var isSubmenu = ParentMenu is not null;
            if (!isSubmenu && ActivationEvent != MouseEvent.MouseOver)
            {
                return; // main menu that doesn't use mouseover
            }

            if (!_isTransient || !IsHoverable(args))
            {
                return;
            }

            // Cancel any pending show operation
            _showDebouncer.Cancel();

            // Schedule the hide operation with debouncing
            await _hideDebouncer.DebounceAsync(async () =>
            {
                if (!HasPointerOver(this))
                {
                    await CloseMenuAsync();
                }
            });
        }

        protected bool HasPointerOver(MudMenu menu)
        {
            if (menu._isPointerOver)
                return true;

            // Recursively check all child submenus.
            return menu._subMenus.Any(HasPointerOver);
        }

        /// <summary>
        /// Use if another action is started or explicitly called.
        /// </summary>
        private void CancelPendingActions()
        {
            _showDebouncer.Cancel();
            _hideDebouncer.Cancel();
        }

        /// <summary>
        /// Implementation of IActivatable.Activate, toggles the menu.
        /// </summary>
        void IActivatable.Activate(object activator, MouseEventArgs args)
        {
            if (activator is MudBaseButton activatorButton &&
                (activatorButton.Class?.Contains("mud-no-activator") ?? false))
            {
                return;
            }
            ToggleMenuAsync(args).CatchAndLog();
        }


        /// <summary>
        /// Disposes managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates if managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelPendingActions();

                ParentMenu?.UnregisterChild(this);
            }
        }

        /// <summary>
        /// Releases resources used by the component.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
