using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class UpdateProfileImageUrlDto
    {
        [Required]
        [Url]
        public string ProfileImageUrl { get; set; }
    }
}
