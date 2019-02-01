using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Services;
using NexxeraWs.Models;
using NexxeraWs.Rules;

namespace NexxeraWs.Controllers
{
    [RoutePrefix("api/UserClinica")]
    public class UserClinicaController : ApiController
    {
        [WebMethod]
        [HttpPost]
        [Route("AddUserClinica")]
        public HttpResponseMessage AddUserClinica(string email, string crm, string nome, int id_especializacao, string descricao, string estado, string cidade, string endereco, string numero, string complemento, string[] titulos)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string usarioClicaCadastrado = UserClinicaRules.VerificaUsuarioClinicaCadastro(crm, conn);
                if (!string.IsNullOrEmpty(usarioClicaCadastrado))
                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, usarioClicaCadastrado);

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT ID FROM TB_USUARIO WHERE EMAIL=@EMAIL LIMIT 1", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while (rdr.Read())
                    {
                        string idUser = rdr[0].ToString();

                        MySqlCommand comando = new MySqlCommand("INSERT INTO TB_USUARIO_CLINICA(ID_USUARIO, CRM, NOME, ID_ESPECIALIZACAO, DESCRICAO, ESTADO, CIDADE, ENDERECO, NUMERO, COMPLEMENTO) " +
                            "VALUES(@ID_USUARIO, @CRM, @NOME, @ID_ESPECIALIZACAO, @DESCRICAO, @ESTADO, @CIDADE, @ENDERECO, @NUMERO, @COMPLEMENTO); ", conn);
                        comando.Parameters.AddWithValue("@ID_USUARIO", rdr[0].ToString());
                        comando.Parameters.AddWithValue("@CRM", crm);
                        comando.Parameters.AddWithValue("@NOME", nome);
                        comando.Parameters.AddWithValue("@ID_ESPECIALIZACAO", id_especializacao);
                        comando.Parameters.AddWithValue("@DESCRICAO", descricao);
                        comando.Parameters.AddWithValue("@ESTADO", estado);
                        comando.Parameters.AddWithValue("@CIDADE", cidade);
                        comando.Parameters.AddWithValue("@ENDERECO", endereco);
                        comando.Parameters.AddWithValue("@NUMERO", numero);
                        comando.Parameters.AddWithValue("@COMPLEMENTO", complemento);

                        rdr.Close();

                        var exec = comando.ExecuteNonQuery();

                        if (exec == 1)
                        {

                            PermissaoUsuarioClinica.DeletaCRM(crm, conn);

                            if (titulos != null && titulos.Count() > 0)
                            {
                                var queryInsertTitulos = string.Empty;
                                MySqlCommand comandoInsertTitulos = new MySqlCommand();
                                comandoInsertTitulos.Connection = conn;

                                try
                                {
                                    for (int i = 0; i < titulos.Count(); i++)
                                    {
                                        queryInsertTitulos += "INSERT INTO TB_USUARIO_CLINICA_TITULO(ID_USUARIO_CLINICA, TITULO) VALUES ((SELECT ID FROM TB_USUARIO_CLINICA WHERE ID_USUARIO = @ID_USUARIO), @TITULO" + i + ");";
                                        comandoInsertTitulos.Parameters.AddWithValue("@TITULO" + i, titulos[i].ToString());
                                    }

                                    comandoInsertTitulos.Parameters.AddWithValue("@ID_USUARIO", idUser);
                                    comandoInsertTitulos.CommandText = queryInsertTitulos;

                                    var execInsertTituls = comandoInsertTitulos.ExecuteNonQuery();

                                    if (execInsertTituls == 0)
                                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Cadastrar os Títulos");
                                }
                                finally
                                {
                                }
                            }

                            return Request.CreateResponse(HttpStatusCode.OK, "Usuário Cadastrado com Sucesso");
                        }
                        else
                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Cadastrar Seu Usuário. Por Favor Verifique seus Dados");
                    }

                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Usuário Não Cadastrado");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [WebMethod]
        [HttpPost]
        [Route("AlterUserClinica")]
        public HttpResponseMessage AlterUserClinica(string email, string crm, string nome, int id_especializacao, string descricao, string estado, string cidade, string endereco, string numero, string complemento, string[] titulos)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string idUser = string.Empty;

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT ID FROM TB_USUARIO WHERE EMAIL=@EMAIL LIMIT 1", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while(rdr.Read())
                    {
                        idUser = rdr[0].ToString();
                    }

                    rdr.Close();

