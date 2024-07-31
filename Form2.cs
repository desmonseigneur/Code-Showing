using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjectCompiler
{
    public partial class Form2 : Form
    {
        private const int ColumnCount = 17;
        private string selectedColumnName = string.Empty;
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

                const string query = "SELECT project_year AS 'Project Year', project_title AS 'Project/Program/Activity', project_location AS 'Location', " +
                                     "project_totalcost AS 'Total Cost', project_budget AS 'Approved Budget in Contract (ABC)', " +
                                     "date_notice AS 'Notice to Proceed', date_start AS 'Date Started', date_target AS 'Target Completion Date', " +
                                     "project_status AS 'Project Status (%)', project_incurred AS 'Total Cost Incurred to Date', " +
                                     "date_inspection AS 'Inspection Date', project_coordinator AS 'Project Coordinator', " +
                                     "project_source AS 'Source of Fund', project_contractor AS 'Contractor', project_encoder AS 'Encoder', " +
                                     "project_id AS 'Id' FROM dmedb.project_tb";
                
                var adapter = new MySqlDataAdapter(query, connection);
                var data = new DataTable();
                adapter.Fill(data);
                DBViewer.DataBindingComplete += (s, e) =>
                {
                    var column = DBViewer.Columns["Project Year"];
                    var column2 = DBViewer.Columns["Encoder"];
                    if (column != null)
                    {
                        column.Visible = false;
                    }
                    if (column2 != null)
                    {
                        column2.Visible = true;
                    }
                };
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
                DBViewer.DataBindingComplete += (s, e) =>
                {
                    var column = DBViewer.Columns["Project Year"];
                    var column2 = DBViewer.Columns["Encoder"];
                    if (column != null)
                    {
                        column.Visible = true;
                    }
                    if (column2 != null)
                    {
                        column2.Visible = false;
                    }
                };
                DBViewer.DataSource = populatedData;                
            }
        }
        private void SearchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedColumnName = SearchCB.SelectedItem.ToString();
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string searchText = SearchBox.Text.Trim();
                if (selectedColumnName == "Project Year" && !IsValidYear(searchText))
                {
                    MessageBox.Show("Invalid year format! Please enter a 4-digit year (e.g., 20XX).");
                    return;
                }
                FilterData(selectedColumnName, searchText);
            }
        }
        private bool IsValidYear(string year)
        {
            var yearRegex = new Regex(@"^\d{4}$");
            return yearRegex.IsMatch(year);
        }
        private void FilterData(string columnName, string searchText)
        {
            DBViewer.SuspendLayout();
            var manager = (CurrencyManager)BindingContext[DBViewer.DataSource];
            manager.SuspendBinding();
            foreach (DataGridViewRow row in DBViewer.Rows)
            {
                row.Visible = string.IsNullOrEmpty(searchText) ||
                              row.Cells[columnName]?.Value?.ToString()
                                  .IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            manager.ResumeBinding();
            DBViewer.ResumeLayout();
        }
        private void Show_Click(object sender, EventArgs e)
        {
            UpdatePopulateDataGridView();
            ReorderColumns();
            Show.Visible = false;
            Revert.Visible = true;
        }
        private void ReorderColumns()
        {
            var desiredOrder = new List<string>
            {
                "Project Year", "Project/Program/Activity", "Location", "Total Cost", "Approved Budget in Contract (ABC)",
                "Notice to Proceed", "Date Started", "No. of Calendar Days", "No. of Extension", "Target Completion Date",
                "Project Status (%)", "Total Cost Incurred to Date", "Photos", "Inspection Date", "Remarks",
                "Project Coordinator", "Source of Fund", "Contractor"
            };
            int newIndex = 0;
            foreach (var columnName in desiredOrder)
            {
                if (DBViewer.Columns.Contains(columnName))
                {
                    DBViewer.Columns[columnName].DisplayIndex = newIndex;
                    newIndex++;
                }
            }
        }
        private void Revert_Click(object sender, EventArgs e)
        {
            PopulateDataGridView();
            Show.Visible = true;
            Revert.Visible = false;
        }
        private void DBViewer_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var selectedRow = DBViewer.Rows[e.RowIndex];

                var id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                var title = selectedRow.Cells["Project/Program/Activity"].Value.ToString();
                var location = selectedRow.Cells["Location"].Value.ToString();
                var totalCost = Convert.ToDecimal(selectedRow.Cells["Total Cost"].Value);
                var budget = Convert.ToDecimal(selectedRow.Cells["Approved Budget in Contract (ABC)"].Value);
                var notice = Convert.ToDateTime(selectedRow.Cells["Notice to Proceed"].Value);
                var start = Convert.ToDateTime(selectedRow.Cells["Date Started"].Value);
                var target = Convert.ToDateTime(selectedRow.Cells["Target Completion Date"].Value);
                var calendar = selectedRow.Cells["No. of Calendar Days"].Value.ToString();
                var extension = selectedRow.Cells["No. of Extension"].Value.ToString();
                var status = Convert.ToInt32(selectedRow.Cells["Project Status (%)"].Value);
                var incurred = Convert.ToDecimal(selectedRow.Cells["Total Cost Incurred to Date"].Value);
                var inspect = Convert.ToDateTime(selectedRow.Cells["Inspection Date"].Value);
                var remarks = selectedRow.Cells["Remarks"].Value.ToString();
                var coordinator = selectedRow.Cells["Project Coordinator"].Value.ToString();
                var source = selectedRow.Cells["Source of Fund"].Value.ToString();
                var contractor = selectedRow.Cells["Contractor"].Value.ToString();
                var encoder = selectedRow.Cells["Encoder"].Value.ToString();

                Form1 form1Instance = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                if (form1Instance == null)
                {
                    form1Instance = new Form1();
                }

                form1Instance.SetProjectData(title, location, totalCost, budget, notice, start, target, calendar, extension, status, incurred, inspect, remarks, coordinator, source, contractor, encoder, id);

                form1Instance.SetReadOnlyState(true);
                form1Instance.LastCreatedFolderPath = selectedRow.Cells["FolderPath"].Value?.ToString(); // Load existing folder path if any
                form1Instance.SetButtonsVisibility(true);
                form1Instance.Show();
                form1Instance.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Transfer_Click(object sender, EventArgs e)
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;

            // Get the path to the executable directory
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string templatePath = Path.Combine(exePath, "Document.xlsx");

            var workbook = excelApp.Workbooks.Open(templatePath);
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

            // Start at row 3
            int startRow = 3;
            int excelColumnIndex = 1;

            // Transfer data from DBViewer to Excel, skipping 'Id' and 'Encoder' columns
            for (int j = 0; j < DBViewer.Columns.Count; j++)
            {
                if (DBViewer.Columns[j].Name != "Id" && DBViewer.Columns[j].Name != "Encoder")
                {
                    worksheet.Cells[startRow - 1, excelColumnIndex] = DBViewer.Columns[j].HeaderText; // Set headers on row 2 (one row above data)
                    int excelRowIndex = startRow; // Start data on row 3
                    foreach (DataGridViewRow row in DBViewer.Rows)
                    {
                        if (row.Visible)
                        {
                            worksheet.Cells[excelRowIndex, excelColumnIndex] = row.Cells[j].Value?.ToString();
                            excelRowIndex++;
                        }
                    }
                    excelColumnIndex++;
                }
            }

            // Get the Downloads folder path
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            // Generate a unique file name to avoid overwriting
            string baseFileName = "SavedDocument";
            string fileExtension = ".xlsx";
            string savePath = Path.Combine(downloadsPath, baseFileName + fileExtension);
            int fileIndex = 1;

            while (File.Exists(savePath))
            {
                savePath = Path.Combine(downloadsPath, baseFileName + fileIndex.ToString() + fileExtension);
                fileIndex++;
            }

            // Save the workbook
            workbook.SaveAs(savePath);
            workbook.Close();
            excelApp.Quit();

            MessageBox.Show("Data transferred to Excel successfully and saved in Downloads as " + Path.GetFileName(savePath));
        }
    }
}
