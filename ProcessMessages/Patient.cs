using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using HL7Connect.V2;

namespace ProcessMessages
{
    internal class Patient
    {
        /// <summary>
        /// Working variables
        /// </summary>
        private String account;

        private String admitSource;
        private String admitType;
        private String bed;
        private String consultingCode;
        private String dateOfBirth;
        private String dtmCurrent;
        private String dtmEntered;
        private String examRoom;
        private String firstName;
        private String gender;
        private String lastName;
        private String location;
        private long masterMRN;
        private String mdCode;
        private String medicalRecord;
        private String messageID;
        private String middleName;
        private String patientType;
        private String registered;
        private String service;
        private String system;
        private String type;
        /// <summary>
        /// Patient constructor
        /// </summary>
        /// <param name="vchMessageID">HL7 Message ID</param>
        public Patient(String vchMessageID)
        {
            this.messageID = vchMessageID;
        }

        public String Account { get { return account; } }

        public String CurrentTime { get { return dtmCurrent; } }

        /// <summary>
        /// Process the patient information
        /// </summary>
        /// <param name="messageID">HL7 Message Identification number</param>
        /// <param name="message">HL7 Message</param>
        public static void Process(String messageID, Message message)
        {
            Patient patient = new Patient(messageID);
            patient.GetParameters(message);

            String valid = patient.Validate();

            if (String.IsNullOrWhiteSpace(valid))
            {
                try
                {
                    patient.InsertIntoTable();
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Failed to Insert the Patient information for account number: {0}\n\t{1}", patient.Account, e.StackTrace));
                }

                //Audit the Call
                try
                {
                    Utils.Audit("HL7_isptblDataPatients", messageID);
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Failed to Audit the Patient information for account number: {0}\n\t{1}", patient.Account, e.Message));
                }
            }
            else
            {
                throw new Exception(valid);
            }

            patient = null;
        }

