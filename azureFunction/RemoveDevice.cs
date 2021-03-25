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
    public static class RemoveDevice
    {
        private static string iotHub = Environment.GetEnvironmentVariable("iotHub");
        private static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHub);



        [FunctionName("RemoveDevice")]
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
                device = await registryManager.GetDeviceAsync(mac);

                if(device != null)
                {
                    await registryManager.RemoveDeviceAsync(device);
                    return new OkObjectResult($"Device with deviceId {mac} has been removed"); 
                }
            }

            return new NotFoundObjectResult($"Device with deviceId {mac} does not exist");
        }
    }
}

