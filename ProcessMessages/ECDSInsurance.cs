using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class ECDSInsurance
    {
        /// <summary>
        /// Working Variables
        /// </summary>
        private String urnECDSAdmPat;

        private String insMnemonic;
        private String insOrderECDSAdmPat;
        private String insuranceName;
        private String subscriberLast;
        private String subscriberFirst;
        private String subscriberMiddle;
        private String subscriberRelationship;
        private String insAddrStreet1;
        private String insAddrStreet2;
        private String insCity;
        private String insState;
        private String insZip;
        private String insPhoneNumber;
        private String policyNumber;
        private String groupNumber;
        private String authNumber;
        private String financialClass;
        private String name;
        private String currentTime;

        /// <summary>
        /// Constructor
        /// </summary>
        public ECDSInsurance()
        {
        }

        /// <summary>
        /// Process the Patient Insurance Information
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        /// <param name="message">HL7 Message</param>
        public static void Process(String vchMessageID, Message message)
        {
            for (int i = 1; i < message.SegmentCount - 1; i++)
            {
                Segment seg = message.GetSegmentByIndex(i);
                if (seg.Code == "IN1")
                {
                    ECDSInsurance ecdsIns = new ECDSInsurance();
                    ecdsIns.InitFromMessage(message, i);

                    try
                    {
                        ecdsIns.InsertIntoTable();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Insert the Insurance information for account number: {0}\n\t{1}", ecdsIns.UrnECDSAdmPat, e.Message));
                    }

                    try
                    {
                        Utils.Audit("HL7_ispECDSInsurance", vchMessageID);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Audit the Insurance information for account number: {0}\n\t{1}", ecdsIns.UrnECDSAdmPat, e.Message));
                    }

                    ecdsIns = null;
                }
            }
        }

        /// <summary>
        /// Initialize data from the HL7 message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        /// <param name="i">Insurance Number</param>
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
                    cmd.CommandText = "SELECT vchDescription, vchSegment, intElement, intRepeat, intComponent, booIsRepeatable FROM tblluHL7ADTMessageMapping WHERE vchSegment IN ('PID', 'IN1', 'PV1')";

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

                                                case "Financial Class":
                                                    try
                                                    {
                                                        this.financialClass = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", segmnt, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Mnemonic":
                                                    try
                                                    {
                                                        this.insMnemonic = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Number":
                                                    try
                                                    {
                                                        this.insOrderECDSAdmPat = Utils.NZ(seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Name":
                                                    try
                                                    {
                                                        this.insuranceName = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Subscriber Last Name":
                                                    try
                                                    {
                                                        this.subscriberLast = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Subscriber First Name":
                                                    try
                                                    {
                                                        this.subscriberFirst = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Subscriber Middle Name":
                                                    try
                                                    {
                                                        this.subscriberMiddle = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Subscriber Relationship":
                                                    try
                                                    {
                                                        this.subscriberRelationship = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Address 1":
                                                    try
                                                    {
                                                        this.insAddrStreet1 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Address 2":
                                                    try
                                                    {
                                                        this.insAddrStreet2 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance City":
                                                    try
                                                    {
                                                        this.insCity = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance State":
                                                    try
                                                    {
                                                        this.insState = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Zip":
                                                    try
                                                    {
                                                        this.insZip = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Insurance Phone":
                                                    try
                                                    {
                                                        this.insPhoneNumber = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Policy Number":
                                                    try
                                                    {
                                                        this.policyNumber = Utils.NZ(seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Group Number":
                                                    try
                                                    {
                                                        this.groupNumber = Utils.NZ(seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Auth Number":
                                                    try
                                                    {
                                                        this.authNumber = Utils.NZ(seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsString);
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
                throw new Exception(String.Format("Failed to parse the ECDSInsurance information for account number: {0}\n\t{1}", this.urnECDSAdmPat, e.StackTrace));
            }

            // format the parameters before inserting
            this.name = Utils.GetFullName(this.subscriberFirst, this.subscriberMiddle, this.subscriberLast);
        }

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
                    cmd.CommandText = "dbo.HL7_ispECDSInsurance";

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
            temp.Add("@InsMnemonic", this.insMnemonic);
            temp.Add("@InsOrderECDSADMPAT", this.insOrderECDSAdmPat);
            temp.Add("@InsuranceName", this.insuranceName);
            temp.Add("@Subscriber", this.name);
            temp.Add("@SubscriberRelationship", this.subscriberRelationship);
            temp.Add("@FinClass", this.financialClass);
            temp.Add("@InsAddrStreet1", this.insAddrStreet1);
            temp.Add("@InsAddrStreet2", this.insAddrStreet2);
            temp.Add("@InsCity", this.insCity);
            temp.Add("@InsState", this.insState);
            temp.Add("@InsZip", this.insZip);
            temp.Add("@InsPhoneNumber", this.insPhoneNumber);
            temp.Add("@PolicyNumber", this.policyNumber);
            temp.Add("@GroupNumber", this.groupNumber);
            temp.Add("@AuthNumber", this.authNumber);
            temp.Add("@Insertdatetime", this.CurrentTime);
            temp.Add("@Updatedatetime", this.CurrentTime);

            return temp;
        }

        public String CurrentTime { get { return this.currentTime; } }

        public String UrnECDSAdmPat { get { return urnECDSAdmPat; } }
    }
}