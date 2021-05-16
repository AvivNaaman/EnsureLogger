using System;
namespace Ensure.Domain.Models
{
    public class ApiResponse<TResponse> : ApiResponse
    {
        public ApiResponse()
        {

        }

        public ApiResponse(string errorMessage) : base(errorMessage)
        {
        }

        public ApiResponse(TResponse response)
        {
            Response = response;
        }

        public ApiResponse(TResponse response, string errorMessage) : base(errorMessage)
        {
            Response = response;
        }

        public TResponse Response { get; set; }
    }
}
