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
    public partial class FrmClientes : Form
    {
        public FrmClientes()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void CarregarClientes()
        {
            try
            {
                using (var conn = Banco.AbrirConexao())
                {
                    string sql = "SELECT * FROM Clientes";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        var reader = cmd.ExecuteReader();
                        var tabela = new DataTable();
                        tabela.Load(reader);
                        dgvClientes.DataSource = tabela;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar clientes: " + ex.Message);
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = Banco.AbrirConexao())
                {
                    string sql = "INSERT INTO Clientes (Nome, Email, Telefone) VALUES (@Nome, @Email, @Telefone)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nome", txtNome.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Telefone", txtTelefone.Text);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Cliente salvo com sucesso!");
                CarregarClientes(); // Recarrega a lista no DataGridView
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }

        private void dgvClientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtNome.Text = dgvClientes.Rows[e.RowIndex].Cells["Nome"].Value.ToString();
                txtEmail.Text = dgvClientes.Rows[e.RowIndex].Cells["Email"].Value.ToString();
                txtTelefone.Text = dgvClientes.Rows[e.RowIndex].Cells["Telefone"].Value.ToString();

                // Salva o ID selecionado no Tag do botão Editar (pode ser em outra variável também)
                btnEditar.Tag = dgvClientes.Rows[e.RowIndex].Cells["Id"].Value;
                btnExcluir.Tag = dgvClientes.Rows[e.RowIndex].Cells["Id"].Value;
            }
        }

        private void FrmClientes_Load(object sender, EventArgs e)
        {
            CarregarClientes();

        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (btnEditar.Tag == null)
            {
                MessageBox.Show("Selecione um cliente para editar.");
                return;
            }

            int id = Convert.ToInt32(btnEditar.Tag);

            using (var conn = Banco.AbrirConexao())
            {
                string sql = "UPDATE Clientes SET Nome = @Nome, Email = @Email, Telefone = @Telefone WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Telefone", txtTelefone.Text);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Cliente atualizado com sucesso!");
            CarregarClientes();

            // Limpar campos
            txtNome.Clear();
            txtEmail.Clear();
            txtTelefone.Clear();
            btnEditar.Tag = null;
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (btnExcluir.Tag == null)
            {
                MessageBox.Show("Selecione um cliente para excluir.");
                return;
            }

            int id = Convert.ToInt32(btnExcluir.Tag);

            var confirmar = MessageBox.Show("Tem certeza que deseja excluir este cliente?", "Confirmar", MessageBoxButtons.YesNo);
            if (confirmar == DialogResult.Yes)
            {
                using (var conn = Banco.AbrirConexao())
                {
                    string sql = "DELETE FROM Clientes WHERE Id = @Id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Cliente excluído com sucesso!");
                CarregarClientes();

                // Limpar campos
                txtNome.Clear();
                txtEmail.Clear();
                txtTelefone.Clear();
                btnExcluir.Tag = null;
            }
        }
    }
}
