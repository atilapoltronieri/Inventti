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
    [RoutePrefix("api/Especializacao")]
    public class EspecializacaoController : ApiController
    {
        [HttpGet]
        [Route("GetListEspecializacao")]
        public HttpResponseMessage GetListEspecializacao()
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<EspecializacaoModels> listEspecializacao = new List<EspecializacaoModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT ESP.ID, ESP.NOME FROM TB_ESPECIALIZACAO ESP", conn);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            EspecializacaoModels userEspecializacao = new EspecializacaoModels();
                            userEspecializacao.id = Convert.ToInt32(rdr[0].ToString());
                            userEspecializacao.nome = rdr[1].ToString();

                            listEspecializacao.Add(userEspecializacao);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listEspecializacao);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao listar as Especializações.");
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