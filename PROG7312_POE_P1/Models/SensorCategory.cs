namespace PROG7312_POE_P1.Models
{
    public enum SensorCategory
    {
        WaterQuality,     // pH, EC, Dissolved Oxygen, Water Temp
        Nutrients,        // Dosing volume, Tank Levels
        Environmental,    // Ambient Temp, Humidity, VPD, CO2
        Actuator          // Dosing pumps, Aeration, Solenoids, Grow lights
    }

    public enum SubsystemZone
    {
        Reservoir,
        GrowingChannel,
        DosingStation,
        AmbientRoom
    }
}
