namespace PROG7312_POE_P1.Models
{
    public class Sensor
    {
        public int Id { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public SensorCategory Category { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public List<SensorAttachment> Attachments { get; set; } = new List<SensorAttachment>();
    }
}
