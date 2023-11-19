using Energy.App.Standalone.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Energy.App.Standalone.DTOs;

public class StorageUnlockInputs 
{
    [Required]
    [StringLength(300,MinimumLength = 1, ErrorMessage = "Please enter a valid key")]
    public string Input { get; set; }

}