        /// <summary>
        /// Retrieves the necessary information from the HL7 message
        /// </summary>
        /// <param name="message">HL7 Message</param>
        public void GetParameters(Message message)
        {
            dtmCurrent = DateTime.Now.ToString();

            //parse the parameters out of the message
            ParseParameters(message);

            //Format Parameters before Inserting
            this.admitSource = Utils.Lookup(ConfigurationManager.AppSettings["AdmissionSourceLookup"], admitSource, admitSource);
            this.admitType = Utils.Lookup(ConfigurationManager.AppSettings["AdmissionTypeLookup"], admitType, admitType);
            this.type = Utils.Lookup(ConfigurationManager.AppSettings["PatientTypeLookup"], type, type);
            this.gender = Utils.Lookup(ConfigurationManager.AppSettings["GenderLookup"], gender, "Unknown");
            this.service = Utils.Lookup(ConfigurationManager.AppSettings["HospitalServiceLookup"], service, service);

            if (this.dateOfBirth != null)
            {
                this.dateOfBirth = Utils.FormatDate(this.dateOfBirth);
            }

            try
            {
                // Get Master MRN By Local MRN
                if (String.IsNullOrWhiteSpace(this.medicalRecord))
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            conn.Open();

                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "HL7_getMasterMRNByLocalMRN";
                            cmd.Parameters.AddWithValue("@vchMRN", this.medicalRecord);

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
                                            if (dt != null)
                                            {
                                                if (dt.Rows.Count > 0)
                                                {
                                                    long number;

                                                    if (dt.Rows[0]["lngMasterMRN"].ToString() != "" && long.TryParse(dt.Rows[0]["lngMasterMRN"].ToString(), out number))
                                                    {
                                                        this.masterMRN = long.Parse(dt.Rows[0]["lngMasterMRN"].ToString());
                                                    }
                                                    else
                                                    {
                                                        this.masterMRN = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    this.masterMRN = 0;
                                                }
                                            }
                                            else
                                            {
                                                this.masterMRN = 0;
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
                throw new Exception(String.Format("Failed to process local MRN functions for account number: {0}\n\t{1}", this.account, e.Message));
            }

            try
            {
                // Get Master MRN By Demographics
                if (this.masterMRN == 0 && !this.firstName.ToLower().Contains("baby"))
                {
                    String[] parameters = new String[] { "@vchFName", "@vchMName", "@vchLName", "@vchGender", "@vchDOB" };
                    String[] values = new String[] { this.firstName, Utils.NZ(this.middleName), this.lastName, this.gender, Utils.NZ(this.dateOfBirth.ToString()) };

                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            conn.Open();

                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "HL7_getMasterMRNByDemographics";

                            for (int i = 0; i < parameters.Length; i++)
                            {
                                cmd.Parameters.AddWithValue(parameters[i], values[i]);
                            }

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
                                            if (dt != null)
                                            {
                                                if (dt.Rows.Count > 0)
                                                {
                                                    if (dt.Rows[0]["lngMasterMRN"].ToString() != null && dt.Rows[0]["lngMasterMRN"].ToString() != "")
                                                    {
                                                        this.masterMRN = long.Parse(dt.Rows[0]["lngMasterMRN"].ToString());
                                                    }
                                                    else
                                                    {
                                                        this.masterMRN = 0;
                                                    }
                                                }
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
                throw new Exception(String.Format("Failed to process demographic MRN functions for account number: {0}\n\t{1}", this.account, e.Message));
            }

            if (type.IndexOf(ConfigurationManager.AppSettings["QuickRegIdentifier"]) >= 0)
            {
                registered = "Q";
            }
            else
            {
                registered = "R";
            }
        }

        /// <summary>
        /// Inserts the patient data into the EMPOWER_IDS database
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
                    cmd.CommandText = "dbo.HL7_isptblDataPatients";

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
        /// Breaks up each message and gathers the data from the designated field
        /// </summary>
        /// <param name="message">HL7 Message</param>
        public void ParseParameters(Message message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EMPOWER_IDS"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT vchDescription, vchSegment, intElement, intRepeat, intComponent FROM tblluHL7ADTMessageMapping WHERE vchSegment IN ('PID', 'PV1')";

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
                                                case "Medical Record Number":
                                                    try
                                                    {
                                                        this.medicalRecord = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Last Name":
                                                    try
                                                    {
                                                        this.lastName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient First Name":
                                                    try
                                                    {
                                                        this.firstName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Middle Name":
                                                    try
                                                    {
                                                        this.middleName = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient Gender":
                                                    try
                                                    {
                                                        this.gender = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Account Number":
                                                    try
                                                    {
                                                        this.account = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                case "Patient DOB":
                                                    try
                                                    {
                                                        this.dateOfBirth = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }

                                            if (message.CountSegment("PV1") > 0)
                                            {
                                                //this.dtmDateTimeEntered = message.GetElement("PV1-44.1").AsString;

                                                switch (row["vchDescription"].ToString())
                                                {
                                                    case "Location":
                                                        try
                                                        {
                                                            this.location = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                            this.location = Utils.Lookup(ConfigurationManager.AppSettings["HospitalLocationLookup"], this.location, this.location);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Room":
                                                        try
                                                        {
                                                            this.examRoom = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Bed":
                                                        try
                                                        {
                                                            this.bed = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Attending MD Code":
                                                        try
                                                        {
                                                            this.mdCode = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Consulting Code":
                                                        try
                                                        {
                                                            this.consultingCode = Utils.NZ(message.GetElement(String.Format("{0}-{1}[{2}].{3}", seg, elem, rep, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Patient Class":
                                                        try
                                                        {
                                                            this.patientType = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Hospital Service":
                                                        try
                                                        {
                                                            this.service = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Patient Type":
                                                        try
                                                        {
                                                            this.type = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Admit Type":
                                                        try
                                                        {
                                                            this.admitType = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Admit Source":
                                                        try
                                                        {
                                                            this.admitSource = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            throw new Exception(String.Format("Could not gather {0} from message", row["vchDescription"].ToString()));
                                                        }
                                                        break;

                                                    case "Admit DateTime":
                                                        try
                                                        {
                                                            this.dtmEntered = Utils.NZ(message.GetElement(String.Format("{0}-{1}.{2}", seg, elem, comp)).AsString);
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
                                                this.location = "";
                                                this.examRoom = "";
                                                this.bed = "";
                                                this.mdCode = "";
                                                this.consultingCode = "";
                                                this.patientType = "";
                                                this.service = "";
                                                this.type = "";
                                                this.admitType = "";
                                                this.admitSource = "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                this.system = ConfigurationManager.AppSettings["HospitalSystem"];
                //this.Registered = mapTable["vchValue"].ToString();
                if (!String.IsNullOrWhiteSpace(this.dtmEntered))
                {
                    this.dtmEntered = Utils.FormatDateTime(this.dtmEntered);
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Failed to parse the Patient information for account number: {0}\n\t{1}", this.account, e.StackTrace));
            }
        }

        /// <summary>
        /// Validates that the data is correct before insertting it into the database table
        /// </summary>
        /// <returns>Blank string if all is okay or the exception message to be thrown</returns>
        public String Validate()
        {
            String valid = "";
            StringBuilder build = new StringBuilder();

            if (String.IsNullOrWhiteSpace(this.account))
            {
                build.AppendFormat("Missing PatientID for message: {0}", this.messageID);
            }

            if (String.IsNullOrWhiteSpace(this.medicalRecord))
            {
                build.AppendFormat(valid, String.Format("\nMissing Medical Record Number for message: {0}", this.messageID));
            }

            if (String.IsNullOrWhiteSpace(this.lastName))
            {
                build.AppendFormat(valid, String.Format("\nMissing Patient Last Name for message: {0}", this.messageID));
            }

            if (String.IsNullOrWhiteSpace(this.firstName))
            {
                build.AppendFormat(valid, String.Format("\nMissing Patient First Name for message: {0}", this.messageID));
            }

            if (!String.IsNullOrWhiteSpace(build.ToString()))
                valid = build.ToString();

            return valid;
        }
        /// <summary>
        /// Sets up a dictionary of key value pairs that will be populated into the patient database of EmpowER
        /// </summary>
        /// <returns>Dictionary of variables</returns>
        private Dictionary<String, object> PopulateDictionary()
        {
            Dictionary<String, object> temp = new Dictionary<String, object>();

            temp.Add("@vchAccount", this.account);
            temp.Add("@vchMedicalRecord", this.medicalRecord);
            temp.Add("@vchNameLast", this.lastName);
            temp.Add("@vchNameFirst", this.firstName);
            temp.Add("@vchNameMiddle", this.middleName);
            temp.Add("@vchGender", this.gender);
            temp.Add("@vchPatientID", this.account);
            temp.Add("@dtmDateTimeEntered", this.dtmEntered);
            temp.Add("@vchDob", this.dateOfBirth);
            temp.Add("@vchLocation", this.location);
            temp.Add("@vchExamRoom", this.examRoom);
            temp.Add("@vchBed", this.bed);
            temp.Add("@vchMDCode", this.mdCode);
            temp.Add("@vchConsultingCode", this.consultingCode);
            temp.Add("@vchPatientType", this.patientType);
            temp.Add("@vchService", this.service);
            temp.Add("@vchType", this.type);
            temp.Add("@vchAdmitType", this.admitType);
            temp.Add("@vchAdmitSource", this.admitSource);
            temp.Add("@lngMasterMRN", this.masterMRN);
            temp.Add("@vchSystem", this.system);
            temp.Add("@vchRegistered", this.registered);

            return temp;
        }
    }
}