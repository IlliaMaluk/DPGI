using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Windows;

namespace lab4
{
    public class AdoAssistant
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["connectionString_ADO"].ConnectionString;
        private DataTable dt;

        public DataTable TableLoad()
        {
            if (dt != null)
                return dt;

            dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Students", con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                try
                {
                    adapter.Fill(dt);
                }
                catch (Exception)
                {
                    MessageBox.Show("Помилка підключення до БД", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return dt;
        }

        public void Insert(int number, string name, string group, string address)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query =
                    "INSERT INTO Students (RecordBookNumber, FullName, GroupName, Address) " +
                    "VALUES (@Num, @Name, @Group, @Addr)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Num", number);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Group", group);
                cmd.Parameters.AddWithValue("@Addr", address);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(int number, string name, string group, string address)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query =
                    "UPDATE Students SET FullName=@Name, GroupName=@Group, Address=@Addr " +
                    "WHERE RecordBookNumber=@Num";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Num", number);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Group", group);
                cmd.Parameters.AddWithValue("@Addr", address);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int number)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query =
                    "DELETE FROM Students WHERE RecordBookNumber=@Num";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Num", number);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ResetDataTable()
        {
            dt = null;
        }
    }
}
