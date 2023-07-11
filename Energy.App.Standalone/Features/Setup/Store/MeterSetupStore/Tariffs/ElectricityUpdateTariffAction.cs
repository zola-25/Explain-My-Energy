﻿using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Models.Tariffs;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.Tariffs;

public class ElectricityUpdateTariffAction
{

    public TariffDetail TariffDetail { get; }

    public ElectricityUpdateTariffAction(TariffDetail tariffDetail)
    {
        TariffDetail = tariffDetail;
    }

    [ReducerMethod]
    public static MeterSetupState Store(MeterSetupState state, ElectricityUpdateTariffAction action)
    {
        var updatedTariffState = action.TariffDetail.eMapToTariffState(addGuidForNewTariff: false);
        
        int index = state.ElectricityMeter.TariffDetails.FindIndex(c => c.GlobalId == updatedTariffState.GlobalId);

        var toUpdate = state.ElectricityMeter.TariffDetails.ToList();
        toUpdate[index] = updatedTariffState;

        return state with
        {
            ElectricityMeter = state.ElectricityMeter with
            {
                TariffDetails = toUpdate.OrderBy(c => c.DateAppliesFrom).ToImmutableList()
            }
        };
    }
}