﻿using Energy.Shared;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;


public class CostCalculator : ICostCalculator
{
    public List<CostedReading> GetCostReadings(
        IReadOnlyCollection<BasicReading> basicReadings,
        IEnumerable<TariffDetailState> meterTariffState)
    {
        var allTariffsOrderedArray = meterTariffState.OrderBy(c => c.DateAppliesFrom).ToArray();

        if (basicReadings.Count == 0)
        {
            return new List<CostedReading>();
        }

        var firstReadingTime = basicReadings.First().Utc;
        if (firstReadingTime < allTariffsOrderedArray[0].DateAppliesFrom)
        {
            string message = $"All tariffs apply after first reading date {firstReadingTime:D}";
            throw new ArgumentException(message);
        }

        //List<(TariffDetailState TariffDetailState, ImmutableList<BasicReading> BasicReadings)> tariffsForReadings = new();
        List<CostedReading> costedReadings = new();
        for (int i = 0; i < allTariffsOrderedArray.Length; i++)
        {
            var currentTariff = allTariffsOrderedArray[i];
            List<BasicReading> tariffReadings;

            if (i == allTariffsOrderedArray.Length - 1)
            {

                tariffReadings = basicReadings.Where(
                   c => c.Utc >= currentTariff.DateAppliesFrom).ToList();

            }
            else
            {
                var nextTariff = allTariffsOrderedArray[i + 1];
                tariffReadings = basicReadings.Where(
                   c => c.Utc >= currentTariff.DateAppliesFrom && c.Utc < nextTariff.DateAppliesFrom).ToList();

            }

            if (tariffReadings.Count == 0)
            {
                continue;
            }

            IEnumerable<HalfHourOfDayPrice> halfHourOfDayPrices;
            if (currentTariff.IsHourOfDayFixed)
            {
                halfHourOfDayPrices = CreateUniformHalfHourOfDayPrices(currentTariff.PencePerKWh);
            }
            else
            {
                halfHourOfDayPrices = currentTariff.HourOfDayPrices.SelectMany(ToHalfHourly);
            }
            decimal halfHourlyStandingChargePence = currentTariff.DailyStandingChargePence / 48m;

            IEnumerable<CostedReading> calculatedReadings = from basicReading in tariffReadings
                                                            join halfHourlyPrice in halfHourOfDayPrices on basicReading.Utc.TimeOfDay
                                                                equals halfHourlyPrice.HourOfDay
                                                            select new CostedReading()
                                                            {
                                                                TariffAppliesFrom = currentTariff.DateAppliesFrom.Value,
                                                                TariffDailyStandingCharge = currentTariff.DailyStandingChargePence,
                                                                TariffHalfHourlyStandingCharge = halfHourlyStandingChargePence,
                                                                TariffPencePerKWh = halfHourlyPrice.PencePerKWh,
                                                                IsFixedCostPerHour = currentTariff.IsHourOfDayFixed,
                                                                UtcTime = basicReading.Utc,
                                                                KWh = basicReading.KWh,
                                                                CostPounds = (basicReading.KWh * halfHourlyPrice.PencePerKWh + halfHourlyStandingChargePence) / 100,
                                                            };
            costedReadings.AddRange(calculatedReadings);
        }

        return costedReadings;

    }

    private IEnumerable<HalfHourOfDayPrice> CreateUniformHalfHourOfDayPrices(decimal fixedPencePerKWh)
    {
        return Enumerable.Range(0, 48).Select(i => new HalfHourOfDayPrice()
        {
            PencePerKWh = fixedPencePerKWh,
            HourOfDay = new TimeSpan(0, i * 30, 0),

        });
    }

    private static IEnumerable<HalfHourOfDayPrice> ToHalfHourly(HourOfDayPriceState price)
    {
        yield return new HalfHourOfDayPrice()
        {
            HourOfDay = price.HourOfDay.Value,
            PencePerKWh = price.PencePerKWh
        };
        yield return new HalfHourOfDayPrice()
        {
            HourOfDay = new TimeSpan(price.HourOfDay.Value.Hours, 30, 0),
            PencePerKWh = price.PencePerKWh
        };
    }
    private record HalfHourOfDayPrice
    {
        public decimal PencePerKWh { get; init; }
        public TimeSpan HourOfDay { get; init; }
    }

