using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrConselhosWs.Models
{
    public class ContribuinteModels
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string cpf { get; set; }
        public int numeroDependentes { get; set; }
        public decimal rendaBruta { get; set; }
        public decimal rendaLiquida { get; set; }
        public decimal ir { get; set; }
    }
}