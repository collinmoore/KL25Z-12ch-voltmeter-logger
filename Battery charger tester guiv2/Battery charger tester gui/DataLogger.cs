using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Battery_charger_tester_gui
{
    class DataLogger
    {
        private static DataLogger instance;
        private ConnectionManager connectionManager;
        private string[] logfiles = { "Systemlog.txt", "valuesLog.csv" };
        private System.Timers.Timer logTimer;
        private Form1 form1;
        private DataStorage dataStorage;
        private int lograte; // default log rate tick timer in ms
        private long elapsedMillis;
        // Constructor for DataLogger
        private DataLogger()
        {
            this.dataStorage = DataStorage.getInstance();
            this.lograte = 1000; // default log rate
            this.elapsedMillis = 0;
            logTimer = new System.Timers.Timer();
        }

        // makes a new instance of instance, given a pointer to Form1
        private void setForm(Form1 form1)
        {
            this.form1 = form1;
        }

        private void setConnManager(ConnectionManager conn1)
        {
            this.connectionManager = conn1;
        }

        public static DataLogger getInstance(Form1 form1)
        {
            instance = getInstance();
            instance.setForm(form1);
            return instance;
        }

        public static DataLogger getInstance(ConnectionManager conn1)
        {
            instance = getInstance();
            instance.setConnManager(conn1);
            return instance;
        }

        // getInstance method for singleton instance
        public static DataLogger getInstance()
        {
            if (instance == null)
            {
                instance = new DataLogger();
            }
            return instance;
        }

        public void prepareLogFiles()
        {
            try
            {
                foreach (string n in logfiles)
                {
                    if (System.IO.File.Exists(n))
                    {
                        System.IO.File.Delete(n); // delete the old one if there is one.
                    }

                    System.IO.File.AppendAllText(n, "**** begin log file at "
                        + DateTime.Now.ToString("h:mm:ss tt") + " ****\r", Encoding.UTF8);

                }
                for (int i = 0; i < dataStorage.getNumADCChannels(); i++)
                {
                    writeToLogFile(1, "Ch " + (i + 1) + ",");
                }
                writeToLogFile(1, "Elapsed Milliseconds\r");
            }
            catch (System.IO.IOException ex)
            {
                form1.appendToRichTextBox1("Error preparing log files\r"+ex.Message + "\r");
            }
        }

        // clear measurement logs
        public void clearLogs()
        {
            try
            {
                if (System.IO.File.Exists(logfiles[1]))
                {
                    System.IO.File.Delete(logfiles[1]);
                }
                writeToLogFile(0, " ****************** Cleared data logs at "
                        + DateTime.Now.ToString("h:mm:ss tt") + " **************\r");
                for (int i = 0; i < dataStorage.getNumADCChannels(); i++)
                {
                    writeToLogFile(1, "Ch " + (i + 1) + ",");
                }
                writeToLogFile(1, "Elapsed Milliseconds\r");
            }
            catch (System.IO.IOException ex)
            {
                form1.appendToRichTextBox1("Error deleting log files\r"+ex.Message + "\r");
            }
        }

        // set the logging rate
        public void setLogRate(int tickRate)
        {
            this.lograte = tickRate;
            logTimer.Interval = tickRate;
        }

        // start the logging timer
        public void startLogTimer()
        {
            /*      TIMER FOR LOGGING       */
            logTimer.Elapsed += new ElapsedEventHandler(logTimer_Tick);
            logTimer.Interval = this.lograte; // timer interval in milliseconds.
            logTimer.Start();
        }

        // stop log timer
        public void stopLogTimer()
        {
            logTimer.Stop();
        }

        // timer tick event handler
        private void logTimer_Tick(object sender, EventArgs e)
        {
            elapsedMillis += (long)logTimer.Interval;
            if (dataStorage.getVerbosity())
            {
                form1.appendToRichTextBox1("Timer tick, refershing logs\r");
            }
            form1.dataAndButton(); // triggers update of all variables
            logData();
        }

        public void logData()
        {
            /* log each channel's value in decimal form. */
            for (int i = 1; i < dataStorage.getNumADCChannels(); i++)
            {
                writeToLogFile(1, dataStorage.getDecimalValues(i - 1) + ",");
            }
            writeToLogFile(1, elapsedMillis + "\r"); // log the elapsed time
            /* update system log file */
            if (dataStorage.getVerbosity())
            {
                form1.appendToRichTextBox1("Logged all ADC channel data at " + DateTime.Now.ToString("h:mm:ss tt") + "\r");
                writeToLogFile(0, "Logged all channel values at " + DateTime.Now.ToString("h:mm:ss tt") + "\r");
            }
        }

        // method to write to the specified logfile
        public void writeToLogFile(int index, string text)
        {
            if (index < logfiles.Length)
            {

                System.IO.File.AppendAllText(logfiles[index], text, Encoding.UTF8);
            }
            else
            {
                throw new Exception("logfile " + index + " does not exist.");
            }
        }
    }
}
