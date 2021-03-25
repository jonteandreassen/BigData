using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace azureFunction
{
    public static class AddDevice
    {
        private static string iotHub = Environment.GetEnvironmentVariable("iotHub");
        private static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHub);

        [FunctionName("AddDevice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            Device device;
            //GET method
            string mac = req.Query["deviceid"];
            //Post method
            dynamic data = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync());

            mac ??= data?.mac;

            if(mac != null)
            {
                //Validate if input is valid mac adress
                if(mac.Length == 17)
                {
                    device = await registryManager.GetDeviceAsync(mac);
                    if(device == null)
                        device = await registryManager.AddDeviceAsync(new Device(mac));

                    if(device.Id == mac)
                        return new OkObjectResult($"{iotHub.Split(";")[0]};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}");

                }
            }
            return new BadRequestObjectResult("DeviceId must be a valid mac-adress ie: xx:xx:xx:xx:xx:xx");
        }
    }
}