                    if (string.IsNullOrEmpty(idUser))
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Usuário Não Cadastrado");
                }
                catch(Exception e)
                {
                    throw e;
                }

                try
                {
                    MySqlCommand comandoAlterUser = new MySqlCommand("UPDATE TB_USUARIO_CLINICA SET NOME=@NOME, ESPECIALIDADE=@ID_ESPECIALIZACAO, DESCRICAO=@DESCRICAO, ESTADO=@ESTADO, CIDADE=@CIDADE, ENDERECO=@ENDERECO, " +
                        "NUMERO=@NUMERO, COMPLEMENTO=@COMPLEMENTO WHERE ID_USUARIO=@ID_USUARIO AND CRM=@CRM; " + 
                        "DELETE FROM TB_USUARIO_CLINICA_TITULO WHERE ID_USUARIO_CLINICA = (SELECT ID FROM TB_USUARIO_CLINICA WHERE ID_USUARIO = @ID_USUARIO);", conn);
                    comandoAlterUser.Parameters.AddWithValue("@ID_USUARIO", idUser);
                    comandoAlterUser.Parameters.AddWithValue("@CRM", crm);
                    comandoAlterUser.Parameters.AddWithValue("@NOME", nome);
                    comandoAlterUser.Parameters.AddWithValue("@ID_ESPECIALIZACAO", id_especializacao);
                    comandoAlterUser.Parameters.AddWithValue("@DESCRICAO", descricao);
                    comandoAlterUser.Parameters.AddWithValue("@ESTADO", estado);
                    comandoAlterUser.Parameters.AddWithValue("@CIDADE", cidade);
                    comandoAlterUser.Parameters.AddWithValue("@ENDERECO", endereco);
                    comandoAlterUser.Parameters.AddWithValue("@NUMERO", numero);
                    comandoAlterUser.Parameters.AddWithValue("@COMPLEMENTO", complemento);

                    var exec = comandoAlterUser.ExecuteNonQuery();

                    if (exec >= 1)
                    {
                        if (titulos != null && titulos.Count() > 0)
                        {
                            var queryInsertTitulos = string.Empty;
                            MySqlCommand comandoInsertTitulos = new MySqlCommand();
                            comandoInsertTitulos.Connection = conn;

                            try
                            {
                                for (int i = 0; i < titulos.Count(); i++)
                                {
                                    queryInsertTitulos += "INSERT INTO TB_USUARIO_CLINICA_TITULO(ID_USUARIO_CLINICA, TITULO) VALUES ((SELECT ID FROM TB_USUARIO_CLINICA WHERE ID_USUARIO = @ID_USUARIO), @TITULO" + i + ");";
                                    comandoInsertTitulos.Parameters.AddWithValue("@TITULO" + i, titulos[i].ToString());
                                }

                                comandoInsertTitulos.Parameters.AddWithValue("@ID_USUARIO", idUser);
                                comandoInsertTitulos.CommandText = queryInsertTitulos;

                                var execInsertTituls = comandoInsertTitulos.ExecuteNonQuery();

                                if (execInsertTituls == 0)
                                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Cadastrar os Títulos");
                            }
                            finally
                            {
                            }
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, "Usuário Alterado com Sucesso");
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Alterar Seu Usuário. Por Favor Verifique seus Dados");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpGet]
        [Route("GetListUserClinica")]
        public HttpResponseMessage GetListUserClinica(int estado, int especializacao, int cidade, int atendimento)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<UserClinicaModels> listUserClinica = new List<UserClinicaModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string sqlAtendimento = string.Empty;

                    MySqlCommand comando = new MySqlCommand("SELECT DISTINCT(USUC.ID), USUC.NOME, USUC.CRM, USUC.DESCRICAO, USUC.ENDERECO, USUC.NUMERO, USUC.COMPLEMENTO, " +
                        "CID.NOME AS CIDADE, (SELECT EST.NOME FROM TB_ESTADO EST WHERE EST.ID = CID.ID_ESTADO) AS ESTADO, " +
                        "(SELECT ESP.NOME FROM TB_ESPECIALIZACAO ESP WHERE ESP.ID = USUC.ID_ESPECIALIZACAO) AS ESPECIALIZACAO " + 
                        "FROM TB_USUARIO_CLINICA USUC " +
                        "JOIN TB_HORARIO_ATENDIMENTO HOR ON HOR.ID_USUARIO_CLINICA = USUC.ID " +
                        "JOIN TB_CIDADE CID ON HOR.ID_CIDADE = CID.ID " +
                        "WHERE CID.ID_ESTADO = @ID_ESTADO " + 
                        "AND USUC.ID_ESPECIALIZACAO = @ID_ESPECIALIZACAO ", conn);

                    if (cidade > 0)
                        comando.CommandText += "AND HOR.ID_CIDADE = @ID_CIDADE";

                    if (atendimento != 3)
                        comando.CommandText += "AND HOR.ATENDIMENTO IN(@ATENDIMENTO, 3)";

                    comando.Parameters.AddWithValue("@ID_ESPECIALIZACAO", especializacao);
                    comando.Parameters.AddWithValue("@ID_ESTADO", estado);
                    comando.Parameters.AddWithValue("@ID_CIDADE", cidade);
                    comando.Parameters.AddWithValue("@ATENDIMENTO", atendimento);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            UserClinicaModels userUserClinica = new UserClinicaModels();
                            userUserClinica.id = Convert.ToInt32(rdr[0].ToString());
                            userUserClinica.nome = rdr[1].ToString();
                            userUserClinica.crm = rdr[2].ToString();
                            userUserClinica.descricao = rdr[3].ToString();
                            userUserClinica.endereco = rdr[4].ToString();
                            userUserClinica.numero = rdr[5].ToString();
                            userUserClinica.complemento = rdr[6].ToString();
                            userUserClinica.cidadeNome = rdr[7].ToString();
                            userUserClinica.estadoNome = rdr[8].ToString();
                            userUserClinica.especializacaoNome = rdr[9].ToString();
                            listUserClinica.Add(userUserClinica);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listUserClinica);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao listar os Médicos.");
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