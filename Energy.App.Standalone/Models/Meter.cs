using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.Models;

public class Meter : IValidatableObject
{
    public Guid GlobalId { get; set; }
    [Required] public MeterType MeterType { get; set; }

    [Required] public string Mpxn { get; set; }

    public bool Authorized { get; set; }
    public bool QueueFreshImport { get; set; }

    /// <summary>
    /// Only for retrieving, do not set on client site with intention of updating DB
    /// </summary>
    public List<TariffDetail> TariffDetails { get; set; }

    public bool HasData { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TariffDetails != null)
        {
            yield return new ValidationResult("Tariffs should not be set here", new[] { "TariffDetails" });
        }

        switch (MeterType)
        {
            case MeterType.Gas:
                {
                    if (Mpxn.eIsNotValidMprn())
                    {
                        yield return new ValidationResult("Gas MPRN Number is Invalid", new[] { "Mpxn" });
                    }

                    break;
                }
            case MeterType.Electricity:
                {
                    if (Mpxn.eIsNotValidMpan())
                    {
                        yield return new ValidationResult("Electricity MPAN Number is Invalid", new[] { "Mpxn" });
                    }

                    break;
                }
        }
    }
}