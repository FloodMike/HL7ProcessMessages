using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class ECDSPersonToNotify
    {
        /// <summary>
        /// Working Variables
        /// </summary>
        private String urnECDSADMPAT;

        private String nameLast;
        private String nameFirst;
        private String nameMiddle;
        private String addrStreet1;
        private String addrStreet2;
        private String city;
        private String state;
        private String zip;
        private String homePhone;
        private String busPhone;
        private String relationship;
        private String name;
        private String currentTime;
        private String role;
        private int intSegID;

        /// <summary>
        /// Constructor
        /// </summary>
        public ECDSPersonToNotify()
        {
        }

        /// <summary>
        /// Initialize variables from the message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        /// <param name="i">Segment count</param>
        public void InitFromMessage(Message message, int i)
        {
            this.currentTime = DateTime.Now.ToString();

            Segment seg = message.GetSegmentByIndex(i);

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT vchDescription, vchSegment, intElement, intRepeat, intComponent, booIsRepeatable FROM tblluHL7ADTMessageMapping WHERE vchSegment IN ('PID', 'NK1')";

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    using (DataSet dataset = new DataSet())
                    {
                        da.Fill(dataset);
                        conn.Close();

                        if (dataset.Tables.Count > 0)
                        {
                            using (DataTable dt = dataset.Tables[0])
                            {
                                if (dt != null)
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            String segmnt = row["vchSegment"].ToString();
                                            String elem = row["intElement"].ToString();
                                            String rep = row["intRepeat"].ToString();
                                            String comp = row["intComponent"].ToString();

                                            switch (row["vchDescription"].ToString())
                                            {
                                                case "Account Number":
                                                    try
                                                    {
                                                        this.urnECDSADMPAT = message.GetElement(String.Format("{0}-{1}[{2}].{3}", segmnt, elem, rep, comp)).AsString;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Number":
                                                    try
                                                    {
                                                        this.intSegID = seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsInteger;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Last Name":
                                                    try
                                                    {
                                                        this.nameLast = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify First Name":
                                                    try
                                                    {
                                                        this.nameFirst = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Middle Name":
                                                    try
                                                    {
                                                        this.nameMiddle = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Address 1":
                                                    try
                                                    {
                                                        this.addrStreet1 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Address 2":
                                                    try
                                                    {
                                                        this.addrStreet2 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify City":
                                                    try
                                                    {
                                                        this.city = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify State":
                                                    try
                                                    {
                                                        this.state = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Zip":
                                                    try
                                                    {
                                                        this.zip = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Home Phone":
                                                    try
                                                    {
                                                        this.homePhone = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Business Phone":
                                                    try
                                                    {
                                                        this.busPhone = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Relationship":
                                                    try
                                                    {
                                                        this.relationship = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString, "");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Person to Notify Role":
                                                    try
                                                    {
                                                        this.role = Utils.NZ(seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to parse the ECDSPersonToNotify information for account number: {0}\n\t{1}", this.urnECDSADMPAT, e.StackTrace));
            }

            this.name = Utils.GetFullName(this.nameFirst, this.nameMiddle, this.nameLast);
            this.relationship = Utils.Lookup(ConfigurationManager.AppSettings["PersonToNotifyRelationshipLookup"], this.relationship, this.relationship);
        }

        /// <summary>
        /// Process the HL7 Message for Person to Notify information
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        /// <param name="message">HL7 Message</param>
        static public void Process(String vchMessageID, Message message)
        {
            for (int i = 1; i < message.SegmentCount - 1; i++)
            {
                Segment seg = message.GetSegmentByIndex(i);
                if (seg.Code == "NK1")
                {
                    ECDSPersonToNotify ptk = new ECDSPersonToNotify();
                    ptk.InitFromMessage(message, i);

                    try
                    {
                        if (ptk.Role.Equals(ConfigurationManager.AppSettings["PersonToNotifyRole"]) || String.IsNullOrWhiteSpace(ptk.Role))
                        {
                            ptk.InsertIntoTable();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to insert the Person To Notify information for account number: {0}", ptk.UrnECDSADMPAT));
                    }

                    try
                    {
                        Utils.Audit("HL7_ispECDSPersonToNotify", vchMessageID);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Audit the Person To Notify information for account number: {0}", ptk.UrnECDSADMPAT));
                    }

                    ptk = null;
                }
            }
        }

        /// <summary>
        /// Insert the Person to Notify data into the Database
        /// </summary>
        public void InsertIntoTable()
        {
            Dictionary<String, object> param = PopulateDictionary();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.HL7_ispECDSPersonToNotify";

                    foreach (String key in param.Keys)
                    {
                        cmd.Parameters.AddWithValue(key, param[key]);
                    }

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Dictionary containing all of the parameters and insert variables for the database
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, object> PopulateDictionary()
        {
            Dictionary<String, object> temp = new Dictionary<String, object>();

            temp.Add("@UrnECDSADMPAT", this.urnECDSADMPAT);
            temp.Add("@PtnName", this.name);
            temp.Add("@PtnAddrStreet1", this.addrStreet1);
            temp.Add("@PtnAddrStreet2", this.addrStreet2);
            temp.Add("@PtnCity", this.city);
            temp.Add("@PtnState", this.state);
            temp.Add("@PtnZip", this.zip);
            temp.Add("@PtnHomePhone", this.homePhone);
            temp.Add("@PtnBusPhone", this.busPhone);
            temp.Add("@PtnRelationship", this.relationship);
            temp.Add("@Insertdatetime", this.CurrentTime);
            temp.Add("@Updatedatetime", this.CurrentTime);
            temp.Add("@intSegID", this.intSegID);

            return temp;
        }

        public String CurrentTime { get { return this.currentTime; } }

        public String Role { get { return role; } }

        public String UrnECDSADMPAT { get { return urnECDSADMPAT; } }
    }
}