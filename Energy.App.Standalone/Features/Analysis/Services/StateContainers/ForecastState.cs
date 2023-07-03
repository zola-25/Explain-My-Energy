//using System.Collections.ObjectModel;
//using Blazored.LocalStorage;
//using Energy.App.Blazor.Client.Services.Analysis.Data;
//using Energy.App.Blazor.Client.Services.Analysis.Interfaces;
//using Energy.App.Blazor.Client.Services.DataLoading.Interfaces;
//using Energy.App.Blazor.Client.Services.DataLoading.Models;
//using Energy.App.Blazor.Shared;
//using Energy.App.Blazor.Shared.Analysis;

//namespace Energy.App.Blazor.Client.StateContainers
//{
//    public class ForecastState
//    {
//        private readonly ITermDateRanges _periodDateRanges;
//        private readonly WeatherDataState _weatherDataState;
//        private readonly ICostCalculator _costCalculator;
//        private readonly ILocalStorageService _localStorageService;
//        private readonly MeterState _meterState;


//        public ForecastState(ITermDateRanges periodDateRanges,
//            WeatherDataState weatherDataState,
//            ICostCalculator costCalculator,
//            ILocalStorageService localStorageService,
//            MeterState meterState)
//        {
//            _periodDateRanges = periodDateRanges;
//            _weatherDataState = weatherDataState;
//            _costCalculator = costCalculator;
//            _localStorageService = localStorageService;
//            _meterState = meterState;
//        }

//        public async Task<LinearCoefficients> GetCoefficients(MeterType meterType)
//        {
//            var globalId = _meterState[meterType].GlobalId;
//            return await _localStorageService.GetItemAsync<LinearCoefficients>(globalId.eMeterCoefficientsKey());
//        }

//        public async ValueTask SaveCoefficients(Guid globalId, LinearCoefficients coefficients)
//        {
//            await _localStorageService.SetItemAsync(globalId.eMeterCoefficientsKey(), coefficients);
//        }

//        public async ValueTask DeleteCoefficients(Guid globalId)
//        {
//            await _localStorageService.RemoveItemAsync(globalId.eMeterCoefficientsKey());
//        }


//        public async Task SetDefaultTempForecastCostReadings(Meter meter)
//        {
//            var coefficients = await _localStorageService.GetItemAsync<LinearCoefficients>(meter.eMeterCoefficientsKey());

//            var earliestDateNeeded = _periodDateRanges.GetCurrentPeriodStartingDate(CalendarTerm.Month);

//            var forecastBasicReadings = _weatherDataState.WeatherReadings.Where(c => c.ReadDate >= earliestDateNeeded)
//                .SelectMany(c =>
//                {
//                    var predictedDaily = coefficients.PredictConsumptionY(c.TemperatureMeanHourly);

//                    return Enumerable.Range(0, 48).Select(i => new BasicReading()
//                    {
//                        LocalTime = c.ReadDate.Date.AddMinutes(i * 30),
//                        KWh = predictedDaily / 48,
//                        Forecast = true
//                    });
//                });

//            var forecastCostReadings = await _costCalculator.GetCostedReadings(meter, forecastBasicReadings, CancellationToken.None);

//            switch (meter.MeterType)
//            {
//                case MeterType.Electricity:
//                    ElectricityDefaultTempForecast = forecastCostReadings.AsReadOnly();
//                    ElectricityTempDiffForecast = forecastCostReadings.ToList().AsReadOnly();
//                    break;
//                case MeterType.Gas:
//                    GasDefaultTempForecast = forecastCostReadings.AsReadOnly();
//                    GasTempDiffForecast = forecastCostReadings.ToList().AsReadOnly();
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        public ReadOnlyCollection<CostedReading> ElectricityDefaultTempForecast { get; private set; }
//        public ReadOnlyCollection<CostedReading> GasDefaultTempForecast { get; private set; }

//        public ReadOnlyCollection<CostedReading> GetDefaultTempForecast(MeterType meterType)
//        {
//            return meterType switch
//            {
//                MeterType.Electricity => ElectricityDefaultTempForecast,
//                MeterType.Gas => GasDefaultTempForecast,
//                _ => throw new ArgumentOutOfRangeException()
//            };
//        }


//        public double ElectricityDegreeDifference { get; private set; }
//        public double GasDegreeDifference { get; private set; }

//        public double GetDegreeDifference(MeterType meterType)
//        {
//            return meterType switch
//            {
//                MeterType.Electricity => ElectricityDegreeDifference,
//                MeterType.Gas => GasDegreeDifference,
//                _ => throw new ArgumentOutOfRangeException()
//            };
//        }

//        private void SetDegreeDifference(MeterType meterType, double degreeDifference)
//        {
//            switch (meterType)
//            {
//                case MeterType.Electricity:
//                    ElectricityDegreeDifference = degreeDifference;
//                    break;
//                case MeterType.Gas:
//                    GasDegreeDifference = degreeDifference;
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        public async Task UpdateDegreeDifference(MeterType meterType, double degreeDifference)
//        {
//            var defaultForecast = GetDefaultTempForecast(meterType);
//            var coefficients = await GetCoefficients(meterType);

//            var weatherReadings = _weatherDataState.WeatherReadings;

//            var start = defaultForecast.First().LocalTime.Date;
//            var end = defaultForecast.Last().LocalTime.Date;

//            var adjustedKWh = weatherReadings.Where(c => c.ReadDate >= start && c.ReadDate <= end)
//                .ToDictionary(c => c.ReadDate,
//                    wr => coefficients.PredictConsumptionY(wr.TemperatureMeanHourly + degreeDifference) / 48);

//            var updatedForecast = defaultForecast.ToList();
//            updatedForecast.ForEach(c =>
//            {
//                var date = c.LocalTime.Date;
//                c.KWh = adjustedKWh[date];
//            });

//            SetAdjustedForecast(meterType, updatedForecast.AsReadOnly());

//            SetDegreeDifference(meterType, degreeDifference);

//            if (NotifyDegreeDifferenceChange != null)
//            {
//                await NotifyDegreeDifferenceChange(meterType, degreeDifference);
//            }
//        }

//        public event Func<MeterType, double, Task> NotifyDegreeDifferenceChange;


//        private void SetAdjustedForecast(MeterType meterType, ReadOnlyCollection<CostedReading> updated)
//        {
//            switch (meterType)
//            {
//                case MeterType.Electricity:
//                    ElectricityTempDiffForecast = updated;
//                    break;
//                case MeterType.Gas:
//                    GasTempDiffForecast = updated;
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        public ReadOnlyCollection<CostedReading> GetTempDiffForecast(MeterType meterType)
//        {
//            return meterType switch
//            {
//                MeterType.Electricity => ElectricityTempDiffForecast,
//                MeterType.Gas => GasTempDiffForecast,
//                _ => throw new ArgumentOutOfRangeException()
//            };
//        }

//        public ReadOnlyCollection<CostedReading> ElectricityTempDiffForecast { get; private set; }

//        public ReadOnlyCollection<CostedReading> GasTempDiffForecast { get; private set; }



//    }

//}
