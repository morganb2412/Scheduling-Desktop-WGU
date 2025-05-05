using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using SchedulingDesktopWGU.Helpers;

namespace SchedulingDesktopWGU.Views
{
    public partial class CustomerForm : Window
    {
        private int selectedCustomerId = -1;

        public CustomerForm()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                DBHelper.OpenConnection();
                string query = @"SELECT c.customerId, c.customerName, a.address, a.phone
                                 FROM customer c
                                 JOIN address a ON c.addressId = a.addressId";

                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                CustomerGrid.ItemsSource = dt.DefaultView;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Load Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private bool ValidateCustomer()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("All fields are required.");
                return false;
            }

            string phone = txtPhone.Text.Trim();
            if (!Regex.IsMatch(phone, @"^[0-9\-]+$"))
            {
                MessageBox.Show("Phone number can only contain digits and dashes.");
                return false;
            }

            return true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCustomer()) return;

            try
            {
                DBHelper.OpenConnection();

                // Insert into address
                string addrQuery = @"INSERT INTO address (address, phone, cityId, createDate, createdBy, lastUpdate, lastUpdateBy)
                                     VALUES (@address, @phone, 1, NOW(), 'admin', NOW(), 'admin')";
                MySqlCommand addrCmd = new MySqlCommand(addrQuery, DBHelper.conn);
                addrCmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                addrCmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                addrCmd.ExecuteNonQuery();

                int addressId = (int)addrCmd.LastInsertedId;

                // Insert into customer
                string custQuery = @"INSERT INTO customer (customerName, addressId, active, createDate, createdBy, lastUpdate, lastUpdateBy) 
                                     VALUES (@name, @addressId, 1, NOW(), 'admin', NOW(), 'admin')";
                MySqlCommand custCmd = new MySqlCommand(custQuery, DBHelper.conn);
                custCmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                custCmd.Parameters.AddWithValue("@addressId", addressId);
                custCmd.ExecuteNonQuery();

                MessageBox.Show("Customer added.");
                ClearForm();
                LoadCustomers();
            }
            catch (MySqlException ex)
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
            if (!ValidateCustomer()) return;

            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Please select a customer to update.");
                return;
            }

            try
            {
                DBHelper.OpenConnection();

                string query = @"
                    UPDATE customer c
                    JOIN address a ON c.addressId = a.addressId
                    SET c.customerName = @name,
                        a.address = @address,
                        a.phone = @phone,
                        c.lastUpdate = NOW(),
                        c.lastUpdateBy = 'admin',
                        a.lastUpdate = NOW(),
                        a.lastUpdateBy = 'admin'
                    WHERE c.customerId = @id";

                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@id", selectedCustomerId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Customer updated.");
                ClearForm();
                LoadCustomers();
            }
            catch (MySqlException ex)
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
            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Please select a customer to delete.");
                return;
            }

            try
            {
                DBHelper.OpenConnection();
                string query = "DELETE FROM customer WHERE customerId = @id";
                MySqlCommand cmd = new MySqlCommand(query, DBHelper.conn);
                cmd.Parameters.AddWithValue("@id", selectedCustomerId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Customer deleted.");
                ClearForm();
                LoadCustomers();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Delete Error: " + ex.Message);
            }
            finally
            {
                DBHelper.CloseConnection();
            }
        }

        private void CustomerGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerGrid.SelectedItem is DataRowView row)
            {
                selectedCustomerId = Convert.ToInt32(row["customerId"]);
                txtName.Text = row["customerName"].ToString();
                txtAddress.Text = row["address"].ToString();
                txtPhone.Text = row["phone"].ToString();
            }
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtAddress.Clear();
            txtPhone.Clear();
            selectedCustomerId = -1;
        }
    }
}
