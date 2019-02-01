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
    [RoutePrefix("api/Convenio")]
    public class ConvenioController : ApiController
    {
        [HttpGet]
        [Route("GetListConvenio")]
        public HttpResponseMessage GetListConvenio()
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<ConvenioModels> listConvenio = new List<ConvenioModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT CON.ID, CON.NOME FROM TB_CONVENIO CON", conn);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            ConvenioModels userConvenio = new ConvenioModels();
                            userConvenio.id = Convert.ToInt32(rdr[0].ToString());
                            userConvenio.nome = rdr[1].ToString();

                            listConvenio.Add(userConvenio);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listConvenio);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao listar os Convênios.");
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