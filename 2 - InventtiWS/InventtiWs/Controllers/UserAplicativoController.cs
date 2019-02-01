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

namespace NexxeraWs.Controllers
{
    [RoutePrefix("api/UserAplicativo")]
    public class UserAplicativoController : ApiController
    {
        [HttpGet]
        [Route("GetListUserPerfil")]
        public HttpResponseMessage GetListUserPerfil(string email)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string joinCrm = string.Empty;
            string loginCrm = string.Empty;
            List<UserAplicativoModels> listUserAplicativo = new List<UserAplicativoModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT APL.ID, APL.ID_USUARIO,APL.NOME, APL.SOBRENOME, APL.TELEFONE, APL.ID_CONVENIO, APL.NUMEROCONVENIO,"
                        + "(SELECT CON.NOME FROM TB_CONVENIO CON WHERE CON.ID=APL.ID_CONVENIO) FROM TB_USUARIO_APLICATIVO APL "
                        + "JOIN TB_USUARIO USU ON USU.ID = APL.ID_USUARIO WHERE USU.EMAIL=@EMAIL", conn);
                    comando.Parameters.AddWithValue("@EMAIL", email);
                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            UserAplicativoModels userAplicativo = new UserAplicativoModels();
                            userAplicativo.id = Convert.ToInt32(rdr[0].ToString());
                            userAplicativo.id_Usuario = Convert.ToInt32(rdr[1].ToString());
                            userAplicativo.nome = rdr[2].ToString();
                            userAplicativo.sobrenome = rdr[3].ToString();
                            if (!string.IsNullOrEmpty(rdr[4].ToString()))
                                userAplicativo.telefone = rdr[4].ToString();
                            if (!string.IsNullOrEmpty(rdr[5].ToString()))
                            {
                                userAplicativo.id_Convenio = Convert.ToInt32(rdr[5].ToString());
                                userAplicativo.convenioNome = rdr[7].ToString();
                            }
                            if (!string.IsNullOrEmpty(rdr[6].ToString()))
                                userAplicativo.numeroConvenio = Convert.ToInt64(rdr[6].ToString());

                            listUserAplicativo.Add(userAplicativo);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listUserAplicativo);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro com seu perfil: " + e.Message);
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

        [WebMethod]
        [HttpPost]
        [Route("AddUserAplicativo")]
        public HttpResponseMessage AddUserAplicativo(string email, string nome, string sobrenome, string telefone, int convenio, string numeroConvenio)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                //string usarioClicaCadastrado = UserAplicativoRules.VerificaConvenio(convenio, numeroConvenio, conn);
                //if (!string.IsNullOrEmpty(usarioClicaCadastrado))
                //    return usarioClicaCadastrado;

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT ID FROM TB_USUARIO WHERE EMAIL=@EMAIL LIMIT 1", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while (!rdr.IsClosed && rdr.Read())
                    {
                        MySqlCommand comando = new MySqlCommand("INSERT INTO TB_USUARIO_APLICATIVO(ID_USUARIO, NOME, SOBRENOME, TELEFONE, ID_CONVENIO, NUMEROCONVENIO) " +
                            "VALUES(@ID_USUARIO, @NOME, @SOBRENOME, @TELEFONE, @CONVENIO, @NUMEROCONVENIO); ", conn);
                        comando.Parameters.AddWithValue("@ID_USUARIO", rdr[0].ToString());
                        comando.Parameters.AddWithValue("@NOME", nome);
                        comando.Parameters.AddWithValue("@SOBRENOME", sobrenome);
                        comando.Parameters.AddWithValue("@TELEFONE", telefone);
                        comando.Parameters.AddWithValue("@CONVENIO", convenio);
                        comando.Parameters.AddWithValue("@NUMEROCONVENIO", numeroConvenio);

                        rdr.Close();

                        var exec = comando.ExecuteNonQuery();

                        if (exec == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Inserir seu Perfil.");
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Perfil Cadastrado com Sucesso");
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Inserir seu Perfil: " + e.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [WebMethod]
        [HttpPost]
        [Route("AlterUserAplicativo")]
        public HttpResponseMessage AlterUserAplicativo(int id, string email, string nome, string sobrenome, string telefone, int convenio, string numeroConvenio)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Verificar necessidade de não poder cadastrar Convênio igual
                //string usarioClicaCadastrado = UserAplicativoRules.VerificaConvenio(convenio, numeroConvenio, conn);
                //if (!string.IsNullOrEmpty(usarioClicaCadastrado))
                //    return usarioClicaCadastrado;

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT ID FROM TB_USUARIO WHERE EMAIL=@EMAIL LIMIT 1", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while (!rdr.IsClosed && rdr.Read())
                    {
                        MySqlCommand comando = new MySqlCommand("UPDATE TB_USUARIO_APLICATIVO SET NOME=@NOME, SOBRENOME=@SOBRENOME, TELEFONE=@TELEFONE, " +
                            "ID_CONVENIO=@CONVENIO, NUMEROCONVENIO=@NUMEROCONVENIO WHERE ID_USUARIO=@ID_USUARIO AND ID=@ID; ", conn);
                        comando.Parameters.AddWithValue("@ID", id);
                        comando.Parameters.AddWithValue("@ID_USUARIO", rdr[0].ToString());
                        comando.Parameters.AddWithValue("@NOME", nome);
                        comando.Parameters.AddWithValue("@SOBRENOME", sobrenome);
                        comando.Parameters.AddWithValue("@TELEFONE", telefone);
                        comando.Parameters.AddWithValue("@CONVENIO", convenio);
                        comando.Parameters.AddWithValue("@NUMEROCONVENIO", numeroConvenio);

                        rdr.Close();

                        var exec = comando.ExecuteNonQuery();

                        if (exec == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Alterar seu Perfil.");
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Perfil Alterado com Sucesso");
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um Erro ao Alterar seu Perfil: " + e.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}