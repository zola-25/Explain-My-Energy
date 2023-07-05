using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using System.Collections.Immutable;
using MathNet.Numerics;
using Fluxor;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.App.Standalone.Features.Setup.Store;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class TempForecastAnalyzer : ITempForecastAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IForecastGenerator _forecastGenerator;
    private readonly ICostCalculator _costCalculator;
    IState<GasReadingsState> _gasReadingsState;
    IState<ElectricityReadingsState> _electricityReadingsState;
    IState<WeatherState> _weatherState;
    IState<LinearCoefficientsState> _linearCoefficientsState;
    
    IState<ElectricityTariffsState> _electricityTariffsState;

    IState<GasTariffsState> _gasTariffsState;


    public TempForecastAnalyzer(Co2ConversionFactors co2Conversion,
                                ITermDateRanges periodDateRanges,
                                IForecastGenerator forecastGenerator,
                                ICostCalculator costCalculator,
                                IState<GasReadingsState> gasReadingsState,
                                IState<ElectricityReadingsState> electricityReadingsState,
                                IState<WeatherState> weatherState,
                                IState<LinearCoefficientsState> linearCoefficientsState,
                                IState<GasTariffsState> gasTariffsState,
                                IState<ElectricityTariffsState> electricityTariffsState)
    {
        _co2Conversion = co2Conversion;
        _periodDateRanges = periodDateRanges;
        _forecastGenerator = forecastGenerator;
        _costCalculator = costCalculator;
        _gasReadingsState = gasReadingsState;
        _electricityReadingsState = electricityReadingsState;
        _weatherState = weatherState;
        _linearCoefficientsState = linearCoefficientsState;
        _gasTariffsState = gasTariffsState;
        _electricityTariffsState = electricityTariffsState;
    }

    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, 
        CalendarTerm duration,
        decimal degreeDifference)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(duration);

        ForecastAnalysis results = ForecastAnalysis(meterType,
                                                    degreeDifference,
                                                    start,
                                                    end);

        return results;
    }

    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, 
        CalendarTerm duration,
        decimal degreeDifference)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);

        ForecastAnalysis results = ForecastAnalysis(meterType,
                                                    degreeDifference,
                                                    start,
                                                    end);

        return results;
    }



    private ForecastAnalysis ForecastAnalysis(
        MeterType meterType,
        decimal degreeDifference,
        DateTime start,
        DateTime end)
    {
        var periodWeatherReadings = _weatherState.Value.WeatherReadings.Where(c => c.UtcReadDate >= start && c.UtcReadDate <= end).ToImmutableList();

        var forecastBasicReadings = _forecastGenerator.GetBasicReadingsForecast(start, end, degreeDifference, _linearCoefficientsState.Value, periodWeatherReadings);
        
        ImmutableList<TariffDetailState> tariffDetailStates;
        switch (meterType)
        {
            case MeterType.Electricity:
                tariffDetailStates = _electricityTariffsState.Value.TariffDetails;
                break;
            case MeterType.Gas:
                tariffDetailStates = _gasTariffsState.Value.TariffDetails;
                break;
            default:
                throw new NotImplementedException();
        }
        
        var forecastCostedReadings = _costCalculator.GetCostReadings(forecastBasicReadings, tariffDetailStates);

        decimal forecastConsumption = forecastCostedReadings.Sum(c => c.KWh);


        ForecastAnalysis results = new ForecastAnalysis()
        {
            NumberOfDays = periodWeatherReadings.Count,
            Start = start,
            End = end,
            ForecastConsumption = forecastConsumption.Round(0),
            ForecastCostPounds = (forecastCostedReadings.Sum(c => c.ReadingTotalCostPence) / 100m).Round(2),
            ForecastCo2 = (forecastConsumption * _co2Conversion.GetCo2ConversionFactor(meterType)).Round(1),
            TemperatureRange = new TemperatureRange()
            {
                LowDailyTemp = Convert.ToInt32(periodWeatherReadings.Min(c => c.TemperatureAverage)),
                HighDailyTemp = Convert.ToInt32(periodWeatherReadings.Max(c => c.TemperatureAverage)),
                AverageTemp = Convert.ToInt32(periodWeatherReadings.Average(c => c.TemperatureAverage))
            }
        };
        return results;
    }

}