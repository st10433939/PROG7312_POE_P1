using System.ComponentModel.DataAnnotations;

namespace PROG7312_POE_P1.Models
{
    public class SensorRegistrationRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public SensorCategory Category { get; set; }
    }
}
