using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrConselhosWs.Rules
{
    public class UserAplicativoRules
    {
        public static string VerificaConvenio(string convenio, string numeroConvenio, MySqlConnection conn)
        {
            MySqlCommand comandoVerificarEmail = new MySqlCommand("SELECT CONVENIO FROM TB_USUARIO WHERE CONVENIO=@CONVENIO AND NUMEROCONVENIO=@NUMEROCONVENIO LIMIT 1", conn);
            comandoVerificarEmail.Parameters.AddWithValue("@CONVENIO", convenio);
            comandoVerificarEmail.Parameters.AddWithValue("@NUMEROCONVENIO", numeroConvenio);

            try
            {
                MySqlDataReader rdr = comandoVerificarEmail.ExecuteReader();

                while (rdr.Read())
                {
                    rdr.Close();

                    return "Usuário com Este Convênio já cadastrado.";
                }

                rdr.Close();
            }
            finally
            {

            }

            return string.Empty;
        }
    }
}