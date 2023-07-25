using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings
{
    public class EnergyImportValidationResult
    {
        public UpdateType UpdateType { get; init; }
        public bool IncludeCosts { get; init; }
        public bool CanUpdate { get; init; }
        public string Message { get; init; }
    }

    public class EnergyImportValidation : IEnergyImportValidation
    {
        public EnergyImportValidationResult Validate(MeterState meterSetup,
                                                        bool forceReload,
                                                        DateTime lastReadingsCheck,
                                                        ImmutableList<BasicReading> existingBasicReadings,
                                                        ImmutableList<CostedReading> existingCostedReadings)
        {
            var meterType = meterSetup.MeterType;
            if (!meterSetup.SetupValid)
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.NotValid,
                    CanUpdate = false,
                    Message = $"{meterType} Readings: Meter not setup"
                };
            }

            if (meterSetup.TariffDetails.eIsNullOrEmpty())
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.NotValid,
                    CanUpdate = false,
                    Message = $"{meterType} Readings: Meter Tariffs not setup"
                };
            }

            if (existingBasicReadings.eIsNullOrEmpty() || forceReload)
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.FullReload,
                    IncludeCosts = true,
                    CanUpdate = true,
                };
            }

            var lastBasicReading = existingBasicReadings.Last().UtcTime;
            bool updateCosts = existingCostedReadings.eIsNullOrEmpty()
                               || existingCostedReadings.Last().UtcTime < lastBasicReading;

            var timeSinceLastReadingsCheck = DateTime.UtcNow - lastReadingsCheck;

            if (lastBasicReading < DateTime.UtcNow.Date.AddDays(-1) && timeSinceLastReadingsCheck.TotalHours >= 6)
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.Update,
                    IncludeCosts = updateCosts,
                    CanUpdate = true,
                };
            }

            if (updateCosts)
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.JustCosts,
                    IncludeCosts = true,
                    CanUpdate = true,
                };
            }

            return new EnergyImportValidationResult
            {
                UpdateType = UpdateType.NoUpdateNeeded,
                CanUpdate = false,
            };
        }


    }
}
