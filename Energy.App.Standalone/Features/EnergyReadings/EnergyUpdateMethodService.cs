using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings
{
    public class EnergyUpdateMethodService : IEnergyUpdateMethodService
    {
        private readonly ICostCalculator _costCalculator;
        private readonly IEnergyReadingService _energyReadingService;

        public EnergyUpdateMethodService(ICostCalculator costCalculator,
                                         IEnergyReadingService energyReadingWorkerService)
        {
            _costCalculator = costCalculator;
            _energyReadingService = energyReadingWorkerService;
        }



        public async Task<UpdateMethod> GetUpdateMethod(MeterState meterSetup,
                                                        bool forceReload,
                                                        ImmutableList<BasicReading> existingBasicReadings,
                                                        ImmutableList<CostedReading> existingCostedReadings)
        {
            var meterType = meterSetup.MeterType;
            if (!meterSetup.SetupValid)
            {
                return new UpdateMethod(UpdateType.NotValid, null, null, 0);
            }

            if (existingBasicReadings.eIsNullOrEmpty() || forceReload)
            {
                var basicReadings = (await _energyReadingService.ImportFromMoveInOrPreviousYear(meterType)).ToImmutableList();
                var costedReadings = CalculateCostedReadings(basicReadings, meterSetup.TariffDetails);
                return new UpdateMethod(UpdateType.FullUpdate, basicReadings, costedReadings, basicReadings.Count);
            }

            var lastBasicReading = existingBasicReadings.Last().UtcTime;

            if (lastBasicReading < DateTime.UtcNow.Date.AddDays(-1))
            {
                var newBasicReadings = await _energyReadingService.ImportFromDate(meterType, lastBasicReading.Date);

                var updatedBasicReadings = existingBasicReadings.ToList();
                if (newBasicReadings.Any())
                {
                    updatedBasicReadings.RemoveAll
                    (
                        x => x.UtcTime >= newBasicReadings.First().UtcTime // just in case there is an overlap
                    );
                    updatedBasicReadings.AddRange(newBasicReadings);
                }
                int basicReadingsUpdated = newBasicReadings.Count;

                var newCostedReadings = CalculateCostedReadings(updatedBasicReadings, meterSetup.TariffDetails);

                return new UpdateMethod(UpdateType.FullUpdate, updatedBasicReadings.ToImmutableList(), newCostedReadings, basicReadingsUpdated);
            }

            if (existingCostedReadings.eIsNullOrEmpty()
                || existingCostedReadings.Last().UtcTime < lastBasicReading)
            {
                var newCostedReadings = CalculateCostedReadings(existingBasicReadings, meterSetup.TariffDetails);
                return new UpdateMethod(UpdateType.JustCosts, existingBasicReadings, newCostedReadings, 0);
            }

            return new UpdateMethod(UpdateType.NoUpdateNeeded, existingBasicReadings, existingCostedReadings, 0);
        }

        private ImmutableList<CostedReading> CalculateCostedReadings(IReadOnlyCollection<BasicReading> basicReadings, ImmutableList<TariffDetailState> tariffDetails)
        {
            var costedReadings = _costCalculator
                .GetCostReadings(basicReadings,
                    tariffDetails).ToImmutableList();
            return costedReadings;
        }
    }
}
