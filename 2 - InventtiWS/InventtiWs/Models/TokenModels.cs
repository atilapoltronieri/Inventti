using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrConselhosWs.Models
{
    public class TokenModels
    {
        public bool Validated { get { return Errors.Count == 0; } }
        public readonly List<TokenValidationStatus> Errors = new List<TokenValidationStatus>();
    }

    public enum TokenValidationStatus
    {
        Expired,
        WrongUser,
        WrongPurpose,
        WrongGuid
    }
}