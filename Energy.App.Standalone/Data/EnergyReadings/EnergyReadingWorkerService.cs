using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.Shared;
using Fluxor;
using SpawnDev.BlazorJS.WebWorkers;

namespace Energy.App.Standalone.Data.EnergyReadings
{
    public class EnergyReadingWorkerService : IEnergyReadingWorkerService
    {

        private readonly IState<HouseholdState> _householdState;
        private readonly WebWorkerService _webWorkerService;


        public EnergyReadingWorkerService(IState<HouseholdState> householdState,
                                          WebWorkerService webWorkerService)
        {
            _householdState = householdState;
            this._webWorkerService = webWorkerService;
        }

        public async Task<List<BasicReading>> ImportFromMoveInOrPreviousYear(MeterType meterType, CancellationToken ctx = default)
        {

            var startDate = _householdState.Value.MoveInDate.Value < DateTime.UtcNow.AddYears(-1) ? DateTime.UtcNow.AddYears(-1) : _householdState.Value.MoveInDate.Value;

            string macId = _householdState.Value.IhdMacId;

            using var webWorker = await _webWorkerService.GetWebWorker();
            var energyReadingRetrieverWorker = webWorker.GetService<IEnergyReadingRetriever>();

            var meterReadings = await energyReadingRetrieverWorker.GetMeterReadings(startDate, DateTime.Today, macId, meterType);
            return meterReadings;
        }

        public async Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default)
        {
            string macId = _householdState.Value.IhdMacId;

            using var webWorker = await _webWorkerService.GetWebWorker();
            var energyReadingRetrieverWorker = webWorker.GetService<IEnergyReadingRetriever>();

            var meterReadings = await energyReadingRetrieverWorker.GetMeterReadings(fromDate,
                                                                                     DateTime.Today,
                                                                                     macId,
                                                                                     meterType);
            return meterReadings;
        }


    }
}
