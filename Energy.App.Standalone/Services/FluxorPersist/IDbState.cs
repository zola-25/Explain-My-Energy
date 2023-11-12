namespace Energy.App.Standalone.Services.FluxorPersist
{
    public interface IDbState
    {
        public string StateName { get; set; }
        public object State { get; set; }
    }
}