﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor.Extensions;
using MudBlazor.Utilities;

namespace MudBlazor
{
    /// <summary>
    /// Represents a base class for designing date picker components.
    /// </summary>
    public abstract partial class MudBaseDatePicker : MudPicker<DateTime?>
    {
        private readonly string _mudPickerCalendarContentElementId;
        private bool _dateFormatTouched;

        protected MudBaseDatePicker() : base(new DefaultConverter<DateTime?>
        {
            Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
            Culture = CultureInfo.CurrentCulture
        })
        {
            _mudPickerCalendarContentElementId = Identifier.Create();
        }

        [Inject]
        protected IScrollManager ScrollManager { get; set; }

        [Inject]
        private IJsApiService JsApiService { get; set; }

        [Inject]
        protected TimeProvider TimeProvider { get; set; }

        /// <summary>
        /// The maximum selectable date.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Validation)]
        public DateTime? MaxDate { get; set; }

        /// <summary>
        /// The minimum selectable date.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Validation)]
        public DateTime? MinDate { get; set; }

        /// <summary>
        /// The initial view to display.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="OpenTo.Date"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public OpenTo OpenTo { get; set; } = OpenTo.Date;

        /// <summary>
        /// The format for selected dates.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public string DateFormat
        {
            get
            {
                return (Converter as DefaultConverter<DateTime?>)?.Format;
            }
            set
            {
                if (Converter is DefaultConverter<DateTime?> defaultConverter)
                {
                    defaultConverter.Format = value;
                    _dateFormatTouched = true;
                }
                DateFormatChangedAsync(value);
            }
        }

        /// <summary>
        /// Occurs when the <see cref="DateFormat"/> has changed.
        /// </summary>
        protected virtual Task DateFormatChangedAsync(string newFormat)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override bool SetCulture(CultureInfo value)
        {
            if (!base.SetCulture(value))
                return false;

            if (!_dateFormatTouched && Converter is DefaultConverter<DateTime?> defaultConverter)
                defaultConverter.Format = value.DateTimeFormat.ShortDatePattern;

            return true;
        }

        /// <summary>
        /// The day representing the first day of the week.
        /// </summary>
        /// <remarks>
        /// Defaults to the current culture's <c>DateTimeFormat.FirstDayOfWeek</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public DayOfWeek? FirstDayOfWeek { get; set; } = null;

        /// <summary>
        /// The current month shown in the date picker.
        /// </summary>
        /// <remarks>
        /// Defaults to the current month.<br />
        /// When bound via <c>@bind-PickerMonth</c>, controls the initial month displayed.  This value is always the first day of a month.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public DateTime? PickerMonth
        {
            get => _picker_month;
            set
            {
                if (value == _picker_month)
                    return;
                _picker_month = value;
                InvokeAsync(StateHasChanged);
                PickerMonthChanged.InvokeAsync(value);
            }
        }

        private DateTime? _picker_month;

        /// <summary>
        /// Represents the currently selected date
        /// </summary>
        /// <remarks>
        /// This date is highlighted in the UI
        /// </remarks>
        protected DateTime? HighlightedDate { get; set; }

        /// <summary>
        /// Occurs when <see cref="PickerMonth"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<DateTime?> PickerMonthChanged { get; set; }

        /// <summary>
        /// The delay, in milliseconds, before closing the picker after a value is selected.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>100</c>.<br />
        /// This delay helps the user see that a date has been selected before the popover disappears.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int ClosingDelay { get; set; } = 100;

        /// <summary>
        /// The number of months to display in the calendar.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int DisplayMonths { get; set; } = 1;

        /// <summary>
        /// The maximum number of months allowed in one row.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.<br />
        /// When <c>null</c>, the <see cref="DisplayMonths"/> is used.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerAppearance)]
        public int? MaxMonthColumns { get; set; }

        /// <summary>
        /// The start month when opening the picker. 
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public DateTime? StartMonth { get; set; }

        /// <summary>
        /// Shows week numbers at the start of each week.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public bool ShowWeekNumbers { get; set; }