    //foreach (var basicReading in basicReadings)
    //{

    //    var tariffsAfterReadDate = allTariffDetailsOrdered.FindAll(
    //        c => basicReading.UtcTime >= c.DateAppliesFrom);

    //    foreach (var tariff in tariffsAfterReadDate)
    //    {
    //        if (basicReading.UtcTime >= tariff.DateAppliesFrom)
    //        {
    //            currentTariff = tariff;
    //        }
    //        else
    //        {
    //            break;
    //        }
    //    }

    //    if (basicReading.UtcTime >= allTariffDetailsOrdered.Last().DateAppliesFrom)
    //    {
    //        currentTariff = allTariffDetailsOrdered.Last();
    //    }
    //    else
    //    {
    //        currentTariff = allTariffDetailsOrdered.First(c => c.DateAppliesFrom <= basicReading.UtcTime);
    //    }
    //}

    //var firstMeterTariffDate = allTariffDetails.Any() ? allTariffDetails.MinBy(c => c.DateAppliesFrom.Value)?.DateAppliesFrom : null;

    //// TODO: Get accurate for historical
    ////TODO: Check vs old engeniq
    ////TODO: Store costed readings for better performance
    ////TODO: Catch failed downloads so not stuck in updating state
    //// TODO check redux stack trace not slowing down

    //List<Tariff> applicableTariffs =
    //    allTariffDetails.Select(c => new Tariff()
    //    {

    //        TariffType = TariffType.User,
    //        DateAppliesFrom = c.DateAppliesFrom.Value,
    //        HalfHourOfDayPrices = c.IsHourOfDayFixed ? CreateUniformHalfHourOfDayPrices(c.PencePerKWh).ToImmutableList() : c.HourOfDayPrices.SelectMany(ToHalfHourly).ToImmutableList(),
    //        DailyStandingChargePence = c.DailyStandingChargePence,
    //        HalfHourlyStandingChargePence = c.DailyStandingChargePence / 48m,

    //    }).OrderBy(c => c.DateAppliesFrom).ToList();



    //IEnumerable<IGrouping<DateTime, BasicReading>> energyReadingsByDate = basicReadings.GroupBy(c => c.UtcTime.Date);

    //Tariff currentTariff = applicableTariffs.First();
    //bool hasNextTariff = applicableTariffs.Count > 1;
    //Tariff nextTariff = hasNextTariff ? applicableTariffs.Skip(1).First() : null;

    //foreach (IGrouping<DateTime, BasicReading> dateReadings in energyReadingsByDate)
    //{
    //    if (dateReadings.Key < currentTariff.DateAppliesFrom)
    //    {
    //        throw new ArgumentException($"Can't find any default or user defined tariff for {dateReadings.Key:D}");
    //    }



    //    Tariff tariffToUse;
    //    if (!hasNextTariff)
    //    {
    //        tariffToUse = currentTariff;
    //    }
    //    else if (dateReadings.Key < nextTariff.DateAppliesFrom)
    //    {
    //        tariffToUse = currentTariff;
    //    }
    //    else if (dateReadings.Key >= nextTariff.DateAppliesFrom)
    //    {

    //        tariffToUse = applicableTariffs.FirstOrDefault(c => c.DateAppliesFrom >= currentTariff.DateAppliesFrom);
    //        if (tariffToUse != null)
    //        {
    //            hasNextTariff = false;
    //        }
    //    }
    //    else
    //        throw new ArgumentException("No tariff found");


}




//private record Tariff
//{
//    public decimal DailyStandingChargePence { get; init; }
//    public decimal HalfHourlyStandingChargePence { get; init; }
//    public DateTime DateAppliesFrom { get; init; }
//    public ImmutableList<HalfHourOfDayPrice> HalfHourOfDayPrices { get; init; }
//    public TariffType TariffType { get; init; }
//}


