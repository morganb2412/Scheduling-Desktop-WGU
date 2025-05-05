using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;
using SchedulingDesktopWGU.Helpers;

namespace SchedulingDesktopWGU.Views
{
    public partial class CalendarView : Window
    {
        public CalendarView()
        {
            InitializeComponent();
            dpSelectDate.SelectedDate = DateTime.Today; // default to today
        }

        private void dpSelectDate_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dpSelectDate.SelectedDate != null)
            {
                LoadAppointmentsForDay(dpSelectDate.SelectedDate.Value);
            }
        }

        private void LoadAppointmentsForDay(DateTime selectedDate)
        {
            try
            {
                DBHelper.OpenConnection();

                DateTime startOfDayUtc = selectedDate.Date.ToUniversalTime();
                DateTime endOfDayUtc = selectedDate.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                string query = @"SELECT c.customerName, a.type, a.start, a.end
                         FROM appointment a
                         JOIN customer c ON a.customerId = c.customerId
                         WHERE a.start BETWEEN @start AND @end";

                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@start", startOfDayUtc);
                cmd.Parameters.AddWithValue("@end", endOfDayUtc);

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Convert UTC → Local for display
                foreach (DataRow row in dt.Rows)
                {
                    DateTime utcStart = Convert.ToDateTime(row["start"]);
                    DateTime utcEnd = Convert.ToDateTime(row["end"]);

                    row["start"] = utcStart.ToLocalTime();
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

    }
}