        /// <summary>
        /// The format of the selected date in the title.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>ddd, dd MMM</c>.<br />
        /// Supported date formats can be found here: <see href="https://learn.microsoft.com/dotnet/standard/base-types/standard-date-and-time-format-strings"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public string TitleDateFormat { get; set; } = "ddd, dd MMM";

        /// <summary>
        /// Closes this picker when a value is selected.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public bool AutoClose { get; set; }

        /// <summary>
        /// The function used to disable one or more dates.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.<br />
        /// When set, a date will be disabled if the function returns <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Validation)]
        public Func<DateTime, bool> IsDateDisabledFunc
        {
            get => _isDateDisabledFunc;
            set
            {
                _isDateDisabledFunc = value ?? (_ => false);
            }
        }
        private Func<DateTime, bool> _isDateDisabledFunc = _ => false;

        /// <summary>
        /// The function which returns CSS classes for a date.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public Func<DateTime, string> AdditionalDateClassesFunc { get; set; }

        /// <summary>
        /// The icon for the button that navigates to the previous month or year.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.ChevronLeft"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerAppearance)]
        public string PreviousIcon { get; set; } = Icons.Material.Filled.ChevronLeft;

        /// <summary>
        /// The icon for the button which navigates to the next month or year.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.ChevronRight"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerAppearance)]
        public string NextIcon { get; set; } = Icons.Material.Filled.ChevronRight;

        /// <summary>
        /// The year to use, which cannot be changed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int? FixYear { get; set; }

        /// <summary>
        /// The month to use, which cannot be changed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int? FixMonth { get; set; }

        /// <summary>
        /// The day to use, which cannot be changed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int? FixDay { get; set; }

        protected virtual bool IsRange { get; } = false;

        protected OpenTo CurrentView;

        protected override async Task OnPickerOpenedAsync()
        {
            await base.OnPickerOpenedAsync();
            if (Editable && Text != null)
            {
                var a = Converter.Get(Text);
                if (a.HasValue)
                {
                    a = new DateTime(a.Value.Year, a.Value.Month, 1);
                    PickerMonth = a;
                }
            }
            if (OpenTo == OpenTo.Date && FixDay.HasValue && FixMonth.HasValue)
            {
                OpenTo = OpenTo.Year;
            }
            if (OpenTo == OpenTo.Date && FixDay.HasValue)
            {
                OpenTo = OpenTo.Month;
            }
            CurrentView = OpenTo;
            if (CurrentView == OpenTo.Year)
                _scrollToYearAfterRender = true;
        }

        /// <summary>
        /// Get the first of the month to display
        /// </summary>
        protected DateTime GetMonthStart(int month)
        {
            var monthStartDate = _picker_month ?? DateTime.Today.StartOfMonth(Culture);
            var correctYear = FixYear ?? monthStartDate.Year;
            var correctMonth = FixMonth ?? monthStartDate.Month;
            monthStartDate = new DateTime(correctYear, correctMonth, monthStartDate.Day, 0, 0, 0, DateTimeKind.Utc);

            // Return the min supported datetime of the calendar when this is year 1 and first month!
            if (_picker_month is { Year: 1, Month: 1 })
            {
                return Culture.Calendar.MinSupportedDateTime;
            }

            if (_picker_month.HasValue && _picker_month.Value.Year == 9999 && _picker_month.Value.Month == 12 && month >= 1)
            {
                return Culture.Calendar.MaxSupportedDateTime;
            }
            return Culture.Calendar.AddMonths(monthStartDate, month);
        }

        /// <summary>
        /// Get the last of the month to display
        /// </summary>
        protected DateTime GetMonthEnd(int month)
        {
            var monthStartDate = _picker_month ?? DateTime.Today.StartOfMonth(Culture);
            return Culture.Calendar.AddMonths(monthStartDate, month).EndOfMonth(Culture);
        }

        protected DayOfWeek GetFirstDayOfWeek()
        {
            if (FirstDayOfWeek.HasValue)
                return FirstDayOfWeek.Value;
            return Culture.DateTimeFormat.FirstDayOfWeek;
        }

