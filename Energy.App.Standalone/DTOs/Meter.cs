using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Energy.Shared;
using MudBlazor;

namespace Energy.App.Standalone.DTOs;

public class Meter : IValidatableObject
{
    public Guid GlobalId { get; set; }
    [Required] public MeterType MeterType { get; set; }

    [Required] public string Mpxn { get; set; }

    public bool Authorized { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

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