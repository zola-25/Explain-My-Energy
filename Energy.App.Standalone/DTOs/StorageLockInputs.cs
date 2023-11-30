using Energy.App.Standalone.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Energy.App.Standalone.DTOs;

public class StorageLockInputs 
{

    [Required]
    [Display(Name = "IHD MAC ID")]
    [StringLength(16, ErrorMessage = "IHD MAC ID Invalid", MinimumLength = 16)]
    [ReadOnly(true)]
    public string IhdMacId { get; set; }

    [Required]
    [Display(Name = "Password")]
    [StringLength(60, ErrorMessage = "Password must be at least 9 characters", MinimumLength = 9)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{9,}$", ErrorMessage = "Password must contain at least one uppercase letter, and one digit")]
    public string Input1 { get; set; }

    [Required]
    [Display(Name = "Confirm Password")]
    [CompareProperty(nameof(Input1), ErrorMessage = "Passwords do not match")]
    public string Input2 { get; set; }

    
}
