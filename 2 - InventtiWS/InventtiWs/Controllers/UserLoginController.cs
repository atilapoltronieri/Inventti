using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace NexxeraWs.Controllers
{
    [RoutePrefix("api/UserLogin")]
    public class UserLoginController : ApiController
    {
        [HttpPost]
        [Route("VerificaLogin")]
        public HttpResponseMessage VerificaLogin(string user, string token)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(token))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = UserController.GetUserDataReader(conn, user, null);

                        if (rdr.Read())
                        {
                            if (rdr[1].ToString() == "1")
                            {
                                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário Bloqueado.");
                            }
                            else
                            {
                                string userId = rdr[0].ToString();
                                rdr.Close();

                                MySqlCommand comando = new MySqlCommand(
                                    "SELECT USUL.ID FROM TB_USUARIO_LOGIN USUL WHERE USUL.ID_USUARIO=@ID_USUARIO AND USUL.TOKEN=@TOKEN", conn);
                                comando.Parameters.AddWithValue("@ID_USUARIO", userId);
                                comando.Parameters.AddWithValue("@TOKEN", token);

                                try
                                {

                                    rdr = comando.ExecuteReader();
                                    if (rdr.Read())
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                }
                                catch
                                {
                                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao Encontrar Login.");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao Encontrar Usuário.");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao Encontrar Tudo.");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Falha ao conectar no servidor: " + e.Message);
            }
        }

        public static void AdicionarToken(string userId, string token, MySqlConnection conn)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return;

            try
            {
                DeletarToken(userId, conn);

                MySqlCommand comando = new MySqlCommand("INSERT INTO TB_USUARIO_LOGIN(ID_USUARIO, TOKEN) VALUES (@ID_USUARIO, @TOKEN)", conn);
                comando.Parameters.AddWithValue("@ID_USUARIO", userId);
                comando.Parameters.AddWithValue("@TOKEN", token);

                try
                {
                    var exec = comando.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
            }
            finally
            {
            }
        }

        public static void DeletarToken(string userId, MySqlConnection conn)
        {
            try
            {
                MySqlCommand comando = new MySqlCommand("DELETE FROM TB_USUARIO_LOGIN WHERE ID_USUARIO=@ID_USUARIO", conn);
                comando.Parameters.AddWithValue("@ID_USUARIO", userId);

                try
                {
                    var exec = comando.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
            }
            finally
            {
            }
        }
    }
}
