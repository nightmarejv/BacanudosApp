using BacanudosApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BacanudosApp
{
    public partial class FrmProdutos : Form
    {
        public FrmProdutos()
        {
            InitializeComponent();
        }
        private void CarregarProdutos()
        {
            try
            {
                using (var conn = Banco.AbrirConexao())
                {
                    string sql = "SELECT * FROM Produtos";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        var tabela = new DataTable();
                        tabela.Load(cmd.ExecuteReader());
                        dgvProdutos.DataSource = tabela;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar produtos: " + ex.Message);
            }
        }
        private void dgvProdutos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtNome.Text = dgvProdutos.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                txtPreco.Text = dgvProdutos.Rows[e.RowIndex].Cells["Preco"].Value.ToString();
                txtEstoque.Text = dgvProdutos.Rows[e.RowIndex].Cells["Estoque"].Value.ToString();

                btnEditar.Tag = dgvProdutos.Rows[e.RowIndex].Cells["Id"].Value;
                btnExcluir.Tag = dgvProdutos.Rows[e.RowIndex].Cells["Id"].Value;
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            using (var conn = Banco.AbrirConexao())
            {
                string sql = "INSERT INTO Produtos (Nome, Preco, Estoque) VALUES (@Nome, @Preco, @Estoque)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("@Preco", decimal.Parse(txtPreco.Text));
                    cmd.Parameters.AddWithValue("@Estoque", int.Parse(txtEstoque.Text));
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Produto cadastrado!");
            CarregarProdutos();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (btnEditar.Tag == null)
            {
                MessageBox.Show("Selecione um produto para editar.");
                return;
            }

            int id = Convert.ToInt32(btnEditar.Tag);

            using (var conn = Banco.AbrirConexao())
            {
                string sql = "UPDATE Produtos SET Nome = @Nome, Preco = @Preco, Estoque = @Estoque WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("@Preco", decimal.Parse(txtPreco.Text));
                    cmd.Parameters.AddWithValue("@Estoque", int.Parse(txtEstoque.Text));
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Produto editado!");
            CarregarProdutos();
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (btnExcluir.Tag == null)
            {
                MessageBox.Show("Selecione um produto para excluir.");
                return;
            }

            int id = Convert.ToInt32(btnExcluir.Tag);

            using (var conn = Banco.AbrirConexao())
            {
                string sql = "DELETE FROM Produtos WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Produto excluído!");
            CarregarProdutos();
        }
    }
}
