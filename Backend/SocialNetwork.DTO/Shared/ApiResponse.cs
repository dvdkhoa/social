using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DTO.Shared
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            
        }

        public ApiResponse(bool isSuccess, string message, string data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
