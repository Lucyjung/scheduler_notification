using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ScheduleNoti.Utilities
{
    class SQL
    {
        public static string connectionString = null;
        public static void InitSQL(string connection)
        {
            connectionString = Base64Decode(connection);
        }
        public static DataTable sendSqlQuery(string sqlCmd)
        {
            bool isError = false;
            return sendSqlQuery(sqlCmd, ref isError);
        }
        public static DataTable sendSqlQuery(string sqlCmd, ref bool isError)
        {
            SqlConnection conn = null;
            DataTable dt = new DataTable();
            isError = false;
            try
            {
                conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(sqlCmd, conn);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                DataTable dtSchema = dr.GetSchemaTable();
                
                // You can also use an ArrayList instead of List<>
                List<DataColumn> listCols = new List<DataColumn>();

                if (dtSchema != null)
                {
                    foreach (DataRow drow in dtSchema.Rows)
                    {
                        string columnName = Convert.ToString(drow["ColumnName"]);
                        DataColumn column = new DataColumn(columnName, (Type)(drow["DataType"]));
                        column.Unique = (bool)drow["IsUnique"];
                        column.AllowDBNull = (bool)drow["AllowDBNull"];
                        column.AutoIncrement = (bool)drow["IsAutoIncrement"];
                        listCols.Add(column);
                        dt.Columns.Add(column);
                    }
                }

                // Read rows from DataReader and populate the DataTable
                while (dr.Read())
                {
                    DataRow dataRow = dt.NewRow();
                    for (int i = 0; i < listCols.Count; i++)
                    {
                        dataRow[((DataColumn)listCols[i])] = dr[i];
                    }
                    dt.Rows.Add(dataRow);
                }
            }
            catch (SqlException ex)
            {
                // handle error
                LogFile.WriteToFile("Error sql : " + ex.ToString());
                isError = true;
            }
            catch (Exception ex)
            {
                // handle error
                LogFile.WriteToFile("Other Error : " + ex.ToString());
                isError = true;
            }
            finally
            {
                conn.Close();
            }
            return dt;

        }
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
