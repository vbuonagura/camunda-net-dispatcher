using Newtonsoft.Json;

namespace SampleWebApi.Models
{
    public class ErrorDetails
    {
        public int RequestStatus { get; set; }
        public string Message { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
