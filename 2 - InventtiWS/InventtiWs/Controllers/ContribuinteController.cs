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
    [RoutePrefix("api/Contribuinte")]
    public class ContribuinteController : ApiController
    {

        [HttpGet]
        [Route("CarregarContribuinte")]
        public HttpResponseMessage CarregarContribuinte()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    var retornoJson = new JavaScriptSerializer().Serialize(CarregaContribuintes(conn));

                    return Request.CreateResponse(HttpStatusCode.OK, retornoJson);
                }
                catch (Exception e)
                {
                    Request.CreateResponse(HttpStatusCode.InternalServerError, "Erro ao carregar as Transferências.");
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "");
        }


        [HttpPost]
        [Route("SalvarContribuinte")]
        public HttpResponseMessage SalvarContribuinte(string contribuinteJson)
        {
            var contribuinteObj = new JavaScriptSerializer().Deserialize<ContribuinteModels>(contribuinteJson);

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    ContribuinteModels retorno = new ContribuinteModels();

                    contribuinteObj = CalcularIR(contribuinteObj, SalarioMinimoController.CarregarSalarioMinimo(conn).valor);

                    if (contribuinteObj.id <= 0)
                        retorno = NovoContribuinte(contribuinteObj, conn);
                    else
                        retorno = AlterarContribuinte(contribuinteObj, conn);

                    return Request.CreateResponse(HttpStatusCode.OK, new JavaScriptSerializer().Serialize(retorno));
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Houve um erro ao salvar seu Contribuinte");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpPost]
        [Route("DeletarContribuinte")]
        public HttpResponseMessage DeletarContribuinte(string contribuinteJson)
        {
            var contribuinteObj = new JavaScriptSerializer().Deserialize<ContribuinteModels>(contribuinteJson);

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    ContribuinteModels retorno = new ContribuinteModels();

                    if (contribuinteObj.id <= 0)
                        Request.CreateResponse(HttpStatusCode.InternalServerError, "ID inválido para Contribuinte");
                    else
                        retorno = DeletarContribuinte(contribuinteObj, conn);

                    return Request.CreateResponse(HttpStatusCode.OK, new JavaScriptSerializer().Serialize(retorno));
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Houve um erro ao salvar seu Contribuinte");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [HttpPost]
        [Route("InserirSalarioMinimo")]
        public HttpResponseMessage InserirSalarioMinimo(string salarioMinimoJson)
        {
            var salarioMinimo = new JavaScriptSerializer().Deserialize<SalarioMinimoModels>(salarioMinimoJson);

            List<ContribuinteModels> listaContribuinte = new List<ContribuinteModels>();

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                listaContribuinte = CarregaContribuintes(conn);

                try
                {
                    SalarioMinimoController.SalvarSalarioMinimo(salarioMinimo.valor, conn);

                    foreach (ContribuinteModels contribuinte in listaContribuinte)
                    {
                        var contribuinteCalculado = CalcularIR(contribuinte, salarioMinimo.valor);
                        contribuinte.rendaLiquida = contribuinteCalculado.rendaLiquida;
                        contribuinte.ir = contribuinteCalculado.ir;

                        AlterarContribuinte(contribuinte, conn);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new JavaScriptSerializer().Serialize(listaContribuinte));
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Houve um erro ao salvar seu Contribuinte");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private ContribuinteModels CalcularIR(ContribuinteModels contribuinte, decimal valorSalarioMinimo)
        {
            contribuinte.rendaLiquida = contribuinte.rendaBruta - ((5 * contribuinte.numeroDependentes) * (contribuinte.rendaBruta / 100));

            if (valorSalarioMinimo <= 0)
                return contribuinte;

            if (contribuinte.rendaLiquida <= (valorSalarioMinimo * 2))
                contribuinte.ir = 0;
            else if (contribuinte.rendaLiquida > (valorSalarioMinimo * 2) && contribuinte.rendaLiquida <= (valorSalarioMinimo * 4))
                contribuinte.ir = Convert.ToDecimal(7.5);
            else if (contribuinte.rendaLiquida > (valorSalarioMinimo * 4) && contribuinte.rendaLiquida <= (valorSalarioMinimo * 5))
                contribuinte.ir = Convert.ToDecimal(15);
            else if (contribuinte.rendaLiquida > (valorSalarioMinimo * 5) && contribuinte.rendaLiquida <= (valorSalarioMinimo * 7))
                contribuinte.ir = Convert.ToDecimal(22.5);
            else
                contribuinte.ir = Convert.ToDecimal(27.5);

            return contribuinte;
        }

        private List<ContribuinteModels> CarregaContribuintes(SqlConnection conn)
        {
            List<ContribuinteModels> listaContribuinte = new List<ContribuinteModels>();

            SqlCommand comandoCarregarContribuinte = new SqlCommand("SELECT * FROM Contribuinte", conn);

            SqlDataReader rdr = comandoCarregarContribuinte.ExecuteReader();

            try
            {
                while (rdr.Read())
                {
                    ContribuinteModels contribuinte = new ContribuinteModels();
                    contribuinte.id = Convert.ToInt32(rdr[0].ToString());
                    contribuinte.nome = rdr[1].ToString();
                    contribuinte.cpf = rdr[2].ToString();
                    contribuinte.numeroDependentes = Convert.ToInt32(rdr[3].ToString());
                    contribuinte.rendaBruta = Convert.ToDecimal(rdr[4].ToString());
                    contribuinte.rendaLiquida = Convert.ToDecimal(rdr[5].ToString());
                    contribuinte.ir = Convert.ToDecimal(rdr[6].ToString());

                    listaContribuinte.Add(contribuinte);
                }

                rdr.Close();
            }
            catch (Exception e)
            {
                Request.CreateResponse(HttpStatusCode.InternalServerError, "Erro ao carregar as Transferências.");
            }

            return listaContribuinte;
        }

        private ContribuinteModels NovoContribuinte(ContribuinteModels contribuinte, SqlConnection conn)
        {
            SqlCommand comandoNovoContribuinte = new SqlCommand("INSERT INTO Contribuinte (nome, cpf, dependentes, renda_bruta, renda_liquida, ir) " +
                "Values (@NOME, @CPF, @DEPENDENTES, @RENDABRUTA, @RENDALIQUIDA, @IR)", conn);

            comandoNovoContribuinte.Parameters.AddWithValue("@NOME", contribuinte.nome);
            comandoNovoContribuinte.Parameters.AddWithValue("@CPF", contribuinte.cpf);
            comandoNovoContribuinte.Parameters.AddWithValue("@DEPENDENTES", contribuinte.numeroDependentes);
            comandoNovoContribuinte.Parameters.AddWithValue("@RENDABRUTA", contribuinte.rendaBruta);
            comandoNovoContribuinte.Parameters.AddWithValue("@RENDALIQUIDA", contribuinte.rendaLiquida);
            comandoNovoContribuinte.Parameters.AddWithValue("@IR", contribuinte.ir);

            try
            {
                var exec = comandoNovoContribuinte.ExecuteNonQuery();

                SqlCommand comandoUltimoContribuinte = new SqlCommand("SELECT TOP 1 id FROM Contribuinte ORDER BY id DESC", conn);

                SqlDataReader rdr = comandoUltimoContribuinte.ExecuteReader();

                if (rdr.Read())
                {
                    contribuinte.id = Convert.ToInt32(rdr[0].ToString());
                }

                rdr.Close();
            }
            catch (Exception e)
            {
                throw e;
            }

            return contribuinte;
        }

        private ContribuinteModels AlterarContribuinte(ContribuinteModels contribuinte, SqlConnection conn)
        {
            SqlCommand comandoEditarContribuinte = new SqlCommand("UPDATE Contribuinte SET nome = @NOME, cpf = @CPF, dependentes = @DEPENDENTES, " +
                "renda_bruta = @RENDABRUTA, renda_liquida = @RENDALIQUIDA, ir = @IR WHERE id = @ID", conn);

            comandoEditarContribuinte.Parameters.AddWithValue("@ID", contribuinte.id);
            comandoEditarContribuinte.Parameters.AddWithValue("@NOME", contribuinte.nome);
            comandoEditarContribuinte.Parameters.AddWithValue("@CPF", contribuinte.cpf);
            comandoEditarContribuinte.Parameters.AddWithValue("@DEPENDENTES", contribuinte.numeroDependentes);
            comandoEditarContribuinte.Parameters.AddWithValue("@RENDABRUTA", contribuinte.rendaBruta);
            comandoEditarContribuinte.Parameters.AddWithValue("@RENDALIQUIDA", contribuinte.rendaLiquida);
            comandoEditarContribuinte.Parameters.AddWithValue("@IR", contribuinte.ir);

            try
            {
                var exec = comandoEditarContribuinte.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }

            return contribuinte;
        }

        private ContribuinteModels DeletarContribuinte(ContribuinteModels contribuinte, SqlConnection conn)
        {
            SqlCommand comandoEditarContribuinte = new SqlCommand("DELETE FROM Contribuinte WHERE id = @ID", conn);

            comandoEditarContribuinte.Parameters.AddWithValue("@ID", contribuinte.id);

            try
            {
                var exec = comandoEditarContribuinte.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }

            return contribuinte;
        }

    }

}