//using System.Collections.ObjectModel;
//using Blazored.LocalStorage;
//using Energy.App.Blazor.Client.Services.DataLoading.Interfaces;
//using Energy.App.Blazor.Client.Services.DataLoading.Models;
//using Energy.App.Blazor.Shared;
//using MathNet.Numerics;

//namespace Energy.App.Blazor.Client.StateContainers
//{
//    public class MeterDataState
//    {
//        private readonly UserState _appState;
//        private readonly MeterState _meterState;
//        private readonly WeatherDataState _weatherDataState;
//        private readonly ILocalStorageService _localStorageService;
//        private readonly ICostCalculator _costCalculator;

//        public MeterDataState(UserState appState,
//            MeterState meterState,
//            WeatherDataState weatherDataState,
//            ILocalStorageService localStorageService,
//            ICostCalculator costCalculator)
//        {
//            _appState = appState;
//            _meterState = meterState;
//            _weatherDataState = weatherDataState;
//            _localStorageService = localStorageService;
//            _costCalculator = costCalculator;
//        }

//        public bool NonHeatingMeterDataLoaded { get; private set; }

//        public bool HeatingMeterDataLoaded { get; private set; }

//        private async Task NotifyMeterTypeLoaded(MeterType meterType, bool loaded)
//        {
//            if (_appState.HouseholdDetails.PrimaryHeatSource == meterType)
//            {
//                HeatingMeterDataLoaded = loaded;

//                if (NotifyHeatingMeterDataLoaded != null)
//                {
//                    await NotifyHeatingMeterDataLoaded.Invoke(meterType, loaded);
//                }
//            }
//            else
//            {
//                NonHeatingMeterDataLoaded = loaded;

//                if (NotifyNonHeatingMeterDataLoaded != null)
//                {
//                    await NotifyNonHeatingMeterDataLoaded.Invoke(meterType, loaded);
//                }
//            }
//        }

//        public event Func<MeterType, bool, Task> NotifyHeatingMeterDataLoaded;
//        public event Func<MeterType, bool, Task> NotifyNonHeatingMeterDataLoaded;



//        public async Task<List<BasicReading>> GetBasicReadings(Guid globalId)
//        {
//            return await _localStorageService.GetItemAsync<List<BasicReading>>(globalId.eBasicReadingsKey());
//        }

//        public async ValueTask DeleteBasicReadings(Guid globalId)
//        {
//            await _localStorageService.RemoveItemAsync(globalId.eBasicReadingsKey());
//            SetLatestReading(_meterState[globalId].MeterType, null);
//        }

//        public async ValueTask SaveBasicReadings(Guid globalId, List<BasicReading> meterReadings)
//        {
//            SetLatestReading(_meterState[globalId].MeterType, meterReadings.Last().LocalTime);
//            await _localStorageService.SetItemAsync(globalId.eBasicReadingsKey(), meterReadings);
//        }

//        public ReadOnlyCollection<CostedReading> ElectricityCostedReadings { get; private set; }
//        public ReadOnlyCollection<CostedReading> GasCostedReadings { get; private set; }

//        public ReadOnlyCollection<CostedReading> GetCostedReadings(MeterType meterType)
//        {
//            return meterType switch
//            {
//                MeterType.Electricity => ElectricityCostedReadings,
//                MeterType.Gas => GasCostedReadings,
//                _ => throw new ArgumentOutOfRangeException()
//            };
//        }

//        public async Task ClearMeterData(Guid globalId)
//        {
//            var meter = _meterState[globalId];
//            var meterType = meter.MeterType;

//            await DeleteBasicReadings(meter.GlobalId);

//            SetCostedReadings(meterType, null);

//            await NotifyMeterTypeLoaded(meterType, false);
//        }

//        public async Task SetMeterData(MeterType meterType, List<BasicReading> basicReadings, List<CostedReading> futureCostedReadings)
//        {
//            var meter = _meterState[meterType];

//            await SaveBasicReadings(meter.GlobalId, basicReadings);

//            var costedReadings = await _costCalculator.GetCostedReadings(meter, basicReadings);

//            costedReadings.AddRange(futureCostedReadings);

//            SetCostedReadings(meterType, costedReadings.AsReadOnly());

//            await NotifyMeterTypeLoaded(meterType, true);
//        }

//        public DateTime GetLatestReading(MeterType meterType)
//        {
//            return meterType switch
//            {
//                MeterType.Electricity => ElectricityLatestReading ?? throw new ArgumentNullException(nameof(ElectricityLatestReading)),
//                MeterType.Gas => GasLatestReading ?? throw new ArgumentNullException(nameof(GasLatestReading)),
//                _ => throw new ArgumentOutOfRangeException()
//            };
//        }

//        private void SetLatestReading(MeterType meterType, DateTime? latestReading)
//        {
//            switch (meterType)
//            {
//                case MeterType.Electricity:
//                    ElectricityLatestReading = latestReading;
//                    break;
//                case MeterType.Gas:
//                    GasLatestReading = latestReading;
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        public DateTime? ElectricityLatestReading { get; private set; }
//        public DateTime? GasLatestReading { get; private set; }

//        public MeterChartData GetMeterChartData(MeterType meterType)
//        {
//            var meter = _meterState[meterType];
//            var costedReadings = GetCostedReadings(meterType).ToList();
//            var firstReading = costedReadings.First().LocalTime;
//            var lastReading = costedReadings.Last().LocalTime;

//            List<TemperatureIconPoint> temperatureIconPoints = null;

//            if (_appState.HouseholdDetails.PrimaryHeatSource == meterType)
//            {
//                temperatureIconPoints = _weatherDataState.WeatherReadings.Where(c => c.ReadDate >= firstReading && c.ReadDate <= lastReading)
//                    .Select(c =>
//                    {
//                        double nearestDegree = c.TemperatureMeanHourly.Round(0);
//                        return new TemperatureIconPoint()
//                        {
//                            DateTicks = c.ReadDate.eToUnixTime(),
//                            TemperatureCelsius = nearestDegree,
//                            Summary = c.Summary,
//                            TemperatureCelsiusUnmodified = nearestDegree
//                        };
//                    }).ToList();
//            }
//            int firstForecast = costedReadings.FindIndex(c => c.Forecast);

//            var latestReading = firstForecast == -1 ? lastReading : costedReadings[firstForecast - 1].LocalTime;
//            return new MeterChartData()
//            {
//                MeterChartProfile = new MeterChartProfile()
//                {
//                    GlobalId = meter.GlobalId,
//                    LatestReading = latestReading.eToUnixTime(),
//                    MostRecentWeekStart = lastReading.Date.AddDays(-7).eToUnixTime(),
//                    ProfileStart = firstReading.eToUnixTime(),
//                    ProfileEnd = lastReading.eToUnixTime(),
//                    ChartReadings = costedReadings.Select(Mapping.MapToChartReading).ToList(),
//                },
//                TemperatureIconPoints = temperatureIconPoints
//            };
//        }


//        private void SetCostedReadings(MeterType meterType, ReadOnlyCollection<CostedReading> costedReadings)
//        {
//            switch (meterType)
//            {
//                case MeterType.Electricity:
//                    ElectricityCostedReadings = costedReadings;
//                    break;
//                case MeterType.Gas:
//                    GasCostedReadings = costedReadings;
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//    }
//}
