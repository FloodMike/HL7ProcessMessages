using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using HL7Connect.Services;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class QueueADT
    {
        /// <summary>
        /// Working Variables
        /// </summary>
        private Library library;

        private HL7V2Manager manager;
        private Message message;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueADT()
        {
            library = new Library();
            LoadMessages();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lib">HL7 Library</param>
        public QueueADT(Library lib)
        {
            library = lib;
            LoadMessages();
        }

        /// <summary>
        /// Delete the HL7 Message from the whole message queue
        /// </summary>
        /// <param name="messageid">HL7 Message ID</param>
        private void DeleteWholeMessage(String messageid)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("DELETE FROM tblDataHL7MessageWholeADT WHERE vchMessageID = '{0}'", messageid);

                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql.ToString();

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Queries the database and loads all of the ADT messages that need to be processed
        /// </summary>
        private void LoadMessages()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT vchMessageID, vchMRN, vchMessageType, vchEvent, intProcessflag FROM tblDataHL7QueueADT WHERE intProcessFlag = 1";

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        using (DataSet dataset = new DataSet())
                        {
                            da.Fill(dataset);
                            conn.Close();

                            if (dataset.Tables.Count > 0)
                            {
                                using (DataTable dt = dataset.Tables[0])
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        Utils.WriteToLog(String.Format("Processing: {0} messages...", dt.Rows.Count), "adt");

                                        foreach (DataRow row in dt.Rows)
                                        {
                                            if (row["vchMessageType"].ToString().Equals("ADT"))
                                            {
                                                ProcessMessage(row["vchMessageID"].ToString());
                                            }

                                            row.Delete();
                                            GC.Collect();
                                        }
                                    }

                                    dt.Clear();
                                }
                            }
                        }
                    }
                }

                if (conn.State == ConnectionState.Open)
                {
                    Utils.WriteToLog("Closing connection...", "service");
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Processes a single message and inserts it into the necessary tables
        /// </summary>
        /// <param name="messageid">HL7 Message ID</param>
        private void ProcessMessage(String messageid)
        {
            int updateStatus = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = String.Format("SELECT vchMessage FROM tblDataHL7MessageWholeADT WHERE vchMessageID = '{0}'", messageid);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet dataset = new DataSet();
                        da.Fill(dataset);

                        conn.Close();

                        if (dataset.Tables.Count > 0)
                        {
                            using (DataTable dt = dataset.Tables[0])
                            {
                                if (dt.Rows.Count > 0)
                                {
                                    manager = library.HL7();
                                    message = manager.CreateMessage();
                                    message.DecodeOptions(dt.Rows[0]["vchMessage"].ToString(), "ER7");

                                    try
                                    {
                                        Patient.Process(messageid, message);
                                        ECDSAdmission.Process(messageid, message);
                                        ECDSPatient.Process(messageid, message);
                                        ECDSGuarantor.Process(messageid, message);
                                        ECDSInsurance.Process(messageid, message);
                                        ECDSNextOfKin.Process(messageid, message);
                                        ECDSPersonToNotify.Process(messageid, message);
                                    }
                                    catch (Exception e)
                                    {
                                        updateStatus = 2;
                                        Utils.WriteToLog(String.Format("Error Processing ADT Message. {0}\n\t{1}", e.Message, e.StackTrace), "debug");

                                        UpdateMessageStatus(messageid, updateStatus);
                                    }

                                    message.Clear();

                                    /**
                                     * Update the message status in the Queue ADT and Delete the whole message from the Whole Message Queue
                                     **/
                                    UpdateMessageStatus(messageid, updateStatus);
                                }
                                else
                                {
                                    Utils.WriteToLog(String.Format("Whole Message not found for Message ID: {0}", messageid), "debug");
                                    UpdateMessageStatus(messageid, updateStatus);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the status of the message in the Queue table to the correct status
        /// </summary>
        /// <param name="messageid">HL7 Message ID</param>
        /// <param name="status">Status to update to</param>
        private void UpdateMessageStatus(String messageid, int status)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendFormat("UPDATE tblDataHL7QueueADT SET intProcessFlag = {0}, dtmDateTimeProcessed = '{1}' WHERE vchMessageID = '{2}'", status, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), messageid);

                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql.ToString();

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }

            DeleteWholeMessage(messageid);
        }
    }
}