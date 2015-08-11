using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Battery_charger_tester_gui
{
    public partial class Form1 : Form
    {
        private LabelManager labelManager;
        private DataLogger dataLogger;
        private ConnectionManager connectionManager;
        private DataStorage dataStorage;
        private static Form1 form1;
        delegate void SetTextCallback(Label label, String text);
        delegate void displayAndButtonDelegate();
        delegate void textboxDelegate(String input);
       
        Boolean logstartClicked = false;


        // sound player, for when battery charging is done
        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"C:\WINDOWS\Media\notify.wav");
        private Form1()
        {
            InitializeComponent();
            Label[] labels = { label1, label2, label3, label4, label5, label6, label7, label8 };
            ArrayList labelList = new ArrayList(labels);
            this.labelManager = new LabelManager(labelList);
            this.dataLogger = DataLogger.getInstance(this);
            this.connectionManager = ConnectionManager.getInstance(this);
            this.dataStorage = DataStorage.getInstance();

            // logging rate selection
            foreach (int i in dataStorage.getLogRates())
            {
                comboBox3.Items.Add(i);
            }
            startLogging.Enabled = false;

            // button9 and button10 control the serial data transfer timeout enable
            // system starts with serialTiemoutEnable true, and the option to disable it on
            enableTimeout.Enabled = false; // button10 is enable
            disableTimeout.Enabled = true; // button9 is disable
            enableTimeout.Text = "";
            refreshData.Enabled = false;
            startLogging.Enabled = false;
            // richTextBox1 is where the serial output is displayed.

            // disable the logging buttons until the files exist
            wipeLogs.Enabled = false;
            startLogging.Enabled = false;
        }

        // method to create singleton instance of Form1
        public static Form1 getInstance()
        {
            if (form1 == null)
            {
                form1 = new Form1();
            }
            return form1;
        }

        // method to invoke to  update labels and log files, and update color of logging button
        public void dataAndButton()
        {
            if (InvokeRequired)
            {
                form1.BeginInvoke(new displayAndButtonDelegate(displaysAndButtonUpdate));
            }
            else displaysAndButtonUpdate();
        }
        private void displaysAndButtonUpdate()
        {
            if (startLogging.BackColor == Color.DarkGreen)
            {
                startLogging.BackColor = Color.LightGreen;
            }
            else
            {
                startLogging.BackColor = Color.DarkGreen;
            }
            Boolean updated = false;
            updated = refreshAllData();
            if (updated)
            {

            }
            else
            {
                /* tell syslog that data was NOT updated properly */
                try
                {
                    dataLogger.writeToLogFile(0, "Failed to update logs at " + DateTime.Now.ToString("h:mm:ss tt") + "\r");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                appendToRichTextBox1("Failed to update logs at " + DateTime.Now.ToString("h:mm:ss tt") + "\r");
            }
        }

        // updates the text in richTextBox1 with a string input
        public void appendToRichTextBox1(string input)
        {
            if (InvokeRequired)
            {
                richTextBox1.BeginInvoke(new textboxDelegate(appendToRichTextBox1), new object[] { input });
            }
            else
            {
                richTextBox1.AppendText(input);
            }
        }

        // sets the text of a label
        private void setLabel(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                SetTextCallback rx = new SetTextCallback(setLabel);
                Invoke(rx, new object[] { label, text });
            }
            else
            {
                label.Text = text;
            }
        }

        // combobox 3 is the log rate selection
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            startLogging.Enabled = true;
            startLogging.Text = "Start logging";
            label14.Text = Convert.ToString(dataStorage.logrates[comboBox3.SelectedIndex]);
            dataLogger.setLogRate(dataStorage.logrates[comboBox3.SelectedIndex]);

        }

        // button 1 opens the port
        private void button1_Click(object sender, EventArgs e) // button1 is the button to open the serial port
        {
            connectButton.Enabled = false;
            if ((connectButton.Text == "Connect") | (connectButton.Text == "Retry")) // on clicking open port, the following should happen
            {
                connectButton.Enabled = false;
                startLogging.Enabled = false;
                wipeLogs.Enabled = false;
                refreshData.Enabled = false;
                Boolean connected = connectionManager.connect();

                connectButton.Enabled = true;
                if (connected)
                {
                    connectButton.Text = "Disconnect";
                    connectButton.BackColor = Color.LightGreen;
                    refreshData.Enabled = true;
                    dataStorage.setNumADCChannels(dataStorage.getNumADCChannels());
                    wipeLogs.Enabled = true;
                }
                else
                {
                    connectButton.Text = "Retry";
                    connectButton.BackColor = Color.DarkRed;
                    appendToRichTextBox1("Error: did not receive handshake from MCU\r Not connected.\r");
                    startLogging.Enabled = false;
                    refreshData.Enabled = false;

                }
            }
            else if (connectButton.Text == "Reconnect")
            { // reconnect just opens the port that already worked.
                connectionManager.reconnect();
                connectButton.Text = "Disconnect";
                connectButton.BackColor = Color.LightGreen;
                startLogging.Enabled = true;
                refreshData.Enabled = true;
            }
            else if (connectButton.Text == "Disconnect")
            { // if button one says close port, disconnect
                connectionManager.disconnect();
                connectButton.Text = "Reconnect";
                connectButton.BackColor = Color.DarkOrange;
                startLogging.Enabled = false;
                refreshData.Enabled = false;
            }
            connectButton.Enabled = true;
        }

        // button2 clears the terminal window
        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        // start or stop the logger
        private void button6_Click(object sender, EventArgs e)
        {
            if (!logstartClicked)
            {
                dataLogger.prepareLogFiles();
                logstartClicked = true;
            }
            if (startLogging.Text == "Start logging" | startLogging.Text == "Start append")
            {
                dataLogger.startLogTimer(); // start the log timer, with default rate if not selected.
                startLogging.Enabled = true;
                startLogging.Text = "Stop logging";
                startLogging.BackColor = Color.DarkGreen;
                dataLogger.writeToLogFile(0, "Started logging data at "
                    + DateTime.Now.ToString("h:mm:ss tt") + " every " + dataStorage.logrates[comboBox3.SelectedIndex] + " ms.\r");
            }
            else if ((startLogging.Text == "Stop logging") | (startLogging.Text == "Error, WAT?"))
            {
                startLogging.BackColor = Color.DarkOrange;
                dataLogger.stopLogTimer();
                startLogging.Text = "Start append";
                /* log start append in system log */
                dataLogger.writeToLogFile(0, "Stopped logging data at "
                      + DateTime.Now.ToString("h:mm:ss tt") + " *****.\r");
            }
        }

        // button 7 clears the log data
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                dataLogger.clearLogs();
            }
            catch (System.NullReferenceException)
            {
                appendToRichTextBox1("Creating log files.\r");
                dataLogger.prepareLogFiles();
                dataLogger.clearLogs();
            }
            dataLogger.writeToLogFile(0, "Wiped logs at " + DateTime.Now.ToString("h:mm:ss tt") + "\r");
            appendToRichTextBox1("Cleared data log file.\r");
        }

        // button10 is enable for serial transfer timeout
        private void button10_Click(object sender, EventArgs e)
        {
            enableTimeout.Enabled = false;
            enableTimeout.Text = "";
            disableTimeout.Enabled = true;
            disableTimeout.Text = "Disable";
            groupBox22.BackColor = Color.DarkGreen;
            connectionManager.enableSerialTimeout();
        }

        // button9 is disable for serial timeout
        private void button9_Click(object sender, EventArgs e)
        {
            disableTimeout.Enabled = false;
            disableTimeout.Text = "";
            enableTimeout.Enabled = true;
            enableTimeout.Text = "Enable";
            groupBox22.BackColor = Color.DarkRed;
            connectionManager.disableSerialTimeout();
        }

        // Button12 is to refresh the ADC readings and duty cycle information
        private void button12_Click(object sender, EventArgs e)
        {
            // connectionManager.readADC(0);
            Boolean refreshed = refreshAllData();
            if (refreshed)
            {
                refreshData.Text = "Refresh Data";
            }
            else
            {
                refreshData.BackColor = Color.DarkRed;
                refreshData.Text = "Failed update";
            }
        }

        private Boolean refreshAllData()
        {
            Boolean freshADC = refreshADCData();
            if (freshADC)
            {
                Boolean displayUpdated = updateLabels();
                if (displayUpdated & freshADC)
                {
                    if (dataStorage.getVerbosity())
                    {
                        appendToRichTextBox1("Successfully refreshed.\r");
                    }
                    refreshData.BackColor = Color.DarkGreen;
                    return true;
                }
                else
                {
                    groupBox4.BackColor = Color.DarkRed;
                    label2.Text = "YES";
                    refreshData.BackColor = Color.DarkRed;
                    if (!freshADC)
                    {
                        appendToRichTextBox1("Failed to update ADC values.\r");
                        label3.Text = "ADC Error";
                    }
                    return false;
                }
            }
            else
            {
                appendToRichTextBox1("Failed to read ADC and duty cycle, are you connected?\r");
                dataLogger.writeToLogFile(0, "Failed to refresh all data readings, possible disconnect.\r");
                return false;
            }
        }

        // Calls refreshRegisterData and refreshADCData and then updates display with info
        private Boolean refreshADCData()
        {
            try
            {
                for (int i = 0; i < dataStorage.getNumADCChannels(); i++)
                {
                    try
                    {
                        connectionManager.readADC(i);
                        if ((dataStorage.getADCCount(i) > 65530) & (dataStorage.getVerbosity()))
                        {
                            appendToRichTextBox1("Channel " + i + " maximum count reached!\rMax ADC input voltage is 2.90V.\r");
                        }
                    }
                    catch (TimeoutException)
                    {
                        appendToRichTextBox1("Timed out reading ADC channel " + i + ", moving on\r");
                        connectionManager.refreshSerial();
                        player.Play();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                appendToRichTextBox1(ex.Message);
                return false;
            }
        }

        // update the text boxes with what is in the registers
        private Boolean updateLabels()   // returns a boolean if it updated successfully
        { // updates all displays on the GUI

            /********** now update the labels that display the measurements **************/
            try
            {
                for (int i = 0; i < dataStorage.getNumADCChannels(); i++)
                {
                    labelManager.setText("" + dataStorage.getDecimalValues(i) + " " + dataStorage.getUnits(i), i);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ADC values are null, connect to MCU first.");
                dataLogger.writeToLogFile(0, ex.Message);
                appendToRichTextBox1(ex.Message);
            }
            /* update background colors for boxes that should be within certain ranges */
            
            return true;
        }
    }
}
