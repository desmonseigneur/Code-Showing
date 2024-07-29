using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjectCompiler
{
    public partial class Form2 : Form
    {
        private const int ColumnCount = 17; // Define constant for column count
        private string selectedColumnName = "";
        private readonly Form1 form1Instance;
        public Form2(Form1 form1)
        {
            InitializeComponent();
            form1Instance = form1;
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            PopulateDataGridView();
            DBViewer.Columns["Id"].Visible = false;
        }
        private MySqlConnection GetConnection()
        {
            const string connstring = "server=localhost;port=3306;database=dmedb;uid=root;password=Edelwe!ss00;";
            var connection = new MySqlConnection(connstring);

            try
            {
                connection.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Error connecting to database: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
                return null;
            }

            return connection;
        }
        private void PopulateDataGridView()
        {
            using (var connection = GetConnection())
            {
                if (connection == null) return;

                const string query = "SELECT project_title AS 'Project/Program/Activity', project_location AS 'Location', " +
                                     "project_totalcost AS 'Total Cost', project_budget AS 'Approved Budget in Contract (ABC)', " +
                                     "date_notice AS 'Notice to Proceed', date_start AS 'Date Started', date_target AS 'Target Completion Date', " +
                                     "project_status AS 'Project Status (%)', project_incurred AS 'Total Cost Incurred to Date', " +
                                     "date_inspection AS 'Inspection Date', project_coordinator AS 'Project Coordinator', " +
                                     "project_source AS 'Source of Fund', project_contractor AS 'Contractor', project_encoder AS 'Encoder', " +
                                     "project_id AS 'Id' FROM dmedb.project_tb";

                var adapter = new MySqlDataAdapter(query, connection);
                var data = new DataTable();
                adapter.Fill(data);
                DBViewer.DataSource = data;
            }
        }
        private void UpdatePopulateDataGridView()
        {
            using (var connection = GetConnection())
            {
                if (connection == null) return;

                const string query = "SELECT project_year AS 'Project Year', project_title AS 'Project/Program/Activity', " +
                                     "project_location AS 'Location', project_totalcost AS 'Total Cost', project_budget AS 'Approved Budget in Contract (ABC)', " +
                                     "date_notice AS 'Notice to Proceed', date_start AS 'Date Started', date_days AS 'No. of Calendar Days', " +
                                     "date_extension AS 'No. of Extension', date_target AS 'Target Completion Date', project_status AS 'Project Status (%)', " +
                                     "project_incurred AS 'Total Cost Incurred to Date', project_photos AS 'Photos', date_inspection AS 'Inspection Date', " +
                                     "project_remarks AS 'Remarks', project_coordinator AS 'Project Coordinator', project_source AS 'Source of Fund', " +
                                     "project_contractor AS 'Contractor', project_encoder AS 'Encoder', project_id AS 'Id' FROM dmedb.project_tb";

                var adapter = new MySqlDataAdapter(query, connection);
                var populatedData = new DataTable();
                adapter.Fill(populatedData);
                DBViewer.DataSource = populatedData;

                // Hide the Encoder column
                DBViewer.Columns["Encoder"].Visible = false;
            }
        }
        private void SetColumnOrderAndVisibility()
        {
            var desiredOrder = new[]
            {
                "Project Year", "Project/Program/Activity", "Location", "Total Cost", "Approved Budget in Contract (ABC)", "Notice to Proceed",
                "Date Started", "No. of Calendar Days", "No. of Extension", "Target Completion", "Project Status (%)",
                "Total Cost Incurred to Date", "Photos", "Inspection Date", "Remarks", "Project Coordinator", "Source of Fund", "Contractor"
            };

            int newIndex = 0;
            foreach (var columnName in desiredOrder)
            {
                if (DBViewer.Columns.Contains(columnName))
                {
                    DBViewer.Columns[columnName].DisplayIndex = newIndex++;
                }
            }
            if (DBViewer.Columns.Contains("Encoder"))
            {
                DBViewer.Columns["Encoder"].Visible = false;
            }
        }
        private void FilterData(string selectedColumn, string searchText)
        {
            // Suspend binding to avoid CurrencyManager issues
            DBViewer.SuspendLayout();
            CurrencyManager manager = (CurrencyManager)BindingContext[DBViewer.DataSource];
            manager.SuspendBinding();

            foreach (DataGridViewRow row in DBViewer.Rows)
            {
                row.Visible = string.IsNullOrEmpty(searchText) ||
                              row.Cells[selectedColumn]?.Value?.ToString().IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
        }
        private void SearchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedColumnName = SearchCB.SelectedItem.ToString();
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            string searchText = SearchBox.Text.Trim();

            if (selectedColumnName == "Project Year" && !string.IsNullOrEmpty(searchText) && !int.TryParse(searchText, out _))
            {
                MessageBox.Show("Invalid year format! Please enter a 4-digit year (e.g., 20XX).");
                return;
            }
            FilterData(selectedColumnName, searchText);
        }
        private void Show_Click(object sender, EventArgs e)
        {
            UpdatePopulateDataGridView();
            SetColumnOrderAndVisibility();
            Show.Visible = false;
            Revert.Visible = true;
        }
        private void Revert_Click(object sender, EventArgs e)
        {
            PopulateDataGridView();
            Show.Visible = true;
            Revert.Visible = false;
        }
        private void DBViewer_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var selectedRow = DBViewer.Rows[e.RowIndex];
            var form1Instance = GetOrCreateForm1Instance();
            form1Instance.LoadRowData(selectedRow);
            form1Instance.Show();
            form1Instance.BringToFront();
            form1Instance.SetButtonsVisibility(true);
        }
        public void UpdateRow(int rowIndex, string[] newValues)
        {
            if (rowIndex < 0 || rowIndex >= DBViewer.Rows.Count || newValues.Length != ColumnCount) return;

            for (int i = 0; i < ColumnCount; i++)
            {
                DBViewer.Rows[rowIndex].Cells[i].Value = newValues[i];
            }
        }
        private Form1 GetOrCreateForm1Instance()
        {
            var form1 = Application.OpenForms.OfType<Form1>().FirstOrDefault() ?? new Form1();
            return form1;
        }
    }
}
