using System.ComponentModel.DataAnnotations;
using MudBlazor;

namespace Energy.App.Standalone.DTOs;

public class OutCode
{
    [Required(ErrorMessage = "Postal Area is required")]
    [Label("Postal Area")]
    public string OutCodeCharacters { get; set; }
}