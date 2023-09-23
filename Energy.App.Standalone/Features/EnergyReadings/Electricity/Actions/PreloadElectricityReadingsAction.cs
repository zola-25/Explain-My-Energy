using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class PreloadElectricityReadingsAction
{
    public ElectricityReadingsState ElectricityReadingsState { get; }

    public PreloadElectricityReadingsAction(ElectricityReadingsState electricityReadingsState)
    {
        ElectricityReadingsState = electricityReadingsState;
    }

    [ReducerMethod]
    public static ElectricityReadingsState OnPreloadElectricityReadingsReducer(ElectricityReadingsState state, PreloadElectricityReadingsAction action)
    {
        state = action.ElectricityReadingsState;
        return state;
    }
}
