using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading
{

    public class CostCalculator : ICostCalculator
    {

        public ImmutableList<CostedReading> GetCostReadings(
            ImmutableList<BasicReading> basicReadings,
            ImmutableList<TariffDetailState> meterTariffDetails)
        {
            var costedReadings = GetCostReadingsEnum(basicReadings, meterTariffDetails).ToImmutableList();
            return costedReadings;
        }


        public IEnumerable<CostedReading> GetCostReadingsEnum(
            ImmutableList<BasicReading> basicReadings,
            ImmutableList<TariffDetailState> allTariffDetails)
        {
            var allTariffDetailsOrdered = allTariffDetails.OrderBy(c => c.DateAppliesFrom).ToArray();

            var firstReadingTime = basicReadings.First().UtcTime;
            if (firstReadingTime < allTariffDetailsOrdered[0].DateAppliesFrom)
            {
                string message = $"All tariffs apply after first reading date {firstReadingTime:D}";
                throw new ArgumentException(message);
            }

            //List<(TariffDetailState TariffDetailState, ImmutableList<BasicReading> BasicReadings)> tariffsForReadings = new();

            for (int i = 0; i < allTariffDetailsOrdered.Length; i++)
            {
                var currentTariff = allTariffDetailsOrdered[i];
                ImmutableList<BasicReading> tariffReadings;

                if (i == allTariffDetailsOrdered.Length - 1)
                {

                    tariffReadings = basicReadings.FindAll(
                       c => c.UtcTime >= currentTariff.DateAppliesFrom);

                }
                else
                {
                    var nextTariff = allTariffDetailsOrdered[i + 1];
                    tariffReadings = basicReadings.FindAll(
                       c => c.UtcTime >= currentTariff.DateAppliesFrom && c.UtcTime < nextTariff.DateAppliesFrom);

                }

                List<HalfHourOfDayPrice> halfHourOfDayPrices = new();
                if (currentTariff.IsHourOfDayFixed)
                {
                    halfHourOfDayPrices = CreateUniformHalfHourOfDayPrices(currentTariff.PencePerKWh).ToList();
                }
                else
                {
                    halfHourOfDayPrices = currentTariff.HourOfDayPrices.SelectMany(ToHalfHourly).ToList();
                }
                decimal halfHourlyStandingChargePence = currentTariff.DailyStandingChargePence / 48m;

                IEnumerable<CostedReading> calculatedReadings = from basicReading in tariffReadings
                                                                join halfHourlyPrice in halfHourOfDayPrices on basicReading.UtcTime.Minute 
                                                                    equals halfHourlyPrice.HourOfDayMinutes
                                                                select new CostedReading()
                                                                {
                                                                    KWh = basicReading.KWh,
                                                                    UtcTime = basicReading.UtcTime,
                                                                    TariffHalfHourlyPencePerKWh = halfHourlyPrice.PencePerKWh,
                                                                    TariffDailyStandingChargePence = currentTariff.DailyStandingChargePence,
                                                                    ReadingTotalCostPence = (basicReading.KWh * halfHourlyPrice.PencePerKWh) + halfHourlyStandingChargePence,
                                                                    TariffHalfHourlyStandingChargePence = halfHourlyStandingChargePence,
                                                                    TariffAppliesFrom = currentTariff.DateAppliesFrom.Value,
                                                                    Forecast = basicReading.Forecast
                                                                };

                foreach (CostedReading calculatedReading in calculatedReadings)
                    yield return calculatedReading;

            }

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

        private IEnumerable<HalfHourOfDayPrice> CreateUniformHalfHourOfDayPrices(decimal fixedPencePerKWh)
        {
            return Enumerable.Range(0, 48).Select(i => new HalfHourOfDayPrice()
            {
                PencePerKWh = fixedPencePerKWh / 48m,
                HourOfDayMinutes = i * 30,
            });
        }

        private static IEnumerable<HalfHourOfDayPrice> ToHalfHourly(HourOfDayPriceState price)
        {
            yield return new HalfHourOfDayPrice()
            {
                HourOfDayMinutes = price.HourOfDay.Value.Minutes,
                PencePerKWh = price.PencePerKWh,
            };
            yield return new HalfHourOfDayPrice()
            {
                HourOfDayMinutes = price.HourOfDay.Value.Minutes + 30,
                PencePerKWh = price.PencePerKWh,
            };
        }
        private record HalfHourOfDayPrice
        {
            public int HourOfDayMinutes { get; init; }
            public decimal PencePerKWh { get; init; }
        }
    }


}




//private record Tariff
//{
//    public decimal DailyStandingChargePence { get; init; }
//    public decimal HalfHourlyStandingChargePence { get; init; }
//    public DateTime DateAppliesFrom { get; init; }
//    public ImmutableList<HalfHourOfDayPrice> HalfHourOfDayPrices { get; init; }
//    public TariffType TariffType { get; init; }
//}










}


