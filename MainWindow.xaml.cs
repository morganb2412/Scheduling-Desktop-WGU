using System.Windows;
using MySql.Data.MySqlClient;
using SchedulingDesktopWGU.Helpers;
using SchedulingDesktopWGU.Views;
using System.IO;


namespace SchedulingDesktopWGU
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            try
            {
                DBHelper.OpenConnection();

                string query = "SELECT * FROM user WHERE userName = @username AND password = @password AND active = 1";
                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    int userId = Convert.ToInt32(reader["userId"]);
                    string dbUsername = reader["userName"].ToString();
                    reader.Close();
                    DBHelper.CloseConnection();

                    // ✅ Log login timestamp
                    LogLoginHistory(dbUsername);

                    MessageBox.Show("Login successful!");

                    // ✅ Check for upcoming appointments
                    CheckUpcomingAppointments(userId);

                    // ✅ Navigate to customer form
                    var customerForm = new CustomerForm();
                    customerForm.Show();
                    this.Close();
                }
                else
                {
                    reader.Close();
                    DBHelper.CloseConnection();
                    MessageBox.Show("Invalid username or password.");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database Error: " + ex.Message);
            }
        }

        private void LogLoginHistory(string username)
        {
            string logPath = "Login_History.txt"; // Saved in app directory
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {username} logged in";

            try
            {
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing login history: " + ex.Message);
            }
        }



        private void CheckUpcomingAppointments(int userId)
        {
            try
            {
                DBHelper.OpenConnection();

                DateTime nowUtc = DateTime.UtcNow;
                DateTime soonUtc = nowUtc.AddMinutes(15);

                string query = @"SELECT appointmentId, start 
                         FROM appointment
                         WHERE userId = @userId AND start BETWEEN @now AND @soon";

                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@now", nowUtc);
                cmd.Parameters.AddWithValue("@soon", soonUtc);

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DateTime utcStart = Convert.ToDateTime(reader["start"]);
                        DateTime localStart = utcStart.ToLocalTime();
                        MessageBox.Show($"You have an appointment at {localStart:t}!", "Upcoming Appointment");
                    }
                }
                else
                {
                    MessageBox.Show("No upcoming appointments in the next 15 minutes.", "Appointment Check");
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking appointments: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }


    }
}
