﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor.Resources;
using MudBlazor.Utilities;

namespace MudBlazor
{
    /// <summary>
    /// A component for selecting time values.
    /// </summary>
    /// <seealso cref="MudDatePicker"/>
    /// <seealso cref="MudDateRangePicker"/>
    public partial class MudTimePicker : MudPicker<TimeSpan?>
    {
        private const string Format24Hours = "HH:mm";
        private const string Format12Hours = "hh:mm tt";

        public MudTimePicker() : base(new DefaultConverter<TimeSpan?>())
        {
            Converter.GetFunc = OnGet;
            Converter.SetFunc = OnSet;
            ((DefaultConverter<TimeSpan?>)Converter).Format = Format24Hours;
            AdornmentIcon = Icons.Material.Filled.AccessTime;
        }

        private string OnSet(TimeSpan? timespan)
        {
            if (timespan == null)
            {
                return string.Empty;
            }

            var time = DateTime.Today.Add(timespan.Value);

            return time.ToString(((DefaultConverter<TimeSpan?>)Converter).Format, Culture);
        }

        private TimeSpan? OnGet(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (DateTime.TryParseExact(value, ((DefaultConverter<TimeSpan?>)Converter).Format, Culture, DateTimeStyles.None, out var time))
            {
                return time.TimeOfDay;
            }

            var m = AmPmRegularExpression().Match(value);
            if (m.Success)
            {
                if (DateTime.TryParseExact(value, Format12Hours, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                {
                    return time.TimeOfDay;
                }
            }
            else
            {
                if (DateTime.TryParseExact(value, Format24Hours, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                {
                    return time.TimeOfDay;
                }
            }

            HandleParsingError();
            return null;
        }

        private void HandleParsingError()
        {
            const string ParsingErrorMessage = LanguageResource.Converter_InvalidTimeSpan;
            Converter.GetError = true;
            Converter.GetErrorMessage = (ParsingErrorMessage, []);
            Converter.OnError?.Invoke(ParsingErrorMessage, []);
        }

        private bool _amPm = false;
        private OpenTo _currentView;
        private string _timeFormat = string.Empty;

        internal TimeSpan? TimeIntermediate { get; private set; }

        /// <summary>
        /// The initial view for this picker.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="OpenTo.Hours"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public OpenTo OpenTo { get; set; } = OpenTo.Hours;

        /// <summary>
        /// Controls which values can be edited.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TimeEditMode.Normal"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public TimeEditMode TimeEditMode { get; set; } = TimeEditMode.Normal;

        /// <summary>
        /// The amount of time, in milliseconds, to wait before closing the picker.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>200</c>. The delay gives users a moment to see the selected time before the popover disappears.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int ClosingDelay { get; set; } = 200;

        /// <summary>
        /// Closes this picker when the value is set or cleared.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When <c>true</c> and <c>PickerActions</c> are defined, 
        /// the hour and the minutes can be selected and the drop-down will close without having to 
        /// click any of the action buttons.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public bool AutoClose { get; set; }

        /// <summary>
        /// The step interval when selecting minutes.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>. For example: a value of <c>15</c> would allow minutes <c>0</c>, <c>15</c>, 
        /// <c>30</c>, and <c>45</c> be selected.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int MinuteSelectionStep { get; set; } = 1;

        /// <summary>
        /// Shows a 12-hour selection clock.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.<br />
        /// When <c>true</c>, hours 1-12 are displayed with an AM or PM marker.<br />
        /// When <c>false</c>, hours 0-23 are displayed.<br />
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool AmPm
        {
            get => _amPm;
            set
            {
                if (value == _amPm)
                {
                    return;
                }

                _amPm = value;

                if (Converter is DefaultConverter<TimeSpan?> defaultConverter && string.IsNullOrWhiteSpace(_timeFormat))
                {
                    defaultConverter.Format = AmPm ? Format12Hours : Format24Hours;
                }

                Touched = true;
                SetTextAsync(Converter.Set(_value), false).CatchAndLog();
            }
        }

        /// <summary>
        /// The format applied to time values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>hh:mm tt</c> when <see cref="AmPm"/> is <c>true</c>, otherwise <c>HH:mm</c>.<br />
        /// Format strings are typically a combination of these characters:<br />
        /// * <c>h</c> (lowercase) for hours in 12-hour time, <br />
        /// * <c>H</c> (uppercase) for hours in 24-hour time, <br />
        /// * <c>m</c> for minutes,<br />
        /// * <c>tt</c> for AM/PM markers.<br />
        /// For example: <c>h:mm tt</c> would display <c>6:32 PM</c>, and <c>HH:mm</c> would display <c>18:32</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public string TimeFormat
        {
            get => _timeFormat;
            set
            {
                if (_timeFormat == value)
                {
                    return;
                }

                _timeFormat = value;
                if (Converter is DefaultConverter<TimeSpan?> defaultConverter)
                {
                    defaultConverter.Format = _timeFormat;
                }

                Touched = true;
                SetTextAsync(Converter.Set(_value), false).CatchAndLog();
            }
        }

        /// <summary>
        /// The currently selected time.
        /// </summary>
        /// <remarks>
        /// When this value changes, <see cref="TimeChanged"/> occurs.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Data)]
        public TimeSpan? Time
        {
            get => _value;
            set => SetTimeAsync(value, true).CatchAndLog();
        }

        /// <summary>
        /// Sets the selected time value.
        /// </summary>
        /// <param name="time">The new value to set.</param>
        /// <param name="updateValue">When <c>true</c>, the <c>Text</c> will also be updated.</param>
        protected async Task SetTimeAsync(TimeSpan? time, bool updateValue)
        {
            if (_value != time)
            {
                Touched = true;
                TimeIntermediate = time;
                _value = time;
                if (updateValue)
                {
                    await SetTextAsync(Converter.Set(_value), false);
                }

                UpdateTimeSetFromTime();
                await TimeChanged.InvokeAsync(_value);
                await BeginValidateAsync();
                FieldChanged(_value);
            }
        }

        /// <summary>
        /// Occurs when <see cref="Time"/> has changed.
        /// </summary>
        [Parameter] public EventCallback<TimeSpan?> TimeChanged { get; set; }

        /// <inheritdoc />
        protected override Task StringValueChangedAsync(string value)
        {
            Touched = true;

            // Update the time property (without updating back the Value property)
            return SetTimeAsync(Converter.Get(value), false);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage] // The last line cannot be tested.
        protected override async Task OnPickerOpenedAsync()
        {
            await base.OnPickerOpenedAsync();
            _currentView = TimeEditMode switch
            {
                TimeEditMode.Normal => OpenTo,
                TimeEditMode.OnlyHours => OpenTo.Hours,
                TimeEditMode.OnlyMinutes => OpenTo.Minutes,
                _ => _currentView
            };
        }

        /// <inheritdoc />
        protected internal override Task SubmitAsync()
        {
            if (GetReadOnlyState())
            {
                return Task.CompletedTask;
            }

            Time = TimeIntermediate;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override async Task ClearAsync(bool close = true)
        {
            TimeIntermediate = null;
            await SetTimeAsync(null, true);

            if (AutoClose)
            {
                await CloseAsync(false);
            }
        }

        /// <summary>
        /// Gets the hour portion of the selected time.
        /// </summary>
        /// <returns>A two-character string depending on whether <see cref="AmPm"/> is set, or <c>--</c> if no value is set.</returns>
        private string GetHourString()
        {
            if (TimeIntermediate == null)
            {
                return "--";
            }

            var h = AmPm ? TimeIntermediate.Value.ToAmPmHour() : TimeIntermediate.Value.Hours;
            return $"{Math.Min(23, Math.Max(0, h)):D2}";
        }

        /// <summary>
        /// Gets the minute portion of the selected time.
        /// </summary>
        /// <returns>A two-digit string for minutes, or <c>--</c> if no value is set.</returns>
        private string GetMinuteString()
        {
            if (TimeIntermediate == null)
            {
                return "--";
            }

            return $"{Math.Min(59, Math.Max(0, TimeIntermediate.Value.Minutes)):D2}";
        }

        private Task UpdateTimeAsync()
        {
            TimeIntermediate = new TimeSpan(_timeSet.Hour, _timeSet.Minute, 0);
            if ((PickerVariant == PickerVariant.Static && PickerActions == null) || (PickerActions != null && AutoClose))
            {
                return SubmitAsync();
            }

            return Task.CompletedTask;
        }

        private async Task OnHourClickAsync()
        {
            _currentView = OpenTo.Hours;
            await FocusAsync();
        }

        private async Task OnMinutesClick()
        {
            _currentView = OpenTo.Minutes;
            await FocusAsync();
        }

        private async Task OnAmClickedAsync()
        {
            _timeSet.Hour %= 12;  // "12:-- am" is "00:--" in 24h.
            await UpdateTimeAsync();
            await FocusAsync();
        }

        private async Task OnPmClickedAsync()
        {
            if (_timeSet.Hour <= 12) // "12:-- pm" is "12:--" in 24h.
            {
                _timeSet.Hour += 12;
            }

            _timeSet.Hour %= 24;
            await UpdateTimeAsync();
            await FocusAsync();
        }

        protected string ToolbarClassname =>
            new CssBuilder("mud-picker-timepicker-toolbar")
                .AddClass("mud-picker-timepicker-toolbar-landscape", Orientation == Orientation.Landscape && PickerVariant == PickerVariant.Static)
                .AddClass(Class)
                .Build();

        protected string HoursButtonClassname =>
            new CssBuilder("mud-timepicker-button")
                .AddClass("mud-timepicker-toolbar-text", _currentView == OpenTo.Minutes)
                .Build();

        protected string MinuteButtonClassname =>
            new CssBuilder("mud-timepicker-button")
                .AddClass("mud-timepicker-toolbar-text", _currentView == OpenTo.Hours)
                .Build();

        protected string AmButtonClassname =>
            new CssBuilder("mud-timepicker-button")
                .AddClass("mud-timepicker-toolbar-text", !IsAm) // gray it out.
                .Build();

        protected string PmButtonClassname =>
            new CssBuilder("mud-timepicker-button")
                .AddClass("mud-timepicker-toolbar-text", !IsPm) // gray it out.
                .Build();

        private string HourDialClassname =>
            new CssBuilder("mud-time-picker-hour")
                .AddClass("mud-time-picker-dial")
                .AddClass("mud-time-picker-dial-out", _currentView != OpenTo.Hours)
                .AddClass("mud-time-picker-dial-hidden", _currentView != OpenTo.Hours)
                .Build();

        private string MinuteDialClassname =>
            new CssBuilder("mud-time-picker-minute")
                .AddClass("mud-time-picker-dial")
                .AddClass("mud-time-picker-dial-out", _currentView != OpenTo.Minutes)
                .AddClass("mud-time-picker-dial-hidden", _currentView != OpenTo.Minutes)
                .Build();

        private bool IsAm => _timeSet.Hour is >= 00 and < 12; // AM is 00:00 to 11:59.
        private bool IsPm => _timeSet.Hour is >= 12 and < 24; // PM is 12:00 to 23:59.

        private string GetClockPinColor()
        {
            return $"mud-picker-time-clock-pin mud-{Color.ToDescriptionString()}";
        }

        private string GetClockPointerColor()
        {
            if (PointerMoving)
            {
                return $"mud-picker-time-clock-pointer mud-{Color.ToDescriptionString()}";
            }
            else
            {
                return $"mud-picker-time-clock-pointer mud-picker-time-clock-pointer-animation mud-{Color.ToDescriptionString()}";
            }
        }

        private string GetClockPointerThumbColor()
        {
            var deg = GetDeg();
            if (deg % 30 == 0)
            {
                return $"mud-picker-time-clock-pointer-thumb mud-onclock-text mud-onclock-primary mud-{Color.ToDescriptionString()}";
            }
            else
            {
                return $"mud-picker-time-clock-pointer-thumb mud-onclock-minute mud-{Color.ToDescriptionString()}-text";
            }
        }

        private string GetNumberColor(int value)
        {
            if (_currentView == OpenTo.Hours)
            {
                var h = _timeSet.Hour;

                if (AmPm)
                {
                    h = _timeSet.Hour % 12;
                    if (_timeSet.Hour % 12 == 0)
                    {
                        h = 12;
                    }
                }

                if (h == value)
                {
                    return $"mud-clock-number mud-theme-{Color.ToDescriptionString()}";
                }
            }
            else if (_currentView == OpenTo.Minutes && _timeSet.Minute == value)
            {
                return $"mud-clock-number mud-theme-{Color.ToDescriptionString()}";
            }

            return "mud-clock-number";
        }

        private double GetDeg()
        {
            double deg = 0;

            if (_currentView == OpenTo.Hours)
            {
                deg = _timeSet.Hour * 30 % 360;
            }

            if (_currentView == OpenTo.Minutes)
            {
                deg = _timeSet.Minute * 6 % 360;
            }

            return deg;
        }

        private static string GetTransform(double angle, double radius, double offsetX, double offsetY)
        {
            angle = angle / 180 * Math.PI;
            var x = ((Math.Sin(angle) * radius) + offsetX).ToString("F3", CultureInfo.InvariantCulture);
            var y = (((Math.Cos(angle) + 1) * radius) + offsetY).ToString("F3", CultureInfo.InvariantCulture);
            return $"transform: translate({x}px, {y}px);";
        }

        private string GetPointerRotation()
        {
            double deg = 0;

            if (_currentView == OpenTo.Hours)
            {
                deg = _timeSet.Hour * 30 % 360;
            }

            if (_currentView == OpenTo.Minutes)
            {
                deg = _timeSet.Minute * 6 % 360;
            }

            return $"rotateZ({deg}deg);";
        }

        private string GetPointerHeight()
        {
            var height = 40;

            if (_currentView == OpenTo.Minutes)
            {
                height = 40;
            }

            if (_currentView == OpenTo.Hours)
            {
                if (!AmPm && _timeSet.Hour > 0 && _timeSet.Hour < 13)
                {
                    height = 26;
                }
                else
                {
                    height = 40;
                }
            }

            return $"{height}%;";
        }

        private readonly SetTime _timeSet = new();
        private int _initialHour;
        private int _initialMinute;
        private DotNetObjectReference<MudTimePicker> _dotNetRef;
        private string _clockElementReferenceId;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            AdornmentAriaLabel ??= Localizer[LanguageResource.MudTimePicker_Open];
            UpdateTimeSetFromTime();
            _currentView = OpenTo;
            _initialHour = _timeSet.Hour;
            _initialMinute = _timeSet.Minute;
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        protected ElementReference ClockElementReference { get; private set; }

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            // Initialize the pointer events for the clock every time it's created (ex: popover opening and closing).
            if (ClockElementReference.Id != _clockElementReferenceId)
            {
                _clockElementReferenceId = ClockElementReference.Id;

                await JsRuntime.InvokeVoidAsyncWithErrorHandling("mudTimePicker.initPointerEvents", ClockElementReference, _dotNetRef);
            }
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();

            if (IsJSRuntimeAvailable)
            {
                await JsRuntime.InvokeVoidAsyncWithErrorHandling("mudTimePicker.destroyPointerEvents", ClockElementReference);
            }

            _dotNetRef?.Dispose();
        }

        private void UpdateTimeSetFromTime()
        {
            if (TimeIntermediate == null)
            {
                _timeSet.Hour = 0;
                _timeSet.Minute = 0;
                return;
            }

            _timeSet.Hour = TimeIntermediate.Value.Hours;
            _timeSet.Minute = TimeIntermediate.Value.Minutes;
        }

        /// <summary>
        /// Whether the pointer button is held down and moving.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, clock animations are disabled.
        /// </remarks>
        public bool PointerMoving { get; set; }

        /// <summary>
        /// Updates the position of the hands on the clock.
        /// </summary>
        /// <param name="value">The minute or hour.</param>
        /// <param name="pointerMoving">Is the pointer being moved?</param>
        /// <remarks>
        /// This method is invoked via JavaScript.
        /// </remarks>
        [JSInvokable]
        public async Task SelectTimeFromStick(int value, bool pointerMoving)
        {
            if (value == -1)
            {
                // This means a stick wasn't the target (which shouldn't happen).
                return;
            }

            PointerMoving = pointerMoving;

            // Update the .NET properties from the JavaScript events.
            if (_currentView == OpenTo.Minutes)
            {
                var minute = RoundToStepInterval(value);
                _timeSet.Minute = minute;
            }
            else if (_currentView == OpenTo.Hours)
            {
                _timeSet.Hour = HourAmPm(value);
            }

            await UpdateTimeAsync();

            // Manually update because the event won't do it from JavaScript.
            StateHasChanged();
        }

        /// <summary>
        /// Performs the click action for the sticks.
        /// </summary>
        /// <param name="value">The minute or hour.</param>
        /// <remarks>
        /// This method is invoked via JavaScript.
        /// </remarks>
        [JSInvokable]
        public async Task OnStickClick(int value)
        {
            // The pointer is up and not moving so animations can be enabled again.
            PointerMoving = false;

            // Clicking a stick will submit the time.
            if (_currentView == OpenTo.Minutes)
            {
                await SubmitAndCloseAsync();
            }
            else if (_currentView == OpenTo.Hours)
            {
                if (TimeEditMode == TimeEditMode.Normal)
                {
                    _currentView = OpenTo.Minutes;
                }
                else if (TimeEditMode == TimeEditMode.OnlyHours)
                {
                    await SubmitAndCloseAsync();
                }
            }

            // Manually update because the event won't do it from JavaScript.
            StateHasChanged();
        }

        private int HourAmPm(int hour)
        {
            if (AmPm)
            {
                if (IsAm && hour == 12)
                {
                    return 0;
                }
                else if (IsPm && hour < 12)
                {
                    return hour + 12;
                }
            }

            return hour;
        }

        private int RoundToStepInterval(int value)
        {
            if (MinuteSelectionStep > 1) // Ignore if step is less than or equal to 1.
            {
                var interval = MinuteSelectionStep % 60;
                value = (value + (interval / 2)) / interval * interval;
                if (value == 60) // For when it rounds up to 60.
                {
                    value = 0;
                }
            }

            return value;
        }

        protected async Task SubmitAndCloseAsync()
        {
            if (PickerActions == null || AutoClose)
            {
                await SubmitAsync();

                if (PickerVariant != PickerVariant.Static)
                {
                    await Task.Delay(ClosingDelay);
                    await CloseAsync(false);
                }
            }
        }

        protected internal override async Task OnHandleKeyDownAsync(KeyboardEventArgs obj)
        {
            if (GetDisabledState() || GetReadOnlyState())
            {
                return;
            }

            await base.OnHandleKeyDownAsync(obj);

            switch (obj.Key)
            {
                case "ArrowRight":
                    if (Open)
                    {
                        if (obj.CtrlKey)
                        {
                            await ChangeHourAsync(1);
                        }
                        else if (obj.ShiftKey)
                        {
                            if (_timeSet.Minute > 55)
                            {
                                await ChangeHourAsync(1);
                            }

                            await ChangeMinuteAsync(5);
                        }
                        else
                        {
                            if (_timeSet.Minute == 59)
                            {
                                await ChangeHourAsync(1);
                            }

                            await ChangeMinuteAsync(1);
                        }
                    }

                    break;
                case "ArrowLeft":
                    if (Open)
                    {
                        if (obj.CtrlKey)
                        {
                            await ChangeHourAsync(-1);
                        }
                        else if (obj.ShiftKey)
                        {
                            if (_timeSet.Minute < 5)
                            {
                                await ChangeHourAsync(-1);
                            }

                            await ChangeMinuteAsync(-5);
                        }
                        else
                        {
                            if (_timeSet.Minute == 0)
                            {
                                await ChangeHourAsync(-1);
                            }

                            await ChangeMinuteAsync(-1);
                        }
                    }

                    break;
                case "ArrowUp":
                    if (!Open && !Editable)
                    {
                        Open = true;
                    }
                    else if (obj.AltKey)
                    {
                        Open = false;
                    }
                    else if (obj.ShiftKey)
                    {
                        await ChangeHourAsync(5);
                    }
                    else
                    {
                        await ChangeHourAsync(1);
                    }

                    break;
                case "ArrowDown":
                    if (!Open && !Editable)
                    {
                        Open = true;
                    }
                    else if (obj.ShiftKey)
                    {
                        await ChangeHourAsync(-5);
                    }
                    else
                    {
                        await ChangeHourAsync(-1);
                    }

                    break;
                case "Escape":
                    await ReturnTimeBackUpAsync();
                    break;
                case "Enter":
                case "NumpadEnter":
                    if (!Open)
                    {
                        await OpenAsync();
                    }
                    else
                    {
                        await SubmitAsync();
                        await CloseAsync();
                        _inputReference?.SetText(Text);
                    }

                    break;
                case " ":
                    if (!Editable)
                    {
                        if (!Open)
                        {
                            await OpenAsync();
                        }
                        else
                        {
                            await SubmitAsync();
                            await CloseAsync();
                            _inputReference?.SetText(Text);
                        }
                    }

                    break;
            }

            StateHasChanged();
        }

        protected Task ChangeMinuteAsync(int minute)
        {
            _currentView = OpenTo.Minutes;
            _timeSet.Minute = (_timeSet.Minute + minute + 60) % 60;

            return UpdateTimeAsync();
        }

        protected Task ChangeHourAsync(int hour)
        {
            _currentView = OpenTo.Hours;
            _timeSet.Hour = (_timeSet.Hour + hour + 24) % 24;

            return UpdateTimeAsync();
        }

        protected async Task ReturnTimeBackUpAsync()
        {
            if (Time == null)
            {
                TimeIntermediate = null;
            }
            else
            {
                _timeSet.Hour = Time.Value.Hours;
                _timeSet.Minute = Time.Value.Minutes;

                await UpdateTimeAsync();
            }
        }

        private record SetTime
        {
            public int Hour { get; set; }

            public int Minute { get; set; }
        }

        [GeneratedRegex("AM|PM", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex AmPmRegularExpression();
    }
}
