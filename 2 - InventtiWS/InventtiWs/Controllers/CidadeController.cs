using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Services;
using System.Configuration;
using NexxeraWs.Models;
using MySql.Data.MySqlClient;

namespace NexxeraWs.Controllers
{
    [RoutePrefix("api/Cidade")]
    public class CidadeController : ApiController
    {
        [HttpGet]
        [Route("GetListCidades")]
        public HttpResponseMessage GetListCidades(int estado)
        {
            HttpResponseMessage retorno = new HttpResponseMessage();
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            List<CidadeModels> listCidade = new List<CidadeModels>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    MySqlCommand comando = new MySqlCommand("SELECT CID.ID, CID.NOME FROM TB_CIDADE CID WHERE CID.ID_ESTADO = @ESTADO", conn);
                    comando.Parameters.AddWithValue("@ESTADO", estado);

                    try
                    {
                        conn.Open();
                        MySqlDataReader rdr = comando.ExecuteReader();

                        while (rdr.Read())
                        {
                            CidadeModels userCidade = new CidadeModels();
                            userCidade.id = Convert.ToInt32(rdr[0].ToString());
                            userCidade.nome = rdr[1].ToString();

                            listCidade.Add(userCidade);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, listCidade);
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