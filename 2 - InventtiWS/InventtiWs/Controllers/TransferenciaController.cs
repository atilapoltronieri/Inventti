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
    [RoutePrefix("api/Transferencia")]
    public class TransferenciaController : ApiController
    {
        [HttpPost]
        [Route("SalvarTransferencia")]
        public HttpResponseMessage SalvarTransferencia(string transferenciaJson)
        {
            var transferenciaObj = new JavaScriptSerializer().Deserialize<Transferencia>(transferenciaJson);

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    Transferencia retorno = new Transferencia();

                    if (transferenciaObj.id <= 0)
                        retorno = NovaTransferencia(transferenciaObj, conn);
                    else
                        retorno = AlterarTransferencia(transferenciaObj, conn);

                    return Request.CreateResponse(HttpStatusCode.OK, new JavaScriptSerializer().Serialize(retorno));
                }
                catch (Exception e)
                {

                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Houve um erro ao salvar sua transferência");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private Transferencia NovaTransferencia(Transferencia transferencia, SqlConnection conn)
        {
            SqlCommand comandoNovaTransferencia = new SqlCommand("INSERT INTO Transferencia (usuario_id, pagador_nome, pagador_banco, pagador_agencia, pagador_conta, " +
                "beneficiario_nome, beneficiario_banco, beneficiario_agencia, beneficiario_conta, valor, tipo, status) Values (" +
                "@USUARIO_ID, @PAGADOR_NOME, @PAGADOR_BANCO, @PAGADOR_AGENCIA, @PAGADOR_CONTA, @BENEFICIARIO_NOME, @BENEFICIARIO_BANCO, @BENEFICIARIO_AGENCIA, " +
                "@BENEFICIARIO_CONTA, @VALOR, @TIPO, @STATUS)", conn);

            comandoNovaTransferencia.Parameters.AddWithValue("@USUARIO_ID", transferencia.usuario_id);
            comandoNovaTransferencia.Parameters.AddWithValue("@PAGADOR_NOME", transferencia.pagadorNome);
            comandoNovaTransferencia.Parameters.AddWithValue("@PAGADOR_BANCO", transferencia.pagadorBanco);
            comandoNovaTransferencia.Parameters.AddWithValue("@PAGADOR_AGENCIA", transferencia.pagadorAgencia);
            comandoNovaTransferencia.Parameters.AddWithValue("@PAGADOR_CONTA", transferencia.pagadorConta);
            comandoNovaTransferencia.Parameters.AddWithValue("@BENEFICIARIO_NOME", transferencia.beneficiarioNome);
            comandoNovaTransferencia.Parameters.AddWithValue("@BENEFICIARIO_BANCO", transferencia.beneficiarioBanco);
            comandoNovaTransferencia.Parameters.AddWithValue("@BENEFICIARIO_AGENCIA", transferencia.beneficiarioAgencia);
            comandoNovaTransferencia.Parameters.AddWithValue("@BENEFICIARIO_CONTA", transferencia.beneficiarioConta);
            comandoNovaTransferencia.Parameters.AddWithValue("@VALOR", transferencia.valor);
            comandoNovaTransferencia.Parameters.AddWithValue("@TIPO", transferencia.tipo);
            comandoNovaTransferencia.Parameters.AddWithValue("@STATUS", transferencia.status);

            try
            {
                var exec = comandoNovaTransferencia.ExecuteNonQuery();

                SqlCommand comandoUltimaTransferencia = new SqlCommand("SELECT TOP 1 id, data FROM Transferencia ORDER BY id DESC", conn);

                SqlDataReader rdr = comandoUltimaTransferencia.ExecuteReader();

                if (rdr.Read())
                {
                    transferencia.id = Convert.ToInt32(rdr[0].ToString());
                    transferencia.data = Convert.ToDateTime(rdr[1].ToString()).ToString("yyyy-MM-ddTHH:mm:ss"); ;
                }

                rdr.Close();
            }
            catch (Exception e)
            {
                throw e;
            }

            return transferencia;
        }

        private Transferencia AlterarTransferencia(Transferencia transferencia, SqlConnection conn)
        {
            SqlCommand comandoEditarTransferencia = new SqlCommand("UPDATE Transferencia SET pagador_nome = @PAGADOR_NOME, pagador_banco = @PAGADOR_BANCO, pagador_agencia = @PAGADOR_AGENCIA, " +
                "pagador_conta = @PAGADOR_CONTA, beneficiario_nome = @BENEFICIARIO_NOME, beneficiario_banco = @BENEFICIARIO_BANCO, beneficiario_agencia =@BENEFICIARIO_AGENCIA, " +
                "beneficiario_conta = @BENEFICIARIO_CONTA, valor = @VALOR, tipo = @TIPO, status = @STATUS WHERE id = @ID", conn);

            comandoEditarTransferencia.Parameters.AddWithValue("@ID", transferencia.id);
            comandoEditarTransferencia.Parameters.AddWithValue("@PAGADOR_NOME", transferencia.pagadorNome);
            comandoEditarTransferencia.Parameters.AddWithValue("@PAGADOR_BANCO", transferencia.pagadorBanco);
            comandoEditarTransferencia.Parameters.AddWithValue("@PAGADOR_AGENCIA", transferencia.pagadorAgencia);
            comandoEditarTransferencia.Parameters.AddWithValue("@PAGADOR_CONTA", transferencia.pagadorConta);
            comandoEditarTransferencia.Parameters.AddWithValue("@BENEFICIARIO_NOME", transferencia.beneficiarioNome);
            comandoEditarTransferencia.Parameters.AddWithValue("@BENEFICIARIO_BANCO", transferencia.beneficiarioBanco);
            comandoEditarTransferencia.Parameters.AddWithValue("@BENEFICIARIO_AGENCIA", transferencia.beneficiarioAgencia);
            comandoEditarTransferencia.Parameters.AddWithValue("@BENEFICIARIO_CONTA", transferencia.beneficiarioConta);
            comandoEditarTransferencia.Parameters.AddWithValue("@VALOR", transferencia.valor);
            comandoEditarTransferencia.Parameters.AddWithValue("@TIPO", transferencia.tipo);
            comandoEditarTransferencia.Parameters.AddWithValue("@STATUS", transferencia.status);

            try
            {
                var exec = comandoEditarTransferencia.ExecuteNonQuery();
                
            }
            catch (Exception e)
            {
                throw e;
            }

            return transferencia;
        }

        [HttpGet]
        [Route("CarregarTransferencia")]
        public HttpResponseMessage CarregarTransferencia(int usuarioId)
        {
            if (usuarioId <= 0)
                Request.CreateResponse(HttpStatusCode.InternalServerError, "Sem usuário.");

            List<Transferencia> listaTransferencia = new List<Transferencia>();

            string connectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand comandoCarregarTransferencia = new SqlCommand("SELECT * FROM Transferencia where usuario_id = @USUARIO_ID AND status != 'DELETADO'", conn);
                comandoCarregarTransferencia.Parameters.AddWithValue("@USUARIO_ID", usuarioId);

                SqlDataReader rdr = comandoCarregarTransferencia.ExecuteReader();

                try
                {
                    while (rdr.Read())
                    {
                        Transferencia transferencia = new Transferencia();
                        transferencia.id = Convert.ToInt32(rdr[0].ToString());
                        transferencia.usuario_id = Convert.ToInt32(rdr[1].ToString());
                        transferencia.pagadorNome = rdr[2].ToString();
                        transferencia.pagadorBanco = rdr[3].ToString();
                        transferencia.pagadorAgencia = rdr[4].ToString();
                        transferencia.pagadorConta = rdr[5].ToString();
                        transferencia.beneficiarioNome = rdr[6].ToString();
                        transferencia.beneficiarioBanco = rdr[7].ToString();
                        transferencia.beneficiarioAgencia = rdr[8].ToString();
                        transferencia.beneficiarioConta = rdr[9].ToString();
                        transferencia.valor = Convert.ToDecimal(rdr[10].ToString());
                        transferencia.tipo = rdr[11].ToString();
                        transferencia.status = rdr[12].ToString();
                        transferencia.data = Convert.ToDateTime(rdr[13].ToString()).ToString("yyyy-MM-ddTHH:mm:ss");

                        listaTransferencia.Add(transferencia);
                    }

                    var retornoJson = new JavaScriptSerializer().Serialize(listaTransferencia);

                    return Request.CreateResponse(HttpStatusCode.OK, retornoJson);
                }
                catch(Exception e)
                {
                    Request.CreateResponse(HttpStatusCode.InternalServerError, "Erro ao carregar as Transferências.");
                }
                finally
                {
                    conn.Close();
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "");
        }
    }

    public class Transferencia
    {
        public int id { get; set; }
        public int usuario_id { get; set; }
        public string pagadorNome { get; set; }
        public string pagadorBanco { get; set; }
        public string pagadorAgencia { get; set; }
        public string pagadorConta { get; set; }
        public string beneficiarioNome { get; set; }
        public string beneficiarioBanco { get; set; }
        public string beneficiarioAgencia { get; set; }
        public string beneficiarioConta { get; set; }
        public decimal valor { get; set; }
        public string tipo { get; set; }
        public string status { get; set; }
        public string data { get; set; }
    }
}