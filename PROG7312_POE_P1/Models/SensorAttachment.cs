namespace PROG7312_POE_P1.Models
{
    public class SensorAttachment
    {
        public int Id { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string RelativeUrl { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
