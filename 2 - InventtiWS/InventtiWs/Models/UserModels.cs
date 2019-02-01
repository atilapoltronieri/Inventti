using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrConselhosWs.Models
{
    public class UserModels
    {
        public int id { get; set; }
        public string email { get; set; }
        public string senha { get; set; }
        public bool bloqueio { get; set; }
        public string SecurityStamp { get { return SecurityStamp; } set { SecurityStamp = setSecurityStamp(); } }

        protected string setSecurityStamp()
        {
            return Guid.NewGuid().ToString("D");
        }

    }
}