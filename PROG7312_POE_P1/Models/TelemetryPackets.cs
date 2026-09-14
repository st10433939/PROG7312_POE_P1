using System.ComponentModel.DataAnnotations;

namespace PROG7312_POE_P1.Models
{
    public class TelemetryPacket<T> where T : struct
    {
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        public T Value { get; set; }

        public string Unit { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
