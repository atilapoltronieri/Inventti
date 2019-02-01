using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace BrConselhosWs.Controllers
{
    [RoutePrefix("api/Login")]
    public class LoginController : ApiController
    {
        [HttpGet]
        [Route("AdicionarUsuario")]
        public HttpResponseMessage AdicionarUsuario(string nome, string cnpj)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand comandoVerificarCNPJ = new SqlCommand("SELECT nome, cnpj FROM usuario WHERE cnpj=@CNPJ", conn);
                comandoVerificarCNPJ.Parameters.AddWithValue("@CNPJ", cnpj);
                try
                {
                    SqlDataReader rdr = comandoVerificarCNPJ.ExecuteReader();

                    while (rdr.Read())
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "CNPJ em Uso.");
                    }

                    rdr.Close();
                }
                finally
                {
                }

                SqlCommand comando = new SqlCommand("INSERT INTO usuario(nome, cnpj) VALUES (@NOME, @CNPJ);", conn);
                comando.Parameters.AddWithValue("@NOME", nome);
                comando.Parameters.AddWithValue("@CNPJ", cnpj);
                try
                {
                    var exec = comando.ExecuteNonQuery();

                    if (exec == 1)
                    {
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
        [Route("Login")]
        public HttpResponseMessage Login(string nome, string cnpj)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand comandoVerificarCNPJ = new SqlCommand("SELECT id, nome, cnpj FROM usuario WHERE nome=@NOME and cnpj=@CNPJ", conn);
                comandoVerificarCNPJ.Parameters.AddWithValue("@NOME", nome);
                comandoVerificarCNPJ.Parameters.AddWithValue("@CNPJ", cnpj);
                try
                {
                    SqlDataReader rdr = comandoVerificarCNPJ.ExecuteReader();

                    while (rdr.Read())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, Convert.ToInt32(rdr[0].ToString()));
                    }

                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Usuário/CNPJ não Cadastrados.");
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}