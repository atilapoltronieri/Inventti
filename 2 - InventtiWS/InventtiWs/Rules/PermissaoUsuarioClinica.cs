using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrConselhosWs.Rules
{
    public class PermissaoUsuarioClinica
    {
        public static string VerificaPermissaoCRM(string crm, MySqlConnection conn)
        {
            MySqlCommand comandoVerificarPermissaoCrm = new MySqlCommand("SELECT CRM FROM TB_PERMISSAO_USUARIO_CLINICA WHERE CRM=@CRM LIMIT 1", conn);
            comandoVerificarPermissaoCrm.Parameters.AddWithValue("@CRM", crm);
            try
            {
                MySqlDataReader rdr = comandoVerificarPermissaoCrm.ExecuteReader();

                if (!rdr.Read())
                {
                    rdr.Close();
                    return "CRM sem Permissão para Cadastro.";
                }

                rdr.Close();
            }
            finally
            {
            }

            return string.Empty;
        }

        public static string DeletaCRM(string crm, MySqlConnection conn)
        {
            MySqlCommand comandoVerificarPermissaoCrm = new MySqlCommand("DELETE FROM TB_PERMISSAO_USUARIO_CLINICA WHERE CRM=@CRM", conn);
            comandoVerificarPermissaoCrm.Parameters.AddWithValue("@CRM", crm);
            try
            {
                var exec = comandoVerificarPermissaoCrm.ExecuteNonQuery();

                if (exec == 0)
                {
                    return "CRM Inexistente.";
                }
            }
            finally
            {
            }

            return string.Empty;
        }
    }
}