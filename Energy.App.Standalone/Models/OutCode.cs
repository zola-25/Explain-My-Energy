using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.Models;

public class OutCode
{
    [Required(ErrorMessage = "Postal Area is required")]
    [Label("Postal Area")]
    public string OutCodeCharacters { get; set; }
}