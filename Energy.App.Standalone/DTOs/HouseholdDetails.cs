using System.ComponentModel.DataAnnotations;
using Energy.Shared;
using MudBlazor;

namespace Energy.App.Standalone.DTOs;

public class HouseholdDetails
{
    [Required(ErrorMessage = "Move-in Date is required")]
    [Display(Name = "Move-in Date")]
    public DateTime? MoveInDate { get; set; }

    [Required(ErrorMessage = "Smart Meter IHD MAC ID is required")]
    [Display(Name = "IHD MAC ID")]
    [StringLength(maximumLength: 16, MinimumLength = 16, ErrorMessage = "IHD MAC ID should be exactly 16 characters and digits")]
    public string IhdMacId { get; set; }


    [Required(ErrorMessage = "Postal Area is required for weather forecasting")]
    [Display(Name = "Postal Area")]
    [ValidateComplexType]
    public OutCode OutCode { get; set; }

    [Required(ErrorMessage = "Primary heating source is required")]
    [Display(Name = "What is your home's primary source of heat?")]
    public MeterType PrimaryHeatSource { get; set; }
}