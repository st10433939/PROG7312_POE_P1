namespace PROG7312_POE_P1.Models
{
    public class GatewaySummary
    {
        public int SensorCount { get; set; }

        public int FloatReadingCount { get; set; }

        public int IntegerReadingCount { get; set; }

        public int BooleanReadingCount { get; set; }

        public double CombinedPowerWatts { get; set; }

        public bool HighPowerLoad { get; set; }

    }
}
