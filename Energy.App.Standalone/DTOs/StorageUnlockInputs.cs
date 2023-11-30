using Energy.App.Standalone.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Energy.App.Standalone.DTOs;

public class StorageUnlockInputs 
{

    [Required]
    [Display(Name = "IHD MAC ID")]
    [StringLength(16, ErrorMessage = "IHD MAC ID should have 16 digits", MinimumLength = 16)]
    public string IhdMacId { get; set; }

    [Required]
    [StringLength(300,MinimumLength = 1, ErrorMessage = "Please enter a valid key")]
    public string Input { get; set; }

}
