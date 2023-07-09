using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.Test;

public class TermDateRangesTest
{
    // Generate XUnit Tests for TermDateRanges as Facts
    
    
    public class TermDateRangeTestData : TheoryData<CalendarTerm, RelativePeriod, DateTime, DateTime, DateTime>
    {
        public TermDateRangeTestData()
        {
            var today = new DateTime(2023, 07, 09);
            
            Add(CalendarTerm.Week, RelativePeriod.Previous, today, new DateTime(2023, 06, 26), new DateTime(2023, 07, 02));
            Add(CalendarTerm.Week, RelativePeriod.Current, today, new DateTime(2023, 07, 03), new DateTime(2023, 07, 09));  
            Add(CalendarTerm.Week, RelativePeriod.Next, today, new DateTime(2023, 07, 10), new DateTime(2023, 07, 16));
            
            
            Add(CalendarTerm.Month, RelativePeriod.Previous, today, new DateTime(2023, 06, 01), new DateTime(2023, 06, 30));
            Add(CalendarTerm.Month, RelativePeriod.Next, today, new DateTime(2023, 08, 01), new DateTime(2023, 08, 31));
            Add(CalendarTerm.Month, RelativePeriod.Current, today, new DateTime(2023, 07, 01), new DateTime(2023, 07, 31));
            
            
            Add(CalendarTerm.Day, RelativePeriod.Previous, today, new DateTime(2023, 07, 08), new DateTime(2023, 07, 08));
            Add(CalendarTerm.Day, RelativePeriod.Current, today, new DateTime(2023, 07, 09), new DateTime(2023, 07, 09));
            Add(CalendarTerm.Day, RelativePeriod.Next, today, new DateTime(2023, 07, 10), new DateTime(2023, 07, 10));
            
            today = new DateTime(2023, 01, 01); // During year change
            Add(CalendarTerm.Day, RelativePeriod.Previous, today, new DateTime(2022, 12, 31), new DateTime(2022, 12, 31));
            Add(CalendarTerm.Day, RelativePeriod.Current, today, new DateTime(2023, 01, 01), new DateTime(2023, 01, 01));
            Add(CalendarTerm.Day, RelativePeriod.Next, today, new DateTime(2023, 01, 02), new DateTime(2023, 01, 02));
            
            Add(CalendarTerm.Week, RelativePeriod.Previous, today, new DateTime(2022, 12, 19), new DateTime(2022, 12, 25));
            Add(CalendarTerm.Week, RelativePeriod.Current, today, new DateTime(2022, 12, 26), new DateTime(2023, 01, 01));
            Add(CalendarTerm.Week, RelativePeriod.Next, today, new DateTime(2023, 01, 02), new DateTime(2023, 01, 08));
            
            Add(CalendarTerm.Month, RelativePeriod.Previous, today, new DateTime(2022, 12, 01), new DateTime(2022, 12, 31));
            Add(CalendarTerm.Month, RelativePeriod.Current, today, new DateTime(2023, 01, 01), new DateTime(2023, 01, 31));
            Add(CalendarTerm.Month, RelativePeriod.Next, today, new DateTime(2023, 02, 01), new DateTime(2023, 02, 28));
            
            today = new DateTime(2023, 10, 25); // During Timezone change
            Add(CalendarTerm.Day, RelativePeriod.Previous, today, new DateTime(2023, 10, 24), new DateTime(2023, 10, 24));
            Add(CalendarTerm.Day, RelativePeriod.Current, today, new DateTime(2023, 10, 25), new DateTime(2023, 10, 25));
            Add(CalendarTerm.Day, RelativePeriod.Next, today, new DateTime(2023, 10, 26), new DateTime(2023, 10, 26));
            
            Add(CalendarTerm.Week, RelativePeriod.Previous, today, new DateTime(2023, 10, 16), new DateTime(2023, 10, 22));
            Add(CalendarTerm.Week, RelativePeriod.Current, today, new DateTime(2023, 10, 23), new DateTime(2023, 10, 29));
            Add(CalendarTerm.Week, RelativePeriod.Next, today, new DateTime(2023, 10, 30), new DateTime(2023, 11, 05));
            
            Add(CalendarTerm.Month, RelativePeriod.Previous, today, new DateTime(2023, 09, 01), new DateTime(2023, 09, 30));
            Add(CalendarTerm.Month, RelativePeriod.Current, today, new DateTime(2023, 10, 01), new DateTime(2023, 10, 31));
            Add(CalendarTerm.Month, RelativePeriod.Next, today, new DateTime(2023, 11, 01), new DateTime(2023, 11, 30));
            

        }
    }

