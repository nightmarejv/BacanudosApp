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
    public partial class FrmVendas : Form
    {
        public FrmVendas()
        {
            InitializeComponent();

        }

        private void FrmVendas_Load(object sender, EventArgs e)
        {
            CarregarClientes();
            CarregarProdutos();
            dgvItensVenda.DataSource = new DataTable(); // Começa vazio
            lblTotal.Text = "0,00";
        }

        private void CarregarClientes()
        {
            using (var conn = Banco.AbrirConexao())
            {
                string sql = "SELECT Id, Nome FROM Clientes";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    cmbClientes.DataSource = dt;
                    cmbClientes.DisplayMember = "Nome";
                    cmbClientes.ValueMember = "Id";
                }
            }
        }

        private void CarregarProdutos()
        {
            using (var conn = Banco.AbrirConexao())
            {
                string sql = "SELECT Id, Nome, Preco, Estoque FROM Produtos";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    cmbProdutos.DataSource = dt;
                    cmbProdutos.DisplayMember = "Nome";
                    cmbProdutos.ValueMember = "Id";
                }
            }
        }

        DataTable itensVenda;
        private void AtualizarTotal()
        {
            decimal total = 0;
            if (itensVenda != null)
            {
                foreach (DataRow row in itensVenda.Rows)
                {
                    total += (decimal)row["Subtotal"];
                }
            }
            lblTotal.Text = total.ToString("C");
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (itensVenda == null)
            {
                itensVenda = new DataTable();
                itensVenda.Columns.Add("ProdutoId", typeof(int));
                itensVenda.Columns.Add("Nome", typeof(string));
                itensVenda.Columns.Add("Quantidade", typeof(int));
                itensVenda.Columns.Add("PrecoUnitario", typeof(decimal));
                itensVenda.Columns.Add("Subtotal", typeof(decimal));
            }

            int produtoId = Convert.ToInt32(cmbProdutos.SelectedValue);
            string nome = cmbProdutos.Text;
            int quantidade;
            decimal precoUnitario;

            if (!int.TryParse(txtQuantidade.Text, out quantidade) || quantidade <= 0)
            {
                MessageBox.Show("Quantidade inválida!");
                return;
            }

            // Pega o preço e estoque do produto selecionado no combo
            DataRowView produtoSelecionado = (DataRowView)cmbProdutos.SelectedItem;
            precoUnitario = Convert.ToDecimal(produtoSelecionado["Preco"]);
            int estoque = Convert.ToInt32(produtoSelecionado["Estoque"]);

            if (quantidade > estoque)
            {
                MessageBox.Show("Quantidade maior que o estoque disponível!");
                return;
            }

            // Verifica se já adicionou esse produto, para somar quantidade
            DataRow[] linhasExistentes = itensVenda.Select("ProdutoId = " + produtoId);
            if (linhasExistentes.Length > 0)
            {
                DataRow linha = linhasExistentes[0];
                int novaQuantidade = (int)linha["Quantidade"] + quantidade;
                if (novaQuantidade > estoque)
                {
                    MessageBox.Show("Quantidade total excede o estoque disponível!");
                    return;
                }
                linha["Quantidade"] = novaQuantidade;
                linha["Subtotal"] = novaQuantidade * precoUnitario;
            }
            else
            {
                itensVenda.Rows.Add(produtoId, nome, quantidade, precoUnitario, quantidade * precoUnitario);
            }

            dgvItensVenda.DataSource = itensVenda;

            AtualizarTotal();

            txtQuantidade.Text = "1";
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (cmbClientes.SelectedValue == null)
            {
                MessageBox.Show("Selecione um cliente!");
                return;
            }

            if (itensVenda == null || itensVenda.Rows.Count == 0)
            {
                MessageBox.Show("Adicione produtos à venda!");
                return;
            }

            int clienteId = Convert.ToInt32(cmbClientes.SelectedValue);
            decimal totalVenda = 0;

            using (var conn = Banco.AbrirConexao())
            {
                var transacao = conn.BeginTransaction();

                try
                {
                    // Calcula total
                    foreach (DataRow row in itensVenda.Rows)
                        totalVenda += (decimal)row["Subtotal"];

                    // Insere na tabela Vendas
                    string sqlVenda = "INSERT INTO Vendas (ClienteId, DataVenda, Total) OUTPUT INSERTED.Id VALUES (@ClienteId, GETDATE(), @Total)";
                    int vendaId;
                    using (var cmd = new SqlCommand(sqlVenda, conn, transacao))
                    {
                        cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                        cmd.Parameters.AddWithValue("@Total", totalVenda);
                        vendaId = (int)cmd.ExecuteScalar();
                    }

                    // Insere ItensVenda e atualiza estoque
                    foreach (DataRow row in itensVenda.Rows)
                    {
                        int produtoId = (int)row["ProdutoId"];
                        int quantidade = (int)row["Quantidade"];
                        decimal precoUnitario = (decimal)row["PrecoUnitario"];

                        // Insere item
                        string sqlItem = "INSERT INTO ItensVenda (VendaId, ProdutoId, Quantidade, PrecoUnitario) VALUES (@VendaId, @ProdutoId, @Quantidade, @PrecoUnitario)";
                        using (var cmd = new SqlCommand(sqlItem, conn, transacao))
                        {
                            cmd.Parameters.AddWithValue("@VendaId", vendaId);
                            cmd.Parameters.AddWithValue("@ProdutoId", produtoId);
                            cmd.Parameters.AddWithValue("@Quantidade", quantidade);
                            cmd.Parameters.AddWithValue("@PrecoUnitario", precoUnitario);
                            cmd.ExecuteNonQuery();
                        }

                        // Atualiza estoque
                        string sqlEstoque = "UPDATE Produtos SET Estoque = Estoque - @Quantidade WHERE Id = @ProdutoId";
                        using (var cmd = new SqlCommand(sqlEstoque, conn, transacao))
                        {
                            cmd.Parameters.AddWithValue("@Quantidade", quantidade);
                            cmd.Parameters.AddWithValue("@ProdutoId", produtoId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transacao.Commit();
                    MessageBox.Show("Venda finalizada com sucesso!");

                    // Limpar tela
                    itensVenda.Clear();
                    dgvItensVenda.DataSource = itensVenda;
                    lblTotal.Text = "0,00";
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                    MessageBox.Show("Erro ao finalizar venda: " + ex.Message);
                }
            }
        }
    }

}
