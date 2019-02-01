using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NexxeraWs.Models
{
    public class UserAplicativoModels
    {
        public int id { get; set; }
        public int id_Usuario { get; set; }
        public string nome { get; set; }
        public string sobrenome { get; set; }
        public string telefone { get; set; }
        public string convenioNome { get; set; }
        public int id_Convenio { get; set; }
        public Int64 numeroConvenio { get; set; }
    }
}