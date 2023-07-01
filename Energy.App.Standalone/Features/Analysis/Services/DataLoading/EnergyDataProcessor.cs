using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading
{
    public class EnergyDataProcessor : IEnergyDataProcessor
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;
        private readonly IForecastCoefficientsCreator _forecastCoefficientsCreator;
        private readonly WeatherDataState _weatherDataState;
        private readonly MeterDataState _meterDataState;
        private readonly ForecastState _forecastState;
        private readonly HistoricalAverageForecastState _historicalAverageForecastState;
        private readonly UserState _appState;

        public EnergyDataProcessor(IEnergyReadingImporter energyReadingImporter,
            IForecastCoefficientsCreator forecastCoefficientsCreator,
            WeatherDataState weatherDataState,
            MeterDataState meterDataState,
            ForecastState forecastState,
            HistoricalAverageForecastState historicalAverageForecastState,
            UserState appState)
        {
            _energyReadingImporter = energyReadingImporter;
            _forecastCoefficientsCreator = forecastCoefficientsCreator;
            _weatherDataState = weatherDataState;
            _meterDataState = meterDataState;
            _forecastState = forecastState;
            _historicalAverageForecastState = historicalAverageForecastState;
            _appState = appState;
        }


        public async Task UpdateData(Meter meter, CancellationToken ctx = default)
        {
            if (meter.GlobalId == default || meter.Authorized == false)
            {
                return;
            }

            var basicReadings = await _meterDataState.GetBasicReadings(meter.GlobalId);
            if (basicReadings == default || !basicReadings.Any())
            {
                basicReadings = await _energyReadingImporter.ImportFromMoveIn(meter, ctx);
            }
            else if (DateTime.Now - basicReadings.Last().LocalTime > TimeSpan.FromHours(24 + 9))
            {
                var fromDate = basicReadings.Last().LocalTime.Date.AddDays(1);
                var newReadings = await _energyReadingImporter.ImportFromDate(meter, fromDate, ctx);

                basicReadings.AddRange(newReadings);
            }

            await SetupReadings(meter, basicReadings);
        }

        private async Task SetupReadings(Meter meter, List<BasicReading> basicReadings)
        {
            if (_appState.HouseholdDetails.PrimaryHeatSource == meter.MeterType)
            {
                await SetupForHeatingMeter(meter, basicReadings);
            }
            else
            {
                await SetupForNonHeatingMeter(meter, basicReadings);
            }
        }

        public async Task RemoveData(Meter meter)
        {
            await _meterDataState.ClearMeterData(meter.GlobalId);
            await _forecastState.DeleteCoefficients(meter.GlobalId);
        }

        public async Task ReloadData(Meter meter, CancellationToken ctx = default)
        {
            await RemoveData(meter);

            if (meter.GlobalId == default || meter.Authorized == false)
            {
                return;
            }

            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveIn(meter, ctx);
            await SetupReadings(meter, basicReadings);
        }

        private async Task SetupForNonHeatingMeter(Meter meter, List<BasicReading> basicReadings)
        {
            await _historicalAverageForecastState.SetHistoricalAverageForecastReadings(meter, basicReadings);

            var forecast = _historicalAverageForecastState.GetHistoricalAverageForecast(meter.MeterType);

            DateTime lastReading = basicReadings.Last().LocalTime;

            var futureCosts = forecast.Where(c => c.LocalTime > lastReading).ToList();

            await _meterDataState.SetMeterData(meter.MeterType, basicReadings, futureCosts);
        }

        private async Task SetupForHeatingMeter(Meter meter, List<BasicReading> basicReadings)
        {
            var weatherReadings = _weatherDataState.WeatherReadings;

            var dailyConsumptionPoints =
                (from er in basicReadings
                 group new { Readings = er } by er.LocalTime.Date
                    into daily
                 join wr in weatherReadings on daily.Key equals wr.ReadDate
                 select new DailyConsumptionPoint
                 {
                     Date = daily.Key,
                     TemperatureCelsius = wr.TemperatureMeanHourly,
                     ConsumptionKWh = daily.Sum(c => c.Readings.KWh)
                 }
                ).ToList();

            var coefficients = _forecastCoefficientsCreator.GetForecastCoefficients(dailyConsumptionPoints);

            DateTime lastReading = basicReadings.Last().LocalTime;

            await _forecastState.SaveCoefficients(meter.GlobalId, coefficients);

            await _forecastState.SetDefaultTempForecastCostReadings(meter);

            var forecast = _forecastState.GetDefaultTempForecast(meter.MeterType);

            var futureCosts = forecast.Where(c => c.LocalTime > lastReading).ToList();

            await _meterDataState.SetMeterData(meter.MeterType, basicReadings, futureCosts);
        }
    }
}
