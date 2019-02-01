using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NexxeraWs.Models
{
    public class FeriadoModels
    {
        public int Id { get; set; }
        public int Id_Cidade { get; set; }
        public int Dia { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }
        public string Nome { get; set; }
        public bool Nacional { get; set; }

        public DateTime Data { get { return new DateTime(Ano, Mes, Dia); } }
    }
}