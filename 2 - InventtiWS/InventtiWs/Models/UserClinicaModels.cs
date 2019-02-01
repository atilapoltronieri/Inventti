using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NexxeraWs.Models
{
    public class UserClinicaModels
    {
        public int id { get; set; }
        public int id_Usuario { get; set; }
        public string crm { get; set; }
        public string nome { get; set; }
        public int id_Especializacao { get; set; }
        public string especializacaoNome { get; set; }
        public string descricao { get; set; }
        public string estadoNome { get; set; }
        public string cidadeNome { get; set; }
        public string endereco { get; set; }
        public string numero { get; set; }
        public string complemento { get; set; }
        public List<string> titulosList { get; set; }
        public bool fade { get { return true; } }
    }
}