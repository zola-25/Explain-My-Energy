using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.DTOs.Tariffs;

public class TariffDetail : IValidatableObject
{
    public Guid GlobalId { get; set; }

    [Required]
    public DateTime? DateAppliesFrom { get; set; }

    [Required]
    [Range(0, 10E6)]
    public decimal PencePerKWh { get; set; }

    [Required]
    public bool IsHourOfDayFixed { get; set; }

    [Required]
    [Range(0, 10E6)]
    public decimal DailyStandingChargePence { get; set; }

    public List<HourOfDayPrice> HourOfDayPrices { get; set; } = new List<HourOfDayPrice>();
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsHourOfDayFixed)
        {
            if (HourOfDayPrices == null || !HourOfDayPrices.Any())
            {
                yield return new ValidationResult("Daily Variable Prices not set", new[] { "HourOfDayPrices" });
            }

            if (HourOfDayPrices.Count != 24)
            {
                yield return new ValidationResult("Missing Hour of Day Prices", new[] { "HourOfDayPrices" });

            }
        }

    }
}