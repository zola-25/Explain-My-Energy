using Energy.App.Standalone.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Energy.App.Standalone.DTOs;

public class StorageLockInputs 
{
    [Required]
    [StringLength(60, ErrorMessage = "Must at least 9 characters", MinimumLength = 9)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{9,}$", ErrorMessage = "Must contain at least one uppercase letter, and one digit")]
    public string Input1 { get; set; }

    [Required]
    [Compare(nameof(Input1))]
    public string Input2 { get; set; }

    
}
