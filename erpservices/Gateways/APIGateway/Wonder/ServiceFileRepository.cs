using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Ocelot.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WonderOcelot
{
    public class ServiceFileRepository : IServiceFileRepository
    {        
        private readonly string serviceFilePath;
        private static readonly object _lock = new object();
        private const string serviceFileName = "services";

        public ServiceFileRepository(IWebHostEnvironment hosting)
        {   
            serviceFilePath = $"{hosting.ContentRootPath}\\{serviceFileName}.json";
        }

        public Task<Response<List<ServiceRegistration>>> Get()
        {
            string jsonServices;
            lock (_lock)
            {
                if (!System.IO.File.Exists(serviceFilePath))
                {
                    System.IO.File.WriteAllText(serviceFilePath, "[]");
                }
                jsonServices = System.IO.File.ReadAllText(serviceFilePath);
            }
            var services = JsonConvert.DeserializeObject<List<ServiceRegistration>>(jsonServices);
            return Task.FromResult<Response<List<ServiceRegistration>>>(new OkResponse<List<ServiceRegistration>>(services));
        }

        public Task<Response> Set(List<ServiceRegistration> services)
        {
            var jsonServices = JsonConvert.SerializeObject(services, Formatting.Indented);
            lock (_lock)
            {
                if (System.IO.File.Exists(serviceFilePath))
                {
                    System.IO.File.Delete(serviceFilePath);
                }
                System.IO.File.WriteAllText(serviceFilePath, jsonServices);
            }
            return Task.FromResult<Response>(new OkResponse());
        }
    }
}
