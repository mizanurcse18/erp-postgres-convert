using Microsoft.AspNetCore.Http;
namespace API.Core
{
    public class ImageModel
    {
        public int KeyValue { get; set; }
        public string Name { get; set; }
        public IFormFile ImageFile { get; set; }
        public bool Remove { get; set; }
    }
}
