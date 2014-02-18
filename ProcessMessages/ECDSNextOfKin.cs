using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class ECDSNextOfKin
    {
        /// <summary>
        /// Working Variables
        /// </summary>
        private String urnECDSAdmPat;

        private String lastName;
        private String firstName;
        private String middleName;
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
        public ECDSNextOfKin()
        {
        }

        /// <summary>
        /// Process the Next of Kin information for a given HL7 Message
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        /// <param name="message">HL7 Message</param>
        public static void Process(String vchMessageID, Message message)
        {
            for (int i = 1; i < message.SegmentCount - 1; i++)
            {
                Segment seg = message.GetSegmentByIndex(i);
                if (seg.Code == "NK1")
                {
                    ECDSNextOfKin nok = new ECDSNextOfKin();
                    nok.InitFromMessage(message, i);

                    try
                    {
                        if (nok.Role.Equals(ConfigurationManager.AppSettings["NextOfKinRole"]) || String.IsNullOrWhiteSpace(nok.Role))
                        {
                            nok.InsertIntoTable();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Insert the Next of Kin information for account number: {0}\n\t{1}", nok.UrnECDSAdmPat, e.Message));
                    }

                    try
                    {
                        Utils.Audit("HL7_ispECDSNextOfKin", vchMessageID);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Audit the Next of Kin information for account number: {0}\n\t{1}", nok.UrnECDSAdmPat, e.Message));
                    }

                    nok = null;
                }
            }
        }

        /// <summary>
        /// Initialize data from the HL7 message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        /// <param name="i">Next of Kin Segment Number</param>
        public void InitFromMessage(Message message, int i)
        {
            this.currentTime = DateTime.Now.ToString();

            //Retrieve Patient Information
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
                                                        this.urnECDSAdmPat = message.GetElement(String.Format("{0}-{1}[{2}].{3}", segmnt, elem, rep, comp)).AsString;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Number":
                                                    try
                                                    {
                                                        this.intSegID = seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsInteger;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Last Name":
                                                    try
                                                    {
                                                        this.lastName = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin First Name":
                                                    try
                                                    {
                                                        this.firstName = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Middle Name":
                                                    try
                                                    {
                                                        this.middleName = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Address 1":
                                                    try
                                                    {
                                                        this.addrStreet1 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Address 2":
                                                    try
                                                    {
                                                        this.addrStreet2 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin City":
                                                    try
                                                    {
                                                        this.city = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin State":
                                                    try
                                                    {
                                                        this.state = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Zip":
                                                    try
                                                    {
                                                        this.zip = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Home Phone":
                                                    try
                                                    {
                                                        this.homePhone = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Business Phone":
                                                    try
                                                    {
                                                        this.busPhone = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Relationship":
                                                    try
                                                    {
                                                        this.relationship = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Next of Kin Role":
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
                throw new Exception(String.Format("Failed to parse the ECDSNextOfKin information for account number: {0}\n\t{1}", this.urnECDSAdmPat, e.StackTrace));
            }

            // format parameters
            this.name = Utils.GetFullName(this.firstName, this.middleName, this.lastName);
            this.relationship = Utils.Lookup(ConfigurationManager.AppSettings["NextOfKinRelationshipLookup"], this.relationship, this.relationship);
        }

        /// <summary>
        /// Insert the Next of Kin information into the database
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
                    cmd.CommandText = "dbo.HL7_ispECDSNextOfKin";

                    foreach (String key in param.Keys)
                    {
                        cmd.Parameters.AddWithValue(key, param[key]);
                    }

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
        }

        private Dictionary<String, object> PopulateDictionary()
        {
            Dictionary<String, object> temp = new Dictionary<String, object>();

            temp.Add("@UrnECDSADMPAT", this.urnECDSAdmPat);
            temp.Add("@NokName", this.name);
            temp.Add("@NokAddrStreet1", this.addrStreet1);
            temp.Add("@NokAddrStreet2", this.addrStreet2);
            temp.Add("@NokCity", this.city);
            temp.Add("@NokState", this.state);
            temp.Add("@NokZip", this.zip);
            temp.Add("@NokHomePhone", this.homePhone);
            temp.Add("@NokBusPhone", this.busPhone);
            temp.Add("@NokRelationship", this.relationship);
            temp.Add("@Insertdatetime", this.CurrentTime);
            temp.Add("@Updatedatetime", this.CurrentTime);
            temp.Add("@intSegID", this.intSegID);

            return temp;
        }

        public String CurrentTime { get { return this.currentTime; } }

        public String UrnECDSAdmPat { get { return urnECDSAdmPat; } }

        public String Role { get { return role; } }
    }
}