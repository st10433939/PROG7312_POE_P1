namespace PROG7312_POE_P1.Models
{
    public class SensorTelemetrySnapshot
    {
        public List<TelemetryPacket<float>> FloatReadings { get; set; } = new List<TelemetryPacket<float>>();

        public List<TelemetryPacket<int>> IntegerReadings { get; set; } = new List<TelemetryPacket<int>>();

        public List<TelemetryPacket<bool>> BooleanReadings { get; set; } = new List<TelemetryPacket<bool>>();
    }
}
