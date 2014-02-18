using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class ECDSPatient
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
        private String socSecNum;
        private String race;
        private String sex;
        private String religion;
        private String maritalStatus;
        private String maidenName;
        private String employer;
        private String occupation;
        private String empAddrStreet1;
        private String empAddrStreet2;
        private String empCity;
        private String empState;
        private String empZip;
        private String empPhone;
        private String ethnicity;
        private String birthDate;
        private String currentTime;
        private String name;

        /// <summary>
        /// Constructor
        /// </summary>
        public ECDSPatient()
        {
        }

        /// <summary>
        /// Process the ECDS Patient information
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        /// <param name="vchMessage">HL7 Message</param>
        static public void Process(String vchMessageID, Message vchMessage)
        {
            ECDSPatient patient = new ECDSPatient();
            patient.InitFromMessage(vchMessage);

            try
            {
                patient.InsertIntoTable();
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to Insert the ECDS Patient information for account number: {0}\n\t{1}", patient.UrnECDSADMPAT, e.Message));
            }

            try
            {
                Utils.Audit("HL7_ispECDSPatient", vchMessageID);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to Audit the ECDS Patient information for account number: {0}\n\t{1}", patient.UrnECDSADMPAT, e.Message));
            }

            patient = null;
        }

        /// <summary>
        /// Initialize data from the HL7 message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        public void InitFromMessage(Message message)
        {
            String guarSSN = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT vchDescription, vchSegment, intElement, intRepeat, intComponent, booIsRepeatable FROM tblluHL7ADTMessageMapping WHERE vchSegment IN ('PID', 'GT1')";

                    this.currentTime = DateTime.Now.ToString();

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

                                            switch (row["vchDescription"].ToString())
                                            {
                                                case "Account Number":
                                                    try
                                                    {
                                                        this.urnECDSADMPAT = message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Last Name":
                                                    try
                                                    {
                                                        this.nameLast = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient First Name":
                                                    try
                                                    {
                                                        this.nameFirst = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Middle Name":
                                                    try
                                                    {
                                                        this.nameMiddle = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Address 1":
                                                    try
                                                    {
                                                        this.addrStreet1 = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Address 2":
                                                    try
                                                    {
                                                        this.addrStreet2 = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient City":
                                                    try
                                                    {
                                                        this.city = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient State":
                                                    try
                                                    {
                                                        this.state = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Zip":
                                                    try
                                                    {
                                                        this.zip = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Home Phone":
                                                    try
                                                    {
                                                        this.homePhone = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient SSN":
                                                    try
                                                    {
                                                        this.socSecNum = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Race":
                                                    try
                                                    {
                                                        this.race = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Gender":
                                                    try
                                                    {
                                                        this.sex = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Religion":
                                                    try
                                                    {
                                                        this.religion = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Marital Status":
                                                    try
                                                    {
                                                        this.maritalStatus = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Maiden Name":
                                                    try
                                                    {
                                                        this.maidenName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Ethnicity":
                                                    try
                                                    {
                                                        this.ethnicity = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient DOB":
                                                    try
                                                    {
                                                        this.birthDate = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }

                                            if (message.CountSegment("GT1") > 0)
                                            {
                                                switch (row["vchDescription"].ToString())
                                                {
                                                    case "Guarantor SSN":
                                                        try
                                                        {
                                                            guarSSN = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer":
                                                        try
                                                        {
                                                            this.employer = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Occupation":
                                                        try
                                                        {
                                                            this.occupation = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer Address 1":
                                                        try
                                                        {
                                                            this.empAddrStreet1 = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer Address 2":
                                                        try
                                                        {
                                                            this.empAddrStreet2 = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer City":
                                                        try
                                                        {
                                                            this.empCity = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer State":
                                                        try
                                                        {
                                                            this.empState = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer Zip":
                                                        try
                                                        {
                                                            this.empZip = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Guarantor Employer Phone":
                                                        try
                                                        {
                                                            this.empPhone = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
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
                                            else
                                            {
                                                this.employer = "";
                                                this.occupation = "";
                                                this.empAddrStreet1 = "";
                                                this.empAddrStreet2 = "";
                                                this.empCity = "";
                                                this.empState = "";
                                                this.empZip = "";
                                                this.empPhone = "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    this.name = Utils.GetFullName(this.nameFirst, this.nameMiddle, this.nameLast);
                    this.birthDate = Utils.FormatDate(birthDate);
                    this.religion = Utils.Lookup(ConfigurationManager.AppSettings["ReligionLookup"], religion, religion);
                    this.race = Utils.Lookup(ConfigurationManager.AppSettings["RaceLookup"], race, race);
                    this.ethnicity = Utils.Lookup(ConfigurationManager.AppSettings["EthnicityLookup"], ethnicity, ethnicity);
                    this.sex = Utils.Lookup(ConfigurationManager.AppSettings["GenderLookup"], sex, sex);

                    if (!guarSSN.Equals(this.socSecNum))
                    {
                        this.employer = "";
                        this.occupation = "";
                        this.empAddrStreet1 = "";
                        this.empAddrStreet2 = "";
                        this.empCity = "";
                        this.empState = "";
                        this.empZip = "";
                        this.empPhone = "";
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to parse the ECDSPatient information for account number: {0}\n\t{1}", this.urnECDSADMPAT, e.Message));
            }
        }

        /// <summary>
        /// Inserts the ECDS patient data into the EmpowER database
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
                    cmd.CommandText = "dbo.HL7_ispECDSPatient";

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
        /// Sets up a dictionary of key value pairs that will be populated into the ECDS patient database of EmpowER
        /// </summary>
        /// <returns>Dictionary of variables</returns>
        private Dictionary<String, object> PopulateDictionary()
        {
            Dictionary<String, object> temp = new Dictionary<String, object>();

            temp.Add("@UrnECDSADMPAT", this.urnECDSADMPAT);
            temp.Add("@Name", this.name);
            temp.Add("@AddrStreet1", this.addrStreet1);
            temp.Add("@AddrStreet2", this.addrStreet2);
            temp.Add("@City", this.city);
            temp.Add("@State", this.state);
            temp.Add("@Zip", this.zip);
            temp.Add("@HomePhone", this.homePhone);
            temp.Add("@SocSecNum", this.socSecNum);
            temp.Add("@Race", this.race);
            temp.Add("@Sex", this.sex);
            temp.Add("@Religion", this.religion);
            temp.Add("@MaritalStatus", this.maritalStatus);
            temp.Add("@BirthDate", this.birthDate);
            temp.Add("@MaidenName", this.maidenName);
            temp.Add("@Employer", this.employer);
            temp.Add("@Occupation", this.occupation);
            temp.Add("@EmpAddrStreet1", this.empAddrStreet1);
            temp.Add("@EmpAddrStreet2", this.empAddrStreet2);
            temp.Add("@EmpCity", this.empCity);
            temp.Add("@EmpState", this.empState);
            temp.Add("@EmpZip", this.empZip);
            temp.Add("@EmpPhone", this.empPhone);
            temp.Add("@Insertdatetime", this.CurrentTime);
            temp.Add("@Updatedatetime", this.CurrentTime);
            temp.Add("@Ethnicity", this.ethnicity);

            return temp;
        }

        public String CurrentTime { get { return this.currentTime; } }

        public String UrnECDSADMPAT { get { return urnECDSADMPAT; } }
    }
}