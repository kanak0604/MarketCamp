using System.Security.Cryptography.X509Certificates;

namespace MarketCampaignProject.DTOs
{
    public class ResponseDto
    {
       public bool Sucess {  get; set; }
        public string Message { get; set; }
        public Object? Data { get; set; }

        public ResponseDto(bool sucess, string message, Object? data = null) { 
            Sucess = sucess;
            Message = message;
            Data = data;
        
        }
    }
}
