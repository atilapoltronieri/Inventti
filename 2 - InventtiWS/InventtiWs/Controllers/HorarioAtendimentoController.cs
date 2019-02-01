using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Services;
using System.Configuration;
using NexxeraWs.Models;
using MySql.Data.MySqlClient;


namespace NexxeraWs.Controllers
{
    [RoutePrefix("api/HorarioAtendimento")]
    public class HorarioAtendimentoController : ApiController
    {
        [HttpGet]
        [Route("GetListHorarioAtendimento")]
        public HttpResponseMessage GetListHorarioAtendimento(int usuarioClinica, int atendimento)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<HorarioAtendimentoModels> listHorarioAtendimento = new List<HorarioAtendimentoModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT HOR.ID, HOR.DIASATENDIMENTO, HOR.HORARIOINICIAL, HOR.HORARIOFINAL, " +
                        "HOR.INTERVALOCONSULTAS, HOR.PACIENTESHORARIO, HOR.ENDERECOATENDIMENTO, HOR.ATENDIMENTO, HOR.DATALIMITE, HOR.MESESLIMITE " + 
                        "FROM TB_HORARIO_ATENDIMENTO HOR " +
                        "WHERE HOR.ID_USUARIO_CLINICA = @USUARIOCLINICA AND HOR.STATUS = 1 AND HOR.ATENDIMENTO IN (@ATENDIMENTO, 3)", conn);

                    comando.Parameters.AddWithValue("@USUARIOCLINICA", usuarioClinica);
                    comando.Parameters.AddWithValue("@ATENDIMENTO", atendimento);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();
                        while (rdr.Read())

                        {
                            HorarioAtendimentoModels userHorarioAtendimento = new HorarioAtendimentoModels();
                            userHorarioAtendimento.id = Convert.ToInt32(rdr[0].ToString());
                            userHorarioAtendimento.diasAtendimento = rdr[1].ToString();
                            userHorarioAtendimento.horarioIncial = TimeSpan.Parse(rdr[2].ToString());
                            userHorarioAtendimento.horarioFinal = TimeSpan.Parse(rdr[3].ToString());
                            userHorarioAtendimento.intervaloConsultas = TimeSpan.Parse(rdr[4].ToString());
                            userHorarioAtendimento.pacientesHorario = Convert.ToInt32(rdr[5].ToString());
                            userHorarioAtendimento.enderecoAtendimento = rdr[6].ToString();
                            userHorarioAtendimento.atendimento = Convert.ToInt32(rdr[7].ToString());
                            userHorarioAtendimento.mesesLimite = Convert.ToInt32(rdr[9].ToString());

                            if (!string.IsNullOrEmpty(rdr[8].ToString()))
                                userHorarioAtendimento.dataLimite = Convert.ToDateTime(rdr[8].ToString());

                            listHorarioAtendimento.Add(userHorarioAtendimento);
                        }

                        rdr.Close();
                        
                        foreach (var horarioAtendimento in listHorarioAtendimento)
                        {
                            horarioAtendimento.dataFinalMax = listHorarioAtendimento.Max(x => x.dataFinal);
                            horarioAtendimento.datasFeriados = FeriadoController.GetFeriadosAnuais(conn, DateTime.Now.Year, horarioAtendimento.dataFinal.Year, horarioAtendimento.id_Cidade);
                            horarioAtendimento.listaAgendamentos = AgendamentoController.GetListAgendamento(conn, usuarioClinica, horarioAtendimento.id);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listHorarioAtendimento);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao listar os Horários.");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Falha ao conectar no servidor: " + e.Message);
            }
        }
    }
}