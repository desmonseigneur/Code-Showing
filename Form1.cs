using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
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
            NoticeDate.Value = DateTime.Now;
            StartDate.Value = DateTime.Now;
            TargetDate.Value = DateTime.Now;
            InspectDate.Value = DateTime.Now;
        }
        private MySqlConnection GetConnection()
        {
            string connstring = "server=localhost;port=3306;database=dmedb;uid=root;password=Edelwe!ss00;";
            return new MySqlConnection(connstring);
        }
        public class Project
        {
            [Required]
            public string Encoder { get; set; }
            [Required]
            public string Title { get; set; }
            [Required]
            public string Location { get; set; }
            [Required]
            public string Coordinator { get; set; }
            [Required]
            public string Contractor { get; set; }
            [Required]
            public string Source { get; set; }
            public decimal TotalCost { get; set; }
            public decimal Budget { get; set; }
            public DateTime? Notice { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? Target { get; set; }
            [StringLength(50)]
            public string Calendar { get; set; }
            [StringLength(50)]
            public string Extension { get; set; }
            public int Status { get; set; }
            [StringLength(50)]
            public string Incurred { get; set; }
            public DateTime? Inspect { get; set; }
            [StringLength(50)]
            public string Remarks { get; set; }
        }
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
        public string Location
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
        private void ClearAll_Click(object sender, EventArgs e)
        {
            EncoderBox.Text = "";
            NameBox.Text = "";
            LocationCB.Text = "";
            PCBox.Text = "";
            ConBox.Text = "";
            SourceBox.Text = "";
            TCBox.Text = "";
            BudgetBox.Text = "";
            CalendarBox.Text = "";
            ExtBox.Text = "";
            StatusBox.Text = "";
            IncurredBox.Text = "";
            RemarksBox.Text = "";

            NoticeDate.Value = DateTime.Now;
            StartDate.Value = DateTime.Now;
            TargetDate.Value = DateTime.Now;
            InspectDate.Value = DateTime.Now;
        }
        public void SetReadOnlyState(bool isReadOnly)
        {
            EncoderBox.ReadOnly = isReadOnly;
            NameBox.ReadOnly = isReadOnly;
            LocationCB.Enabled = !isReadOnly; // ComboBox doesn't have ReadOnly property
            PCBox.ReadOnly = isReadOnly;
            ConBox.ReadOnly = isReadOnly;
            SourceBox.ReadOnly = isReadOnly;
            TCBox.ReadOnly = isReadOnly;
            BudgetBox.ReadOnly = isReadOnly;
            NoticeDate.Enabled = !isReadOnly; // DateTimePicker doesn't have ReadOnly property
            StartDate.Enabled = !isReadOnly;
            CalendarBox.ReadOnly = isReadOnly;
            ExtBox.ReadOnly = isReadOnly;
            StatusBox.ReadOnly = isReadOnly;
            IncurredBox.ReadOnly = isReadOnly;
            InspectDate.Enabled = !isReadOnly;
            RemarksBox.ReadOnly = isReadOnly;
        }
        private void Edit_Click(object sender, EventArgs e)
        {
            // Toggle read-only state
            bool isReadOnly = !EncoderBox.ReadOnly;
            SetReadOnlyState(isReadOnly);
        }

        private async void Submit_Click(object sender, EventArgs e)
        {
            try
            {
                Project project = new Project
                {
                    Encoder = EncoderBox.Text,
                    Title = NameBox.Text,
                    Location = LocationCB.Text,
                    Coordinator = PCBox.Text,
                    Contractor = ConBox.Text,
                    Source = SourceBox.Text,
                    TotalCost = decimal.Parse(TCBox.Text, System.Globalization.NumberStyles.Currency),
                    Budget = decimal.Parse(BudgetBox.Text, System.Globalization.NumberStyles.Currency),
                    Notice = NoticeDate.Value,
                    Start = StartDate.Value,
                    Target = TargetDate.Value,
                    Calendar = CalendarBox.Text,
                    Extension = ExtBox.Text,
                    Status = Convert.ToInt32(StatusBox.Text),
                    Incurred = decimal.Parse(IncurredBox.Text, System.Globalization.NumberStyles.Currency).ToString("0.000"),
                    Inspect = InspectDate.Value,
                    Remarks = RemarksBox.Text
                };

                if (ValidateProject(project))
                {
                    await InsertProjectAsync(project);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Please fill the form. Error: {ex.Message}");
            }
        }
        private bool ValidateProject(Project project)
        {
            ValidationContext context = new ValidationContext(project);
            List<ValidationResult> results = new List<ValidationResult>();
            Validator.TryValidateObject(project, context, results, true);

            if (results.Count > 0)
            {
                StringBuilder errorMessage = new StringBuilder("Please fix the following errors:\n");
                results.ForEach(result => errorMessage.AppendLine($"- {result.ErrorMessage}"));
                MessageBox.Show(errorMessage.ToString(), "Validation Error");
                return false;
            }
            return true;
        }
        private async Task InsertProjectAsync(Project project)
        {
            int currentYear = DateTime.Now.Year;
            using (MySqlConnection con = GetConnection())
            {
                await con.OpenAsync();
                string query = "INSERT INTO project_tb (project_year, project_title, project_location, project_totalcost, project_budget, date_notice, date_start, date_days, date_extension, date_target, project_status, project_incurred, date_inspection, project_remarks, project_coordinator, project_source, project_contractor, project_encoder) VALUES (@year, @title, @loc, @tc, @budget, @notice, @start, @days, @ext, @target, @status, @incurred, @inspect, @remarks, @pc, @source, @con, @enc)";
                MySqlCommand command = new MySqlCommand(query, con);

                command.Parameters.AddWithValue("@year", currentYear);
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

                try
                {
                    await command.ExecuteNonQueryAsync();
                    MessageBox.Show("Project submitted successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in submitting project: {ex.Message}");
                }
            }
        }
        private void ProjectsList_Click(object sender, EventArgs e)
        {
            using (Form3 loginForm = new Form3())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    if (form2Instance == null)
                    {
                        form2Instance = new Form2(this);
                        form2Instance.FormClosed += (s, args) => form2Instance = null;
                    }
                    form2Instance.Show();
                    form2Instance.BringToFront();
                }
                else
                {
                    MessageBox.Show("Login failed. Please try again.");
                }
            }
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
    }
}
