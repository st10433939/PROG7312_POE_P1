namespace PROG7312_POE_P1.Models
{
    public readonly struct MeterReading
    {
        public double Watts { get; }

        public MeterReading(double watts) 
        { 
            Watts = watts; 
        }

        //Operator overloading
        public static MeterReading operator +(
            MeterReading first,
            MeterReading second)
        {
            return new MeterReading ( first.Watts +  second.Watts );
        }

        public static MeterReading operator -(
            MeterReading first,
            MeterReading second)
        {
            return new MeterReading( first.Watts - second.Watts );
        }

        public static bool operator >(
            MeterReading first,
            MeterReading second)
        {
            return first.Watts > second.Watts;
        }

        public static bool operator <(
            MeterReading first,
            MeterReading second)
        {
            return first.Watts < second.Watts;
        }
    }
}
