using PROG7312_POE_P1.Models;

namespace PROG7312_POE_P1.Services
{
    public class SensorService
    {
        private readonly List<Sensor> _sensors =
            new List<Sensor>();

        private readonly List<TelemetryPacket<float>> _floatTelemetry =
            new List<TelemetryPacket<float>>();

        private readonly List<TelemetryPacket<int>> _integerTelemetry =
            new List<TelemetryPacket<int>>();

        private readonly List<TelemetryPacket<bool>> _booleanTelemetry =
            new List<TelemetryPacket<bool>>();

        private readonly List<TelemetryAlert> _alerts =
            new List<TelemetryAlert>();


        private int _nextSensorId = 1;

        private int _nextAttachmentId = 1;


        // =========================================
        // SENSOR MANAGEMENT
        // =========================================

        public List<Sensor> GetSensors()
        {
            return _sensors
                .OrderByDescending(
                    sensor => sensor.RegisteredAt)
                .ToList();
        }


        public Sensor? GetSensor(int id)
        {
            return _sensors
                .FirstOrDefault(
                    sensor => sensor.Id == id);
        }


        public Sensor? GetSensorByDeviceId(
            string deviceId)
        {
            return _sensors
                .FirstOrDefault(
                    sensor =>
                        sensor.DeviceId.Equals(
                            deviceId,
                            StringComparison.OrdinalIgnoreCase));
        }


        public Sensor RegisterSensor(
            SensorRegistrationRequest request)
        {
            bool exists =
                _sensors.Any(
                    sensor =>
                        sensor.DeviceId.Equals(
                            request.DeviceId,
                            StringComparison.OrdinalIgnoreCase));


            if (exists)
            {
                throw new InvalidOperationException(
                    "A sensor with this Device ID already exists.");
            }


            Sensor sensor =
                new Sensor
                {
                    Id =
                        _nextSensorId++,

                    DeviceId =
                        request.DeviceId.Trim(),

                    Location =
                        request.Location.Trim(),

                    Category =
                        request.Category,

                    RegisteredAt =
                        DateTime.UtcNow
                };


            _sensors.Add(sensor);


            return sensor;
        }


        // =========================================
        // FLOAT TELEMETRY
        // =========================================

        public void AddTelemetry(
            int sensorId,
            TelemetryPacket<float> packet)
        {
            Sensor sensor =
                GetRequiredSensor(sensorId);


            ValidateDevice(
                sensor,
                packet.DeviceId);


            _floatTelemetry.Add(packet);

            // Added to AddTelemetry(int sensorId, TelemetryPacket<float> packet)
            if (packet.Unit.Equals("pH", StringComparison.OrdinalIgnoreCase))
            {
                if (packet.Value < 5.5f || packet.Value > 6.5f)
                {
                    AddAlertInternal(sensor, "Critical", $"pH level out of range: {packet.Value} {packet.Unit} (Target: 5.5 - 6.5)");
                }
            }
            else if (packet.Unit.Equals("°C", StringComparison.OrdinalIgnoreCase) && sensor.Category == SensorCategory.WaterQuality)
            {
                if (packet.Value > 24.0f)
                {
                    AddAlertInternal(sensor, "Warning", $"Water temperature too high: {packet.Value}°C (Risk of root rot)");
                }
            }


            if (packet.Value > 7)
            {
                AddAlertInternal(
                    sensor,
                    "Critical",
                    $"High environmental reading: " +
                    $"{packet.Value} {packet.Unit}");
            }
            else if (packet.Value < 5)
            {
                AddAlertInternal(
                    sensor,
                    "Warning",
                    $"Low environmental reading: " +
                    $"{packet.Value} {packet.Unit}");
            }
        }


        // =========================================
        // INTEGER TELEMETRY
        // =========================================

        public void AddTelemetry(
            int sensorId,
            TelemetryPacket<int> packet)
        {
            Sensor sensor =
                GetRequiredSensor(sensorId);


            ValidateDevice(
                sensor,
                packet.DeviceId);


            _integerTelemetry.Add(packet);

            // Added to AddTelemetry(int sensorId, TelemetryPacket<int> packet)
            if (packet.Unit.Equals("mS/cm", StringComparison.OrdinalIgnoreCase) && packet.Value > 2500)
            {
                AddAlertInternal(sensor, "Warning", $"EC level high: {packet.Value} µS/cm");
            }

            if (packet.Value > 1500)
            {
                AddAlertInternal(
                    sensor,
                    "Critical",
                    $"High power consumption detected: " +
                    $"{packet.Value} {packet.Unit}");
            }
        }


        // =========================================
        // BOOLEAN TELEMETRY
        // =========================================

        public void AddTelemetry(
            int sensorId,
            TelemetryPacket<bool> packet)
        {
            Sensor sensor =
                GetRequiredSensor(sensorId);


            ValidateDevice(
                sensor,
                packet.DeviceId);


            _booleanTelemetry.Add(packet);


            if (packet.Value)
            {
                AddAlertInternal(
                    sensor,
                    "Info",
                    "Actuator state changed to ON.");
            }
        }


        // =========================================
        // GET TELEMETRY
        // =========================================

