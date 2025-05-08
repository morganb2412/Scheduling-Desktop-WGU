using System;
using System.Data;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;
using SchedulingDesktopWGU.Helpers;

namespace SchedulingDesktopWGU.Views
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow()
        {
            InitializeComponent();
        }

        private void reportSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (reportSelector.SelectedIndex == 0)
                ShowAppointmentsByTypeAndMonth();
            else if (reportSelector.SelectedIndex == 1)
                ShowScheduleByUser();
            else if (reportSelector.SelectedIndex == 2)
                ShowAppointmentsByCity();
        }

        private void ShowAppointmentsByTypeAndMonth()
        {
            try
            {
                DBHelper.OpenConnection();
                string query = "SELECT type, start FROM appointment";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, DBHelper.conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                var report = dt.AsEnumerable()
                    .GroupBy(r => new
                    {
                        Type = r.Field<string>("type"),
                        Month = r.Field<DateTime>("start").ToString("MMMM")
                    })
                    .Select(g => new
                    {
                        Type = g.Key.Type,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .ToList();

                ReportGrid.ItemsSource = report;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private void ShowScheduleByUser()
        {
            try
            {
                DBHelper.OpenConnection();
                string query = @"SELECT u.userName, a.type, a.start, a.end
                                 FROM appointment a
                                 JOIN user u ON a.userId = u.userId";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, DBHelper.conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                var report = dt.AsEnumerable()
                    .Select(r => new
                    {
                        User = r.Field<string>("userName"),
                        Type = r.Field<string>("type"),
                        Start = r.Field<DateTime>("start").ToLocalTime(),
                        End = r.Field<DateTime>("end").ToLocalTime()
                    })
                    .ToList();

                ReportGrid.ItemsSource = report;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private void ShowAppointmentsByCity()
        {
            try
            {
                DBHelper.OpenConnection();
                string query = @"SELECT ci.city, a.appointmentId
                                 FROM appointment a
                                 JOIN customer c ON a.customerId = c.customerId
                                 JOIN address ad ON c.addressId = ad.addressId
                                 JOIN city ci ON ad.cityId = ci.cityId";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, DBHelper.conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                var report = dt.AsEnumerable()
                    .GroupBy(r => r.Field<string>("city"))
                    .Select(g => new
                    {
                        City = g.Key,
                        Appointments = g.Count()
                    })
                    .ToList();

                ReportGrid.ItemsSource = report;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }
    }
}
