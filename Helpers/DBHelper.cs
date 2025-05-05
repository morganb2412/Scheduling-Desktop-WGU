using MySql.Data.MySqlClient;

namespace SchedulingDesktopWGU.Helpers
{
    public static class DBHelper
    {
        private const string connectionString = "server=localhost;user id=root;password=Cruz072724$;database=client_schedule;";
        public static MySqlConnection conn = new MySqlConnection(connectionString);

        public static void OpenConnection()
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
        }

        public static void CloseConnection()
        {
            if (conn.State != System.Data.ConnectionState.Closed)
                conn.Close();
        }
    }
}