        public SensorTelemetrySnapshot GetTelemetry(
            int sensorId)
        {
            Sensor sensor =
                GetRequiredSensor(sensorId);


            return new SensorTelemetrySnapshot
            {
                FloatReadings =
                    _floatTelemetry
                        .Where(
                            reading =>
                                reading.DeviceId.Equals(
                                    sensor.DeviceId,
                                    StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(
                            reading => reading.Timestamp)
                        .ToList(),


                IntegerReadings =
                    _integerTelemetry
                        .Where(
                            reading =>
                                reading.DeviceId.Equals(
                                    sensor.DeviceId,
                                    StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(
                            reading => reading.Timestamp)
                        .ToList(),


                BooleanReadings =
                    _booleanTelemetry
                        .Where(
                            reading =>
                                reading.DeviceId.Equals(
                                    sensor.DeviceId,
                                    StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(
                            reading => reading.Timestamp)
                        .ToList()
            };
        }


        // =========================================
        // JAGGED ARRAY
        // =========================================

        public int ProcessRawFloatBatch(
            int sensorId,
            double[][] rawBatch,
            string unit)
        {
            Sensor sensor =
                GetRequiredSensor(sensorId);


            List<TelemetryPacket<float>> converted =
                new List<TelemetryPacket<float>>();


            foreach (double[] row in rawBatch)
            {
                if (row.Length < 2)
                {
                    continue;
                }


                long timestamp =
                    (long)row[0];


                float value =
                    (float)row[1];


                TelemetryPacket<float> packet =
                    new TelemetryPacket<float>
                    {
                        DeviceId =
                            sensor.DeviceId,

                        Timestamp =
                            DateTimeOffset
                                .FromUnixTimeSeconds(timestamp)
                                .UtcDateTime,

                        Value =
                            value,

                        Unit =
                            unit
                    };


                converted.Add(packet);
            }


            _floatTelemetry.AddRange(converted);


            return converted.Count;
        }


        // =========================================
        // ATTACHMENTS
        // =========================================

        public SensorAttachment AddAttachment(
            int sensorId,
            string originalName,
            string storedName,
            string relativeUrl,
            long size)
        {
            Sensor sensor =
                GetRequiredSensor(sensorId);


            SensorAttachment attachment =
                new SensorAttachment
                {
                    Id =
                        _nextAttachmentId++,

                    OriginalFileName =
                        originalName,

                    StoredFileName =
                        storedName,

                    RelativeUrl =
                        relativeUrl,

                    SizeBytes =
                        size,

                    UploadedAt =
                        DateTime.UtcNow
                };


            sensor.Attachments.Add(attachment);


            return attachment;
        }


        // =========================================
        // ALERTS
        // =========================================

        public List<TelemetryAlert> GetAlerts()
        {
            return _alerts
                .OrderByDescending(
                    alert => alert.Timestamp)
                .Take(20)
                .ToList();
        }


        private void AddAlertInternal(
            Sensor sensor,
            string severity,
            string message)
        {
            _alerts.Add(
                new TelemetryAlert
                {
                    SensorId =
                        sensor.Id,

                    DeviceId =
                        sensor.DeviceId,

                    Severity =
                        severity,

                    Message =
                        message,

                    Timestamp =
                        DateTime.UtcNow
                });
        }


        // =========================================
        // OPERATOR OVERLOADING
        // =========================================

        private MeterReading CalculateCombinedPower()
        {
            HashSet<string> powerSensors =
                _sensors
                    .Where(
                        sensor =>
                            sensor.Category ==
                            SensorCategory.Actuator)
                    .Select(
                        sensor => sensor.DeviceId)
                    .ToHashSet(
                        StringComparer.OrdinalIgnoreCase);


            List<TelemetryPacket<int>> latestReadings =
                _integerTelemetry
                    .Where(
                        reading =>
                            powerSensors.Contains(
                                reading.DeviceId))
                    .GroupBy(
                        reading =>
                            reading.DeviceId)
                    .Select(
                        group =>
                            group
                                .OrderByDescending(
                                    reading => reading.Timestamp)
                                .First())
                    .ToList();


            MeterReading total =
                new MeterReading(0);


            foreach (
                TelemetryPacket<int> reading
                in latestReadings)
            {
                MeterReading meter =
                    new MeterReading(
                        reading.Value);


                total =
                    total + meter;
            }


            return total;
        }


        // =========================================
        // SUMMARY
        // =========================================

        public GatewaySummary GetSummary()
        {
            MeterReading total =
                CalculateCombinedPower();


            MeterReading highThreshold =
                new MeterReading(3000);


            return new GatewaySummary
            {
                SensorCount =
                    _sensors.Count,

                FloatReadingCount =
                    _floatTelemetry.Count,

                IntegerReadingCount =
                    _integerTelemetry.Count,

                BooleanReadingCount =
                    _booleanTelemetry.Count,

                CombinedPowerWatts =
                    total.Watts,

                HighPowerLoad =
                    total > highThreshold
            };
        }


        // =========================================
        // HELPERS
        // =========================================

        private Sensor GetRequiredSensor(
            int sensorId)
        {
            Sensor? sensor =
                _sensors.FirstOrDefault(
                    sensor =>
                        sensor.Id == sensorId);


            if (sensor == null)
            {
                throw new KeyNotFoundException(
                    "Sensor was not found.");
            }


            return sensor;
        }


        private static void ValidateDevice(
            Sensor sensor,
            string deviceId)
        {
            if (!sensor.DeviceId.Equals(
                    deviceId,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Telemetry Device ID does not match " +
                    "the selected sensor.");
            }
        }
    }
}