using System.ComponentModel.DataAnnotations;

namespace APISeasonalMedic.DTOs
{
    public class UpdateProfileImageUrlDto
    {
        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string? ProfileImageUrl { get; set; }
    }
}
