using Energy.App.Blazor.Client.Services.Api.Interfaces;
using Energy.App.Blazor.Shared;

namespace Energy.App.Blazor.Client.StateContainers;

public class MeterState
{
    private readonly IMeterApi _meterApi;

    public MeterState(IMeterApi meterApi)
    {
        _meterApi = meterApi;
    }

    public Meter this[Guid meterId]
    {
        get
        {
            if (GasMeter.GlobalId == meterId)
            {
                return GasMeter;
            }

            if (ElectricityMeter.GlobalId == meterId)
            {
                return ElectricityMeter;
            }

            return null;
        }
    }

    public Meter this[MeterType meterType]
    {
        get
        {
            switch (meterType)
            {
                case MeterType.Gas:
                    return GasMeter;
                case MeterType.Electricity:
                    return ElectricityMeter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null);
            }
        }
    }

    public Meter Get(MeterType meterType)
    {
        switch (meterType)
        {
            case MeterType.Gas:
                return GasMeter;
            case MeterType.Electricity:
                return ElectricityMeter;
            default:
                throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null);
        }
    }

    public IEnumerable<Meter> GetAll()
    {
        if (GasMeter != null)
        {
            yield return GasMeter;
        }

        if (ElectricityMeter != null)
        {
            yield return ElectricityMeter;
        }
    }

    public Meter GasMeter { get; private set; }
    public Meter ElectricityMeter { get; private set; }


    public async Task SetMeter(Meter meter)
    {
        switch (meter.MeterType)
        {
            case MeterType.Electricity:
                await SetElectricityMeter(meter);
                break;
            case MeterType.Gas:
                await SetGasMeter(meter);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task SetElectricityMeter(Meter updatedMeter)
    {
        if (updatedMeter.MeterType != MeterType.Electricity)
            throw new ArgumentException("Expecting Electricity");

        ElectricityMeter = updatedMeter;

        if (NotifyElectricityMeterChange != null)
        {
            await NotifyElectricityMeterChange.Invoke(updatedMeter);
        }
    }

    public event Func<Meter, Task> NotifyElectricityMeterChange;

    public async Task SetGasMeter(Meter updatedMeter)
    {
        if (updatedMeter.MeterType != MeterType.Gas)
            throw new ArgumentException("Expecting Gas Meter");

        GasMeter = updatedMeter;

        if (NotifyGasMeterChange != null)
        {
            await NotifyGasMeterChange.Invoke(updatedMeter);
        }
    }

    public event Func<Meter, Task> NotifyGasMeterChange;

    public async Task LoadAll(CancellationToken ctx = default)
    {
        var meters = await _meterApi.GetAllMetersAsync(ctx);

        foreach (var meter in meters.Values)
        {
            await SetMeter(meter);
        }
    }

    public async Task ReloadMeter(Guid globalId)
    {
        var meter = await _meterApi.GetMeterAsync(globalId);
        await SetMeter(meter);
    }

    public async Task RemoveMeter(Guid globalId)
    {

        if (GasMeter.GlobalId == globalId)
        {
            GasMeter = null;
            if (NotifyGasMeterChange != null)
            {
                await NotifyGasMeterChange.Invoke(null);
            };
        }

        if (ElectricityMeter.GlobalId == globalId)
        {
            ElectricityMeter = null;
            if (NotifyElectricityMeterChange != null)
            {
                await NotifyElectricityMeterChange.Invoke(null);
            };
        }
    }
}