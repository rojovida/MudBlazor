﻿#pragma warning disable BL0005 // Set parameter outside component

using System.Diagnostics;
using System.Globalization;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.UnitTests.TestComponents.DatePicker;
using NUnit.Framework;
using static Bunit.ComponentParameterFactory;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class DateRangePickerTests : BunitTest
    {
        [Test]
        public void Default()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();
            var picker = comp.Instance;

            picker.Text.Should().Be(null);
            picker.DateRange.Should().Be(null);
            picker.MaxDate.Should().Be(null);
            picker.MinDate.Should().Be(null);
            picker.OpenTo.Should().Be(OpenTo.Date);
            picker.FirstDayOfWeek.Should().Be(null);
            picker.ClosingDelay.Should().Be(100);
            picker.DisplayMonths.Should().Be(2);
            picker.MaxMonthColumns.Should().Be(null);
            picker.StartMonth.Should().Be(null);
            picker.ShowWeekNumbers.Should().BeFalse();
            picker.AutoClose.Should().BeFalse();
            picker.FixYear.Should().Be(null);
            picker.FixMonth.Should().Be(null);
            picker.FixDay.Should().Be(null);
            picker.PlaceholderStart.Should().Be(null);
            picker.PlaceholderEnd.Should().Be(null);
            picker.SeparatorIcon.Should().Be(Icons.Material.Filled.ArrowRightAlt);
        }

        [Test]
        public void DateRangePickerPlaceHolders()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();
            comp.SetParametersAndRender(
                parameters =>
                parameters
                .Add(picker => picker.PlaceholderStart, "Start")
                .Add(picker => picker.PlaceholderEnd, "End")
                );

            var startInput = comp.Find("input").Attributes["placeholder"].Value.Should().Be("Start");
            var endInput = comp.FindAll("input").Skip(1).First().Attributes["placeholder"].Value.Should().Be("End");
        }

        [Test]
        public void DateRangePickerSeparatorIcon()
        {
            var newIcon = Icons.Material.Filled.Star;
            var comp = Context.RenderComponent<MudDateRangePicker>();
            comp.SetParametersAndRender(
                parameters =>
                parameters
                .Add(picker => picker.SeparatorIcon, newIcon)
                );
            var markup = comp.Markup;

            // Only check first svg section
            string startText = "<svg", endText = "</svg>";
            var sectionStart = markup.IndexOf(startText);
            var length = markup.IndexOf(endText) - sectionStart + endText.Length;
            var section = markup.Substring(sectionStart, length);

            section.Should().Contain(newIcon);
        }

        [Test]
        public void DateRangePickerOpenButtonDefaultAriaLabel()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();
            var openButton = comp.Find(".mud-input-adornment button");
            openButton.Attributes.GetNamedItem("aria-label")?.Value.Should().Be("Open");
        }

        [Test]
        public void DateRangePicker_Preset_No_Timestamp()
        {
            var comp = Context.RenderComponent<DateRangePickerPresetWithoutTimestampTest>();

            comp.Markup.Should().Contain("mud-range-start-selected");
            comp.Markup.Should().Contain("mud-range-end-selected");
        }

        [Test]
        public void DateRangePicker_Preset_Timestamp()
        {
            var comp = Context.RenderComponent<DateRangePickerPresetRangeWithTimestampTest>();

            comp.Markup.Should().Contain("mud-range-start-selected");
            comp.Markup.Should().Contain("mud-range-end-selected");
        }

        [Test]
        public void DateRangePickerLabelFor()
        {
            var comp = Context.RenderComponent<DateRangePickerValidationTest>();
            var label = comp.Find(".mud-input-label");
            label.Attributes.GetNamedItem("for")?.Value.Should().Be("dateRangeLabelTest");
        }

        [Test]
        [Ignore("Unignore for performance measurements, not needed for code coverage")]
        public void RenderDateRangePicker_10000_Times_CheckPerformance()
        {
            // warmup
            Context.RenderComponent<MudDateRangePicker>();
            // measure
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
                Context.RenderComponent<MudDateRangePicker>();
            watch.Stop();
            watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        }

        [Test]
        [Ignore("Unignore for performance measurements, not needed for code coverage")]
        public async Task Open_Close_DateRangePicker_10000_Times_CheckPerformance()
        {
            // warmup
            var comp = Context.RenderComponent<MudDateRangePicker>();
            var datepicker = comp.Instance;
            // measure
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                await comp.InvokeAsync(() => datepicker.OpenAsync());
                await comp.InvokeAsync(() => datepicker.CloseAsync());
            }
            watch.Stop();
            watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task SetPickerValue_CheckDateRange_SetPickerDate_CheckValue()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();
            // select elements needed for the test
            var picker = comp.Instance;
            picker.Text.Should().BeNullOrEmpty();
            picker.DateRange.Should().Be(null);
            await comp.InvokeAsync(() => picker.Text = RangeConverter<DateTime>.Join(new DateTime(2021, 01, 01).ToShortDateString(), new DateTime(2021, 01, 10).ToShortDateString()));
            picker.DateRange.Start.Should().Be(new DateTime(2021, 01, 01));
            picker.DateRange.End.Should().Be(new DateTime(2021, 01, 10));
            await comp.InvokeAsync(() => picker.DateRange = new DateRange(new DateTime(2020, 12, 26), new DateTime(2021, 02, 01)));
            picker.Text.Should().Be(RangeConverter<DateTime>.Join(new DateTime(2020, 12, 26).ToShortDateString(), new DateTime(2021, 02, 01).ToShortDateString()));
        }

        [Test]
        public void RangeConverter_RoundTrip_Ok()
        {
            var d1 = "val1";
            var d2 = "val2";

            var repr = RangeConverter<DateTime>.Join(d1, d2);
            RangeConverter<DateTime>.Split(repr, out var c1, out var c2).Should().BeTrue();

            c1.Should().Be(d1);
            c2.Should().Be(d2);
        }

        [Test]
        public void Open_SelectTheSameDateTwice_RangeStartShouldEqualsEnd()
        {
            var comp = OpenPicker();
            // clicking a day button to select a date
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            // clicking a same date then close
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.Instance.DateRange.Start.Should().Be(comp.Instance.DateRange.End);
        }

        [Test]
        public void OpenPickerWithCustomStartMonth_SetDateRange_CheckValue()
        {
            var start = DateTime.Now.AddMonths(-1);
            var comp = OpenPicker(Parameter(nameof(MudDateRangePicker.StartMonth), start));
            // clicking a day buttons to select a range and close
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("12")).First().Click();
            //check result
            comp.Instance.DateRange.Start.Value.Month.Should().Be(start.Month);
            comp.Instance.DateRange.End.Value.Month.Should().Be(start.Month);
        }

        public IRenderedComponent<SimpleMudMudDateRangePickerTest> OpenPicker(ComponentParameter parameter)
        {
            return OpenPicker(new ComponentParameter[] { parameter });
        }

        public IRenderedComponent<SimpleMudMudDateRangePickerTest> OpenPicker(ComponentParameter[] parameters = null)
        {
            IRenderedComponent<SimpleMudMudDateRangePickerTest> comp;
            if (parameters is null)
            {
                comp = Context.RenderComponent<SimpleMudMudDateRangePickerTest>();
            }
            else
            {
                comp = Context.RenderComponent<SimpleMudMudDateRangePickerTest>(parameters);
            }

            // should not be open
            comp.FindAll("div.mud-picker-open").Count.Should().Be(0);
            // click to to open menu
            comp.Find("input").Click();
            // now its open
            comp.FindAll("div.mud-picker-open").Count.Should().Be(1);
            return comp;
        }

        [Test]
        public void Open_CloseByClickingOutsidePicker_CheckClosed()
        {
            var comp = OpenPicker();
            // clicking outside to close
            comp.Find("div.mud-overlay").Click();
            // should not be open any more
            comp.FindAll("div.mud-picker-open").Count.Should().Be(0);
        }

        [Test]
        public void Open_CloseBySelectingADateRange_CheckClosed()
        {
            var comp = OpenPicker();
            // clicking a day buttons to select a range and close
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("23")).First().Click();
            comp.WaitForAssertion(() => comp.FindAll("div.mud-picker-open").Count.Should().Be(0), TimeSpan.FromSeconds(5));
            comp.Instance.DateRange.Should().NotBeNull();
        }

        [Test]
        public void Open_SelectEndDateLowerThanStart_CheckClosed()
        {
            var comp = OpenPicker();
            // clicking a day buttons to select a range and close
            comp.SelectDate("10");
            comp.SelectDate("8");
            comp.FindAll("div.mud-picker-open").Count.Should().Be(1);
            comp.WaitForAssertion(() => comp.FindAll("div.mud-picker-open").Count.Should().Be(0), TimeSpan.FromSeconds(5));
            comp.Instance.DateRange.Should().NotBeNull();
            comp.Instance.DateRange.Start.Should().Be(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 8));
            comp.Instance.DateRange.End.Should().Be(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10));
        }

        [Test]
        public void OpenToYear_CheckYearsShown()
        {
            var comp = OpenPicker(Parameter("OpenTo", OpenTo.Year));
            comp.Instance.DateRange.Should().BeNull();
            // should show years
            comp.FindAll("div.mud-picker-year-container").Count.Should().Be(1);
        }

        [Test]
        public void OpenToYear_ClickYear_CheckMonthsShown()
        {
            var comp = OpenPicker(Parameter("OpenTo", OpenTo.Year));
            comp.Instance.DateRange.Should().BeNull();
            // should show years
            comp.FindAll("div.mud-picker-year-container").Count.Should().Be(1);
            comp.FindAll("div.mud-picker-year")[0].Click();
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
        }

        [Test]
        public void OpenToYear_ClickYear_CheckMonthsShown_Close_Reopen_CheckYearsShown()
        {
            var comp = OpenPicker(Parameter("OpenTo", OpenTo.Year));
            comp.Instance.DateRange.Should().BeNull();
            // should show years
            comp.FindAll("div.mud-picker-year-container").Count.Should().Be(1);
            comp.FindAll("div.mud-picker-year")[0].Click();
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
            // clicking outside to close
            comp.Find("div.mud-overlay").Click();
            // should not be open any more
            comp.FindAll("div.mud-picker-open").Count.Should().Be(0);
            comp.Find("input").Click();
            // should show years
            comp.FindAll("div.mud-picker-year-container").Count.Should().Be(1);
        }

        [Test]
        public void OpenToMonth_CheckMonthsShown()
        {
            var comp = OpenPicker(Parameter("OpenTo", OpenTo.Month));
            comp.Instance.DateRange.Should().BeNull();
            // should show months
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
        }

        [Test]
        public void Open_ClickCalendarHeader_CheckMonthsShown()
        {
            var comp = OpenPicker();
            // should show months
            comp.FindAll("button.mud-picker-calendar-header-transition")[0].Click();
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
        }

        [Test]
        public void Open_ClickYear_CheckYearsShown()
        {
            var comp = OpenPicker(Parameter("OpenTo", OpenTo.Month));
            // should show years
            comp.FindAll("button.mud-picker-calendar-header-transition")[0].Click();
            comp.FindAll("div.mud-picker-year-container").Count.Should().Be(1);
        }

        [Test]
        public void OpenToMonth_Select3rdMonth_Select2ndDay_CheckDateRange()
        {
            var comp = OpenPicker(Parameter("OpenTo", OpenTo.Month));
            comp.Instance.DateRange.Should().BeNull();
            // should show months
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
            comp.FindAll("div.mud-picker-calendar-container > div.mud-picker-month-container > button.mud-picker-month")[2].Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("2")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.Instance.DateRange.Start.Should().Be(new DateTime(DateTime.Now.Year, 3, 2));
        }

        public IRenderedComponent<SimpleMudMudDateRangePickerTest> OpenTo12thMonth()
        {
            var comp = OpenPicker(Parameter("PickerMonth", new DateTime(DateTime.Now.Year, 12, 01)));
            comp.Instance.PickerMonth.Value.Month.Should().Be(12);
            return comp;
        }

        [Test]
        public void Open_ClickCalendarHeader_Click4thMonth_Click23rdDay_CheckDateRange()
        {
            var comp = OpenPicker();
            var picker = comp.Instance;
            comp.Find("button.mud-picker-calendar-header-transition").Click();
            // should show months
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
            comp.FindAll("div.mud-picker-calendar-container > div.mud-picker-month-container > button.mud-picker-month")
                [3].Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("12")).First().Click();
            comp.Instance.DateRange.End.Should().Be(new DateTime(DateTime.Now.Year, 4, 12));
        }

        [Test]
        public void OpenTo12thMonth_NavigateBack_CheckMonth()
        {
            var comp = OpenTo12thMonth();
            var picker = comp.Instance;
            comp.Find("div.mud-picker-calendar-header-switch > button:nth-child(1)").Click();
            picker.PickerMonth.Value.Month.Should().Be(11);
            picker.PickerMonth.Value.Year.Should().Be(DateTime.Now.Year);
        }

        [Test]
        public void OpenTo12thMonth_NavigateForward_CheckYear()
        {
            var comp = OpenTo12thMonth();
            var picker = comp.Instance;
            comp.Find("div.mud-picker-calendar-header-switch > button:nth-child(3)").Click();
            picker.PickerMonth.Value.Month.Should().Be(1);
            picker.PickerMonth.Value.Year.Should().Be(DateTime.Now.Year + 1);
        }

        [Test]
        public void Open_ClickYear_ClickCurrentYear_Click2ndMonth_Click1_Click3_CheckDateRange()
        {
            var comp = OpenPicker();
            comp.Find("div.mud-picker-datepicker-toolbar > button.mud-button-year").Click();
            comp.FindAll("div.mud-picker-calendar-container > div.mud-picker-year-container").Count.Should().Be(1);
            comp.FindAll("div.mud-picker-calendar-container > div.mud-picker-year-container > div.mud-picker-year")
                .Where(x => x.TrimmedText().Contains("2022")).First().Click();
            comp.FindAll("div.mud-picker-month-container").Count.Should().Be(1);
            comp.FindAll("div.mud-picker-calendar-container > div.mud-picker-month-container > button.mud-picker-month")[1].Click();
            comp.FindAll("div.mud-picker-calendar-container > div.mud-picker-calendar-header").Count.Should().Be(2);
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("1")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("3")).First().Click();
            comp.Instance.DateRange.End.Should().Be(new DateTime(2022, 2, 3));
        }

        [Test]
        public async Task Open_Programmatically_CheckOpen_Close_Programmatically_CheckClosed()
        {
            var comp = Context.RenderComponent<SimpleMudMudDateRangePickerTest>();
            comp.FindAll("div.mud-picker-content").Count.Should().Be(0);
            // open programmatically
            await comp.Instance.Open();
            comp.FindAll("div.mud-picker-content").Count.Should().Be(1);
            // closing programmatically
            await comp.Instance.Close();

            comp.FindAll("div.mud-picker-content").Count.Should().Be(0);
        }

        [Test]
        public void SetPickerValue_CheckText()
        {
            var date = DateTime.Now;
            var comp = Context.RenderComponent<MudDateRangePicker>(
                Parameter(nameof(MudDateRangePicker.DateRange), new DateRange(date, date.AddDays(5))));
            // select elements needed for the test
            var picker = comp.Instance;

            var textStart = date.ToShortDateString();
            var textEnd = date.AddDays(5).ToShortDateString();

            picker.Text.Should().Be(RangeConverter<DateTime>.Join(textStart, textEnd));
            var inputs = comp.FindAll("input");
            ((IHtmlInputElement)inputs[0]).Value.Should().Be(textStart);
            ((IHtmlInputElement)inputs[1]).Value.Should().Be(textEnd);
        }

        [Test]
        public void SetPickerToday_CheckSelected()
        {
            var today = DateTime.Now.Date;
            var comp = OpenPicker(Parameter(nameof(MudDateRangePicker.DateRange), new DateRange(today, today)));
            comp.FindAll("button.mud-selected").Count.Should().Be(1);
        }

        [Test]
        public void IsDateDisabledFunc_DisablesCalendarDateButtons()
        {
            Func<DateTime, bool> isDisabledFunc = date => true;
            var comp = OpenPicker(Parameter(nameof(MudDateRangePicker.IsDateDisabledFunc), isDisabledFunc));

            comp.Instance.IsDateDisabledFunc.Should().Be(isDisabledFunc);
            comp.FindAll("button.mud-picker-calendar-day").Select(button => ((IHtmlButtonElement)button).IsDisabled)
                .Should().OnlyContain(disabled => disabled);
        }

        [Test]
        public void IsDateDisabledFunc_SettingRangeToIncludeADisabledDateYieldsNull()
        {
            var today = DateTime.Today;
            var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));
            var twoDaysAgo = DateTime.Today.Subtract(TimeSpan.FromDays(2));
            var wasEventCallbackCalled = false;

            Func<DateTime, bool> isDisabledFunc = date => date == yesterday;
            var comp = Context.RenderComponent<MudDateRangePicker>(
                Parameter(nameof(MudDateRangePicker.IsDateDisabledFunc), isDisabledFunc),
                EventCallback("DateRangeChanged", (DateRange _) => wasEventCallbackCalled = true)
            );

            comp.SetParam(picker => picker.DateRange, new DateRange(twoDaysAgo, today));

            comp.Instance.DateRange.Should().BeNull();
            wasEventCallbackCalled.Should().BeFalse();
        }

        [Test]
        public void IsDateDisabledFunc_SettingRangeToExcludeADisabledDateYieldsTheRange()
        {
            var today = DateTime.Today;
            var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));
            var twoDaysAgo = DateTime.Today.Subtract(TimeSpan.FromDays(2));
            var wasEventCallbackCalled = false;

            Func<DateTime, bool> isDisabledFunc = date => date == twoDaysAgo;
            var range = new DateRange(yesterday, today);
            var comp = Context.RenderComponent<MudDateRangePicker>(
                Parameter(nameof(MudDateRangePicker.IsDateDisabledFunc), isDisabledFunc),
                EventCallback("DateRangeChanged", (DateRange _) => wasEventCallbackCalled = true)
            );

            comp.SetParam(picker => picker.DateRange, range);

            comp.Instance.DateRange.Should().Be(range);
            wasEventCallbackCalled.Should().BeTrue();
        }

        [Test]
        public void IsDateDisabledFunc_NoDisabledDatesByDefault()
        {
            var comp = OpenPicker();
            comp.FindAll("button.mud-picker-calendar-day").Select(button => ((IHtmlButtonElement)button).IsDisabled)
                .Should().OnlyContain(disabled => disabled == false);
        }

        [Test]
        public void AdditionalDateClassesFunc_ClassIsAdded()
        {
            Func<DateTime, string> additionalDateClassesFunc = date => "__addedtestclass__";

            var comp = OpenPicker(Parameter(nameof(MudDateRangePicker.AdditionalDateClassesFunc), additionalDateClassesFunc));

            var daysCount = comp.FindAll("button.mud-picker-calendar-day")
                                .Select(button => (IHtmlButtonElement)button)
                                .Count();

            comp.FindAll("button.mud-day")
                .Where(button => ((IHtmlButtonElement)button).ClassName.Contains("__addedtestclass__"))
                .Should().HaveCount(daysCount);
        }

        [Test]
        public void SetRangeTextFunc_NullInputNoError()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>(parameters =>
                parameters.Add(p => p.DateRange,
                    new DateRange(new DateTime(2020, 12, 26), null)));
            comp.Find("input").Change("");
            comp.Instance.DateRange.End.Should().BeNull();
            comp.Instance.DateRange.Start.Should().BeNull();

        }

        [Test]
        public void SetRangeTextFunc_NullRangeTextNoError()
        {
            var dateTime = new DateTime(2020, 12, 26);
            var comp = Context.RenderComponent<MudDateRangePicker>(parameters =>
                parameters.Add(p => p.DateRange, null)
                    .Add(p => p.Culture, CultureInfo.CurrentCulture));
            comp.Find("input").Change(dateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));
            comp.Instance.DateRange.Start.Should().Be(dateTime);

        }


        [Test]
        public void SetDateRange_NoChangedIfSameValues()
        {
            var dr1 = new DateRange(new DateTime(2021, 10, 08), new DateTime(2021, 10, 09));
            var dr2 = new DateRange(new DateTime(2021, 10, 08), new DateTime(2021, 10, 09));

            var wasEventCallbackCalled = false;

            var comp = Context.RenderComponent<MudDateRangePicker>(
                Parameter(nameof(MudDateRangePicker.DateRange), dr1),
                EventCallback(nameof(MudDateRangePicker.DateRangeChanged), (DateRange _) => wasEventCallbackCalled = true));

            comp.SetParam(nameof(MudDateRangePicker.DateRange), dr2);

            comp.Instance.DateRange.Should().Be(dr2);
            wasEventCallbackCalled.Should().BeFalse();
        }

        [Test]
        public void InitializeDateRange_DefaultConstructor()
        {
            var range = new DateRange();

            var comp = OpenPicker(Parameter(nameof(MudDateRangePicker.DateRange), range));

            comp.Instance.DateRange.Should().NotBeNull();
            comp.Instance.DateRange.Start.Should().NotBe(default);
            comp.Instance.DateRange.End.Should().NotBe(default);
            comp.Instance.DateRange.Start.Should().BeNull();
            comp.Instance.DateRange.End.Should().BeNull();
        }

        [Test]
        public void InitializeDateRange_AllNullValues()
        {
            var range = new DateRange(null, null);

            var comp = OpenPicker(Parameter(nameof(MudDateRangePicker.DateRange), range));

            comp.Instance.DateRange.Should().NotBeNull();
            comp.Instance.DateRange.Start.Should().NotBe(default);
            comp.Instance.DateRange.End.Should().NotBe(default);
            comp.Instance.DateRange.Start.Should().BeNull();
            comp.Instance.DateRange.End.Should().BeNull();
        }

        [Test]
        public async Task DateRangePicker_RequiredValidation()
        {
            // define some "constant" values
            var errorMessage = "A valid date has to be picked";
            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now.Date.AddDays(2);

            // create the component
            var dateRangePickerComponent = Context.RenderComponent<MudDateRangePicker>(Parameter(nameof(MudDateRangePicker.Required), true), Parameter(nameof(MudDateRangePicker.RequiredError), errorMessage));

            // select the instance to work with
            var dateRangePickerInstance = dateRangePickerComponent.Instance;

            // assert default's
            dateRangePickerInstance.Text.Should().BeNullOrEmpty();
            dateRangePickerInstance.DateRange.Should().Be(null);

            // validated the picker
            await dateRangePickerComponent.InvokeAsync(() => dateRangePickerInstance.Validate());
            dateRangePickerInstance.Error.Should().BeTrue("Value is required and should be handled as invalid");
            dateRangePickerInstance.ErrorText.Should().Be(errorMessage);

            // set a value
            await dateRangePickerComponent.InvokeAsync(() => dateRangePickerInstance.Text = RangeConverter<DateTime>.Join(startDate.ToShortDateString(), endDate.ToShortDateString()));

            // asert new values have been applied
            dateRangePickerInstance.DateRange.Start.Should().Be(startDate);
            dateRangePickerInstance.DateRange.End.Should().Be(endDate);
            dateRangePickerInstance.Error.Should().BeFalse("Value has been set and should be handled as valid");
            dateRangePickerInstance.ErrorText.Should().BeNullOrWhiteSpace();

            // reset value
            await dateRangePickerComponent.InvokeAsync(() => dateRangePickerInstance.ClearAsync());

            // assert values have benn nulled
            dateRangePickerInstance.Text.Should().BeNullOrEmpty();
            dateRangePickerInstance.DateRange.Should().Be(null);
            dateRangePickerInstance.Error.Should().BeTrue("Value has been cleared and should be handled as invalid");
            dateRangePickerInstance.ErrorText.Should().Be(errorMessage);
        }

        [Test]
        public void CheckAutoCloseDateRangePicker_DoNotCloseWhenValueIsOff()
        {
            // Define a date range for comparison
            var initialDateRange = new DateRange(
               new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01),
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 02));

            // Get access to the date range picker of the instance
            var comp = Context.RenderComponent<AutoCloseDateRangePickerTest>(
                Parameter(nameof(AutoCloseDateRangePickerTest.DateRange), initialDateRange));

            // Open the date range picker
            comp.Find("input").Click();

            // Clicking day buttons to select a date range
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("11")).First().Click();

            // Check that the date range should remain the same because autoclose is false even when actions are defined
            comp.Instance.DateRange.Should().Be(initialDateRange);
            comp.WaitForAssertion(() => comp.FindAll("div.mud-popover").Count.Should().Be(1));
        }

        [Test]
        public void CheckAutoCloseDateRangePicker_CloseWhenValueIsOn()
        {
            // Define a date range for comparison
            var initialDateRange = new DateRange(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01),
                  new DateTime(DateTime.Now.Year, DateTime.Now.Month, 02));

            // Get access to the date range picker of the instance
            var comp = Context.RenderComponent<AutoCloseDateRangePickerTest>(
                Parameter(nameof(AutoCloseDateRangePickerTest.DateRange), initialDateRange),
                Parameter(nameof(AutoCloseDateRangePickerTest.AutoClose), true));

            // Open the date range picker
            comp.Find("input").Click();
            // verify open
            comp.WaitForAssertion(() => comp.FindAll("div.mud-popover-open").Count.Should().Be(1));

            // Clicking day buttons to select a date range
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("10")).First().Click();
            comp.FindAll("button.mud-picker-calendar-day")
                .Where(x => x.TrimmedText().Equals("11")).First().Click();

            // Check that the date range is changed because autoclose is true even when actions are defined
            comp.Instance.DateRange.Should().NotBe(initialDateRange);
            comp.Instance.DateRange.Should().Be(new DateRange(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10),
                  new DateTime(DateTime.Now.Year, DateTime.Now.Month, 11)));
            comp.WaitForAssertion(() => comp.FindAll("div.mud-popover-open").Count.Should().Be(0));
        }

        [Test]
        public void CurrentDate_ShouldBeMarked()
        {
            var currentDate = DateTime.Now.Date;
            var comp = OpenPicker();

            // Check that only one date is marked
            comp.FindAll("button.mud-current").Count.Should().Be(1);

            // Check that the marked date is the current date
            comp.Find("button.mud-current").Click();
            comp.Find("button.mud-range-start-selected").Click();
            comp.Instance.DateRange.Should().Be(new DateRange(currentDate, currentDate));
        }

        [Test]
        public async Task SingleDayRange_Should_Render_Selected()
        {
            var today = DateTime.Today;
            var initialRange = new DateRange(new DateTime(today.Year, today.Month, 01), new DateTime(today.Year, today.Month, 05));

            var comp = Context.RenderComponent<AutoCloseDateRangePickerTest>(Parameter(nameof(AutoCloseDateRangePickerTest.DateRange), initialRange));

            comp.Find("input").Click();

            //Select same date as start and end
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("10")).First().ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("10")).First().ClickAsync(new MouseEventArgs());

            // Check that the date range should remain the same because autoclose is false
            comp.Instance.DateRange.Should().Be(initialRange);
            comp.WaitForAssertion(() => comp.FindAll("div.mud-popover").Count.Should().Be(1));

            //mud-selected should be applied instead of mud-range-start-selected and mud-range-end-selected
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("10")).First()
                .ToMarkup().Should().Contain("mud-selected")
                .And.NotContain("mud-range-start-selected")
                .And.NotContain("mud-range-end-selected");
        }

        [Test]
        public void DateRangePicker_Should_Clear()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();
            // select elements needed for the test
            var picker = comp.Instance;
            picker.Text.Should().Be(null);
            picker.DateRange.Should().Be(null);
            comp.SetParam(p => p.Clearable, true);
            comp.SetParam(p => p.DateRange, new DateRange(new DateTime(2020, 10, 26), new DateTime(2020, 10, 29)));
            picker.DateRange.Should().Be(new DateRange(new DateTime(2020, 10, 26), new DateTime(2020, 10, 29)));

            comp.Find("button").Click(); //clear the input

            picker.DateRange.Should().Be(new DateRange(null, null));
        }

        [Test]
        public async Task OnPointerOver_ShouldCallJavaScriptFunction()
        {
            var comp = OpenPicker();

            var button = comp
                .FindAll(".mud-button-root.mud-icon-button.mud-ripple.mud-ripple-icon.mud-picker-calendar-day.mud-day")
                .Single(x => x.GetAttribute("style") == "--day-id: 5;");

            await button.PointerOverAsync(new());

            Context.JSInterop.VerifyInvoke("mudWindow.updateStyleProperty", 1);
            Context.JSInterop.Invocations["mudWindow.updateStyleProperty"].Single()
                .Arguments
                .Should()
                .HaveCount(3)
                .And
                .HaveElementAt(1, "--selected-day")
                .And
                .HaveElementAt(2, 5);
        }

        /// <summary>
        /// Optional DateRangePicker should not have required attribute and aria-required should be false.
        /// </summary>
        [Test]
        public void OptionalDateRangePicker_Should_NotHaveRequiredAttributeAndAriaRequiredShouldBeFalse()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();

            comp.FindAll("input").Should().AllSatisfy(input =>
            {
                input.HasAttribute("required").Should().BeFalse();
                input.GetAttribute("aria-required").Should().Be("false");
            });
        }

        /// <summary>
        /// Required DateRangePicker should have required and aria-required attributes.
        /// </summary>
        [Test]
        public void RequiredDateRangePicker_Should_HaveRequiredAndAriaRequiredAttributes()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>(parameters => parameters
                .Add(p => p.Required, true));

            comp.FindAll("input").Should().AllSatisfy(input =>
            {
                input.HasAttribute("required").Should().BeTrue();
                input.GetAttribute("aria-required").Should().Be("true");
            });
        }

        /// <summary>
        /// Required and aria-required DateRangePicker attributes should be dynamic.
        /// </summary>
        [Test]
        public void RequiredAndAriaRequiredDateRangePickerAttributes_Should_BeDynamic()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();

            comp.FindAll("input").Should().AllSatisfy(input =>
            {
                input.HasAttribute("required").Should().BeFalse();
                input.GetAttribute("aria-required").Should().Be("false");
            });

            comp.SetParametersAndRender(parameters => parameters
                .Add(p => p.Required, true));

            comp.FindAll("input").Should().AllSatisfy(input =>
            {
                input.HasAttribute("required").Should().BeTrue();
                input.GetAttribute("aria-required").Should().Be("true");
            });
        }

        [Test]
        [SetCulture("en-US")]
        public void FormatFirst_Should_RenderCorrectly()
        {
            DateRange range = new DateRange(new DateTime(2024, 04, 22), new DateTime(2024, 04, 23));
            var comp = Context.RenderComponent<DateRangePickerFormatTest>
            (parameters =>
            {
                parameters.Add(p => p.DateRange, range);
                parameters.Add(p => p.FormatFirst, true);
            });
            var instance = comp.FindComponent<MudDateRangePicker>().Instance;
            instance.DateRange.Should().Be(range);
            instance.DateFormat.Should().Be("yyyy MMMM dd");
            comp.Markup.Should().Contain("2024 April 22");
            comp.Markup.Should().Contain("2024 April 23");
        }

        [Test]
        [SetCulture("en-US")]
        public void FormatLast_Should_RenderCorrectly()
        {
            DateRange range = new DateRange(new DateTime(2024, 04, 22), new DateTime(2024, 04, 23));
            var comp = Context.RenderComponent<DateRangePickerFormatTest>
            (parameters =>
            {
                parameters.Add(p => p.DateRange, range);
                parameters.Add(p => p.FormatFirst, false);
            });
            var instance = comp.FindComponent<MudDateRangePicker>().Instance;
            instance.DateRange.Should().Be(range);
            instance.DateFormat.Should().Be("yyyy MMMM dd");
            comp.Markup.Should().Contain("2024 April 22");
            comp.Markup.Should().Contain("2024 April 23");
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CheckCloseOnClearDateRangePicker(bool closeOnClear)
        {
            // Define a date range for comparison
            var initialDateRange = new DateRange(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01),
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 02));

            // Get access to the date range picker of the instance
            var comp = Context.RenderComponent<DateRangePickerCloseOnClearTest>(
                Parameter(nameof(DateRangePickerCloseOnClearTest.DateRange), initialDateRange),
                Parameter(nameof(DateRangePickerCloseOnClearTest.CloseOnClear), closeOnClear));

            // Open the date range picker
            comp.Find("input").Click();

            // Clicking day buttons to select a date range
            comp
                .FindAll("button.mud-button").First(x => x.TrimmedText().Equals("Clear")).Click();

            // Check that the date range was cleared
            comp.Instance.DateRange.Should().NotBe(initialDateRange);
            if (closeOnClear)
            {
                // Check that the component is closed
                comp.WaitForAssertion(() => comp.Markup.Should().NotContain("mud-popover-open"));
            }
            else
            {
                // Check that the component is open
                comp.WaitForAssertion(() => comp.Find("div.mud-popover").ClassList.Should().Contain("mud-popover-open"));
            }
        }

        [Test]
        public void Should_respect_underline_parameter()
        {
            var underlinedComp = Context.RenderComponent<MudDateRangePicker>(parameters
                => parameters.Add(p => p.Underline, true));
            var notUnderlinedComp = Context.RenderComponent<MudDateRangePicker>(parameters
                => parameters.Add(p => p.Underline, false));

            underlinedComp.FindAll(".mud-input-underline").Should().HaveCount(1);
            notUnderlinedComp.FindAll(".mud-input-underline").Should().HaveCount(0);
        }

        [Test]
        public void StrictCaptureRange_ShouldCaptureDisabledDates_WhenFalse()
        {
            var today = DateTime.Today;
            var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));
            var twoDaysAgo = DateTime.Today.Subtract(TimeSpan.FromDays(2));
            var wasEventCallbackCalled = false;

            var range = new DateRange(twoDaysAgo, today);
            Func<DateTime, bool> isDisabledFunc = date => date == yesterday;
            var comp = Context.RenderComponent<MudDateRangePicker>(
                Parameter(nameof(MudDateRangePicker.AllowDisabledDatesInRange), true),
                Parameter(nameof(MudDateRangePicker.IsDateDisabledFunc), isDisabledFunc),
                EventCallback("DateRangeChanged", (DateRange _) => wasEventCallbackCalled = true)
            );

            comp.SetParam(picker => picker.DateRange, new DateRange(twoDaysAgo, today));

            comp.Instance.DateRange.Should().Be(range);
            wasEventCallbackCalled.Should().BeTrue();
        }

        [Test]
        public async Task TestDateRangeClearableWithFormat()
        {
            var comp = Context.RenderComponent<DateRangePickerClearableTest>();
            var picker = comp.FindComponents<MudDateRangePicker>();
            picker.Count.Should().Be(2);
            var openBtn = picker[0].FindComponents<MudIconButton>();
            openBtn.Count.Should().Be(1);
            var openBtnElement = openBtn[0].Find("button");
            await openBtnElement.TriggerEventAsync("onclick", new MouseEventArgs());
            await Task.Delay(500);
            IElement DayButton(string dayNumber) =>
                comp.FindAll("button")
                    .SingleOrDefault(x => x.GetStyle().GetPropertyValue("--day-id") == dayNumber);
            await DayButton("5").TriggerEventAsync("onclick", new MouseEventArgs());
            await Task.Delay(200);
            await DayButton("7").TriggerEventAsync("onclick", new MouseEventArgs());
            await Task.Delay(200);

            IReadOnlyList<IRenderedComponent<MudIconButton>> IconButtons(int index) =>
                picker[index].FindComponents<MudIconButton>();

            IconButtons(0).Count.Should().Be(2);
            IconButtons(1).Count.Should().Be(2);
            IconButtons(0)[0].Find("button").Click();
            IconButtons(1)[0].Find("button").Click();
            IconButtons(0).Count.Should().Be(1);
            IconButtons(1).Count.Should().Be(1);
        }

        [Test]
        [SetCulture("en-US")]
        public async Task DateRangePickerToolbar_DisplaysSelectedDate()
        {
            var selectedRange = new DateRange(new DateTime(2025, 1, 10).Date, new DateTime(2025, 1, 20).Date);
            var comp = Context.RenderComponent<DateRangePickerPresetWithoutTimestampTest>(p => p.Add(x => x.DateRange, selectedRange));

            comp.FindAll("button.mud-picker-calendar-day")
                .First(x => x.TrimmedText().Equals("10"))
                .ToMarkup().Should().Contain("mud-range-start-selected");
            comp.FindAll("button.mud-picker-calendar-day")
                .First(x => x.TrimmedText().Equals("20"))
                .ToMarkup().Should().Contain("mud-range-end-selected");
            comp.Find("button.mud-button-date .mud-button-label").InnerHtml.Should().Be("Fri, 10 Jan - Mon, 20 Jan");
            comp.Find("button.mud-button-year .mud-button-label").InnerHtml.Should().Be("2025");

            //navigate to previous month
            await comp.Find(".mud-picker-nav-button-prev").ClickAsync(new MouseEventArgs());

            //toolbar should display original range
            comp.Find("button.mud-button-year .mud-button-label").InnerHtml.Should().Be("2025");
            comp.Find("button.mud-button-date .mud-button-label").InnerHtml.Should().Be("Fri, 10 Jan - Mon, 20 Jan");
            comp.FindAll("button.mud-picker-calendar-day").First(x => x.TrimmedText().Equals("10")).ToMarkup().Should().NotContain("mud-selected");

            //select new month
            await comp.FindAll("button.mud-button-month")[0].ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-picker-month").First(x => x.TrimmedText().Equals("May")).ClickAsync(new MouseEventArgs());

            //toolbar should display 2025 and original range
            comp.Find("button.mud-button-year .mud-button-label").InnerHtml.Should().Be("2025");
            comp.Find("button.mud-button-date .mud-button-label").InnerHtml.Should().Be("Fri, 10 Jan - Mon, 20 Jan");
            comp.FindAll("button.mud-picker-calendar-day").First(x => x.TrimmedText().Equals("10")).ToMarkup().Should().NotContain("mud-selected");

            //select new year
            await comp.FindAll("button.mud-button-month")[0].ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-picker-calendar-header-transition")[0].ClickAsync(new MouseEventArgs());
            await comp.FindAll("div.mud-picker-year").First(x => x.TrimmedText().Equals("2022")).ClickAsync(new MouseEventArgs());

            //toolbar should display 2025 and original range
            comp.Find("button.mud-button-year .mud-button-label").InnerHtml.Should().Be("2025");
            comp.Find("button.mud-button-date .mud-button-label").InnerHtml.Should().Be("Fri, 10 Jan - Mon, 20 Jan");
        }

        [Test]
        [SetCulture("en-US")]
        public async Task DateRangePicker_HighlightSelectedMonthOnly()
        {
            var selectedRange = new DateRange(new DateTime(2025, 1, 10).Date, new DateTime(2025, 1, 20).Date);
            var comp = Context.RenderComponent<DateRangePickerPresetWithoutTimestampTest>(p => p.Add(x => x.DateRange, selectedRange));

            //go to month view
            await comp.FindAll("button.mud-button-month")[0].ClickAsync(new MouseEventArgs());

            //confirm Jan is highlighted
            comp.FindAll("button.mud-picker-month").First(x => x.TrimmedText().Equals("Jan")).ToMarkup().Should().Contain("mud-picker-month-selected");

            //select new month (March)
            await comp.FindAll("button.mud-picker-month").First(x => x.TrimmedText().Equals("Mar")).ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-button-month")[0].ClickAsync(new MouseEventArgs());

            //confirm Jan is highlighted
            comp.FindAll("button.mud-picker-month").First(x => x.TrimmedText().Equals("Jan")).ToMarkup().Should().Contain("mud-picker-month-selected");

            //change year
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());

            //confirm no month is highlighted
            comp.Find(".mud-picker-month-container").ToMarkup().Should().NotContain("mud-picker-month-selected");

            //back to present year
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Next year']").ClickAsync(new MouseEventArgs());
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Next year']").ClickAsync(new MouseEventArgs());

            //confirm Jan is highlighted
            comp.FindAll("button.mud-picker-month").First(x => x.TrimmedText().Equals("Jan")).ToMarkup().Should().Contain("mud-picker-month-selected");
        }

        [Test]
        [SetCulture("en-US")]
        public async Task DateRangePicker_HighlightSelectedYearOnly()
        {
            var selectedRange = new DateRange(new DateTime(2025, 1, 10).Date, new DateTime(2025, 1, 20).Date);
            var comp = Context.RenderComponent<DateRangePickerPresetWithoutTimestampTest>(p => p.Add(x => x.DateRange, selectedRange));

            //go to year view
            await comp.FindAll("button.mud-button-month")[0].ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-picker-calendar-header-transition")[0].ClickAsync(new MouseEventArgs());

            //2025 is highlighted
            comp.FindAll("div.mud-picker-year").First(x => x.TrimmedText().Equals("2025")).ToMarkup().Should().Contain("mud-picker-year-selected");

            //select new year
            await comp.FindAll("div.mud-picker-year").First(x => x.TrimmedText().Equals("2020")).ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-picker-calendar-header-transition")[0].ClickAsync(new MouseEventArgs());

            //2025 is still highlighted
            comp.FindAll("div.mud-picker-year").First(x => x.TrimmedText().Equals("2025")).ToMarkup().Should().Contain("mud-picker-year-selected");
        }

        [Test]
        [SetCulture("en-US")]
        public async Task DatePicker_JumpToYear()
        {
            var selectedRange = new DateRange(new DateTime(2025, 1, 10).Date, new DateTime(2025, 1, 20).Date);
            var comp = Context.RenderComponent<DateRangePickerPresetWithoutTimestampTest>(p => p.Add(x => x.DateRange, selectedRange));
            var picker = comp.Instance;

            await comp.FindAll("button.mud-button-month")[0].ClickAsync(new MouseEventArgs());

            //back 5 years
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());
            await comp.Find(".mud-picker-calendar-header-switch button[aria-label^='Previous year']").ClickAsync(new MouseEventArgs());

            //Jump to 2020
            await comp.FindAll("button.mud-picker-calendar-header-transition")[0].ClickAsync(new MouseEventArgs());

            picker.PickerReference.PickerMonth!.Value.Year.Should().Be(2020);
            comp.FindAll("div.mud-picker-year").First(x => x.TrimmedText().Equals("2025")).ToMarkup().Should().Contain("mud-picker-year-selected");

            //Jump to 2025
            await comp.Find("button.mud-button-year").ClickAsync(new MouseEventArgs());

            picker.PickerReference.PickerMonth!.Value.Year.Should().Be(2025);
            comp.FindAll("div.mud-picker-year").First(x => x.TrimmedText().Equals("2025")).ToMarkup().Should().Contain("mud-picker-year-selected");
        }

        [Test]
        public async Task DateRangePicker_MinMaxDays()
        {
            //no restrictions - minimum of 3 days
            var startingRange = new DateRange(new DateTime(2025, 1, 1).Date, new DateTime(2025, 1, 1).Date);
            var comp = Context.RenderComponent<DateRangePickerMinMaxDaysTest>(p => p.Add(x => x.DateRange, startingRange));

            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("16")).First().ClickAsync(new MouseEventArgs());
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("17")).First().ToMarkup().Should().Contain("disabled");
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("18")).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Should().Be(new DateRange(new DateTime(2025, 1, 16).Date, new DateTime(2025, 1, 18).Date));

            //no restrictions - maximum of 7 days 
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("16")).First().ClickAsync(new MouseEventArgs());
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("17")).First().ToMarkup().Should().Contain("disabled"); //2 days not allowed
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("18")).First().ToMarkup().Should().NotContain("disabled"); //3 days valid
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("19")).First().ToMarkup().Should().NotContain("disabled"); //4 days valid
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ToMarkup().Should().NotContain("disabled"); //5 days valid
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("21")).First().ToMarkup().Should().NotContain("disabled"); //6 days valid
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ToMarkup().Should().NotContain("disabled"); //7 days valid
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("23")).First().ToMarkup().Should().Contain("disabled"); //8 days not allowed
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("24")).First().ToMarkup().Should().Contain("disabled"); //9 days not allowed
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Should().Be(new DateRange(new DateTime(2025, 1, 16).Date, new DateTime(2025, 1, 22).Date));

            //weekends not allowed - minimum of 3 days - count disabled
            comp.Instance.AllowWeekends = false;
            comp.Render();

            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("16")).First().ClickAsync(new MouseEventArgs()); //                              [1]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("17")).First().ToMarkup().Should().Contain("disabled"); //2 days not allowed           [2]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("18")).First().ToMarkup().Should().Contain("disabled"); //3 disabled (weekend)         [3]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("19")).First().ToMarkup().Should().Contain("disabled"); //4 disabled (weekend)         [4]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ToMarkup().Should().NotContain("disabled"); //5 days valid              [5]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("21")).First().ToMarkup().Should().NotContain("disabled"); //6 days valid              [6]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ToMarkup().Should().NotContain("disabled"); //7 days valid              [7]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("23")).First().ToMarkup().Should().Contain("disabled"); //8 days not allowed
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("24")).First().ToMarkup().Should().Contain("disabled"); //9 days not allowed
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Should().Be(new DateRange(new DateTime(2025, 1, 16).Date, new DateTime(2025, 1, 20).Date)); //min valid range 5 days

            //weekends not allowed - maximum of 7 days - count disabled
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("16")).First().ClickAsync(new MouseEventArgs()); //                              [1]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("17")).First().ToMarkup().Should().Contain("disabled"); //2 days not allowed           [2]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("18")).First().ToMarkup().Should().Contain("disabled"); //3 disabled (weekend)         [3]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("19")).First().ToMarkup().Should().Contain("disabled"); //4 disabled (weekend)         [4]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ToMarkup().Should().NotContain("disabled"); //5 days valid              [5]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("21")).First().ToMarkup().Should().NotContain("disabled"); //6 days valid              [6]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ToMarkup().Should().NotContain("disabled"); //7 days valid              [7]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("23")).First().ToMarkup().Should().Contain("disabled"); //8 days not allowed
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("24")).First().ToMarkup().Should().Contain("disabled"); //9 days not allowed
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Should().Be(new DateRange(new DateTime(2025, 1, 16).Date, new DateTime(2025, 1, 22).Date)); //max valid range 7 days

            //weekends not allowed - minimum of 3 days - exclude disabled (skip weekends)
            comp.Instance.CountDisabledDays = false;
            comp.Render();

            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("16")).First().ClickAsync(new MouseEventArgs());  //                             [1]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("17")).First().ToMarkup().Should().Contain("disabled"); //2 days not allowed           [2]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("18")).First().ToMarkup().Should().Contain("disabled"); //3 disabled (weekend)         [ ]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("19")).First().ToMarkup().Should().Contain("disabled"); //4 disabled (weekend)         [ ]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ToMarkup().Should().NotContain("disabled"); //5 days valid              [3]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("21")).First().ToMarkup().Should().NotContain("disabled"); //6 days valid              [4]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ToMarkup().Should().NotContain("disabled"); //7 days valid              [5]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("23")).First().ToMarkup().Should().NotContain("disabled"); //8 days valid              [6]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("24")).First().ToMarkup().Should().NotContain("disabled"); //9 days valid              [7]
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Should().Be(new DateRange(new DateTime(2025, 1, 16).Date, new DateTime(2025, 1, 20).Date)); //min valid range 5 days

            //weekends not allowed - maximum of 7 days - exclude disabled (skip weekends)
            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("16")).First().ClickAsync(new MouseEventArgs());  //                             [1]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("17")).First().ToMarkup().Should().Contain("disabled"); //2 days not allowed           [2]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("18")).First().ToMarkup().Should().Contain("disabled"); //3 disabled (weekend)         [ ]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("19")).First().ToMarkup().Should().Contain("disabled"); //4 disabled (weekend)         [ ]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("20")).First().ToMarkup().Should().NotContain("disabled"); //5 days valid              [3]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("21")).First().ToMarkup().Should().NotContain("disabled"); //6 days valid              [4]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("22")).First().ToMarkup().Should().NotContain("disabled"); //7 days valid              [5]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("23")).First().ToMarkup().Should().NotContain("disabled"); //8 days valid              [6]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("24")).First().ToMarkup().Should().NotContain("disabled"); //9 days valid              [7]
            comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("25")).First().ToMarkup().Should().Contain("disabled"); //9 days valid                 [8]

            await comp.FindAll("button.mud-picker-calendar-day").Where(x => x.TrimmedText().Equals("24")).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Should().Be(new DateRange(new DateTime(2025, 1, 16).Date, new DateTime(2025, 1, 24).Date)); //max valid range 9 days
        }

        [Test]
        public async Task DateRangePicker_MaxSelectableDateTest()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>();

            comp.SetParametersAndRender(parameters => parameters.Add(picker => picker.MaxDays, 30)
                                                                .Add(picker => picker.PickerVariant, PickerVariant.Static)
                                                                .Add(picker => picker.IsDateDisabledFunc, x => x.Date > DateTime.Today));

            var today = DateTime.Today;

            await comp.FindAll("button.mud-picker-calendar-day")
                      .Where(x => x.TrimmedText().Equals(today.Day.ToString())).First().ClickAsync(new MouseEventArgs());
            await comp.FindAll("button.mud-picker-calendar-day")
                      .Where(x => x.TrimmedText().Equals(today.Day.ToString())).First().ClickAsync(new MouseEventArgs());

            comp.Instance.DateRange.Start.Should().Be(comp.Instance.DateRange.End);
        }

        [Test]
        public async Task DateRangePicker_BlurAsync()
        {
            var comp = Context.RenderComponent<MudDateRangePicker>(parameters => parameters
                    .Add(picker => picker.ReadOnly, false)
                    .Add(picker => picker.Editable, true));

            var input = comp.Find("input");

            await comp.Instance.FocusStartAsync();

            // the input is actually never focused because the test is run in a headless browser
            //comp.Find("input").IsFocused.Should().BeTrue();

            await comp.Instance.BlurAsync();

            comp.Find("input").IsFocused.Should().BeFalse();
        }
    }

    public static class DatePickerRenderedFragmentExtensions
    {
        public static void SelectDate(this IRenderedFragment comp, string day, bool firstOccurrence = true)
        {
            comp.ValidateSelection(day, firstOccurrence).Click();
        }

        public static async Task SelectDateAsync(this IRenderedFragment comp, string day, bool firstOccurrence = true)
        {
            await comp.ValidateSelection(day, firstOccurrence).ClickAsync(new MouseEventArgs());
        }

        private static IElement ValidateSelection(this IRenderedFragment comp, string day, bool firstOccurrence)
        {
            var matchingDays = comp.FindAll("button.mud-picker-calendar-day")
                       .Where(x => !x.ClassList.Contains("mud-hidden") && x.TrimmedText().Equals(day))
                       .ToList();

            Assert.That(matchingDays.Count != 0, $"Invalid day ({day}) selected");

            if (!firstOccurrence)
                Assert.That(matchingDays.Count == 2, $"Only one instance of date ({day}) found");

            var selectedDate = matchingDays[firstOccurrence ? 0 : 1];

            Assert.That(!selectedDate.IsDisabled(), $"Selected date ({day}) is disabled");

            return selectedDate;
        }
    }
}
