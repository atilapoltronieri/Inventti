using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NexxeraWs.Models
{
    public class HorarioAtendimentoModels
    {
        public int id { get; set; }
        public int id_Usuario_Clinica { get; set; }
        public int id_Cidade { get; set; }
        public string cidadeNome { get; set; }
        public int id_Estado { get; set; }
        public string estadoNome { get; set; }
        public string diasAtendimento { get; set; }
        public TimeSpan horarioIncial { get; set; }
        public TimeSpan horarioFinal { get; set; }
        public TimeSpan intervaloConsultas { get; set; }
        public int pacientesHorario { get; set; }
        public string enderecoAtendimento { get; set; }
        public int[] diasValidos { get { return DiasValidos(); } }
        public List<string> horarios { get { return HorariosAtendimento(); } }
        public int status { get; set; }
        public int atendimento { get; set; }
        public DateTime? dataLimite { get; set; }
        public int? mesesLimite { get; set; }
        // Variável para armazenar a última data com horário disponível
        public DateTime dataFinal { get { if (mesesLimite > 0) return DateTime.Now.AddMonths((int)mesesLimite); else return dataLimite.Value.Date;  } }
        // Variável para armazenar a última data máxima dos Horários de Atendimentos. Deve-se bloquear todo agendamento até esta data.
        public DateTime dataFinalMax { get; set; }
        public List<DateTime> datasFeriados { get; set; }
        public List<AgendamentoModels> listaAgendamentos { get; set; }
        public List<DateTime> datasBloqueadas { get { return DatasAgendadas(); } }
        public List<DatasRestritas> datasRestritas { get; set; }

        internal int[] DiasValidos()
        {
            List<int> diasSemana = new List<int>(); //{0, 1, 2, 3, 4, 5, 6};
            List<string> diasAtendidos = this.diasAtendimento.Split(',').ToList();
            foreach (var dia in diasAtendidos)
            {
                if (dia == "SUN")
                    diasSemana.Add(0);
                else if (dia == "MON")
                    diasSemana.Add(1);
                else if (dia == "TUE")
                    diasSemana.Add(2);
                else if (dia == "WED")
                    diasSemana.Add(3);
                else if (dia == "THU")
                    diasSemana.Add(4);
                else if (dia == "FRI")
                    diasSemana.Add(5);
                else if (dia == "SAT")
                    diasSemana.Add(6);
            }

            return diasSemana.ToArray();
        }

        internal List<string> HorariosAtendimento()
        {
            List<string> horarios = new List<string>();
            var horarioIni = this.horarioIncial;

            while ((horarioIni + this.intervaloConsultas) <= this.horarioFinal)
            {
                horarios.Add(horarioIni.ToString());

                horarioIni += this.intervaloConsultas;
            }

            return horarios;
        }

        internal List<DateTime> DatasAgendadas()
        {
            this.datasRestritas = new List<DatasRestritas>();
            List<DateTime> datasParabloqueio = new List<DateTime>();
            DateTime dataAgendamento = new DateTime();
            List<string> horasAgendamento = new List<string>();
            List<string> horasBloqueadas = new List<string>();

            // Bloqueando todas as datas que constam como Feriado entre o dia de Hoje e o Limite das Datas Disponíveis.
            if (datasFeriados!= null)
                datasParabloqueio.AddRange(datasFeriados.Where(x => x >= DateTime.Now && x <= dataLimite));

            // Caso a Data Final deste Horário de Atendimento seja Menor que a Data Final de Maior valor dos Horáris de Atendimento
            // Deverão constar como Bloqueados todos os horários que ele possua.
            if (this.dataFinal < this.dataFinalMax)
                this.listaAgendamentos.AddRange(DatasAposFinalBloqueadas());

            // Irá verificar os dias com Horários já Marcados e dias sem Horário Disponível
            foreach (var agendamentoGroup in this.listaAgendamentos.DistinctBy(x => x.data).OrderBy(y => y.data))
            {
                dataAgendamento = agendamentoGroup.data;
                horasAgendamento = new List<string>();
                horasBloqueadas = new List<string>();

                foreach (var agendamento in this.listaAgendamentos.Where(x => x.data == agendamentoGroup.data))
                {
                    horasAgendamento.Add(agendamento.hora.ToString());
                }

                foreach (var horario in horarios)
                {
                    if (horasAgendamento.Where(x => x == horario).Count() > 0 && horasAgendamento.Where(x => x == horario).Count() <= this.pacientesHorario)
                        horasBloqueadas.Add(horario);
                }

                if (horasBloqueadas.Count() == horarios.Count())
                    datasParabloqueio.Add(agendamentoGroup.data.AddHours(3));
                else if (horasBloqueadas.Count() > 0)
                    this.datasRestritas.Add(new DatasRestritas(dataAgendamento.Date.ToShortDateString(), horasBloqueadas.ToArray()));
            }
            
            return datasParabloqueio;
        }

        internal List<AgendamentoModels> DatasAposFinalBloqueadas()
        {
            List<AgendamentoModels> listaDatasAposFinalBloqueadas = new List<AgendamentoModels>();

            for (DateTime data = this.dataFinal; data <= this.dataFinalMax; data = data.AddDays(1))
            {
                if (this.diasValidos.Contains((int)data.DayOfWeek))
                {
                    foreach (string horario in this.horarios)
                    {
                        AgendamentoModels agendamentoBloqueio = new AgendamentoModels();
                        agendamentoBloqueio.data = data;
                        agendamentoBloqueio.hora = TimeSpan.Parse(horario);
                        listaDatasAposFinalBloqueadas.Add(agendamentoBloqueio);
                    }
                }
            }

            return listaDatasAposFinalBloqueadas;
        }
    }

    public class DatasRestritas
    {
        public string data { get; set; }
        public string[] horas { get; set; }

        public DatasRestritas(string _data, string[] _horas)
        {
            this.data = _data;
            this.horas = _horas;
        }
    }
}