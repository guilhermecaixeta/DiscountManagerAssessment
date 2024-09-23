using System.ComponentModel.DataAnnotations;

namespace DiscountFrontEnd.ViewModels;

public class GenerateDiscountCodeViewModel
{
    [Required]
    [Range(1, 2000)]
    public uint Count { get; set; }

    [Required]
    [Range(7,8)]
    public uint Length { get; set; }
}
