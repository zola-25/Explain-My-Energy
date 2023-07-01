using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading
{
    public class CostCalculator : ICostCalculator
    {

        public IEnumerable<CostedReading> GetCostReadings(
            IEnumerable<BasicReading> basicReadings,
            ICollection<TariffDetailState> meterTariffDetails)
        {
            var firstMeterTariffDate = meterTariffDetails.Any() ? meterTariffDetails.MinBy(c => c.DateAppliesFrom.Value)?.DateAppliesFrom : null;


            List<Tariff> applicableTariffs =
                meterTariffDetails.Select(c => new Tariff()
                {
                    TariffType = TariffType.User,
                    DateAppliesFrom = c.DateAppliesFrom.Value,
                    HourOfDayPrices = c.HourOfDayPrices.SelectMany(ToHalfHourly).ToList(),
                    DailyStandingChargePence = c.DailyStandingChargePence,
                    HalfHourlyStandingCharge = c.DailyStandingChargePence / 48
                }).OrderBy(c => c.DateAppliesFrom).ToList();



            IEnumerable<IGrouping<DateTime, BasicReading>> energyReadingsByDate = basicReadings.GroupBy(c => c.LocalTime.Date);

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
                                                                join c in currentTariff.HourOfDayPrices on e.LocalTime.TimeOfDay equals c.HourOfDay
                                                                select new CostedReading()
                                                                {
                                                                    KWh = e.KWh,
                                                                    LocalTime = e.LocalTime,
                                                                    PencePerKWh = c.PencePerKWh,
                                                                    DailyStandingChargePence = tariffToUse.DailyStandingChargePence,
                                                                    HalfHourlyStandingCharge = tariffToUse.HalfHourlyStandingCharge,
                                                                    TariffAppliesFrom = tariffToUse.DateAppliesFrom,
                                                                    TariffType = tariffToUse.TariffType,
                                                                    Forecast = e.Forecast
                                                                };

                foreach (CostedReading calculatedReading in calculatedReadings)
                    yield return calculatedReading;

            }
        }

        private class Tariff
        {
            public double DailyStandingChargePence { get; set; }
            public double HalfHourlyStandingCharge { get; set; }
            public DateTime DateAppliesFrom { get; set; }
            public ICollection<HourOfDayPrice> HourOfDayPrices { get; set; }
            public TariffType TariffType { get; set; }
        }

        private class HourOfDayPrice
        {
            public TimeSpan HourOfDay { get; set; }
            public double PencePerKWh { get; set; }
        }



        private static IEnumerable<HourOfDayPrice> ToHalfHourly(HourOfDayPriceState price)
        {
            yield return new HourOfDayPrice()
            {
                HourOfDay = price.HourOfDay.Value,
                PencePerKWh = price.PencePerKWh,
            };
            yield return new HourOfDayPrice()
            {
                HourOfDay = price.HourOfDay.Value.Add(TimeSpan.FromMinutes(30)),
                PencePerKWh = price.PencePerKWh,
            };
        }

    }
}
