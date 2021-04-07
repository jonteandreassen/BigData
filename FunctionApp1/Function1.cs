using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FunctionApp1.Models;

namespace FunctionApp1
{
    public static class Function1
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("Function1")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "iotHub", ConsumerGroup = "newsql")]EventData message, ILogger log)
        {
            var sqlConnection = Environment.GetEnvironmentVariable("sqlConnection");
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            var data = JsonConvert.DeserializeObject<Data>(Encoding.UTF8.GetString(message.Body.Array));
            var _deviceId = message.SystemProperties["iothub-connection-device-id"].ToString();
            var _type = message.Properties["type"].ToString();
            var _latitude = message.Properties["latitude"].ToString();
            var _longitude = message.Properties["longitude"].ToString();
            var _vendor = message.Properties["vendor"].ToString();
            var _model = message.Properties["model"].ToString();
            var _typeName = message.Properties["typeName"].ToString();
            var _deviceName = message.Properties["deviceName"].ToString();
            long measureUnixTime = data.epochTime;
            long _timeStampId;


            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();

                using (var cmd = new SqlCommand("", conn))
                {
                    //*

                    //Timestamp
                    cmd.CommandText = "IF NOT EXISTS (SELECT UnixUtcTime FROM TimeTable WHERE UnixUtcTime = @UnixUtcTime) INSERT INTO TimeTable OUTPUT inserted.UnixUtcTime VALUES (@UnixUtcTime) ELSE SELECT UnixUtcTime FROM TimeTable WHERE UnixUtcTime = @UnixUtcTime";
                    cmd.Parameters.AddWithValue("@UnixUtcTime", measureUnixTime);
                    _timeStampId = Convert.ToInt32(cmd.ExecuteScalar());

                    // DeviceVendors
                    cmd.CommandText = "IF NOT EXISTS (SELECT Id FROM DeviceVendors WHERE VendorName = @Vendor) INSERT INTO DeviceVendors OUTPUT inserted.Id VALUES(@Vendor) ELSE SELECT Id FROM DeviceVendors WHERE VendorName = @Vendor";
                    cmd.Parameters.AddWithValue("@Vendor", _vendor);
                    var vendorId = int.Parse(cmd.ExecuteScalar().ToString());

                    // DeviceModels 
                    cmd.CommandText = "IF NOT EXISTS (SELECT Id FROM DeviceModels WHERE ModelName = @ModelName)INSERT INTO DeviceModels OUTPUT inserted.Id VALUES(@ModelName, @VendorId) ELSE SELECT Id FROM DeviceModels WHERE ModelName = @ModelName";
                    cmd.Parameters.AddWithValue("@ModelName", _model);
                    cmd.Parameters.AddWithValue("@VendorId", vendorId);
                    var modelId = int.Parse(cmd.ExecuteScalar().ToString());

                    // DeviceTypes
                    cmd.CommandText = "IF NOT EXISTS (SELECT Id FROM DeviceTypes WHERE TypeName = @TypeName) INSERT INTO DeviceTypes OUTPUT inserted.Id VALUES(@TypeName) ELSE SELECT Id FROM DeviceTypes WHERE TypeName = @TypeName";
                    cmd.Parameters.AddWithValue("@TypeName", _type);
                    var deviceTypeId = int.Parse(cmd.ExecuteScalar().ToString());

                    // GeoLocations
                    cmd.CommandText = "IF NOT EXISTS (SELECT Id FROM GeoLocations WHERE Latitude = @Latitude AND Longitude = @Longitude) INSERT INTO GeoLocations OUTPUT inserted.Id VALUES(@Latitude, @Longitude) ELSE SELECT Id FROM GeoLocations WHERE Latitude = @Latitude AND Longitude = @Longitude";
                    cmd.Parameters.AddWithValue("@Latitude", _latitude);
                    cmd.Parameters.AddWithValue("@Longitude", _longitude);
                    var geoLocationId = long.Parse(cmd.ExecuteScalar().ToString());

                    // Devices 
                    cmd.CommandText = "IF NOT EXISTS (SELECT Id FROM Devices WHERE DeviceName = @DeviceName) INSERT INTO Devices OUTPUT inserted.Id VALUES(@DeviceName, @DeviceTypeId, @GeoLocationId, @ModelId) ELSE SELECT Id FROM Devices WHERE DeviceName = @DeviceName";
                    cmd.Parameters.AddWithValue("@DeviceName", _deviceName);
                    cmd.Parameters.AddWithValue("@DeviceTypeId", deviceTypeId);
                    cmd.Parameters.AddWithValue("@GeoLocationId", geoLocationId);
                    cmd.Parameters.AddWithValue("@ModelId", modelId);
                    var DeviceId = int.Parse(cmd.ExecuteScalar().ToString());


                    //Status
                    cmd.CommandText = "IF NOT EXISTS (SELECT Id FROM TemperatureAlerts WHERE Status = @Status) INSERT INTO TemperatureAlerts OUTPUT inserted.Id VALUES(@Status) ELSE SELECT Id FROM TemperatureAlerts WHERE Status = @Status";
                    cmd.Parameters.AddWithValue("@Status", data.TemperatureAlert);
                    var _temperatureAlert = int.Parse(cmd.ExecuteScalar().ToString());

                    //DhtMeasurements
                    cmd.CommandText = "INSERT INTO DhtMeasurements VALUES(@DeviceId, @MacAdress, @MeasureUnixTime, @Temperature, @Humidity, @TemperatureAlert)";
                    cmd.Parameters.AddWithValue("@Temperature", data.Temperature);
                    cmd.Parameters.AddWithValue("@Humidity", data.Humidity);
                    cmd.Parameters.AddWithValue("@TemperatureAlert", _temperatureAlert);
                    cmd.Parameters.AddWithValue("@MacAdress", _deviceId);
                    cmd.Parameters.AddWithValue("@DeviceId", DeviceId);
                    cmd.Parameters.AddWithValue("@MeasureUnixTime", _timeStampId);
                    cmd.ExecuteNonQuery();
                   
                    //*/
                }

            }

        }
    }
}
