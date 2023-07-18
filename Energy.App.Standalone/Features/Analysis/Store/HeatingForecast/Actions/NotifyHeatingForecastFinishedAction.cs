namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

class NotifyHeatingForecastFinishedAction
    {
        public NotifyHeatingForecastFinishedAction()
        {
        }

    public NotifyHeatingForecastFinishedAction(bool v1, string v2)
    {
        V1 = v1;
        V2 = v2;
    }

    public bool V1 { get; }
    public string V2 { get; }
}
