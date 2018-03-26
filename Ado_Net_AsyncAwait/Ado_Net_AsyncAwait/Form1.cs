using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ado_Net_AsyncAwait
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=CR5-00\SQLEXPRESS;Initial Catalog=Library; Integrated Security=true;";
        SqlConnection sqlConnection = null;
        DataTable dataTable = null;
        public Form1()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if(open.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Before async call");
                await GetDataAsync(open.FileName);
                MessageBox.Show("After asyc call");
                
            }
        }

        async private Task GetDataAsync(string fileName)
        {
            byte[] data = null;
            using (FileStream fs = File.Open(fileName, FileMode.Open))
            {
                data = new byte[fs.Length];
                await fs.ReadAsync(data, 0, (int)fs.Length);
            }
            textBox1.Text = Encoding.Default.GetString(data);

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            using(sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();
                string sql = "waitfor delay '00:00:10'";
                sql += textBox2.Text;
                SqlCommand sqlCommand = new SqlCommand(sql,sqlConnection);
                dataTable = new DataTable();
                SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();

                int line = 0;
                do
                {
                    while (await sqlDataReader.ReadAsync())
                    {
                        if(line++ == 0)
                        {
                            for (int i = 0; i < sqlDataReader.FieldCount; i++)
                            {
                                dataTable.Columns.Add(sqlDataReader.GetName(i));
                            }
                        }

                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        {
                            dataRow[i] = await sqlDataReader.GetFieldValueAsync<Object>(i);
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                } while (await sqlDataReader.NextResultAsync());
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dataTable;
            }
        }
    }
}
