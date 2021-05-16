using System;
namespace Ensure.Domain.Models
{
    public class ApiResponse
    {
        public ApiResponse()
        {

        }

        public ApiResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage); 
    }

}
