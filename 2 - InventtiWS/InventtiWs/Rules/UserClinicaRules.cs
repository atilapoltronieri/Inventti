using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrConselhosWs.Rules
{
    public class UserClinicaRules
    {
        public static string VerificaUsuarioClinicaCadastro(string crm, MySqlConnection conn)
        {
            string permissaoCRM = PermissaoUsuarioClinica.VerificaPermissaoCRM(crm, conn);
            if (!string.IsNullOrEmpty(permissaoCRM))
                return permissaoCRM;

            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

            MySqlCommand comandoVerificarCrm = new MySqlCommand("SELECT CRM FROM TB_USUARIO_CLINICA WHERE CRM=@CRM LIMIT 1", conn);
            comandoVerificarCrm.Parameters.AddWithValue("@CRM", crm);
            try
            {
                MySqlDataReader rdr = comandoVerificarCrm.ExecuteReader();

                if (rdr.Read())
                {
                    rdr.Close();
                    return "CRM já Cadastrado.";
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