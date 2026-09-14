using PROG7312_POE_P1.Services;
using PROG7312_POE_P1.Models;
using Microsoft.AspNetCore.Mvc;

namespace PROG7312_POE_P1.Controllers.Api
{
    [ApiController]
    [Route("api/sensors")]
    public class SensorsController
        : ControllerBase
    {
        private readonly SensorService
            _sensorService;

        private readonly IWebHostEnvironment
            _environment;


        public SensorsController(
            SensorService sensorService,
            IWebHostEnvironment environment)
        {
            _sensorService =
                sensorService;

            _environment =
                environment;
        }


        // =================================
        // GET ALL SENSORS
        // GET /api/sensors
        // =================================

        [HttpGet]
        public IActionResult GetSensors()
        {
            return Ok(
                _sensorService.GetSensors());
        }


        // =================================
        // GET ONE SENSOR
        // =================================

        [HttpGet("{id:int}")]
        public IActionResult GetSensor(
            int id)
        {
            Sensor? sensor =
                _sensorService.GetSensor(id);


            if (sensor == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Sensor not found."
                    });
            }


            return Ok(sensor);
        }


        // =================================
        // REGISTER SENSOR
        // POST /api/sensors
        // =================================

        [HttpPost]
        public IActionResult RegisterSensor(
            [FromBody]
            SensorRegistrationRequest request)
        {
            try
            {
                Sensor sensor =
                    _sensorService
                        .RegisterSensor(request);


                return CreatedAtAction(
                    nameof(GetSensor),
                    new { id = sensor.Id },
                    sensor);
            }
            catch (
                InvalidOperationException exception)
            {
                return Conflict(
                    new
                    {
                        message =
                            exception.Message
                    });
            }
        }


        // =================================
        // FLOAT TELEMETRY
        // =================================

        [HttpPost(
            "{id:int}/telemetry/float")]
        public IActionResult AddFloatTelemetry(
            int id,
            [FromBody]
            TelemetryPacket<float> packet)
        {
            try
            {
                _sensorService.AddTelemetry(
                    id,
                    packet);


                return Ok(
                    new
                    {
                        message =
                            "Float telemetry received."
                    });
            }
            catch (
                Exception exception)
            {
                return BadRequest(
                    new
                    {
                        message =
                            exception.Message
                    });
            }
        }


        // =================================
        // INTEGER TELEMETRY
        // =================================

        [HttpPost(
            "{id:int}/telemetry/int")]
        public IActionResult AddIntegerTelemetry(
            int id,
            [FromBody]
            TelemetryPacket<int> packet)
        {
            try
            {
                _sensorService.AddTelemetry(
                    id,
                    packet);


                return Ok(
                    new
                    {
                        message =
                            "Integer telemetry received."
                    });
            }
            catch (
                Exception exception)
            {
                return BadRequest(
                    new
                    {
                        message =
                            exception.Message
                    });
            }
        }


        // =================================
        // BOOLEAN TELEMETRY
        // =================================

        [HttpPost(
            "{id:int}/telemetry/bool")]
        public IActionResult AddBooleanTelemetry(
            int id,
            [FromBody]
            TelemetryPacket<bool> packet)
        {
            try
            {
                _sensorService.AddTelemetry(
                    id,
                    packet);


                return Ok(
                    new
                    {
                        message =
                            "Boolean telemetry received."
                    });
            }
            catch (
                Exception exception)
            {
                return BadRequest(
                    new
                    {
                        message =
                            exception.Message
                    });
            }
        }


        // =================================
        // GET SENSOR TELEMETRY
        // =================================

        [HttpGet(
            "{id:int}/telemetry")]
        public IActionResult GetTelemetry(
            int id)
        {
            try
            {
                return Ok(
                    _sensorService
                        .GetTelemetry(id));
            }
            catch (
                KeyNotFoundException exception)
            {
                return NotFound(
                    new
                    {
                        message =
                            exception.Message
                    });
            }
        }


        // =================================
        // JAGGED ARRAY SIMULATION
        // =================================

        [HttpPost(
            "{id:int}/simulate-batch")]
        public IActionResult SimulateBatch(
            int id)
        {
            try
            {
                long now =
                    DateTimeOffset
                        .UtcNow
                        .ToUnixTimeSeconds();


                Random random =
                    new Random();


                // Jagged array
                //
                // Each row:
                // [Unix Timestamp, Sensor Value]

                double[][] rawBatch =
                {
                    new double[]
                    {
                        now - 35,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now - 30,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now - 25,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now - 20,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now - 15,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now - 10,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now - 5,
                        random.NextDouble() * 40
                    },

                    new double[]
                    {
                        now,
                        random.NextDouble() * 40
                    }
                };


                int processed =
                    _sensorService
                        .ProcessRawFloatBatch(
                            id,
                            rawBatch,
                            "°C");


                return Ok(
                    new
                    {
                        message =
                            $"{processed} raw telemetry " +
                            "records processed.",

                        recordsProcessed =
                            processed
                    });
            }
            catch (
                Exception exception)
            {
                return BadRequest(
                    new
                    {
                        message =
                            exception.Message
                    });
            }
        }


        // =================================
        // FILE UPLOAD
        // =================================

        [HttpPost("{id:int}/files")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult>
            UploadFile(
                int id,
                [FromForm]
                IFormFile file)
        {
            Sensor? sensor =
                _sensorService.GetSensor(id);


            if (sensor == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Sensor not found."
                    });
            }


            if (file == null ||
                file.Length == 0)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Please select a file."
                    });
            }


            if (file.Length > 5_242_880)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Maximum file size is 5 MB."
                    });
            }


            string extension =
                Path
                    .GetExtension(file.FileName)
                    .ToLowerInvariant();


            string[] allowedExtensions =
            {
                ".json",
                ".txt",
                ".log",
                ".cfg",
                ".jpg",
                ".jpeg",
                ".png",
                ".pdf"
            };


            if (!allowedExtensions.Contains(
                    extension))
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Unsupported file type."
                    });
            }


            string safeOriginalName =
                Path.GetFileName(
                    file.FileName);


            string storedName =
                $"{Guid.NewGuid()}{extension}";


            string uploadFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    id.ToString());


            Directory.CreateDirectory(
                uploadFolder);


            string fullPath =
                Path.Combine(
                    uploadFolder,
                    storedName);


            await using (
                FileStream stream =
                    new FileStream(
                        fullPath,
                        FileMode.Create))
            {
                await file.CopyToAsync(
                    stream);
            }


            string relativeUrl =
                $"/uploads/{id}/{storedName}";


            SensorAttachment attachment =
                _sensorService.AddAttachment(
                    id,
                    safeOriginalName,
                    storedName,
                    relativeUrl,
                    file.Length);


            return Ok(attachment);
        }


        // =================================
        // DASHBOARD SUMMARY
        // =================================

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            return Ok(
                _sensorService.GetSummary());
        }


        // =================================
        // LIVE ALERTS
        // =================================

        [HttpGet("alerts")]
        public IActionResult GetAlerts()
        {
            return Ok(
                _sensorService.GetAlerts());
        }
    }
}