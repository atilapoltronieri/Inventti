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
    [RoutePrefix("api/Estado")]
    public class EstadoController : ApiController
    {
        [HttpGet]
        [Route("GetListEstado")]
        public HttpResponseMessage GetListEstado()
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<EstadoModels> listEstado = new List<EstadoModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT EST.ID, EST.NOME, EST.SIGLA FROM TB_ESTADO EST", conn);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            EstadoModels userEstado = new EstadoModels();
                            userEstado.id = Convert.ToInt32(rdr[0].ToString());
                            userEstado.nome = rdr[1].ToString();
                            userEstado.sigla = rdr[2].ToString();

                            listEstado.Add(userEstado);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listEstado);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Houve um erro ao listar os Estados.");
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