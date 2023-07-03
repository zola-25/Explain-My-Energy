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
            ImmutableList<TariffDetailState> meterTariffDetails)
        {
            var firstMeterTariffDate = meterTariffDetails.Any() ? meterTariffDetails.MinBy(c => c.DateAppliesFrom.Value)?.DateAppliesFrom : null;

            // TODO: Get accurate for historical
            //TODO: Check vs old engeniq
            //TODO: Store costed readings for better performance
            //TODO: Catch failed downloads so not stuck in updating state
            // TODO check redux stack trace not slowing down

            List<Tariff> applicableTariffs =
                meterTariffDetails.Select(c => new Tariff()
                {

                    TariffType = TariffType.User,
                    DateAppliesFrom = c.DateAppliesFrom.Value,
                    HalfHourOfDayPrices = c.IsHourOfDayFixed ? CreateUniformHalfHourOfDayPrices(c.PencePerKWh).ToImmutableList() : c.HourOfDayPrices.SelectMany(ToHalfHourly).ToImmutableList(),
                    DailyStandingChargePence = c.DailyStandingChargePence,
                    HalfHourlyStandingChargePence = c.DailyStandingChargePence / 48m,

                }).OrderBy(c => c.DateAppliesFrom).ToList();



            IEnumerable<IGrouping<DateTime, BasicReading>> energyReadingsByDate = basicReadings.GroupBy(c => c.UtcTime.Date);

            Tariff currentTariff = applicableTariffs.First();
            bool hasNextTariff = applicableTariffs.Count > 1;
            Tariff nextTariff = hasNextTariff ? applicableTariffs.Skip(1).First() : null;

            foreach (IGrouping<DateTime, BasicReading> dateReadings in energyReadingsByDate)
            {
                if (dateReadings.Key < currentTariff.DateAppliesFrom)
                {
                    throw new ArgumentException($"Can't find any default or user defined tariff for {dateReadings.Key:D}");
                }

                Tariff tariffToUse;
                if (!hasNextTariff)
                {
                    tariffToUse = currentTariff;
                }
                else if (dateReadings.Key < nextTariff.DateAppliesFrom)
                {
                    tariffToUse = currentTariff;
                }
                else if (dateReadings.Key >= nextTariff.DateAppliesFrom)
                {
                    currentTariff = nextTariff;
                    tariffToUse = currentTariff;
                    nextTariff = applicableTariffs.FirstOrDefault(c => c.DateAppliesFrom > currentTariff.DateAppliesFrom);
                    if (nextTariff == null)
                    {
                        hasNextTariff = false;
                    }
                }
                else
                    throw new ArgumentException("No tariff found");

                IEnumerable<CostedReading> calculatedReadings = from e in dateReadings
                                                                join c in currentTariff.HalfHourOfDayPrices on e.UtcTime.TimeOfDay equals c.HourOfDay
                                                                select new CostedReading()
                                                                {
                                                                    KWh = e.KWh,
                                                                    LocalTime = e.UtcTime,
                                                                    PencePerKWh = c.PencePerKWh,
                                                                    DailyStandingChargePence = tariffToUse.DailyStandingChargePence,
                                                                    CostPence = (e.KWh * c.PencePerKWh) + tariffToUse.HalfHourlyStandingChargePence,
                                                                    HalfHourlyStandingChargePence = tariffToUse.HalfHourlyStandingChargePence,
                                                                    TariffAppliesFrom = tariffToUse.DateAppliesFrom,
                                                                    TariffType = tariffToUse.TariffType,
                                                                    Forecast = e.Forecast
                                                                };

                foreach (CostedReading calculatedReading in calculatedReadings)
                    yield return calculatedReading;
            }
        }

        private IEnumerable<HalfHourOfDayPrice> CreateUniformHalfHourOfDayPrices(decimal fixedPencePerKWh)
        {
            return Enumerable.Range(0, 48).Select(i => new HalfHourOfDayPrice()
            {
                PencePerKWh = fixedPencePerKWh / 48m,
                HourOfDay = TimeSpan.FromMinutes(i * 30),
            });
        }

        private record Tariff
        {
            public decimal DailyStandingChargePence { get; init; }
            public decimal HalfHourlyStandingChargePence { get; init; }
            public DateTime DateAppliesFrom { get; init; }
            public ImmutableList<HalfHourOfDayPrice> HalfHourOfDayPrices { get; init; }
            public TariffType TariffType { get; init; }
        }

        private record HalfHourOfDayPrice
        {
            public TimeSpan HourOfDay { get; init; }
            public decimal PencePerKWh { get; init; }
        }



        private static IEnumerable<HalfHourOfDayPrice> ToHalfHourly(HourOfDayPriceState price)
        {
            yield return new HalfHourOfDayPrice()
            {
                HourOfDay = price.HourOfDay.Value,
                PencePerKWh = price.PencePerKWh,
            };
            yield return new HalfHourOfDayPrice()
            {
                HourOfDay = price.HourOfDay.Value.Add(TimeSpan.FromMinutes(30)),
                PencePerKWh = price.PencePerKWh,
            };
        }
    }




}


