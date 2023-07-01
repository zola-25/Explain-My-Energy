using System.Collections.ObjectModel;
using Blazored.LocalStorage;
using Energy.App.Blazor.Client.Services.Analysis;
using Energy.App.Blazor.Client.Services.Analysis.Data;
using Energy.App.Blazor.Client.Services.Analysis.Interfaces;
using Energy.App.Blazor.Client.Services.DataLoading.Interfaces;
using Energy.App.Blazor.Client.Services.DataLoading.Models;
using Energy.App.Blazor.Shared;
using Energy.App.Blazor.Shared.Analysis;

namespace Energy.App.Blazor.Client.StateContainers
{
    public class HistoricalAverageForecastState
    {
        private readonly ITermDateRanges _termDateRanges;
        private readonly ICostCalculator _costCalculator;
        private readonly MeterState _meterState;
        private readonly WeatherDataState _weatherDataState;

        public HistoricalAverageForecastState(ITermDateRanges termDateRanges,
            ICostCalculator costCalculator,
            MeterState meterState,
            WeatherDataState weatherDataState)
        {
            _termDateRanges = termDateRanges;
            _costCalculator = costCalculator;
            _meterState = meterState;
            _weatherDataState = weatherDataState;
        }

        public async Task SetHistoricalAverageForecastReadings(Meter meter, List<BasicReading> basicReadings)
        {
            var startOfThisMonth = _termDateRanges.GetCurrentPeriodStartingDate(CalendarTerm.Month);

            var dailyAverageKWh = basicReadings.Where(c => c.LocalTime < startOfThisMonth).TakeLast(30)
                .GroupBy(c => c.LocalTime.Date).Select(c => c.Sum(d => d.KWh)).Average();

            var lastWeatherDate = _weatherDataState.WeatherReadings.Last().ReadDate;

            var lastReadingDate = basicReadings.Last().LocalTime.Date;

            var forecastBasicReadings = lastReadingDate.AddDays(1).eGenerateAllDatesBetween(lastWeatherDate).SelectMany(
                futureDate =>
                {
                    return Enumerable.Range(0, 48).Select(i => new BasicReading()
                    {
                        LocalTime = futureDate.AddMinutes(i * 30),
                        KWh = dailyAverageKWh / 48,
                        Forecast = true
                    });
                });

            var forecastCostReadings = await _costCalculator.GetCostedReadings(meter, forecastBasicReadings, CancellationToken.None);

            SetHistoricalAverageForecast(meter.MeterType, forecastCostReadings.AsReadOnly());
        }

        private void SetHistoricalAverageForecast(MeterType meterType, ReadOnlyCollection<CostedReading> forecastCostReadings)
        {
            switch (meterType)
            {
                case MeterType.Electricity:
                    ElectricityHistoricalAverageForecast = forecastCostReadings;
                    break;
                case MeterType.Gas:
                    GasHistoricalAverageForecast = forecastCostReadings;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ReadOnlyCollection<CostedReading> ElectricityHistoricalAverageForecast { get; private set; }
        public ReadOnlyCollection<CostedReading> GasHistoricalAverageForecast { get; private set; }

        public ReadOnlyCollection<CostedReading> GetHistoricalAverageForecast(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityHistoricalAverageForecast,
                MeterType.Gas => GasHistoricalAverageForecast,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
