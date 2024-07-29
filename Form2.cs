using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.RegularExpressions;
using ProjectCompiler;

namespace ProjectCompiler
{
    public partial class Form2 : Form
    {
        private string selectedColumnName = ""; // Store selected column name
        private string previousSearchText = ""; // Store previous search text
        private Form1 form1Instance;
        public Form2(Form1 form1)
        {
            InitializeComponent();
            form1Instance = form1;            
        }
        public Form2()
        {
            InitializeComponent();
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            PopulateDataGridView();
        }
        private MySqlConnection GetConnection()
        {
            string connstring = "server=localhost;port=3306;database=dmedb;uid=root;password=Edelwe!ss00;";
            try
            {
                MySqlConnection connection = new MySqlConnection(connstring);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                return connection;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error connecting to database: " + ex.Message);
                return null; // Indicate connection failure
            }
        }
        private void PopulateDataGridView()
        {
            MySqlConnection connection = GetConnection(); // Call GetConnection to get connection
            try
            {
                if (connection != null)
                {
                    string query = "SELECT project_title AS 'Project/Program/Activity', project_location AS 'Location', project_totalcost AS 'Total Cost', project_budget AS 'Approved Budget in Contract (ABC)', date_notice AS 'Notice to Proceed', date_start AS 'Date Started', date_target AS 'Target Completion Date', project_status AS 'Project Status (%)', project_incurred AS 'Total Cost Incurred to Date', date_inspection AS 'Inspection Date', project_coordinator AS 'Project Coordinator', project_source AS 'Source of Fund', project_contractor AS 'Contractor', project_encoder AS 'Encoder' FROM dmedb.project_tb"; // Modify if needed
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);

                    DataTable data = new DataTable();
                    adapter.Fill(data);

                    DBViewer.DataSource = data;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error retrieving data: " + ex.Message);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }
        private void UpdatePopulateDataGridView()
        {
            MySqlConnection connection = GetConnection(); // Call GetConnection to get connection
            try
            {
                if (connection != null)
                {
                    string query = "SELECT project_year AS 'Project Year', project_title AS 'Project/Program/Activity', project_location AS 'Location', project_totalcost AS 'Total Cost', project_budget AS 'Approved Budget in Contract (ABC)', date_notice AS 'Notice to Proceed', date_start AS 'Date Started', date_days AS 'No. of Calendar Days', date_extension AS 'No. of Extension', date_target AS 'Target Completion Date', project_status AS 'Project Status (%)', project_incurred AS 'Total Cost Incurred to Date', project_photos AS 'Photos', date_inspection AS 'Inspection Date', project_remarks AS 'Remarks', project_coordinator AS 'Project Coordinator', project_source AS 'Source of Fund', project_contractor AS 'Contractor', project_encoder AS 'Encoder' FROM dmedb.project_tb"; // Modify if needed
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);

                    DataTable populatedData = new DataTable(); // Rename data to populatedData or dataTable
                    adapter.Fill(populatedData);

                    DBViewer.DataSource = populatedData;

                    // Hide the Encoder column
                    DBViewer.Columns["Encoder"].Visible = false;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error retrieving data: " + ex.Message);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }
        private void SearchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedColumnName = SearchCB.SelectedItem.ToString(); // Update selected column name
        }
        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (SearchBox.Text != previousSearchText)
            {
                previousSearchText = SearchBox.Text;
            }
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Console.WriteLine(selectedColumnName);                
                string searchText = SearchBox.Text.Trim(); // Trim leading/trailing whitespaces from search text

                if (string.IsNullOrEmpty(searchText) && selectedColumnName == "Project Year") // Handle empty search text specifically when "Project Year" is selected
                {                    
                    searchText = ""; // Treat empty search text for "Project Year" as showing all data
                }
                else if (selectedColumnName == "Project Year")
                {                    
                    Regex yearRegex = new Regex(@"^\d{4}$"); // Validate year format (YYYY) using regular expression
                    if (!yearRegex.IsMatch(searchText))
                    {
                        MessageBox.Show("Invalid year format! Please enter a 4-digit year (e.g., 20XX).");
                        return; // Exit the function if validation fails
                    }
                }
                FilterData(selectedColumnName, searchText);
            }
        }
        private void Show_Click(object sender, EventArgs e)
        {
            UpdatePopulateDataGridView();

            DataGridViewColumnCollection columns = DBViewer.Columns; // Get a reference to the DataGridView's Columns collection 

            List<string> desiredOrder = new List<string>() // Define the desired column order (list can be adjusted based on your needs)  
    {
        "Project Year", "Project/Program/Activity", "Location", "Total Cost", "Approved Budget in Contract (ABC)", "Notice to Proceed", "Date Started", "No. of Calendar Days", "No. of Extension", "Target Completion", "Project Status (%)", "Total Cost Incurred to Date", "Photos", "Inspection Date", "Remarks", "Project Coordinator", "Source of Fund", "Contractor"
    };

            int newIndex = 0;
            foreach (string columnName in desiredOrder) // Re-arrange columns based on the desired order  
            {
                if (columns.Contains(columnName))
                {
                    columns[columnName].DisplayIndex = newIndex;
                }
                newIndex++;
            }

            // Hide the Encoder column
            if (columns.Contains("Encoder"))
            {
                columns["Encoder"].Visible = false;
            }

            Show.Visible = false; // Make Show button invisible  
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
            try
            {
                DataGridViewRow selectedRow = DBViewer.Rows[e.RowIndex];

                // Ensure that each cell has a valid value before conversion
                form1Instance.Encoder = selectedRow.Cells["Encoder"].Value?.ToString() ?? "N/A";
                form1Instance.Title = selectedRow.Cells["Project/Program/Activity"].Value?.ToString() ?? "N/A";
                form1Instance.Location = selectedRow.Cells["Location"].Value?.ToString() ?? "N/A";
                form1Instance.TotalCost = Convert.ToDecimal(selectedRow.Cells["Total Cost"].Value ?? 0);
                form1Instance.Budget = Convert.ToDecimal(selectedRow.Cells["Approved Budget in Contract (ABC)"].Value ?? 0);
                form1Instance.Notice = Convert.ToDateTime(selectedRow.Cells["Notice to Proceed"].Value ?? DateTime.Now);
                form1Instance.Start = Convert.ToDateTime(selectedRow.Cells["Date Started"].Value ?? DateTime.Now);
                form1Instance.Target = Convert.ToDateTime(selectedRow.Cells["Target Completion Date"].Value ?? DateTime.Now);
                form1Instance.Calendar = selectedRow.Cells["No. of Calendar Days"].Value?.ToString() ?? "0";
                form1Instance.Extension = selectedRow.Cells["No. of Extension"].Value?.ToString() ?? "0";
                form1Instance.Status = Convert.ToInt32(selectedRow.Cells["Project Status (%)"].Value ?? 0);
                form1Instance.Incurred = Convert.ToDecimal(selectedRow.Cells["Total Cost Incurred to Date"].Value ?? 0);
                form1Instance.Inspect = Convert.ToDateTime(selectedRow.Cells["Inspection Date"].Value ?? DateTime.Now);
                form1Instance.Remarks = selectedRow.Cells["Remarks"].Value?.ToString() ?? "N/A";
                form1Instance.Coordinator = selectedRow.Cells["Project Coordinator"].Value?.ToString() ?? "N/A";
                form1Instance.Source = selectedRow.Cells["Source of Fund"].Value?.ToString() ?? "N/A";
                form1Instance.Contractor = selectedRow.Cells["Contractor"].Value?.ToString() ?? "N/A";

                // Set Form1 controls to read-only after populating them
                form1Instance.SetReadOnlyState(true);

                form1Instance.Show(); // Bring Form1 to the front
                form1Instance.BringToFront();
            }
            catch (Exception ex)
            {
                // Handle exceptions and provide feedback
                MessageBox.Show("Please click 'Show Full Table' first before transferring data");
            }
        }
        private void FilterData(string selectedColumn, string searchText)
        {
            // Suspend binding to avoid CurrencyManager issues
            DBViewer.SuspendLayout();
            CurrencyManager manager = (CurrencyManager)BindingContext[DBViewer.DataSource];
            manager.SuspendBinding();

            DataTable data = (DataTable)DBViewer.DataSource;

            if (!string.IsNullOrEmpty(SearchBox.Text.Trim())) // Use SearchBox.Text instead of searchText
            {                
                foreach (DataGridViewRow row in DBViewer.Rows) // Filter by selected column (existing logic)
                {
                    if (!string.IsNullOrEmpty(SearchBox.Text.Trim())) // Use SearchBox.Text instead of searchText
                    {
                        try // Wrap filtering logic in a try block
                        {
                            if (DBViewer.Columns.Contains(selectedColumn)) // Check if column exists
                            {
                                string cellValue = row.Cells[selectedColumn].Value?.ToString().ToLowerInvariant() ?? "";

                                if (selectedColumn == "Project Status (%)" || selectedColumn == "Project/Activity/Title" || selectedColumn == "Project Year")
                                {                                    
                                    if (!cellValue.StartsWith(SearchBox.Text.Trim().ToLowerInvariant())) // Exact match starting from the first number for project status
                                    {
                                        row.Visible = false;
                                        continue;
                                    }
                                }
                                else if (selectedColumn.Contains("Notice to Proceed") || selectedColumn.Contains("Date Started") || selectedColumn.Contains("Target Completion Date") || selectedColumn.Contains("Inspection Date"))
                                {
                                    DateTime dateValue;
                                    if (DateTime.TryParse(row.Cells[selectedColumn].Value?.ToString(), out dateValue))
                                    {
                                        int searchMonthValue;                                        
                                        if (int.TryParse(SearchBox.Text.Trim(), out searchMonthValue) && searchMonthValue >= 1 && searchMonthValue <= 12) // Try parsing search text as a number (month)
                                        {                                            
                                            int monthValue = dateValue.Month; // Search by month value
                                            if (monthValue != searchMonthValue)
                                            {
                                                row.Visible = false;
                                                continue;
                                            }
                                        }
                                        else
                                        {                                            
                                            if (!dateValue.ToString("MMMM").ToLowerInvariant().Contains(SearchBox.Text.Trim().ToLowerInvariant())) // Search by month name (assuming search text is not a valid number)
                                            {
                                                row.Visible = false;
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {                                                                                
                                        Console.WriteLine($"Error parsing date in column '{selectedColumn}' for row: {row.Index}"); // Handle invalid date format
                                        MessageBox.Show($"Invalid date format in '{selectedColumn}' for row {row.Index + 1}. Please enter a valid date format (e.g., YYYY-MM-DD).");
                                    }
                                }
                                else
                                {                                    
                                    if (!cellValue.Contains(SearchBox.Text.Trim().ToLowerInvariant())) // Default behavior (partial match for other columns)
                                    {
                                        row.Visible = false;
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex) // Catch any potential exceptions during filtering
                        {                            
                            Console.WriteLine($"Error filtering data: {ex.Message}"); // Handle unexpected errors gracefully (e.g., log the error)
                        }
                    }
                    else
                    {
                        row.Visible = true; // Show all rows if no search text
                    }
                }                
            }
            else
            {                
                foreach (DataGridViewRow row in DBViewer.Rows) // Show all rows if no search text
                {
                    row.Visible = true;
                }
            }
        }
    }
}
