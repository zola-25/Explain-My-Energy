using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Data.EnergyReadings
{
    public class EnergyReadingService : IEnergyReadingService
    {
        private readonly IState<HouseholdState> _householdState;
        private readonly IEnergyReadingRetriever _energyReadingRetriever;

        public EnergyReadingService(IState<HouseholdState> householdState, IEnergyReadingRetriever energyReadingRetriever)
        {
            _householdState = householdState;
            this._energyReadingRetriever = energyReadingRetriever;
        }

        public async Task<List<BasicReading>> ImportFromMoveInOrPreviousYear(MeterType meterType, CancellationToken ctx = default)
        {

            var earliestDate = DateTime.UtcNow.AddYears(-1).AddDays(-30).Date;
            var startDate = _householdState.Value.MoveInDate.Value < earliestDate ? earliestDate : _householdState.Value.MoveInDate.Value;

            string macId = _householdState.Value.IhdMacId;


            var meterReadings = await _energyReadingRetriever.GetMeterReadings(startDate, DateTime.Today, macId, meterType);
            return meterReadings;
        }

        public async Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default)
        {
            string macId = _householdState.Value.IhdMacId;


            var meterReadings = await _energyReadingRetriever.GetMeterReadings(fromDate,
                                                                                     DateTime.Today,
                                                                                     macId,
                                                                                     meterType);
            return meterReadings;
        }
    }
}
