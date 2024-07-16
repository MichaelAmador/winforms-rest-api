using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Todo.Entities;
using Newtonsoft.Json.Linq;
using System.Security.Policy;

namespace Todo
{
    public partial class RestSharp : Form
    {
        public RestSharp()
        {
            InitializeComponent();
        }

        private void RestSharp_Shown(object sender, EventArgs e)
        {
            CreateColumns();
            FormatDGV();
            getTodos();
            getCategories();
        }

        private void RestSharp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) getTodos();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtId.Text = "0";
            txtDescription.Text = "";
            cbDone.Checked = false;
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            if (!IsDataValid()) return;
            if (txtId.Text == "0") post();
            else Put();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsDataValid()) return;
            updateMany();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvData.Rows.Count > 0 && dgvData.SelectedRows.Count > 0)
            {
                txtId.Text = dgvData.CurrentRow.Cells["id"].Value.ToString();
                txtDescription.Text = dgvData.CurrentRow.Cells["description"].Value.ToString();
                cbDone.Checked = Convert.ToBoolean(dgvData.CurrentRow.Cells["done"].Value);
                cmbCategory.Text = dgvData.CurrentRow.Cells["category"].Value.ToString();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvData.Rows.Count > 0 && dgvData.SelectedRows.Count > 0)
            {
                Delete(Convert.ToInt32(dgvData.CurrentRow.Cells["id"].Value));
            }
        }

        private void dgvData_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) => RightClickMouseDgv(ref dgvData, e);

        #region RestSharp 
        //HttpClient client = new HttpClient();
        string api = "http://localhost:3000";

        async void getCategories()
        {
            var options = new RestClientOptions(api);
            var client = new RestClient(options);
            try
            {
                var request = new RestRequest("category", Method.Get);
                var response = await client.GetAsync(request);

                if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());

                var categories = JsonConvert.DeserializeObject<List<Category>>(response.Content);
                cmbCategory.DataSource = categories;
                cmbCategory.ValueMember = "idCategory";
                cmbCategory.DisplayMember = "name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void getTodos()
        {
            var options = new RestClientOptions(api);
            var client = new RestClient(options);
            try
            {
                var response = await client.GetAsync(new RestRequest("todo", Method.Get));
                var todos = JsonConvert.DeserializeObject(response.Content);
                var data = JArray.Parse(response.Content);

                dgvData.Rows.Clear();

                foreach (var item in data)
                    dgvData.Rows.Add(item["id"], item["description"], item["done"], item["category"]["idcategory"], item["category"]["name"]);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void post()
        {
            var options = new RestClientOptions(api);
            var client = new RestClient(options);
            try
            {
                var body = new
                {
                    description = txtDescription.Text,
                    done = cbDone.Checked,
                    idCategory = Convert.ToInt32(cmbCategory.SelectedValue),
                };

                var response = await client.PostAsync(new RestRequest("todo", Method.Post).AddJsonBody(body));
                if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());
                MessageBox.Show("Saved");
                Clear();
                getTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void Delete(int id)
        {
            if (MessageBox.Show("Are you sure you want to delete this?", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            var options = new RestClientOptions(api);
            var client = new RestClient(options);
            try
            {
                var response = await client.DeleteAsync(new RestRequest($"todo/{id}"));
                if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());
                MessageBox.Show("Deleted");
                getTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void Put()
        {
            if (MessageBox.Show("Are you sure you want to update this?", "Update", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            var options = new RestClientOptions(api);
            var client = new RestClient(options);
            try
            {
                var body = new
                {
                    description = txtDescription.Text,
                    done = cbDone.Checked,
                    idCategory = Convert.ToInt32(cmbCategory.SelectedValue),
                };

                var response = await client.PutAsync(new RestRequest($"todo/{txtId.Text}").AddJsonBody(body));
                if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());
                MessageBox.Show("Updated");
                Clear();
                getTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void updateMany()
        {
            if (MessageBox.Show("Are you sure you want to update this?", "Update", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            var options = new RestClientOptions(api);
            var client = new RestClient(options);
            try
            {
                var body = new Tasks[]
                {
                    new Tasks{
                        id = Convert.ToInt32(txtId.Text),
                        description = txtDescription.Text,
                        done = Convert.ToBoolean(cbDone.Checked),
                        idCategory = Convert.ToInt32(cmbCategory.SelectedValue),
                    },
                    new Tasks{
                        id = Convert.ToInt32(txtId.Text),
                        description = txtDescription.Text + "-2",
                        done = Convert.ToBoolean(!cbDone.Checked),
                        idCategory = Convert.ToInt32(cmbCategory.SelectedValue),
                    },
                };

                //var response = await client.PostAsync(new RestRequest($"todo/many").AddJsonBody(body));
                var response = await client.PostAsync(new RestRequest($"todo/many").AddJsonBody(body));
                if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode.ToString());
                MessageBox.Show("Created many");
                Clear();
                getTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Methods

        bool IsDataValid()
        {
            EP.Clear();
            if (txtDescription.Text.Trim() == "")
            {
                EP.SetError(txtDescription, "Required");
                return false;
            }
            return true;
        }

        void Clear()
        {
            txtId.Text = "0";
            txtDescription.Text = "";
            cbDone.Checked = false;
        }

        void CreateColumns()
        {
            dgvData.Columns.Add("id", "ID");
            dgvData.Columns.Add("description", "Description");
            dgvData.Columns.Add(new DataGridViewCheckBoxColumn() { Name = "done", HeaderText = "Done" });
            dgvData.Columns.Add("idcategory", "idCategory");
            dgvData.Columns.Add("category", "Category");
        }

        void FormatDGV()
        {
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.Columns["idcategory"].Visible = false;
        }

        public static void RightClickMouseDgv(ref DataGridView dgv, DataGridViewCellMouseEventArgs e)
        {
            if (dgv.Rows.Count > 0 && e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                dgv.Rows[e.RowIndex].Selected = true;
            }
        }

        void HandleHttpError(HttpStatusCode code)
        {
            if (code == HttpStatusCode.NotFound) throw new Exception("Not Found");
            if (code == HttpStatusCode.InternalServerError) throw new Exception("Server error");
            if (code == HttpStatusCode.NotImplemented) throw new Exception("Not Found");
            if (code == HttpStatusCode.NotFound) throw new Exception("Not Found");
            throw new Exception("Something went wrong");
        }

        #endregion

    }
}
