using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Data.EnergyReadings;

public class EnergyReadingService : IEnergyReadingService
{
    private readonly IState<HouseholdState> _householdState;
    private readonly IEnergyReadingRetriever _energyReadingRetriever;
    private readonly TimeProvider _timeProvider;

    public EnergyReadingService(IState<HouseholdState> householdState, IEnergyReadingRetriever energyReadingRetriever, TimeProvider timeProvider)
    {
        _householdState = householdState;
        this._energyReadingRetriever = energyReadingRetriever;
        _timeProvider = timeProvider;
    }

    public async Task<List<BasicReading>> ImportFromMoveInOrPreviousYear(MeterType meterType, CancellationToken ctx = default)
    {

        var earliestDate = _timeProvider.eUtcNow().AddYears(-1).AddDays(-30).Date;
        var startDate = _householdState.Value.MoveInDate.Value < earliestDate ? earliestDate : _householdState.Value.MoveInDate.Value;

        string macId = _householdState.Value.IhdMacId;


        var meterReadings = await _energyReadingRetriever.GetMeterReadings(startDate, _timeProvider.eToday(), macId, meterType);
        return meterReadings;
    }

    public async Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default)
    {
        string macId = _householdState.Value.IhdMacId;


        var meterReadings = await _energyReadingRetriever.GetMeterReadings(fromDate,
                                                                                 _timeProvider.eToday(),
                                                                                 macId,
                                                                                 meterType);
        return meterReadings;
    }
}
