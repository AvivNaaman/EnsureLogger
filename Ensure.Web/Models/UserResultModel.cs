using System;
using System.Collections.Generic;

namespace Ensure.Web.Models
{
    public class UserResultModel
    {
        public List<string> Errors { get; set; } = new();

        public bool Succeeded => Errors is null || Errors.Count is 0;
    }
}