        /// <summary>
        /// Gets the n-th week of the currently displayed month. 
        /// </summary>
        /// <param name="month">offset from _picker_month</param>
        /// <param name="index">between 0 and 4</param>
        protected IEnumerable<DateTime> GetWeek(int month, int index)
        {
            if (index is < 0 or > 5)
                throw new ArgumentException("Index must be between 0 and 5");
            var month_first = GetMonthStart(month);
            if ((Culture.Calendar.MaxSupportedDateTime - month_first).Days >= index * 7)
            {
                var week_first = month_first.AddDays(index * 7).StartOfWeek(GetFirstDayOfWeek());
                for (var i = 0; i < 7; i++)
                {
                    if ((Culture.Calendar.MaxSupportedDateTime - week_first).Days >= i)
                        yield return week_first.AddDays(i);
                    else
                        yield return Culture.Calendar.MaxSupportedDateTime;
                }
            }
        }

        private string GetWeekNumber(int month, int index)
        {
            if (index is < 0 or > 5)
                throw new ArgumentException("Index must be between 0 and 5");
            var month_first = GetMonthStart(month);
            var week_first = month_first.AddDays(index * 7).StartOfWeek(GetFirstDayOfWeek());
            //january 1st
            if (month_first.Month == 1 && index == 0)
            {
                week_first = month_first;
            }

            if (week_first.Month != month_first.Month && week_first.AddDays(6).Month != month_first.Month)
                return "";

            return Culture.Calendar.GetWeekOfYear(week_first,
                Culture.DateTimeFormat.CalendarWeekRule, FirstDayOfWeek ?? Culture.DateTimeFormat.FirstDayOfWeek).ToString();
        }

        protected virtual OpenTo? GetNextView()
        {
            OpenTo? nextView = CurrentView switch
            {
                OpenTo.Year => !FixMonth.HasValue ? OpenTo.Month : !FixDay.HasValue ? OpenTo.Date : null,
                OpenTo.Month => !FixDay.HasValue ? OpenTo.Date : null,
                _ => null,
            };
            return nextView;
        }

        protected virtual async Task SubmitAndCloseAsync()
        {
            if (PickerActions == null)
            {
                await SubmitAsync();

                if (PickerVariant != PickerVariant.Static)
                {
                    await Task.Delay(ClosingDelay);
                    await CloseAsync(false);
                }
            }
        }

        protected virtual bool IsDayDisabled(DateTime date)
        {
            return date < MinDate ||
                   date > MaxDate ||
                   IsDateDisabledFunc(date);
        }

        protected abstract string GetDayClasses(int month, DateTime day);

        /// <summary>
        /// User clicked on a day
        /// </summary>
        protected abstract Task OnDayClickedAsync(DateTime dateTime);

