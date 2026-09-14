namespace PROG7312_POE_P1.Models
{
    public class TelemetryAlert
    {
        public int SensorId { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public string Severity { get; set; } = "Info";

        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
