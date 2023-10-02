using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.DTOs.Tariffs;

public class HourOfDayPrice
{

    [Required]
    public TimeSpan? HourOfDay { get; set; }

    [Required]
    [Range(0, 10E6)]
    public decimal PencePerKWh { get; set; }
}
