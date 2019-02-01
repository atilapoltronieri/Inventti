using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace BrConselhosWs.Controllers
{
    [RoutePrefix("api/Teste")]
    public class TesteController : ApiController
    {
        [HttpGet]
        [Route("TesteGet")]
        public string Get()
        {
            string retorno = "";
            MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3307;User Id=root;database=uniconsulta; password=root");
            conn.Open();
            MySqlCommand comando = new MySqlCommand("SELECT EMAIL FROM TB_USUARIO LIMIT 1", conn);
            try
            {
                MySqlDataReader rdr = comando.ExecuteReader();

                while (rdr.Read())
                {
                    retorno = rdr[0].ToString();
                }
            }
            finally
            {
                conn.Close();
            }

            return retorno;
        }
    }
}