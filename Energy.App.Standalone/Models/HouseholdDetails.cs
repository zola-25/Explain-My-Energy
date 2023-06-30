using Energy.Shared;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.Models;

public class HouseholdDetails
{
    [Required(ErrorMessage = "Move-in Date is required")]
    [Label("Move-in Date")]
    public DateTime? MoveInDate { get; set; }

    [Required(ErrorMessage = "Smart Mater MAC ID is required")]
    [Label("IHD MAC ID")]
    [StringLength(maximumLength: 16, MinimumLength = 16)]
    public string IhdMacId { get; set; }


    [Required(ErrorMessage = "Postal Area is required for weather forecasting")]
    [Label("Postal Area")]
    [ValidateComplexType]
    public OutCode OutCode { get; set; }

    [Required(ErrorMessage = "Primary heating source is required")]
    [Label("What is your home's primary source of heat?")]
    public MeterType PrimaryHeatSource { get; set; }
}