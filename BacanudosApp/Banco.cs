using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

namespace BacanudosApp.Data
{
    public static class Banco
    {
        public static SqlConnection AbrirConexao()
        {
            SqlConnection conexao = new SqlConnection(
            ConfigurationManager.ConnectionStrings["MinhaConexao"].ConnectionString);
            conexao.Open();
            return conexao;
        }
    }
}
