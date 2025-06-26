using System.Security.Cryptography;
using System.Text;

namespace APISeasonalMedic.Services
{
    public class CloudinaryService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _cloudName;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _baseUrl;

        public CloudinaryService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _cloudName = _configuration["Cloudinary:CloudName"] ?? throw new ArgumentNullException("CloudName");
            _apiKey = _configuration["Cloudinary:ApiKey"] ?? throw new ArgumentNullException("ApiKey");
            _apiSecret = _configuration["Cloudinary:ApiSecret"] ?? throw new ArgumentNullException("ApiSecret");
            _baseUrl = _configuration["Cloudinary:BaseUrl"] ?? "https://api.cloudinary.com/v1_1";
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                {
                    Console.WriteLine("URL no válida o no es de Cloudinary");
                    return true; // No es una imagen de Cloudinary
                }

                var publicId = ExtractPublicIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                {
                    Console.WriteLine("No se pudo extraer el publicId de la URL");
                    return false;
                }

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var signature = GenerateSignature(publicId, timestamp);

                var formData = new List<KeyValuePair<string, string>>
        {
            new("public_id", publicId),
            new("timestamp", timestamp.ToString()),
            new("api_key", _apiKey),
            new("signature", signature)
        };

                var formContent = new FormUrlEncodedContent(formData);
                var url = $"{_baseUrl}/{_cloudName}/image/destroy";

                Console.WriteLine($"Attempting to delete image with publicId: {publicId}");
                Console.WriteLine($"DELETE URL: {url}");

                var response = await _httpClient.PostAsync(url, formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Cloudinary Response Status: {response.StatusCode}");
                Console.WriteLine($"Cloudinary Response Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error de Cloudinary: {responseContent}");
                }

                // Verificar múltiples respuestas posibles
                var isSuccess = responseContent.Contains("\"result\":\"ok\"") ||
                               responseContent.Contains("\"result\":\"not found\""); // Si ya fue eliminada

                Console.WriteLine($"Delete operation successful: {isSuccess}");

                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando imagen de Cloudinary: {ex.Message}");
                Console.WriteLine($"URL: {imageUrl}");
                return false;
            }
        }

        private string ExtractPublicIdFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !url.Contains("cloudinary.com"))
                    return null;

                var uri = new Uri(url);
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                var uploadIndex = Array.FindIndex(pathSegments, segment => segment == "upload");
                if (uploadIndex == -1) return null;

                // Obtener segmentos después de "upload"
                var publicIdSegments = pathSegments.Skip(uploadIndex + 1).ToList();

                if (publicIdSegments.Count == 0) return null;

                // Verificar si hay parámetros de transformación (como v1234567890)
                if (publicIdSegments[0].StartsWith("v") &&
                    publicIdSegments[0].Length > 1 &&
                    publicIdSegments[0].Substring(1).All(char.IsDigit))
                {
                    publicIdSegments = publicIdSegments.Skip(1).ToList();
                }

                if (publicIdSegments.Count == 0) return null;

                // Remover extensión del último segmento
                var lastSegment = publicIdSegments.Last();
                var dotIndex = lastSegment.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    publicIdSegments[publicIdSegments.Count - 1] = lastSegment.Substring(0, dotIndex);
                }

                var publicId = string.Join("/", publicIdSegments);

                // Debug: Log para verificar
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Extracted PublicId: {publicId}");

                return publicId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting publicId from URL {url}: {ex.Message}");
                return null;
            }
        }
        

        private string GenerateSignature(string publicId, long timestamp)
        {
            var parameters = $"public_id={publicId}&timestamp={timestamp}{_apiSecret}";

            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(parameters));
                return Convert.ToHexString(hash).ToLower();
            }
        }
    }
}