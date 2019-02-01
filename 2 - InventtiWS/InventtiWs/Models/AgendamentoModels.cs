using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NexxeraWs.Models
{
    public class AgendamentoModels
    {
        public int id { get; set; }
        public int id_Usuario_Aplicativo { get; set; }
        public int id_Usuario_Clinica { get; set; }
        public int id_Horario_Atendimento { get; set; }
        public int id_Agendamento { get; set; }
        public string sobrenome { get; set; }
        public string telefone { get; set; }
        public string convenio { get; set; }
        public string tipoConsulta { get; set; }
        public DateTime data { get; set; }
        public TimeSpan hora { get; set; }
        public Int64 numeroConvenio { get; set; }
        public int status { get; set; }
        public string estado { get; set; }
        public string descricao { get; set; }
        public bool fade { get { return false; } }
        public bool agendaRetorno { get; set; }
        public AgendamentoComplementar dadosComplementares { get; set; }
    }

    public class AgendamentoComplementar
    {
        public string dataConsulta { get; set; }
        public string horaConsulta { get; set; }
        public string nomeUsuarioAplicativo { get; set; }
        public string nomeUsuarioClinica { get; set; }
        public string enderecoUsuarioClinica { get; set; }
        public string cidadeEstadUsuarioClinica { get; set; }
    }
}