    [Theory]
    [ClassData(typeof(TermDateRangeTestData))]
    public void GetPeriodDates(CalendarTerm term, RelativePeriod period, DateTime today, DateTime expectedStart, DateTime expectedEnd)
    {
        var termDateRanges = new TermDateRanges(today);
        switch (period)
        {
            case RelativePeriod.Previous:
            {
                var (start, end) = termDateRanges.GetPreviousPeriodDates(term);
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
                return;
            }
            case RelativePeriod.Current:
            {
                var (start, end) = termDateRanges.GetCurrentPeriodDates(term);
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
                return;
            }
            case RelativePeriod.Next:
            {
                var (start, end) = termDateRanges.GetNextPeriodDates(term);
                Assert.Equal(expectedStart, start);
                Assert.Equal(expectedEnd, end);
                return;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(period), period, null);
        }
    }
    
    //
    // [Fact]
    // public void GetCurrentPeriodDates_WeeklyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetCurrentPeriodDates(CalendarTerm.Week);
    //     Assert.Equal(new DateTime(2023, 07, 03), start);
    //     Assert.Equal(new DateTime(2023, 07, 9), end);
    //     
    // }
    //
    // [Fact]
    //
    // public void GetCurrentPeriodDates_MonthlyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetCurrentPeriodDates(CalendarTerm.Month);
    //     Assert.Equal(new DateTime(2023, 07, 01), start);
    //     Assert.Equal(new DateTime(2023, 07, 31), end);
    // }
    //
    // [Fact]
    // public void GetCurrentPeriodDates_DailyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetCurrentPeriodDates(CalendarTerm.Day);
    //     Assert.Equal(new DateTime(2023, 07, 09), start);
    //     Assert.Equal(new DateTime(2023, 07, 09), end);
    // }
    //
    // [Fact]
    // public void GetPreviousPeriodDates_WeeklyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetPreviousPeriodDates(CalendarTerm.Week);
    //     Assert.Equal(new DateTime(2023, 06, 26), start);
    //     Assert.Equal(new DateTime(2023, 07, 02), end);
    // }
    //
    // [Fact]
    // public void GetPreviousPeriodDates_MonthlyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetPreviousPeriodDates(CalendarTerm.Month);
    //     Assert.Equal(new DateTime(2023, 06, 01), start);
    //     Assert.Equal(new DateTime(2023, 06, 30), end);
    // }
    //
    // [Fact]
    // public void GetPreviousPeriodDates_DailyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetPreviousPeriodDates(CalendarTerm.Day);
    //     Assert.Equal(new DateTime(2023, 07, 08), start);
    //     Assert.Equal(new DateTime(2023, 07, 08), end);
    // }
    //
    // [Fact]
    // public void GetNextPeriodDates_WeeklyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetNextPeriodDates(CalendarTerm.Week);
    //     Assert.Equal(new DateTime(2023, 07, 10), start);
    //     Assert.Equal(new DateTime(2023, 07, 16), end);
    // }
    //
    // [Fact]
    // public void GetNextPeriodDates_MonthlyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetNextPeriodDates(CalendarTerm.Month);
    //     Assert.Equal(new DateTime(2023, 08, 01), start);
    //     Assert.Equal(new DateTime(2023, 08, 31), end);
    // }
    //
    // [Fact]
    // public void GetNextPeriodDates_DailyTerm()
    // {
    //     var termDateRanges = new TermDateRanges(Today);
    //     var (start, end) = termDateRanges.GetNextPeriodDates(CalendarTerm.Day);
    //     Assert.Equal(new DateTime(2023, 07, 10), start);
    //     Assert.Equal(new DateTime(2023, 07, 10), end);
    // }
    
}