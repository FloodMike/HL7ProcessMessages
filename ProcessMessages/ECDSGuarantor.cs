using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class ECDSGuarantor
    {
        private String addrStreet1;

        private String addrStreet2;

        private String busPhone;

        private String city;

        private String currentTime;

        private String employer;

        private String homePhone;

        /// <summary>
        /// Working Variables
        /// </summary>
        private int intSegID;

        private String name;
        private String nameFirst;
        private String nameLast;
        private String nameMiddle;
        private String occupation;
        private String relationship;
        private String socSecNum;
        private String state;
        private String urnECDSADMPAT;
        private String zip;
        /// <summary>
        /// Constructor
        /// </summary>
        public ECDSGuarantor()
        {
        }

        public String CurrentTime { get { return this.currentTime; } }

        public String UrnECDSADMPAT { get { return urnECDSADMPAT; } }

        /// <summary>
        /// Process all Guarantor segments
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        /// <param name="message">HL7 Message</param>
        public static void Process(String vchMessageID, Message message)
        {
            for (int i = 1; i < message.SegmentCount - 1; i++)
            {
                Segment seg = message.GetSegmentByIndex(i);
                if (seg.Code.CompareTo("GT1") == 0)
                {
                    ECDSGuarantor guar = new ECDSGuarantor();
                    guar.InitFromMessage(message, i);

                    try
                    {
                        guar.InsertIntoTable();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Insert the Guarantor information for account number: {0}\n\t{1}", guar.UrnECDSADMPAT, e.Message));
                    }

                    try
                    {
                        Utils.Audit("HL7_ispECDSGuarantor", vchMessageID);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Failed to Audit the Guarantor information for account number: {0}\n\t{1}", guar.UrnECDSADMPAT, e.Message));
                    }

                    guar = null;
                }
            }
        }

        /// <summary>
        /// Initialize variables from the HL7 message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        /// <param name="i">Guarantor index</param>
        public void InitFromMessage(Message message, int i)
        {
            this.currentTime = DateTime.Now.ToString();

            //Retrieve Patient Information
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT vchDescription, vchSegment, intElement, intRepeat, intComponent, booIsRepeatable FROM tblluHL7ADTMessageMapping WHERE vchSegment IN ('PID', 'GT1')";

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
                                            Segment seg = message.GetSegmentByIndex(i);

                                            this.intSegID = seg.GetElement("1.1").AsInteger;

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

                                                case "Guarantor Last Name":
                                                    try
                                                    {
                                                        this.nameLast = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor First Name":
                                                    try
                                                    {
                                                        this.nameFirst = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Middle Name":
                                                    try
                                                    {
                                                        this.nameMiddle = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Address 1":
                                                    try
                                                    {
                                                        this.addrStreet1 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Address 2":
                                                    try
                                                    {
                                                        this.addrStreet2 = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor City":
                                                    try
                                                    {
                                                        this.city = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor State":
                                                    try
                                                    {
                                                        this.state = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Zip":
                                                    try
                                                    {
                                                        this.zip = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Home Phone":
                                                    try
                                                    {
                                                        this.homePhone = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Business Phone":
                                                    try
                                                    {
                                                        this.busPhone = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor SSN":
                                                    try
                                                    {
                                                        this.socSecNum = Utils.NZ(seg.GetElement(String.Format("{0}.{1}", elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Relationship":
                                                    try
                                                    {
                                                        this.relationship = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Employer":
                                                    try
                                                    {
                                                        this.employer = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Guarantor Occupation":
                                                    try
                                                    {
                                                        this.occupation = Utils.NZ(seg.GetElement(String.Format("{0}[{1}].{2}", elem, rep, comp)).AsString);
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
                throw new Exception(String.Format("Failed to parse the ECDSGuarantor information for account number: {0}\n\t{1}", this.urnECDSADMPAT, e.StackTrace));
            }

            this.name = Utils.GetFullName(nameFirst, nameMiddle, nameLast);
            this.relationship = Utils.Lookup(ConfigurationManager.AppSettings["GuarantorRelationshipLookup"], this.relationship, this.relationship);
        }
        /// <summary>
        /// Insert Guarantor Information into the EmpowER database
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
                    cmd.CommandText = "dbo.HL7_ispECDSGuarantor";

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
        /// Create dictionare of all items to add to the EmpowER database
        /// </summary>
        /// <returns>Dictionary of DB entries</returns>
        private Dictionary<String, object> PopulateDictionary()
        {
            Dictionary<String, object> temp = new Dictionary<String, object>();

            temp.Add("@UrnECDSADMPAT", this.urnECDSADMPAT);
            temp.Add("@GuarName", this.name);
            temp.Add("@GuarAddrStreet1", this.addrStreet1);
            temp.Add("@GuarAddrStreet2", this.addrStreet2);
            temp.Add("@GuarCity", this.city);
            temp.Add("@GuarState", this.state);
            temp.Add("@GuarZip", this.zip);
            temp.Add("@GuarHomePhone", this.homePhone);
            temp.Add("@GuarBusPhone", this.busPhone);
            temp.Add("@GuarSocSecNum", this.socSecNum);
            temp.Add("@GuarRelationship", this.relationship);
            temp.Add("@GuarEmployer", this.employer);
            temp.Add("@GuarOccupation", this.occupation);
            temp.Add("@Insertdatetime", this.CurrentTime);
            temp.Add("@Updatedatetime", this.CurrentTime);
            temp.Add("@intSegID", this.intSegID);

            return temp;
        }
    }
}