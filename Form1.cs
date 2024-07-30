using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjectCompiler
{
    public partial class Form1 : Form
    {
        private Form2 form2Instance;
        public Form1()
        {
            InitializeComponent();
            SetDefaultDates();
        }
        // Method to set default dates
        private void SetDefaultDates()
        {
            DateTime now = DateTime.Now;
            NoticeDate.Value = now;
            StartDate.Value = now;
            TargetDate.Value = now;
            InspectDate.Value = now;
        }
        // Method to get database connection
        private MySqlConnection GetConnection()
        {
            string connectionString = "server=localhost;port=3306;database=dmedb;uid=root;password=Edelwe!ss00;";
            return new MySqlConnection(connectionString);
        }
        // Project class
        public class Project
        {
            [Required] public string Encoder { get; set; }
            [Required] public string Title { get; set; }
            [Required] public string Location { get; set; }
            [Required] public string Coordinator { get; set; }
            [Required] public string Contractor { get; set; }
            [Required] public string Source { get; set; }
            public decimal TotalCost { get; set; }
            public decimal Budget { get; set; }
            public DateTime? Notice { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? Target { get; set; }
            [StringLength(50)] public string Calendar { get; set; }
            [StringLength(50)] public string Extension { get; set; }
            public int Status { get; set; }
            [StringLength(50)] public string Incurred { get; set; }
            public DateTime? Inspect { get; set; }
            [StringLength(50)] public string Remarks { get; set; }
            public int Id { get; set; }
        }

        // Properties
        public string Encoder
        {
            get { return EncoderBox.Text; }
            set { EncoderBox.Text = value; }
        }
        public string Title
        {
            get { return NameBox.Text; }
            set { NameBox.Text = value; }
        }
        public string Loc
        {
            get { return LocationCB.Text; }
            set { LocationCB.Text = value; }
        }
        public string Coordinator
        {
            get { return PCBox.Text; }
            set { PCBox.Text = value; }
        }
        public string Contractor
        {
            get { return ConBox.Text; }
            set { ConBox.Text = value; }
        }
        public string Source
        {
            get { return SourceBox.Text; }
            set { SourceBox.Text = value; }
        }
        public decimal TotalCost
        {
            get { return decimal.Parse(TCBox.Text); }
            set { TCBox.Text = value.ToString(); }
        }
        public decimal Budget
        {
            get { return decimal.Parse(BudgetBox.Text); }
            set { BudgetBox.Text = value.ToString(); }
        }
        public DateTime Notice
        {
            get { return NoticeDate.Value; }
            set { NoticeDate.Value = value; }
        }
        public DateTime Start
        {
            get { return StartDate.Value; }
            set { StartDate.Value = value; }
        }
        public DateTime Target
        {
            get { return TargetDate.Value; }
            set { TargetDate.Value = value; }
        }
        public string Calendar
        {
            get { return CalendarBox.Text; }
            set { CalendarBox.Text = value; }
        }
        public string Extension
        {
            get { return ExtBox.Text; }
            set { ExtBox.Text = value; }
        }
        public int Status
        {
            get { return int.Parse(StatusBox.Text); }
            set { StatusBox.Text = value.ToString(); }
        }
        public decimal Incurred
        {
            get { return decimal.Parse(IncurredBox.Text); }
            set { IncurredBox.Text = value.ToString("0.000"); }
        }
        public DateTime Inspect
        {
            get { return InspectDate.Value; }
            set { InspectDate.Value = value; }
        }
        public string Remarks
        {
            get { return RemarksBox.Text; }
            set { RemarksBox.Text = value; }
        }
        // Button click handlers
        private async void Submit_Click(object sender, EventArgs e)
        {
            try
            {
                var project = GetProjectFromForm();
                if (ValidateProject(project))
                {
                    await InsertProjectAsync(project);
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Invalid format: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private async void Replace_Click(object sender, EventArgs e)
        {
            try
            {
                var project = GetProjectFromForm();
                if (ValidateProject(project))
                {
                    await UpdateProjectAsync(project);
                    var form2 = Application.OpenForms.OfType<Form2>().FirstOrDefault();
                    form2?.RefreshDataGridView();
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Invalid format: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            Replace.Visible = false;
        }
        private void Edit_Click(object sender, EventArgs e)
        {
            SetReadOnlyState(!EncoderBox.ReadOnly);
            Edit.Visible = false;
        }
        private void ClearAll_Click(object sender, EventArgs e)
        {
            foreach (Control control in Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Clear();
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = -1;
                }
            }
            SetDefaultDates();
        }
        private void ProjectsList_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(this);
            form2.Show();
            form2.BringToFront();
        }
        private void StartDate_ValueChanged(object sender, EventArgs e)
        {
            UpdateTargetDate();
        }
        private void CalendarBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateTargetDate();
            }
        }
        // Helper methods
        private async Task InsertProjectAsync(Project project)
        {
            using (var con = GetConnection())
            {
                await con.OpenAsync();
                string query = "INSERT INTO project_tb (project_year, project_title, project_location, project_totalcost, project_budget, date_notice, date_start, date_days, date_extension, date_target, project_status, project_incurred, date_inspection, project_remarks, project_coordinator, project_source, project_contractor, project_encoder) VALUES (@year, @title, @loc, @tc, @budget, @notice, @start, @days, @ext, @target, @status, @incurred, @inspect, @remarks, @pc, @source, @con, @enc)";
                using (var command = new MySqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@year", DateTime.Now.Year);
                    command.Parameters.AddWithValue("@title", project.Title);
                    command.Parameters.AddWithValue("@loc", project.Location);
                    command.Parameters.AddWithValue("@tc", project.TotalCost);
                    command.Parameters.AddWithValue("@budget", project.Budget);
                    command.Parameters.AddWithValue("@notice", project.Notice);
                    command.Parameters.AddWithValue("@start", project.Start);
                    command.Parameters.AddWithValue("@days", project.Calendar);
                    command.Parameters.AddWithValue("@ext", project.Extension);
                    command.Parameters.AddWithValue("@target", project.Target);
                    command.Parameters.AddWithValue("@status", project.Status);
                    command.Parameters.AddWithValue("@incurred", project.Incurred);
                    command.Parameters.AddWithValue("@inspect", project.Inspect);
                    command.Parameters.AddWithValue("@remarks", project.Remarks);
                    command.Parameters.AddWithValue("@pc", project.Coordinator);
                    command.Parameters.AddWithValue("@source", project.Source);
                    command.Parameters.AddWithValue("@con", project.Contractor);
                    command.Parameters.AddWithValue("@enc", project.Encoder);

                    await command.ExecuteNonQueryAsync();
                    MessageBox.Show("Project submitted successfully!");
                }
            }
        }
        private async Task UpdateProjectAsync(Project project)
        {
            using (var con = GetConnection())
            {
                await con.OpenAsync();
                string query = "UPDATE project_tb SET project_title = @title, project_location = @loc, project_totalcost = @tc, project_budget = @budget, date_notice = @notice, date_start = @start, date_days = @days, date_extension = @ext, date_target = @target, project_status = @status, project_incurred = @incurred, date_inspection = @inspect, project_remarks = @remarks, project_coordinator = @pc, project_source = @source, project_contractor = @con, project_encoder = @enc WHERE project_id = @id";
                using (var command = new MySqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@id", project.Id);
                    command.Parameters.AddWithValue("@title", project.Title);
                    command.Parameters.AddWithValue("@loc", project.Location);
                    command.Parameters.AddWithValue("@tc", project.TotalCost);
                    command.Parameters.AddWithValue("@budget", project.Budget);
                    command.Parameters.AddWithValue("@notice", project.Notice);
                    command.Parameters.AddWithValue("@start", project.Start);
                    command.Parameters.AddWithValue("@days", project.Calendar);
                    command.Parameters.AddWithValue("@ext", project.Extension);
                    command.Parameters.AddWithValue("@target", project.Target);
                    command.Parameters.AddWithValue("@status", project.Status);
                    command.Parameters.AddWithValue("@incurred", project.Incurred);
                    command.Parameters.AddWithValue("@inspect", project.Inspect);
                    command.Parameters.AddWithValue("@remarks", project.Remarks);
                    command.Parameters.AddWithValue("@pc", project.Coordinator);
                    command.Parameters.AddWithValue("@source", project.Source);
                    command.Parameters.AddWithValue("@con", project.Contractor);
                    command.Parameters.AddWithValue("@enc", project.Encoder);

                    await command.ExecuteNonQueryAsync();
                    MessageBox.Show("Project updated successfully!");
                }
            }
        }
        private bool ValidateProject(Project project)
        {
            // Add validation logic here
            return true;
        }
        private Project GetProjectFromForm()
        {
            return new Project
            {
                Encoder = EncoderBox.Text,
                Title = NameBox.Text,
                Location = LocationCB.Text,
                Coordinator = PCBox.Text,
                Contractor = ConBox.Text,
                Source = SourceBox.Text,
                TotalCost = decimal.Parse(TCBox.Text, NumberStyles.Currency),
                Budget = decimal.Parse(BudgetBox.Text, NumberStyles.Currency),
                Notice = NoticeDate.Value,
                Start = StartDate.Value,
                Target = TargetDate.Value,
                Calendar = CalendarBox.Text,
                Extension = ExtBox.Text,
                Status = int.Parse(StatusBox.Text),
                Incurred = decimal.Parse(IncurredBox.Text, NumberStyles.Currency).ToString("0.000"),
                Inspect = InspectDate.Value,
                Remarks = RemarksBox.Text
            };
        }
        private void LoadProjectIntoForm(Project project)
        {
            EncoderBox.Text = project.Encoder;
            NameBox.Text = project.Title;
            LocationCB.Text = project.Location;
            PCBox.Text = project.Coordinator;
            ConBox.Text = project.Contractor;
            SourceBox.Text = project.Source;
            TCBox.Text = project.TotalCost.ToString("C");
            BudgetBox.Text = project.Budget.ToString("C");
            NoticeDate.Value = project.Notice ?? DateTime.Now;
            StartDate.Value = project.Start ?? DateTime.Now;
            TargetDate.Value = project.Target ?? DateTime.Now;
            CalendarBox.Text = project.Calendar;
            ExtBox.Text = project.Extension;
            StatusBox.Text = project.Status.ToString();
            IncurredBox.Text = project.Incurred;
            InspectDate.Value = project.Inspect ?? DateTime.Now;
            RemarksBox.Text = project.Remarks;
            SetReadOnlyState(true);
            SetButtonsVisibility(true);
        }
        private Project GetProjectFromSelectedRow(int rowIndex)
        {
            // Extract project details from the selected row and return a Project object
            return new Project();
        }
        private void UpdateTargetDate()
        {
            if (string.IsNullOrEmpty(CalendarBox.Text) || string.IsNullOrEmpty(StartDate.Text))
            {
                CalendarBox.Text = "";
                TargetDate.Value = DateTime.Now;
                return;
            }
            try
            {
                DateTime startDate = DateTime.Parse(StartDate.Text);
                if (int.TryParse(CalendarBox.Text, out int daysToAdd))
                {
                    TargetDate.Value = startDate.AddDays(daysToAdd);
                }
                else
                {
                    CalendarBox.Text = "Invalid number";
                }
            }
            catch (FormatException)
            {
                CalendarBox.Text = "Invalid Date Format";
            }
        }
        public bool LoadRowData(DataGridViewRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            try
            {
                Encoder = row.Cells["Encoder"].Value?.ToString() ?? "N/A";
                Title = row.Cells["Project/Program/Activity"].Value?.ToString() ?? "N/A";
                Loc = row.Cells["Location"].Value?.ToString() ?? "N/A";
                TotalCost = Convert.ToDecimal(row.Cells["Total Cost"].Value ?? 0);
                Budget = Convert.ToDecimal(row.Cells["Approved Budget in Contract (ABC)"].Value ?? 0);
                Notice = Convert.ToDateTime(row.Cells["Notice to Proceed"].Value ?? DateTime.Now);
                Start = Convert.ToDateTime(row.Cells["Date Started"].Value ?? DateTime.Now);
                Target = Convert.ToDateTime(row.Cells["Target Completion Date"].Value ?? DateTime.Now);
                Calendar = row.Cells["No. of Calendar Days"].Value?.ToString() ?? "0";
                Extension = row.Cells["No. of Extension"].Value?.ToString() ?? "0";
                Status = Convert.ToInt32(row.Cells["Project Status (%)"].Value ?? 0);
                Incurred = Convert.ToDecimal(row.Cells["Total Cost Incurred to Date"].Value ?? 0);
                Inspect = Convert.ToDateTime(row.Cells["Inspection Date"].Value ?? DateTime.Now);
                Remarks = row.Cells["Remarks"].Value?.ToString() ?? "N/A";
                Coordinator = row.Cells["Project Coordinator"].Value?.ToString() ?? "N/A";
                Source = row.Cells["Source of Fund"].Value?.ToString() ?? "N/A";
                Contractor = row.Cells["Contractor"].Value?.ToString() ?? "N/A";

                // Set Form1 controls to read-only after populating them
                SetReadOnlyState(true);
                return true; // Indicate success
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Please click 'Show Full Table' first before transferring data.");
                return false; // Indicate failure
            }
        }
        // Method to set read-only state for controls
        public void SetReadOnlyState(bool isReadOnly)
        {
            foreach (Control control in Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.ReadOnly = isReadOnly;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.Enabled = !isReadOnly;
                }
                else if (control is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.Enabled = !isReadOnly;
                }
            }
        }
        // Method to set button visibility
        public void SetButtonsVisibility(bool isVisible)
        {
            Edit.Visible = isVisible;
            Replace.Visible = isVisible;
        }
        // Method to handle cell double-click event in DBViewer
        private void DBViewer_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedProject = GetProjectFromSelectedRow(e.RowIndex);
                LoadProjectIntoForm(selectedProject);
            }
        }
    }
}
