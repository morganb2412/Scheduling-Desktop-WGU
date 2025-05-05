using System;
using System.Collections.Generic;
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
                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                var results = dt.AsEnumerable()
                    .GroupBy(row => new
                    {
                        Type = row.Field<string>("type"),
                        Month = row.Field<DateTime>("start").ToString("MMMM")
                    })
                    .Select(group => new
                    {
                        Type = group.Key.Type,
                        Month = group.Key.Month,
                        Count = group.Count()
                    }).ToList();

                ReportGrid.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Report Error: " + ex.Message);
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
                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                var results = dt.AsEnumerable()
                    .Select(row => new
                    {
                        User = row.Field<string>("userName"),
                        Type = row.Field<string>("type"),
                        Start = row.Field<DateTime>("start").ToLocalTime(),
                        End = row.Field<DateTime>("end").ToLocalTime()
                    }).ToList();

                ReportGrid.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Report Error: " + ex.Message);
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
                string query = @"SELECT a.type, a.start, ci.city
                                 FROM appointment a
                                 JOIN customer c ON a.customerId = c.customerId
                                 JOIN address ad ON c.addressId = ad.addressId
                                 JOIN city ci ON ad.cityId = ci.cityId";
                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                var results = dt.AsEnumerable()
                    .GroupBy(row => row.Field<string>("city"))
                    .Select(group => new
                    {
                        City = group.Key,
                        Appointments = group.Count()
                    }).ToList();

                ReportGrid.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Report Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }
    }
}
