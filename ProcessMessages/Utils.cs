using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NLog;

namespace ProcessMessages
{
    internal class Utils
    {
        /// <summary>
        /// Perfoms a Lookup in the Database from a corresponding value to a code provided
        /// </summary>
        /// <param name="category">Category Name to lookup</param>
        /// <param name="code">Code to translate</param>
        /// <param name="defaultOption">Default value if none found</param>
        /// <returns>Lookup value</returns>
        public static String Lookup(String category, String code, String defaultOption)
        {
            String output = defaultOption;
            String query = String.Format("SELECT [value] FROM tblluHL7Lookup WHERE category = '{0}' AND code = '{1}'", category, code);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            try
                            {
                                da.Fill(ds);
                                conn.Close();

                                if (ds.Tables.Count > 0)
                                {
                                    using (DataTable lkUpTbl = ds.Tables[0])
                                    {
                                        DataRow dr = lkUpTbl.Rows[0];
                                        output = dr["value"].ToString();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Per the HL7 date standard the date comes in YYYYMMDD,
        /// this will format it to be MM-DD-YYYY
        /// </summary>
        /// <param name="dtm"></param>
        /// <returns></returns>
        public static String FormatDate(String dtm)
        {
            String result = "";

            if (dtm != null && !dtm.Equals(""))
            {
                String year = dtm.Substring(0, 4);
                String month = dtm.Substring(4, 2);
                String day = dtm.Substring(6, 2);

                result = month + "/" + day + "/" + year;
            }
            else
            {
                result = "";
            }

            return result;
        }

        public static String FormatDateTime(String dtm)
        {
            String year = "";
            String month = "";
            String day = "";
            String hour;
            String min;

            year = dtm.Substring(0, 4);
            month = dtm.Substring(4, 2);
            day = dtm.Substring(6, 2);

            try
            {
                hour = dtm.Substring(8, 2);
                min = dtm.Substring(10, 2);
            }
            catch
            {
                hour = "00";
                min = "00";
            }

            return month + "/" + day + "/" + year + " " + hour + ":" + min;
        }

        public static String FormatTimeStamp(String dtm, String format)
        {
            String year, month, day, hour, minute, second;

            year = dtm.Substring(format.IndexOf("y"), format.Count(f => f == 'y'));
            month = dtm.Substring(format.IndexOf("m"), format.Count(f => f == 'm'));
            day = dtm.Substring(format.IndexOf("d"), format.Count(f => f == 'd'));
            hour = dtm.Substring(format.IndexOf("h"), format.Count(f => f == 'h'));
            minute = dtm.Substring(format.IndexOf("n"), format.Count(f => f == 'n'));
            second = dtm.Substring(format.IndexOf("s"), format.Count(f => f == 's'));

            return year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second;
        }

        /// <summary>
        /// Performs an Audit insert into the Database
        /// </summary>
        /// <param name="vchSQL">SQL statement that was originally executed</param>
        /// <param name="messageID">HL7 Message ID</param>
        public static void Audit(String vchSQL, String messageID)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "HL7_isptblDataHL7Audit";

                    cmd.Parameters.AddWithValue("@dtmDateTime", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    cmd.Parameters.AddWithValue("@vchMessageID", messageID);
                    cmd.Parameters.AddWithValue("@vchSQL", vchSQL.Replace("'", "''"));

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Determines if String is null or empty and returns the correct response
        /// </summary>
        /// <param name="str">Original String</param>
        /// <param name="deflt">Default response</param>
        /// <returns></returns>
        public static String NZ(String str, String deflt = "")
        {
            return (!String.IsNullOrEmpty(str) && !String.IsNullOrWhiteSpace(str)) ? str : deflt;
        }

        public static String GetFullName(String firstName, String middleName, String lastName)
        {
            StringBuilder output = new StringBuilder();
            output.Append((!String.IsNullOrWhiteSpace(lastName)) ? lastName + ", " : "");
            output.Append((!String.IsNullOrWhiteSpace(firstName)) ? firstName + " " : "");
            output.Append((!String.IsNullOrWhiteSpace(middleName)) ? middleName : "");

            return output.ToString().Trim();
        }

        /// <summary>
        /// Writes to a specified Log file
        /// If the environment is user interactive it will also write to a console
        /// </summary>
        /// <param name="message">Message to Log</param>
        /// <param name="logName">Name of Log</param>
        public static void WriteToLog(String message, String logName)
        {
            Logger logger = LogManager.GetLogger(logName);
            logger.Info(message);

            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Writes to a specified Log file
        /// If the environment is user interactive it will also write to a console
        /// </summary>
        /// <param name="message">Message to Log</param>
        /// <param name="logName">Name of Log</param>
        /// <param name="logType">Type of Log</param>
        public static void WriteToLog(String message, String logName, String logType)
        {
            Logger logger = LogManager.GetLogger(logName);
            logger.Info(message);

            if (Environment.UserInteractive)
            {
                if (String.IsNullOrWhiteSpace(logType))
                {
                    Console.Write(logType.ToUpper() + ": ");
                }

                Console.Write(message + "\r\n");
            }
        }

        public static String HL7Format(String elem, Boolean isRepeatable)
        {
            String output = "";

            if (isRepeatable)
            {
                output = "{0}-{1}[{2}].{3}";
            }
            else
            {
                output = "{0}-{1}.{2}";
            }

            if (Int32.Parse(ConfigurationManager.AppSettings["WarningLevel"]) == 2)
            {
                WriteToLog(elem, "message");
            }

            return output;
        }
    }
}