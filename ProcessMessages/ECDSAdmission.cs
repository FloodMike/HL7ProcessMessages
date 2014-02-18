using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class ECDSAdmission
    {
        /// <summary>
        /// Working Variables
        /// </summary>
        private String urnECDSAdmPat;

        private String accountNumber;
        private String unitNumber;
        private String ecdsNumber;
        private String emrMDCode;
        private String primCareMDCode;
        private String admitClerk;
        private String admitComment;
        private String reasonForVisit;
        private String otherDoctorCode;
        private String emrMDLastName;
        private String emrMDFirstName;
        private String otherDoctorLastName;
        private String otherDoctorFirstName;
        private String primCareMDLastName;
        private String primCareMDFirstName;
        private String otherDoc;
        private String primCare;
        private String emrMD;
        private String admitTime;
        private String admitDate;
        private String currentTime;

        /// <summary>
        /// Constructor
        /// </summary>
        public ECDSAdmission()
        {
        }

        /// <summary>
        /// Process the ECDS Admission information
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        /// <param name="vchMessage">HL7 Message</param>
        static public void Process(String vchMessageID, Message vchMessage)
        {
            ECDSAdmission ecdsAdmit = new ECDSAdmission();

            ecdsAdmit.InitFromMessage(vchMessage);

            try
            {
                ecdsAdmit.InsertIntoTable();
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to Insert the Admission information for account number: {0}\n\t{1}", ecdsAdmit.AccountNumber, e.Message));
            }

            try
            {
                Utils.Audit("HL7_ispECDSAdmission", vchMessageID);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to Audit the Admission information for account number: {0}\n\t{1}", ecdsAdmit.AccountNumber, e.Message));
            }

            ecdsAdmit = null;
        }

        /// <summary>
        /// Initialize data from the HL7 message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        public void InitFromMessage(Message message)
        {
            this.currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT vchDescription, vchSegment, intElement, intRepeat, intComponent, booIsRepeatable FROM tblluHL7ADTMessageMapping WHERE vchSegment IN ('PID', 'EVN', 'PV1', 'PV2')";

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
                                            String seg = row["vchSegment"].ToString();
                                            String elem = row["intElement"].ToString();
                                            String rep = row["intRepeat"].ToString();
                                            String comp = row["intComponent"].ToString();

                                            //Parse the patients information from the message
                                            switch (row["vchDescription"].ToString())
                                            {
                                                case "Account Number":
                                                    try
                                                    {
                                                        this.urnECDSAdmPat = message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString;
                                                        this.accountNumber = message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString;
                                                        this.ecdsNumber = message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Medical Record Number":
                                                    try
                                                    {
                                                        this.unitNumber = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Admit DateTime":
                                                    try
                                                    {
                                                        this.admitDate = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        this.admitTime = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Attending MD Code":
                                                    try
                                                    {
                                                        this.emrMDCode = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Primary MD Code":
                                                    try
                                                    {
                                                        this.primCareMDCode = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Admit Clerk":
                                                    try
                                                    {
                                                        this.admitClerk = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Admit Comment":
                                                    try
                                                    {
                                                        this.admitComment = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Chief Complaint":
                                                    try
                                                    {
                                                        this.reasonForVisit = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        this.reasonForVisit = "";
                                                    }
                                                    break;

                                                case "Other MD Code":
                                                    try
                                                    {
                                                        this.otherDoctorCode = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Attending MD Last Name":
                                                    try
                                                    {
                                                        this.emrMDLastName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Attending MD First Name":
                                                    try
                                                    {
                                                        this.emrMDFirstName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Other MD Last Name":
                                                    try
                                                    {
                                                        this.otherDoctorLastName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Other MD First Name":
                                                    try
                                                    {
                                                        this.otherDoctorFirstName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Primary MD Last Name":
                                                    try
                                                    {
                                                        this.primCareMDLastName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Primary MD First Name":
                                                    try
                                                    {
                                                        this.primCareMDFirstName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
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

                                        this.admitDate = (!String.IsNullOrWhiteSpace(this.admitDate)) ? Utils.FormatDateTime(this.admitDate) : DateTime.Now.ToString();
                                        this.admitTime = (!String.IsNullOrWhiteSpace(this.admitTime)) ? Utils.FormatDateTime(this.admitTime) : DateTime.Now.ToString();

                                        try
                                        {
                                            this.emrMD = GetFullNameWithCode(this.emrMDLastName, this.emrMDFirstName, this.emrMDCode);
                                            this.primCare = GetFullNameWithCode(this.primCareMDLastName, this.primCareMDFirstName, this.primCareMDCode);
                                            this.otherDoc = GetFullNameWithCode(this.otherDoctorLastName, this.otherDoctorFirstName, this.otherDoctorCode);
                                        }
                                        catch (Exception e)
                                        {
                                            throw new Exception("Formatting Doctor name failed.");
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
                throw new Exception(String.Format("Failed to parse the ECDSAdmission information for account number: {0}\n\t{1}", this.urnECDSAdmPat, e.Message));
            }
        }

        /// <summary>
        /// Inserts the ECDS admission data into the EmpowER database
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
                    cmd.CommandText = "dbo.HL7_ispECDSAdmission";

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
        /// Sets up a dictionary of key value pairs that will be populated into the ECDS admission database of EmpowER
        /// </summary>
        /// <returns>Dictionary of variables</returns>
        private Dictionary<String, object> PopulateDictionary()
        {
            Dictionary<String, object> temp = new Dictionary<String, object>();

            temp.Add("@UrnECDSADMPAT", this.urnECDSAdmPat);
            temp.Add("@AccountNumber", this.accountNumber);
            temp.Add("@UnitNumber", this.unitNumber);
            temp.Add("@ECDSNumber", this.ecdsNumber);
            temp.Add("@AdmitDate", this.admitDate);
            temp.Add("@AdmitTime", this.admitTime);
            temp.Add("@EmrMd", this.emrMD);
            temp.Add("@PrimCareMd", this.primCare);
            temp.Add("@AdmitClerk", this.admitClerk);
            temp.Add("@AdmitComment", this.admitComment);
            temp.Add("@ReasonForVisit", this.reasonForVisit);
            temp.Add("@OtherDoctor", this.otherDoc);
            temp.Add("@Insertdatetime", this.CurrentTime);
            temp.Add("@Updatedatetime", this.CurrentTime);

            return temp;
        }

        /// <summary>
        /// Formats the name of a Doctor with a doctor code
        /// </summary>
        /// <param name="vchNameLast">String for Last Name</param>
        /// <param name="vchNameFirst">String for First Name</param>
        /// <param name="vchNameMiddle">String for Middle Name</param>
        /// <param name="vchCode">Doctor code</param>
        /// <returns>Formatted Doctor name and code</returns>
        private static String GetFullNameWithCode(String vchNameLast, String vchNameFirst, String vchCode, String vchNameMiddle = "")
        {
            String output;

            if (vchNameMiddle.Equals("") || vchNameMiddle == null)
            {
                output = String.Format("{0}, {1}", vchNameLast, vchNameFirst);
            }
            else
            {
                output = String.Format("{0}, {1} {2}", vchNameLast, vchNameFirst, vchNameMiddle);
            }

            if (vchCode != null && !vchCode.Equals(""))
            {
                output = String.Format("{0} {1}", vchCode, output);
            }

            return output;
        }

        public String CurrentTime { get { return this.currentTime; } }

        public String AccountNumber { get { return accountNumber; } }
    }
}