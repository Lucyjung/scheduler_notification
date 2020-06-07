using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ScheduleNoti.Utilities
{
    class SQL
    {
        public static string connectionString;
        public static int sendSqlQuery(string sqlCmd)
        {
            SqlConnection connection;
            SqlCommand command;
            SqlDataReader dataReader;
            int status = 0;

            if (connectionString != null)
            {
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    command = new SqlCommand(sqlCmd, connection);
                    dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        status = Int32.Parse(dataReader.GetValue(2).ToString());
                    }
                    command.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    LogFile.WriteToFile("Error sql : " + ex.ToString());
                }
            }

            return status;
        }
    }
}
