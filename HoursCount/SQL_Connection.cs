using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoursCount
{
    class SQL_Connection
    {
        static public DataSet MainConn(string DB_string)
        {
            string machineName = Environment.MachineName.ToString();
            string connectionString;

            if (machineName != "LEAD")
                connectionString = @"Server = " + machineName + @"\WINCC; database = WorkingHours; Integrated Security = SSPI";
            else
                connectionString = @"Server = LEAD; database = WorkingHours; Integrated Security = SSPI";

            SqlConnection conn = new SqlConnection(connectionString);          
            conn.Open();            

            SqlCommand cmd = new SqlCommand(DB_string, conn);
            SqlDataAdapter reader = new SqlDataAdapter(cmd);
            DataSet ds_result = new DataSet();

            reader.Fill(ds_result);
            conn.Close();
            return ds_result;            
        }
        static public DataSet timeSelect(/*string date,*/ string pmpNumber, string orderType, string condition)
        {
            string DB_string;
            string machineName = Environment.MachineName.ToString();
            string connectionString;

            if (machineName != "LEAD")
                connectionString = @"Server = " + machineName + @"\WINCC; database = WorkingHours; Integrated Security = SSPI";
            else
                connectionString = @"Server = LEAD; database = WorkingHours; Integrated Security = SSPI";

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            DB_string = @"SELECT TOP 1 Hours as Часы, Minutes as Минуты, Secondes as Секунды
                                                FROM main
                                                WHERE AddDate " + condition + @"'
                                                AND PumpNumber LIKE '"+ pmpNumber + @"'
                                                ORDER BY id "+ orderType + "";

            SqlCommand cmd = new SqlCommand(DB_string, conn);
            SqlDataAdapter reader = new SqlDataAdapter(cmd);
            DataSet ds_result = new DataSet();

            reader.Fill(ds_result);
            conn.Close();
            return ds_result;
        }
    }
}
