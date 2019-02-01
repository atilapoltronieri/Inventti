using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Http;
using NexxeraWs.Models;
using NexxeraWs.Rules;

namespace NexxeraWs.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        [HttpPost]
        [Route("Login")]
        public HttpResponseMessage Login(UserModels user, string crm = null)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string joinCrm = string.Empty;
            string loginCrm = string.Empty;

            if (!string.IsNullOrEmpty(crm))
            {
                joinCrm = "JOIN TB_USUARIO_CLINICA CLI ON USU.ID = CLI.ID_USUARIO";
                loginCrm = "AND CLI.CRM = @CRM";
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand comando = new MySqlCommand("SELECT USU.SENHA, USU.BLOQUEIO FROM TB_USUARIO USU " + joinCrm + " WHERE USU.EMAIL=@EMAIL " + loginCrm, conn);
                comando.Parameters.AddWithValue("@EMAIL", user.email);
                comando.Parameters.AddWithValue("@CRM", crm ?? string.Empty);
                try
                {
                    conn.Open();
                    MySqlDataReader rdr = comando.ExecuteReader();

                    while (rdr.Read())
                    {
                        if (rdr[1].ToString() == "1")
                            return Request.CreateResponse(HttpStatusCode.OK, "Usuário Bloqueado.");

                        if (!UserRules.VerifyHash(user.senha, "MD5", rdr[0].ToString()))
                            return Request.CreateResponse(HttpStatusCode.OK, "Usuário/Senha Inválido.");
                        else
                            return Request.CreateResponse(HttpStatusCode.OK, user);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, "Usuário/Senha Inválido.");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpPost]
        [Route("Login")]
        public HttpResponseMessage Login(string email, string password, string crm = null)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var rdr = GetUserDataReader(conn, email, crm);
                    if (rdr.Read())
                    {
                        if (rdr[2].ToString() == "1")
                        {
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário Bloqueado.");
                        }
                        if (!UserRules.VerifyHash(password, "MD5", rdr[1].ToString()))
                        {
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário/Senha Inválido.");
                        }
                        else
                        {
                            var id_user = rdr[0].ToString();
                            rdr.Close();
                            var newToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

                            UserLoginController.AdicionarToken(id_user, newToken, conn);
                            return Request.CreateResponse(HttpStatusCode.OK, newToken);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário/Senha Inválido.");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static MySqlDataReader GetUserDataReader(MySqlConnection conn, string email, string crm)
        {
            string joinCrm = string.Empty;
            string loginCrm = string.Empty;

            if (!string.IsNullOrEmpty(crm))
            {
                joinCrm = "JOIN TB_USUARIO_CLINICA CLI ON USU.ID = CLI.ID_USUARIO";
                loginCrm = "AND CLI.CRM = @CRM";
            }

            MySqlCommand comando = new MySqlCommand("SELECT USU.ID, USU.SENHA, USU.BLOQUEIO FROM TB_USUARIO USU " + joinCrm + " WHERE USU.EMAIL=@EMAIL " + loginCrm, conn);
            comando.Parameters.AddWithValue("@EMAIL", email);
            comando.Parameters.AddWithValue("@CRM", crm ?? string.Empty);

            return comando.ExecuteReader();
        }

        [HttpPost]
        [Route("AddUser")]
        public HttpResponseMessage AddUser(string email, string password, string nome, string sobrenome)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT SENHA, BLOQUEIO FROM TB_USUARIO WHERE EMAIL=@EMAIL", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while (rdr.Read())
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "Email em Uso.");
                    }

                    rdr.Close();
                }
                finally
                {
                }

                MySqlCommand comando = new MySqlCommand("INSERT INTO TB_USUARIO(EMAIL, SENHA, BLOQUEIO) VALUES (@EMAIL, @SENHA, 0);", conn);
                comando.Parameters.AddWithValue("@EMAIL", email);
                comando.Parameters.AddWithValue("@SENHA", UserRules.ComputeHash(password, "MD5", null));
                try
                {
                    var exec = comando.ExecuteNonQuery();

                    if (exec == 1)
                    {
                        comando = new MySqlCommand("INSERT INTO TB_USUARIO_APLICATIVO(ID_USUARIO, NOME, SOBRENOME) VALUES " +
                                                   "((SELECT ID FROM TB_USUARIO WHERE EMAIL = @EMAIL), @NOME, @SOBRENOME);", conn);
                        comando.Parameters.AddWithValue("@EMAIL", email);
                        comando.Parameters.AddWithValue("@NOME", nome);
                        comando.Parameters.AddWithValue("@SOBRENOME", sobrenome);

                        comando.ExecuteNonQuery();

                        return Request.CreateResponse(HttpStatusCode.OK, Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "Houve um Erro ao Adicionar seu Usário. Por Favor Entre em Contato com Nossa Central de Atendimento.");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpPost]
        [Route("AlterUser")]
        public HttpResponseMessage AlterUser(string email, string oldPassword, string newPassword)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                
                try
                {
                    var rdr = GetUserDataReader(conn, email, null);

                    while (rdr.Read())
                    {
                        if (rdr[2].ToString() == "1")
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário Bloqueado");

                        if (!UserRules.VerifyHash(oldPassword, "MD5", rdr[1].ToString()))
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário e Senha Não Conferem");

                        rdr.Close();

                        newPassword = UserRules.ComputeHash(newPassword, "MD5", null);

                        MySqlCommand comandoAlterUser = new MySqlCommand("UPDATE TB_USUARIO SET SENHA = @SENHA WHERE EMAIL = @EMAIL;", conn);
                        comandoAlterUser.Parameters.AddWithValue("@EMAIL", email);
                        comandoAlterUser.Parameters.AddWithValue("@SENHA", newPassword);
                        try
                        {
                            var exec = comandoAlterUser.ExecuteNonQuery();

                            if (exec == 1)
                            {
                                var newToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

                                UserLoginController.AdicionarToken(rdr[0].ToString(), newToken, conn);
                                return Request.CreateResponse(HttpStatusCode.OK, newToken);
                            }
                            else
                                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Houve um Erro ao Alterar seu Usário. Por Favor Entre em Contato com Nossa Central de Atendimento");
                        }
                        finally { }
                    }

                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário e Senha Não Conferem");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpPost]
        [Route("RememberUser")]
        public HttpResponseMessage RememberUser(string email)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT ID FROM TB_USUARIO WHERE EMAIL=@EMAIL", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while (rdr.Read())
                    {
                        try
                        {
                            var id_user = rdr[0].ToString();
                            rdr.Close();

                            MySqlCommand comandoDeletarToken = new MySqlCommand("DELETE FROM TB_RECUPERAR_SENHA WHERE ID_USUARIO=@IDUSUARIO", conn);
                            comandoDeletarToken.Parameters.AddWithValue("@IDUSUARIO", id_user);
                            comandoDeletarToken.ExecuteNonQuery();

                            // Gerando código chave para usuário recuperar senha
                            string userRememberPass = UserRules.RememberPassGenerator();

                            MailMessage mail = new MailMessage();
                            SmtpClient client = new SmtpClient();
                            MailAddress fromAddress = new MailAddress("noreply@uniconsulta.kinghost.net", "NoReply");
                            MailAddress toAddress = new MailAddress(email, "Aqui vai o Ususário");
                            client.Port = 587;
                            client.Host = "smtp.kinghost.net";
                            client.EnableSsl = false;
                            client.Timeout = 100000;
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new System.Net.NetworkCredential("noreply@uniconsulta.kinghost.net", "emaildeenvio12");
                            mail.From = fromAddress;
                            mail.To.Add(toAddress);
                            mail.Subject = "Recuperação de senha : Usuário " + email;
                            mail.Body = "Para gerar sua nova senha digite o seguinte código no seu aplicativo : " + userRememberPass;
                            mail.BodyEncoding = UTF8Encoding.UTF8;
                            mail.SubjectEncoding = System.Text.Encoding.Default;
                            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                            client.Send(mail);

                            MySqlCommand comandoInserirToken = new MySqlCommand("INSERT INTO TB_RECUPERAR_SENHA (ID_USUARIO, TOKEN) VALUES (@IDUSUARIO, @TOKEN)", conn);
                            comandoInserirToken.Parameters.AddWithValue("@IDUSUARIO", id_user);
                            comandoInserirToken.Parameters.AddWithValue("@TOKEN", UserRules.ComputeHash(userRememberPass, "MD5", null));
                            comandoInserirToken.ExecuteNonQuery();

                            return Request.CreateResponse(HttpStatusCode.OK, "Email enviado. Digite o Token enviado e sua nova senha.");
                        }
                        catch (Exception e)
                        {
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Erro ao enviar emai. Por favor verifique seu usuário.");
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Erro ao enviar emai. Por favor verifique seu usuário.");

                    rdr.Close();
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpPost]
        [Route("AlterRememberUser")]
        public HttpResponseMessage AlterRememberUser(string email, string novaSenha, string token)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT ID FROM TB_USUARIO WHERE EMAIL=@EMAIL", conn);
                comandoVerificarEmail.Parameters.AddWithValue("@EMAIL", email);
                try
                {
                    MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                    while (rdr.Read())
                    {
                        try
                        {
                            var id_user = rdr[0].ToString();
                            rdr.Close();

                            MySqlCommand comandoGetToken = new MySqlCommand("SELECT TOKEN FROM TB_RECUPERAR_SENHA WHERE ID_USUARIO=@IDUSUARIO", conn);
                            comandoGetToken.Parameters.AddWithValue("@IDUSUARIO", id_user);
                            rdr = comandoGetToken.ExecuteReader();

                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    if (UserRules.VerifyHash(token, "MD5", rdr[0].ToString()))
                                    {
                                        rdr.Close();

                                        MySqlCommand comandoDeletarToken = new MySqlCommand("DELETE FROM TB_RECUPERAR_SENHA WHERE ID_USUARIO=@IDUSUARIO", conn);
                                        comandoDeletarToken.Parameters.AddWithValue("@IDUSUARIO", id_user);
                                        comandoDeletarToken.ExecuteNonQuery();

                                        MySqlCommand comandoAlterUser = new MySqlCommand("UPDATE TB_USUARIO SET SENHA = @SENHA WHERE EMAIL = @EMAIL;", conn);
                                        comandoAlterUser.Parameters.AddWithValue("@EMAIL", email);
                                        comandoAlterUser.Parameters.AddWithValue("@SENHA", UserRules.ComputeHash(novaSenha, "MD5", null));
                                        try
                                        {
                                            var exec = comandoAlterUser.ExecuteNonQuery();

                                            if (exec == 1)
                                            {
                                                UserLoginController.DeletarToken(id_user, conn);
                                                return Request.CreateResponse(HttpStatusCode.OK, "Usuário Alterado com Sucesso");
                                            }
                                            else
                                                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Houve um Erro ao Alterar seu Usário. Por Favor Entre em Contato com Nossa Central de Atendimento");
                                        }
                                        catch
                                        {
                                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Houve um Erro ao Alterar seu Usário. Por Favor Entre em Contato com Nossa Central de Atendimento");
                                        }

                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.Forbidden, "Token inválido. Verifique se o digitou corretamente.");
                                    }
                                }
                            }
                            else
                            {
                                RememberUser(email);
                                return Request.CreateResponse(HttpStatusCode.Forbidden, "Não há token para seu Usuário. Foi enviado um ao seu email.");
                            }
                        }
                        catch (Exception e)
                        {
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Erro ao enviar emai. Por favor verifique seu usuário.");
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Erro ao enviar emai. Por favor verifique seu usuário.");
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}