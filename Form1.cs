using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
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
        private MySqlConnection GetConnection()
        {
            string connstring = "server=0.0.0.0;port=3306;database=dmedb;uid=root;password=Edelwe!ss00;";
            return new MySqlConnection(connstring);
        }
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
        // Methods
        private void ClearAll_Click(object sender, EventArgs e)
        {
            foreach (Control control in Controls)
            {
                switch (control)
                {
                    case TextBox textBox:
                        textBox.Clear();
                        break;
                    case ComboBox comboBox:
                        comboBox.SelectedIndex = -1;
                        break;
                }
            }
            SetDefaultDates();
        }
        public void SetReadOnlyState(bool isReadOnly)
        {
            foreach (Control control in Controls)
            {
                switch (control)
                {
                    case TextBox textBox:
                        textBox.ReadOnly = isReadOnly;
                        break;
                    case ComboBox comboBox:
                        comboBox.Enabled = !isReadOnly;
                        break;
                    case DateTimePicker dateTimePicker:
                        dateTimePicker.Enabled = !isReadOnly;
                        break;
                }
            }
        }
        private void Edit_Click(object sender, EventArgs e)
        {
            SetReadOnlyState(!EncoderBox.ReadOnly);
            Edit.Visible = false;
        }
        public void SetButtonsVisibility(bool isVisible)
        {
            Edit.Visible = isVisible;
            Update.Visible = isVisible;
        }
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
            catch
            {
                MessageBox.Show($"Please complete filling the form.");
            }
        }
        private async void Update_Click(object sender, EventArgs e)
        {
            try
            {
                var project = GetProjectFromForm();
                if (ValidateProject(project))
                {
                    await UpdateProjectAsync(project);
                    var form2 = Application.OpenForms.OfType<Form2>().FirstOrDefault();
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
            Update.Visible = false;
        }
        private Project GetProjectFromForm()
        {
            int id = 0;
            int.TryParse(IDBox.Text, out id);
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
                Remarks = RemarksBox.Text,
                Id = id
            };
        }
        private bool ValidateProject(Project project)
        {
            ValidationContext context = new ValidationContext(project);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(project, context, results, true))
            {
                var errorMessage = new StringBuilder("Please fix the following errors:\n");
                results.ForEach(result => errorMessage.AppendLine($"- {result.ErrorMessage}"));
                MessageBox.Show(errorMessage.ToString(), "Validation Error");
                return false;
            }
            return true;
        }
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
                string query = @"UPDATE project_tb 
                 SET project_title = @title, project_location = @loc, project_totalcost = @tc, 
                     project_budget = @budget, date_notice = @notice, date_start = @start, 
                     date_days = @days, date_extension = @ext, date_target = @target, 
                     project_status = @status, project_incurred = @incurred, 
                     date_inspection = @inspect, project_remarks = @remarks, 
                     project_coordinator = @pc, project_source = @source, 
                     project_contractor = @con, project_encoder = @enc 
                 WHERE project_id = @id";
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
                    MessageBox.Show($"Project updated successfully!");
                }
            }
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
                if (DateTime.TryParse(StartDate.Text, out DateTime startDate) &&
                    int.TryParse(CalendarBox.Text, out int daysToAdd))
                {
                    TargetDate.Value = startDate.AddDays(daysToAdd);
                }
                else
                {
                    CalendarBox.Text = "Invalid input";
                }
            }
            catch (FormatException)
            {
                CalendarBox.Text = "Invalid Date Format";
            }
        }
        public void SetProjectData(string title, string location, decimal totalCost, decimal budget, DateTime notice, DateTime start, DateTime target, string calendar, string extension, int status, decimal incurred, DateTime inspect, string remarks, string coordinator, string source, string contractor, string encoder, int id)
        {
            NameBox.Text = title;
            LocationCB.Text = location;
            TCBox.Text = totalCost.ToString("0.000");
            BudgetBox.Text = budget.ToString("0.000");
            NoticeDate.Value = notice;
            StartDate.Value = start;
            TargetDate.Value = target;
            CalendarBox.Text = calendar;
            ExtBox.Text = extension;
            StatusBox.Text = status.ToString();
            IncurredBox.Text = incurred.ToString("0.000");
            InspectDate.Value = inspect;
            RemarksBox.Text = remarks;
            PCBox.Text = coordinator;
            SourceBox.Text = source;
            ConBox.Text = contractor;
            EncoderBox.Text = encoder;
            IDBox.Text = id.ToString();
        }

        private void Upload_Click(object sender, EventArgs e)
        {
            // Get the path to the Documents folder under 'This PC'
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Find the next available folder number
            int folderNumber = 1;
            string newFolderPath;
            do
            {
                newFolderPath = Path.Combine(documentsPath, folderNumber.ToString());
                folderNumber++;
            } while (Directory.Exists(newFolderPath));

            // Create the new folder
            Directory.CreateDirectory(newFolderPath);

            // Store the path of the created folder
            lastCreatedFolderPath = newFolderPath;

            // Open a dialog to select multiple image files
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Copy selected files to the new folder
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        string fileName = Path.GetFileName(filePath);
                        string destinationPath = Path.Combine(newFolderPath, fileName);
                        File.Copy(filePath, destinationPath);
                    }

                    MessageBox.Show("Images uploaded successfully.");
                }
            }
        }
        private string lastCreatedFolderPath;
        private void Open_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastCreatedFolderPath) && Directory.Exists(lastCreatedFolderPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", lastCreatedFolderPath);
            }
            else
            {
                MessageBox.Show("No folder to open. Please upload images first.");
            }
        }
    }
}
