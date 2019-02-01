using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NexxeraWs.Models;

namespace NexxeraWs.Controllers
{
    public class FeriadoController
    {
        public static List<DateTime> GetFeriadosAnuais(MySqlConnection conn, int anoInicial, int anoFinal, int? idCidade = null)
        {
            var listaFeriadosAnuais = new List<DateTime>();

            if (idCidade == 0)
                idCidade = null;

            var listaFeriados = GetFeriados(conn, idCidade);

            for (int ano = anoInicial; ano <= anoFinal; ano++)
            {
                foreach (FeriadoModels feriado in listaFeriados)
                {
                    feriado.Ano = ano;
                    listaFeriadosAnuais.Add(feriado.Data);
                }
            }

            return listaFeriadosAnuais;
        }

        private static List<FeriadoModels> GetFeriados(MySqlConnection conn, int? idCidade)
        {
            var listaFeriados = new List<FeriadoModels>();

            MySqlCommand comandoCarregaFeriados = new MySqlCommand("SELECT FB.ID, FB.ID_CIDADE, FB.DIA, FB.MES, FB.NOME, FB.NACIONAL " +
                "FROM TB_FERIADO FB WHERE FB.NACIONAL=TRUE ", conn);

            if (idCidade > 0)
                comandoCarregaFeriados.CommandText += "OR FB.ID_CIDADE = @ID_CIDADE ";

            comandoCarregaFeriados.CommandText += "ORDER BY FB.MES, FB.DIA";

            comandoCarregaFeriados.Parameters.AddWithValue("@ID_CIDADE", idCidade);
            try
            {
                MySqlDataReader rdr = comandoCarregaFeriados.ExecuteReader();

                while (rdr.Read())
                {
                    var feriado = new FeriadoModels();
                    feriado.Id = Convert.ToInt32(rdr[0].ToString());
                    feriado.Dia = Convert.ToInt32(rdr[2].ToString());
                    feriado.Mes = Convert.ToInt32(rdr[3].ToString());
                    feriado.Nome = rdr[4].ToString();
                    feriado.Nacional = (bool)rdr[5];

                    if (!string.IsNullOrEmpty(rdr[1].ToString()))
                        feriado.Id_Cidade = (int)rdr[1];

                    listaFeriados.Add(feriado);
                }

                rdr.Close();
            }
            catch (Exception e)
            {

            }

            return listaFeriados;
        }
    }
}