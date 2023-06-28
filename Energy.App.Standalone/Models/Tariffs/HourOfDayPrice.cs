using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.Models.Tariffs
{
    public class HourOfDayPrice
    {

        [Required]
        public TimeSpan? HourOfDay { get; set; }

        [Required]
        [Range(0, 10E6)]
        public double PencePerKWh { get; set; }
    }
}
