﻿using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings
{
    public interface IEnergyUpdateMethodService
    {
        Task<UpdateMethod> GetUpdateMethod(MeterState meterSetup, bool forceReload, ImmutableList<BasicReading> existingBasicReadings, ImmutableList<CostedReading> existingCostedReadings);
    }
}