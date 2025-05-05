using MySql.Data.MySqlClient;

namespace SchedulingDesktopWGU.Helpers
{
    public static class DBHelper
    {
        private const string connectionString = "server=localhost;user id=sqlUser;password=Passw0rd!;database=client_schedule;";
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