        /// <summary>
        /// user clicked on a month
        /// </summary>
        /// <param name="month"></param>
        protected virtual Task OnMonthSelectedAsync(DateTime month)
        {
            PickerMonth = month;
            var nextView = GetNextView();
            if (nextView != null)
            {
                CurrentView = (OpenTo)nextView;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// user clicked on a year
        /// </summary>
        /// <param name="year"></param>
        protected virtual Task OnYearClickedAsync(int year)
        {
            var current = GetMonthStart(0);
            PickerMonth = new DateTime(year, current.Month, 1, Culture.Calendar);
            var nextView = GetNextView();
            if (nextView != null)
            {
                CurrentView = (OpenTo)nextView;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// user clicked on a month
        /// </summary>
        protected virtual void OnMonthClicked(int month)
        {
            CurrentView = OpenTo.Month;
            _picker_month = _picker_month?.AddMonths(month);
            StateHasChanged();
        }

        /// <summary>
        /// Check if month is disabled
        /// </summary>
        /// <param name="month">Month given with first day of the month</param>
        /// <returns>True if month should be disabled, false otherwise</returns>
        private bool IsMonthDisabled(DateTime month)
        {
            if (!FixDay.HasValue)
            {
                return month.EndOfMonth(Culture) < MinDate || month > MaxDate;
            }
            if (DateTime.DaysInMonth(month.Year, month.Month) < FixDay!.Value)
            {
                return true;
            }
            var day = new DateTime(month.Year, month.Month, FixDay!.Value);
            return day < MinDate || day > MaxDate || IsDateDisabledFunc(day);
        }

        /// <summary>
        /// return Mo, Tu, We, Th, Fr, Sa, Su in the right culture
        /// </summary>
        protected IEnumerable<string> GetAbbreviatedDayNames()
        {
            var dayNamesNormal = Culture.DateTimeFormat.AbbreviatedDayNames;
            var dayNamesShifted = Shift(dayNamesNormal, (int)GetFirstDayOfWeek());
            return dayNamesShifted;
        }

        /// <summary>
        /// Shift array and cycle around from the end
        /// </summary>
        private static T[] Shift<T>(T[] array, int positions)
        {
            var copy = new T[array.Length];
            Array.Copy(array, 0, copy, array.Length - positions, positions);
            Array.Copy(array, positions, copy, 0, array.Length - positions);
            return copy;
        }

        protected string GetMonthName(int month)
        {
            return GetMonthStart(month).ToString(Culture.DateTimeFormat.YearMonthPattern, Culture);
        }

        protected abstract string GetTitleDateString();

        protected string FormatTitleDate(DateTime? date)
        {
            return date?.ToString(TitleDateFormat ?? "ddd, dd MMM", Culture) ?? "";
        }

        protected string GetFormattedYearString()
        {
            var selectedYear = HighlightedDate ?? GetMonthStart(0);

            return GetCalendarYear(selectedYear).ToString();
        }

        private void OnPreviousMonthClick()
        {
            // It is impossible to go further into the past after the first year and the first month!
            if (PickerMonth.HasValue && PickerMonth.Value.Year == 1 && PickerMonth.Value.Month == 1)
            {
                return;
            }
            PickerMonth = GetMonthStart(0).AddDays(-1).StartOfMonth(Culture);
        }

        private void OnNextMonthClick()
        {
            PickerMonth = GetMonthEnd(0).AddDays(1);
        }

        private void OnPreviousYearClick()
        {
            PickerMonth = GetMonthStart(0).AddYears(-1);
        }

        private void OnNextYearClick()
        {
            PickerMonth = GetMonthStart(0).AddYears(1);
        }

        private void OnYearClick()
        {
            if (!FixYear.HasValue)
            {
                CurrentView = OpenTo.Year;
                StateHasChanged();
                _scrollToYearAfterRender = true;
            }
        }

        private void GoToSelectedYear()
        {
            PickerMonth = HighlightedDate;
            OnYearClick();
        }

        /// <summary>
        /// We need a random id for the year items in the year list so we can scroll to the item safely in every DatePicker.
        /// </summary>
        private readonly string _componentId = Identifier.Create();

        /// <summary>
        /// Is set to true to scroll to the actual year after the next render
        /// </summary>
        private bool _scrollToYearAfterRender = false;

        /// <summary>
        /// Scrolls to the current year.
        /// </summary>
        public async void ScrollToYear()
        {
            _scrollToYearAfterRender = false;
            var id = $"{_componentId}{Culture.Calendar.GetYear(GetMonthStart(0))}";
            await ScrollManager.ScrollToYearAsync(id);
            StateHasChanged();
        }

        private int GetMinYear()
        {
            if (MinDate.HasValue)
                return Culture.Calendar.GetYear(MinDate.Value);
            return Culture.Calendar.GetYear(DateTime.Today) - 100;
        }

        private int GetMaxYear()
        {
            if (MaxDate.HasValue)
                return Culture.Calendar.GetYear(MaxDate.Value);
            return Culture.Calendar.GetYear(DateTime.Today) + 100;
        }

        private string GetYearClasses(int year)
        {
            var selectedYear = HighlightedDate ?? GetMonthStart(0);

            if (year == Culture.Calendar.GetYear(selectedYear))
                return $"mud-picker-year-selected mud-{Color.ToDescriptionString()}-text";
            return null;
        }

        private string GetCalendarHeaderClasses(int month)
        {
            return new CssBuilder("mud-picker-calendar-header")
                .AddClass($"mud-picker-calendar-header-{month + 1}")
                .AddClass($"mud-picker-calendar-header-last", month == DisplayMonths - 1)
                .Build();
        }

        private Typo GetYearTypo(int year)
        {
            var selectedYear = HighlightedDate ?? GetMonthStart(0);

            if (year == Culture.Calendar.GetYear(selectedYear))
                return Typo.h5;

            return Typo.subtitle1;
        }

        private void OnFormattedDateClick()
        {
            // todo: raise an event the user can handle
        }


        private IEnumerable<DateTime> GetAllMonths()
        {
            var current = GetMonthStart(0);
            var calendarYear = Culture.Calendar.GetYear(current);
            var firstOfCalendarYear = Culture.Calendar.ToDateTime(calendarYear, 1, 1, 0, 0, 0, 0);
            for (var i = 0; i < Culture.Calendar.GetMonthsInYear(calendarYear); i++)
                yield return Culture.Calendar.AddMonths(firstOfCalendarYear, i);
        }

        private string GetAbbreviatedMonthName(DateTime month)
        {
            var calendarMonth = Culture.Calendar.GetMonth(month);
            return Culture.DateTimeFormat.AbbreviatedMonthNames[calendarMonth - 1];
        }

        private string GetMonthName(DateTime month)
        {
            var calendarMonth = Culture.Calendar.GetMonth(month);
            return Culture.DateTimeFormat.MonthNames[calendarMonth - 1];
        }

        private string GetMonthClasses(DateTime month)
        {
            var selectedMonth = HighlightedDate ?? GetMonthStart(0);

            if (Culture.Calendar.GetYear(month) != Culture.Calendar.GetYear(selectedMonth))
                return null;

            if (Culture.Calendar.GetMonth(month) == Culture.Calendar.GetMonth(selectedMonth) && !IsMonthDisabled(selectedMonth))
                return $"mud-picker-month-selected mud-{Color.ToDescriptionString()}-text";

            return null;
        }

        private Typo GetMonthTypo(DateTime month)
        {
            var selectedMonth = HighlightedDate ?? GetMonthStart(0);

            if (Culture.Calendar.GetYear(month) != Culture.Calendar.GetYear(selectedMonth))
                return Typo.subtitle1;

            if (Culture.Calendar.GetMonth(month) == Culture.Calendar.GetMonth(selectedMonth))
                return Typo.h5;

            return Typo.subtitle1;
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            AdornmentAriaLabel ??= Localizer[Resources.LanguageResource.MudBaseDatePicker_Open];
            CurrentView = OpenTo;

            if (HighlightedDate is not null) return;

            var today = TimeProvider.GetLocalNow().Date;

            var year = FixYear ?? today.Year;
            var month = FixMonth ?? (year == today.Year ? today.Month : 1);
            var day = FixDay ?? 1;

            if (DateTime.TryParseExact($"{year}-{month}-{day}", "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                HighlightedDate = date;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _picker_month ??= GetCalendarStartOfMonth();
            }

            if (firstRender && CurrentView == OpenTo.Year)
            {
                ScrollToYear();
                return;
            }

            if (_scrollToYearAfterRender)
                ScrollToYear();
        }

        protected abstract DateTime GetCalendarStartOfMonth();

        private int GetCalendarDayOfMonth(DateTime date)
        {
            return Culture.Calendar.GetDayOfMonth(date);
        }

        /// <summary>
        /// Converts gregorian date into whatever year it is in the provided culture
        /// </summary>
        /// <param name="yearDate">Gregorian Date</param>
        /// <returns>Year according to culture</returns>
        protected abstract int GetCalendarYear(DateTime yearDate);

        private ValueTask HandleMouseoverOnPickerCalendarDayButton(int tempId)
        {
            return JsApiService.UpdateStyleProperty(_mudPickerCalendarContentElementId, "--selected-day", tempId);
        }
    }
}
