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
        public bool CanUpdate { get; init; }
        public string Message { get; init; }
    }

    public class EnergyImportValidation : IEnergyImportValidation
    {
        public EnergyImportValidationResult Validate(MeterState meterSetup,
                                                        bool forceReload,
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
                    CanUpdate = true,
                };
            }

            var lastBasicReading = existingBasicReadings.Last().UtcTime;

            if (lastBasicReading < DateTime.UtcNow.Date.AddDays(-1))
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.Update,
                    CanUpdate = true,
                };
            }

            if (existingCostedReadings.eIsNullOrEmpty()
                || existingCostedReadings.Last().UtcTime < lastBasicReading)
            {
                return new EnergyImportValidationResult
                {
                    UpdateType = UpdateType.JustCosts,
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
