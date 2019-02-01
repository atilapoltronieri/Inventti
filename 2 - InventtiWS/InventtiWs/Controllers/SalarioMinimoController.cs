using BrConselhosWs.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace BrConselhosWs.Controllers
{
    [RoutePrefix("api/SalarioMinimo")]
    public class SalarioMinimoController : ApiController
    {
        [HttpGet]
        [Route("CarregarSalarioMinimo")]
        public HttpResponseMessage CarregarSalarioMinimo()
        {
            List<SalarioMinimoModels> listaContribuinte = new List<SalarioMinimoModels>();

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                try
                {
                    SalarioMinimoModels salarioMinimo = CarregarSalarioMinimo(conn);

                    return Request.CreateResponse(HttpStatusCode.OK, salarioMinimo.valor);
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Houve um erro ao carregar seu Salário Mínimo");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static SalarioMinimoModels CarregarSalarioMinimo(SqlConnection conn)
        {

            SqlCommand comandoCarregarSalarioMinimo = new SqlCommand("SELECT * FROM SalarioMinimo", conn);

            SqlDataReader rdr = comandoCarregarSalarioMinimo.ExecuteReader();

            try
            {
                SalarioMinimoModels salarioMinimo = new SalarioMinimoModels();

                if (rdr.Read())
                {
                    salarioMinimo.id = Convert.ToInt32(rdr[0].ToString());
                    salarioMinimo.valor = Convert.ToDecimal(rdr[1].ToString());
                }
                else
                {
                    salarioMinimo.id = 0;
                    salarioMinimo.valor = 0;
                }

                rdr.Close();

                return salarioMinimo;
            }
            catch (Exception e)
            {
                return new SalarioMinimoModels();
            }
        }

        public static SalarioMinimoModels SalvarSalarioMinimo(decimal valor, SqlConnection conn)
        {
            try
            {
                SalarioMinimoModels retorno = new SalarioMinimoModels();
                retorno.valor = valor;

                SqlCommand comandoDeletarSalarioMinimo = new SqlCommand("DELETE FROM SalarioMinimo", conn);

                var execDeletar = comandoDeletarSalarioMinimo.ExecuteNonQuery();

                SqlCommand comandoSalvarSalarioMinimo = new SqlCommand("INSERT INTO SalarioMinimo (valor) VALUES (@VALOR)", conn);
                comandoSalvarSalarioMinimo.Parameters.AddWithValue("@VALOR", valor);

                var execInserir = comandoSalvarSalarioMinimo.ExecuteReader();
                execInserir.Close();

                return retorno;
            }
            catch (Exception e)
            {
                return new SalarioMinimoModels();
            }
        }
    }
}