using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;
using SchedulingDesktopWGU.Helpers;
using System.Linq;
using System.Globalization;

namespace SchedulingDesktopWGU.Views
{
    public partial class AppointmentForm : Window
    {
        private int selectedAppointmentId = -1;

        public AppointmentForm()
        {
            InitializeComponent();
            LoadAppointments();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                DBHelper.OpenConnection();
                var query = "SELECT customerId, customerName FROM customer";
                var cmd = new MySqlCommand(query, DBHelper.conn);
                var reader = cmd.ExecuteReader();
                var items = new List<Tuple<int, string>>();

                while (reader.Read())
                    items.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1)));

                cbCustomers.ItemsSource = items;
                cbCustomers.DisplayMemberPath = "Item2";
                cbCustomers.SelectedValuePath = "Item1";

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private void LoadAppointments()
        {
            try
            {
                DBHelper.OpenConnection();
                string query = @"SELECT a.appointmentId, c.customerName, a.type, a.start, a.end
                                 FROM appointment a
                                 JOIN customer c ON a.customerId = c.customerId";

                var adapter = new MySqlDataAdapter(query, DBHelper.conn);
                var dt = new DataTable();
                adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    DateTime utcStart = Convert.ToDateTime(row["start"]);
                    DateTime utcEnd = Convert.ToDateTime(row["end"]);

                    row["start"] = utcStart.ToLocalTime(); // Convert for display
                    row["end"] = utcEnd.ToLocalTime();
                }

                AppointmentGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private bool IsWithinBusinessHours(DateTime localStart, DateTime localEnd)
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            DateTime startEST = TimeZoneInfo.ConvertTime(localStart, estZone);
            DateTime endEST = TimeZoneInfo.ConvertTime(localEnd, estZone);

            bool isWeekday = startEST.DayOfWeek >= DayOfWeek.Monday && startEST.DayOfWeek <= DayOfWeek.Friday;
            bool inHours = startEST.TimeOfDay >= TimeSpan.FromHours(9) && endEST.TimeOfDay <= TimeSpan.FromHours(17);

            return isWeekday && inHours;
        }

        private bool HasOverlap(DateTime start, DateTime end)
        {
            DBHelper.OpenConnection();
            string query = @"SELECT * FROM appointment 
                     WHERE (@start < end AND @end > start)";
            var cmd = new MySqlCommand(query, DBHelper.conn);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);

            var reader = cmd.ExecuteReader();
            bool overlap = reader.HasRows;
            reader.Close();
            DBHelper.CloseConnection();

            return overlap;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbCustomers.SelectedValue == null || string.IsNullOrWhiteSpace(txtType.Text) || dpDate.SelectedDate == null)
                {
                    MessageBox.Show("All fields are required.");
                    return;
                }

                int customerId = (int)cbCustomers.SelectedValue;
                string type = txtType.Text.Trim();
                string title = "General Appointment";
                string description = "No description";
                string location = "Online";
                string contact = "Not specified";
                string url = "N/A";

                DateTime startLocal = DateTime.ParseExact($"{dpDate.SelectedDate:yyyy-MM-dd} {txtStart.Text}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                DateTime endLocal = DateTime.ParseExact($"{dpDate.SelectedDate:yyyy-MM-dd} {txtEnd.Text}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                if (!IsWithinBusinessHours(startLocal, endLocal))
                {
                    MessageBox.Show("Appointment must be within business hours (9:00 AM – 5:00 PM EST, Monday–Friday).");
                    return;
                }

                DateTime startUtc = startLocal.ToUniversalTime();
                DateTime endUtc = endLocal.ToUniversalTime();

                if (HasOverlap(startUtc, endUtc))
                {
                    MessageBox.Show("Appointment overlaps with an existing one.");
                    return;
                }

                DBHelper.OpenConnection();

                string query = @"INSERT INTO appointment 
                         (title, description, location, contact, type, url, customerId, userId, start, end, createDate, createdBy, lastUpdate, lastUpdateBy)
                         VALUES 
                         (@title, @description, @location, @contact, @type, @url, @custId, 1, @start, @end, NOW(), 'admin', NOW(), 'admin')";

                var cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@location", location);
                cmd.Parameters.AddWithValue("@contact", contact);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@url", url);
                cmd.Parameters.AddWithValue("@custId", customerId);
                cmd.Parameters.AddWithValue("@start", startUtc);
                cmd.Parameters.AddWithValue("@end", endUtc);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Appointment added.");
                LoadAppointments();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }



        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAppointmentId == -1)
            {
                MessageBox.Show("Select an appointment to update.");
                return;
            }

            try
            {
                int customerId = (int)cbCustomers.SelectedValue;
                string type = txtType.Text.Trim();
                string title = "General Appointment";
                string description = "No description";

                DateTime startLocal = DateTime.ParseExact($"{dpDate.SelectedDate:yyyy-MM-dd} {txtStart.Text}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                DateTime endLocal = DateTime.ParseExact($"{dpDate.SelectedDate:yyyy-MM-dd} {txtEnd.Text}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                if (!IsWithinBusinessHours(startLocal, endLocal))
                {
                    MessageBox.Show("Appointment must be within business hours.");
                    return;
                }

                DateTime startUtc = startLocal.ToUniversalTime();
                DateTime endUtc = endLocal.ToUniversalTime();

                DBHelper.OpenConnection();

                string query = @"UPDATE appointment 
                         SET title=@title, description=@description, customerId=@custId, type=@type, start=@start, end=@end, 
                             lastUpdate=NOW(), lastUpdateBy='admin'
                         WHERE appointmentId=@id";

                var cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@custId", customerId);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@start", startUtc);
                cmd.Parameters.AddWithValue("@end", endUtc);
                cmd.Parameters.AddWithValue("@id", selectedAppointmentId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Appointment updated.");
                LoadAppointments();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }



        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAppointmentId == -1)
            {
                MessageBox.Show("Select an appointment to delete.");
                return;
            }

            try
            {
                DBHelper.OpenConnection();
                string query = "DELETE FROM appointment WHERE appointmentId=@id";
                var cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@id", selectedAppointmentId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Appointment deleted.");
                LoadAppointments();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private void AppointmentGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AppointmentGrid.SelectedItem is DataRowView row)
            {
                selectedAppointmentId = Convert.ToInt32(row["appointmentId"]);
                txtType.Text = row["type"].ToString();
                dpDate.SelectedDate = Convert.ToDateTime(row["start"]);
                txtStart.Text = Convert.ToDateTime(row["start"]).ToString("HH:mm");
                txtEnd.Text = Convert.ToDateTime(row["end"]).ToString("HH:mm");
            }
        }

        private void ClearForm()
        {
            cbCustomers.SelectedIndex = -1;
            txtType.Clear();
            txtStart.Text = "09:00";
            txtEnd.Text = "10:00";
            dpDate.SelectedDate = null;
            selectedAppointmentId = -1;
        }
    }
}
