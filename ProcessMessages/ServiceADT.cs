using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using HL7Connect.Services;

namespace ProcessMessages
{
    partial class ServiceADT : ServiceBase
    {
        /// <summary>
        /// Working variables
        /// </summary>
        private Timer timerADT;

        private Library library;

        /// <summary>
        /// Constructor
        /// </summary>
        public ServiceADT()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Starts the Service and Initializes the timer thread
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            library = new Library();
            library.InitLocal(Path.Combine(Path.GetTempPath(), ConfigurationManager.AppSettings["HL7ConnectTypeLibraryLocation"]),
                              null, null, ConfigurationManager.AppSettings["HL7ConnectInitPath"]);

            InitializeThreads();
        }

        /// <summary>
        /// Destroys the timer thread when the service stops
        /// </summary>
        protected override void OnStop()
        {
            timerADT.Enabled = false;
            timerADT.Dispose();
            timerADT = null;
        }

        /// <summary>
        /// Initialize timer thread
        /// </summary>
        private void InitializeThreads()
        {
            if (Boolean.Parse(ConfigurationManager.AppSettings["ADTQueue"]))
            {
                timerADT = new Timer();
                timerADT.AutoReset = false;
                timerADT.Interval = Int32.Parse(ConfigurationManager.AppSettings["ADTTimeout"]);
                timerADT.Elapsed += new ElapsedEventHandler(timerADT_Elapsed);
                timerADT.Start();
            }
        }

        /// <summary>
        /// Initialize the QueueADT and start the timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerADT_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sender is Timer)
            {
                GC.Collect();
                Timer thisTimer = (Timer)sender;
                QueueADT qADT = new QueueADT(library);
                thisTimer.Start();
            }
        }
    }
}