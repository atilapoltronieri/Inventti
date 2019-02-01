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
    [RoutePrefix("api/Agendamento")]
    public class AgendamentoController : ApiController
    {
        [HttpGet]
        [Route("GetListAgendamento")]
        public HttpResponseMessage GetListAgendamento(string usuario)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<AgendamentoModels> listAgendamento = new List<AgendamentoModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT AGD.ID, AGD.ID_USUARIO_APLICATIVO, AGD.DATA, AGD.HORA," +
                        "AGD.STATUS, AGD.ESTADO, AGD.DESCRICAO, " +
                        "CONCAT(APL.NOME, ' ', APL.SOBRENOME), CLI.NOME, " +
                        "CONCAT(CLI.ENDERECO, ', Número ', CLI.NUMERO), " +
                        "CONCAT(CID.NOME, '/', (SELECT EST.SIGLA FROM TB_ESTADO EST WHERE EST.ID = CID.ID_ESTADO)), " +
                        "AGD.ID_USUARIO_CLINICA " +
                        "FROM TB_AGENDAMENTO AGD " +
                        "JOIN TB_USUARIO_CLINICA CLI ON CLI.ID = AGD.ID_USUARIO_CLINICA " +
                        "JOIN TB_USUARIO_APLICATIVO APL ON APL.ID = AGD.ID_USUARIO_APLICATIVO " +
                        "JOIN TB_USUARIO USU ON USU.ID = APL.ID_USUARIO " +
                        "JOIN TB_HORARIO_ATENDIMENTO HOR ON HOR.ID = AGD.ID_HORARIO_ATENDIMENTO " +
                        "JOIN TB_CIDADE CID " +
                        "WHERE CID.ID = HOR.ID_CIDADE " +
                        "AND USU.EMAIL = 'abpoltronieri@hotmail.com' " +
                        "ORDER BY AGD.DATA, AGD.HORA", conn);

                    comando.Parameters.AddWithValue("@USUARIO", usuario);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            AgendamentoModels userAgendamento = new AgendamentoModels();
                            userAgendamento.dadosComplementares = new AgendamentoComplementar();
                            userAgendamento.id = Convert.ToInt32(rdr[0].ToString());
                            userAgendamento.id_Usuario_Aplicativo = Convert.ToInt32(rdr[1].ToString());
                            userAgendamento.data = Convert.ToDateTime(rdr[2].ToString());
                            userAgendamento.dadosComplementares.dataConsulta = userAgendamento.data.ToShortDateString();
                            userAgendamento.hora = TimeSpan.Parse(rdr[3].ToString());
                            userAgendamento.dadosComplementares.horaConsulta = userAgendamento.hora.ToString().Substring(0, 5);
                            userAgendamento.status = Convert.ToInt32(rdr[4].ToString());
                            userAgendamento.estado = rdr[5].ToString();
                            userAgendamento.descricao = rdr[6].ToString();
                            userAgendamento.dadosComplementares.nomeUsuarioAplicativo = rdr[7].ToString();
                            userAgendamento.dadosComplementares.nomeUsuarioClinica = rdr[8].ToString();
                            userAgendamento.dadosComplementares.enderecoUsuarioClinica = rdr[9].ToString();
                            userAgendamento.dadosComplementares.cidadeEstadUsuarioClinica = rdr[10].ToString();
                            userAgendamento.id_Usuario_Clinica = Convert.ToInt32(rdr[11].ToString());

                            listAgendamento.Add(userAgendamento);
                        }

                        foreach (var agendamento in listAgendamento)
                        {
                            if (listAgendamento.Where(x => x.id_Agendamento == agendamento.id).Count() > 0)
                                agendamento.agendaRetorno = false;
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listAgendamento);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao listar os Agendamentos.");
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

        [HttpPost]
        [Route("AddAgendamento")]
        public HttpResponseMessage AddAgendamento(int id_Usuario_Aplicativo, string data, TimeSpan hora, int id_Usuario_Clinica, int id_Horario_Atendimento, int? id_Agendamento)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<AgendamentoModels> listAgendamento = new List<AgendamentoModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {

                    MySqlCommand comandoVerificaAgendamentoUsuario = new MySqlCommand("SELECT AGD.ID, AGD.DATA, AGD.HORA FROM TB_AGENDAMENTO AGD " +
                        "WHERE AGD.ID_USUARIO_APLICATIVO = @USUARIOAPLICATIVO AND AGD.STATUS = 1 AND AGD.ID_HORARIO_ATENDIMENTO = @HORARIOATENDIMENTO " +
                        "AND AGD.DATA >= '" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "'", conn);

                    comandoVerificaAgendamentoUsuario.Parameters.AddWithValue("@USUARIOAPLICATIVO", id_Usuario_Aplicativo);
                    comandoVerificaAgendamentoUsuario.Parameters.AddWithValue("@HORARIOATENDIMENTO", id_Horario_Atendimento);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comandoVerificaAgendamentoUsuario.ExecuteReader();

                        if (rdr.Read())
                        {
                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Usuário já possui uma Consulta Agendada para este Médico: " + Convert.ToDateTime(rdr[1].ToString()).ToShortDateString() + " às " + rdr[2].ToString());
                        }

                        rdr.Close();
                    }
                    catch (Exception ex) { }

                    if (!VerificaAgendamentoLotado(conn, id_Usuario_Clinica, id_Horario_Atendimento, Convert.ToDateTime(data), hora))
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Desculpe, mas já não há mais vagas para este horário.");

                    MySqlCommand comando = new MySqlCommand("INSERT INTO TB_AGENDAMENTO (ID_USUARIO_APLICATIVO, DATA, HORA, " +
                        "STATUS, DESCRICAO, ID_USUARIO_CLINICA, ID_HORARIO_ATENDIMENTO, ID_AGENDAMENTO) VALUES (@USUARIOAPLICATIVO, " +
                        "@DATA, @HORA, @STATUS, @DESCRICAO, @USUARIOCLINICA, @HORARIOATENDIMENTO, @AGENDAMENTO)", conn);

                    comando.Parameters.AddWithValue("@USUARIOAPLICATIVO", id_Usuario_Aplicativo);
                    comando.Parameters.AddWithValue("@DATA", Convert.ToDateTime(data).Date);
                    comando.Parameters.AddWithValue("@HORA", hora);
                    comando.Parameters.AddWithValue("@STATUS", 1);
                    comando.Parameters.AddWithValue("@DESCRICAO", string.Empty);
                    comando.Parameters.AddWithValue("@USUARIOCLINICA", id_Usuario_Clinica);
                    comando.Parameters.AddWithValue("@HORARIOATENDIMENTO", id_Horario_Atendimento);
                    comando.Parameters.AddWithValue("@AGENDAMENTO", id_Agendamento);

                    try
                    {
                        if (conn.State == System.Data.ConnectionState.Closed)
                            conn.Open();

                        var exec = comando.ExecuteNonQuery();

                        if (exec == 0)
                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao inserir o Agendamento.");

                        return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao inserir o Agendamento.");
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

        public static List<AgendamentoModels> GetListAgendamento(MySqlConnection conn, int usuarioClinica, int horarioAtendimento)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<AgendamentoModels> listAgendamento = new List<AgendamentoModels>();

            try
            {
                MySqlCommand comando = new MySqlCommand("SELECT AGD.ID, AGD.ID_USUARIO_APLICATIVO, AGD.DATA, AGD.HORA, " +
                    "AGD.STATUS, AGD.ESTADO, AGD.DESCRICAO FROM TB_AGENDAMENTO AGD " +
                    "WHERE AGD.ID_USUARIO_CLINICA = @USUARIOCLINICA AND AGD.STATUS = 1", conn);
                if (horarioAtendimento > 0)
                    comando.CommandText += " AND AGD.ID_HORARIO_ATENDIMENTO = @HORARIOATENDIMENTO";

                comando.Parameters.AddWithValue("@USUARIOCLINICA", usuarioClinica);
                comando.Parameters.AddWithValue("@HORARIOATENDIMENTO", horarioAtendimento);

                try
                {
                    conn.Open();
                    MySqlDataReader rdr = comando.ExecuteReader();

                    while (rdr.Read())
                    {
                        AgendamentoModels userAgendamento = new AgendamentoModels();
                        userAgendamento.id = Convert.ToInt32(rdr[0].ToString());
                        userAgendamento.id_Usuario_Aplicativo = Convert.ToInt32(rdr[1].ToString());
                        userAgendamento.data = Convert.ToDateTime(rdr[2].ToString());
                        userAgendamento.hora = TimeSpan.Parse(rdr[3].ToString());
                        userAgendamento.status = Convert.ToInt32(rdr[4].ToString());
                        userAgendamento.estado = rdr[5].ToString();
                        userAgendamento.descricao = rdr[6].ToString();

                        listAgendamento.Add(userAgendamento);
                    }

                    rdr.Close();
                    return listAgendamento;
                }
                catch (Exception e)
                {
                    return new List<AgendamentoModels>();
                }
                finally
                {
                }
            }
            catch (Exception e)
            {
                return new List<AgendamentoModels>();
            }
        }

        public static bool VerificaAgendamentoLotado(MySqlConnection conn, int usuarioClinica, int horarioAtendimento, DateTime data, TimeSpan hora)
        {
            try
            {
                int pacientesHorario = 0;

                MySqlCommand comandoHorarioAtendimento = new MySqlCommand("SELECT HOR.PACIENTESHORARIO FROM TB_HORARIO_ATENDIMENTO HOR " +
                    "WHERE HOR.ID = @HORARIOATENDIMENTO", conn);

                comandoHorarioAtendimento.Parameters.AddWithValue("@HORARIOATENDIMENTO", horarioAtendimento);

                try
                {
                    MySqlDataReader rdr = comandoHorarioAtendimento.ExecuteReader();

                    while (rdr.Read())
                    {
                        pacientesHorario = Convert.ToInt32(rdr[0].ToString());
                    }

                    rdr.Close();

                }
                catch (Exception e)
                {
                }

                MySqlCommand comandoAgendamento = new MySqlCommand("SELECT AGD.ID, AGD.ID_USUARIO_APLICATIVO, AGD.DATA, AGD.HORA, " +
                    "AGD.STATUS, AGD.ESTADO, AGD.DESCRICAO FROM TB_AGENDAMENTO AGD " +
                    "WHERE AGD.ID_USUARIO_CLINICA = @USUARIOCLINICA AND AGD.STATUS = 1  AND AGD.ID_HORARIO_ATENDIMENTO = @HORARIOATENDIMENTO " +
                    "AND AGD.DATA = @DATA AND AGD.HORA = @HORA", conn);

                comandoAgendamento.Parameters.AddWithValue("@USUARIOCLINICA", usuarioClinica);
                comandoAgendamento.Parameters.AddWithValue("@HORARIOATENDIMENTO", horarioAtendimento);
                comandoAgendamento.Parameters.AddWithValue("@DATA", data.Date);
                comandoAgendamento.Parameters.AddWithValue("@HORA", hora.ToString());

                try
                {
                    MySqlDataReader rdr = comandoAgendamento.ExecuteReader();
                    int contadorAgendamento = 0;

                    while (rdr.Read())
                    {
                        contadorAgendamento++;

                        if (contadorAgendamento >= pacientesHorario)
                        {
                            rdr.Close();
                            return false;
                        }
                    }

                    rdr.Close();
                }
                catch (Exception e)
                {
                }
            }
            catch (Exception e)
            {
            }

            return true;
        }
    }
}