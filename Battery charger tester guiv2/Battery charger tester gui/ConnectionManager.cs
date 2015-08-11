using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports; // for using the serial ports
using System.Windows.Forms;
using System.Threading;




namespace Battery_charger_tester_gui
{
    class ConnectionManager
    {
        /*                       CONSTANTS                */
        // Packet values defined in TxPacket and RxPacket for each type of packet. \\
        // delegate to parse incoming data
        delegate void parseDataDelegate(Byte command, Byte regaddr, Byte value);
        // serial transfer variables
        Boolean serialTimeoutEnable = true; // use this to enable/disable timeouts on serial transfers
        private SerialPort serialPort;
        private int[] baudValues = { 2400, 4800, 7200, 9600, 14400, 19200, 38400, 57600, 115200 }; // 128000 gives parameter incorrect for serial port
        private int serialTimeoutCounter = 0;
        private int serialTimeout = 200;  /*******************  ms to time out a serial transaction *****/
        private const int maxSerialTimeout = 1000;
        private const int minSerialTimeout = 200;
        private const int connectTimeout = 20; // ms to time out the serial connection attempt
        private Boolean serialDone = true;
        private TxPacket txPacket;
        private RxPacket rxPacket;
        //   private Queue<TxPacket> outgoingPackets; // queue of outgoing packets, to be sent
        private Queue<RxPacket> incomingPackets; // queue of incoming packets, to be processed
        private Form1 form1; // form1 gives access to update labels and text boxes.
        private DataStorage dataStorage;
        private DataLogger log; // DataLogger gives access to log files to record happenings.
        private static ConnectionManager instance; // singleton instance for connManager

        /* Constructor for class ConnectionManager */
        private ConnectionManager()
        {
            serialPort = new SerialPort(); // declare and instantiate a serial port
            //    outgoingPackets = new Queue<TxPacket>();
            incomingPackets = new Queue<RxPacket>();
            dataStorage = DataStorage.getInstance();
            log = DataLogger.getInstance(this);
        }

        // method to get singleton instance of instance
        private void setForm(Form1 form1)
        {
            this.form1 = form1;
        }

        public static ConnectionManager getInstance(Form1 form1)
        {
            if (instance == null)
            {
                instance = getInstance();
            }
            instance.setForm(form1);
            return instance;
        }

        public static ConnectionManager getInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionManager();
            }
            return instance;
        }

        // method to connect to any open serial ports and attempt handshake. 
        // On success, method exits and logs successful baud rate and port name

        public Boolean connect()
        {
            if (serialPort.IsOpen)
            {
                return true;
            }
            else
            {
                // set properties that we know will be always true
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;
                serialPort.ReceivedBytesThreshold = 3;
                serialPort.ReadTimeout = 500;
                string[] AvailableSerialPorts = null; // clear out
                // Populate the list box with currently available COM ports
                AvailableSerialPorts = SerialPort.GetPortNames();
                foreach (string s in AvailableSerialPorts) // put each available port in as an item
                {
                    foreach (int i in baudValues)
                    {
                        try
                        {
                            serialPort.PortName = s;
                            serialPort.BaudRate = i;
                            serialPort.Open(); // open serial port on a COM and a baud from the baud values array
                            txPacket = new TxPacket(TxPacket.INSTRUCTION_HANDSHAKE, TxPacket.DATA1_HANDSHAKE, TxPacket.DATA2_HANDSHAKE);
                            // send the packet as an array of bytes
                            serialPort.DiscardOutBuffer();
                            serialPort.DiscardInBuffer();
                            serialPort.Write(txPacket.asByteArray(), 0, 3);
                            rxPacket = new RxPacket(); // make a new incoming packet to store data in.
                            while ((serialPort.BytesToRead < 3) && (serialTimeoutCounter < connectTimeout))
                            {
                                Thread.Sleep(1);
                                serialTimeoutCounter++;
                            }
                            serialTimeoutCounter = 0;
                            if (dataStorage.getVerbosity())
                            {
                                form1.appendToRichTextBox1("Attempting connection on " + serialPort.PortName
                                        + " " + serialPort.BaudRate + " baud\r");
                            }
                            if (serialPort.BytesToRead >= 3)
                            {
                                rxPacket.setInstruction((Byte)serialPort.ReadByte());
                                rxPacket.setData1((Byte)serialPort.ReadByte());
                                rxPacket.setData2((Byte)serialPort.ReadByte());

                                if ((rxPacket.getInstruction() == RxPacket.RX_INSTRUCTION_HANDSHAKE) && (rxPacket.getData2() == RxPacket.RX_DATA2_HANDSHAKE))
                                {
                                    incomingPackets.Enqueue(rxPacket); // put the handshake response packet in the incoming packets queue.
                                    parseData(incomingPackets);
                                    //  parseData(rxPacket);
                                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler); // start the handler for future packets.
                                    form1.appendToRichTextBox1("Connected to device on port " + s + " at " + i + " baud.\r");
                                }
                                else
                                {
                                    if (dataStorage.getVerbosity())
                                    {
                                        form1.appendToRichTextBox1("An MCU at " + serialPort.PortName + ", " + serialPort.BaudRate +
                                            " baud replied with " + rxPacket.getInstruction().ToString("x") + " " +
                                            rxPacket.getData1().ToString("x") + " " + rxPacket.getData2().ToString("x") + "\r");
                                    }
                                    serialPort.DiscardInBuffer();
                                    serialPort.DiscardOutBuffer();
                                    serialPort.Close();
                                }
                            }
                            else
                            {
                                serialPort.DiscardInBuffer();
                                serialPort.DiscardOutBuffer();
                                serialPort.Close();
                                if (dataStorage.getVerbosity())
                                {
                                    form1.appendToRichTextBox1("Failed to connect to " + serialPort.PortName + " at "
                                        + serialPort.BaudRate + " baud rate.\r");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            serialPort.DiscardInBuffer();
                            serialPort.DiscardOutBuffer();
                            serialPort.Close();
                            form1.appendToRichTextBox1("Error connecting to port " + serialPort.PortName + " at "
                                + serialPort.BaudRate + " baud, message: " + ex.Message + "\r");
                        }
                        if (serialPort.IsOpen)
                        {
                            return true;
                        }
                    }
                    if (serialPort.IsOpen)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // disconnect the serial port
        public void disconnect()
        {
            try
            {
                serialPort.Close();
                form1.appendToRichTextBox1("Closed connection on " + serialPort.PortName + ".\r");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void reconnect()
        {
            try
            {
                serialPort.Open();
                form1.appendToRichTextBox1("Re-opened port " + serialPort.PortName + " at " + serialPort.BaudRate + "\r");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // takes care of incoming data, depending on the last command sent.
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            serialDone = false;
            if (serialPort.IsOpen)
            {
                try
                {
                    while (serialPort.BytesToRead >= 3)
                    {
                        rxPacket = new RxPacket();
                        rxPacket.setInstruction((Byte)serialPort.ReadByte());
                        rxPacket.setData1((Byte)serialPort.ReadByte());
                        rxPacket.setData2((Byte)serialPort.ReadByte());
                        // put the new input packet into the queue
                        incomingPackets.Enqueue(rxPacket);
                    }
                    //if out of the while loop, then the byte inByte is 0xFF, EOF
                    // serialPort.DiscardInBuffer(); // clear out the input buffer (seems to mess stuff up)
                }
                catch (IndexOutOfRangeException ex)
                {
                    MessageBox.Show(ex.Message);
                    Exception real = ex.GetBaseException();
                    MessageBox.Show(real.Message);
                }
                catch (Exception ex)
                {
                    form1.appendToRichTextBox1(ex.Message);
                }
                parseData(incomingPackets);
                serialDone = true;
            }
        }

        // parses the data received in the packet into appropriate memory locations
        private void parseData(Queue<RxPacket> incomingPackets/*RxPacket rxPacket*/)
        {
            while (incomingPackets.Count > 0)
            {
                rxPacket = incomingPackets.Dequeue();
                switch (rxPacket.getInstruction())
                {
                    case RxPacket.RX_INSTRUCTION_HANDSHAKE:
                        int numChannels = rxPacket.getData1();
                        if (numChannels < 8)
                        {
                            form1.appendToRichTextBox1("MCU has " + numChannels + " channels.\r");
                        }
                        if (numChannels > 8)
                        {
                            numChannels = 8;
                            form1.appendToRichTextBox1("MCU has more than 8 channels.\rUsing channels 0-7\r");
                         
                        }
                        dataStorage.setNumADCChannels(numChannels);
                        form1.appendToRichTextBox1("received handshake packet from MCU.\r");
                        break;
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_ZERO:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_ONE:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_TWO:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_THREE:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_FOUR:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_FIVE:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_SIX:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_SEVEN:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_EIGHT:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_NINE:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_TEN:
                    case RxPacket.RX_INSTRUCTION_READ_ADC_CHANNEL_ELEVEN:
                        UInt16 ADCcount = (UInt16)((UInt16)rxPacket.getData1() << 8);
                        ADCcount |= (UInt16)rxPacket.getData2();
                        dataStorage.setADCCount(rxPacket.getInstruction(), ADCcount); // packet instruction is ADC channel to measure
                        if (dataStorage.getVerbosity())
                        {
                            form1.appendToRichTextBox1("Read ADC channel " + rxPacket.getInstruction() + " value is 0x"
                                + ADCcount.ToString("x") + "\r");
                        }
                        break;
                    case RxPacket.RX_INSTRUCTION_FAILED_ADC_READ:
                        form1.appendToRichTextBox1("MCU failed to read ADC\r");
                        break;
                    case RxPacket.RX_INSTRUCTION_EOF:
                        break;
                    case RxPacket.RX_INSTRUCTION_UNKNOWN_COMMAND:
                        form1.appendToRichTextBox1("MCU sent error code 0xAA, says it received instruction 0x"
                            + rxPacket.getData1().ToString("x") + " and data1 0x" + rxPacket.getData2().ToString("x") + "\r");
                        break;
                    default:
                        form1.appendToRichTextBox1("Error reading fom MCU: \r   received instruction code 0x"
                            + rxPacket.getInstruction().ToString("x") + "\r   data 1 was 0x" + rxPacket.getData1().ToString("x")
                            + "\r   data 2 was 0x" + rxPacket.getData2().ToString("x") + "\r");
                        //throw new Exception("Error parsing data from MCU");
                        break;
                }
            }
        }

        // get ADC count from an ADC channel
        public void readADC(int channel)
        {
            txPacket.setInstruction(TxPacket.INSTRUCTION_READ_ADC);
            txPacket.setData1((Byte)channel);
            txPacket.setData2((Byte)0x00U);
            serialPort.Write(txPacket.asByteArray(), 0, 3);
            serialDone = false;
            try
            {
                timeoutWait();
            }
            catch (TimeoutException ex)
            {
                if (serialTimeout < maxSerialTimeout)
                {
                    serialTimeout+=100;
                    serialDone = true;
                    readADC(channel);
                }
                else throw new TimeoutException(ex.Message);
            }
            if (dataStorage.getVerbosity())
            {
                form1.appendToRichTextBox1("Sent command to read ADC channel " + channel + "\r");
            }
        }

        // just waits till the timout and throws an error if timeout is enabled.
        private void timeoutWait()
        {
            // for the two commands in a row, set serialDone to false so this method won't move on before the dataReceived even triggers
            try
            {
                while (serialDone == false)
                {
                    serialTimeoutCounter++;
                    Thread.Sleep(1); // wait for 1 ms to get the parsing done and be able to update ADC voltage properly
                    if ((serialTimeoutCounter >= serialTimeout) & serialTimeoutEnable)
                    {
                        serialTimeoutCounter = 0;
                        throw new TimeoutException();
                    }
                    // does nothing until the serial input packets have been read up to the EOF, 
                    //at which point the input buffer is empty and the data has been parsed into registers/ADC
                }
                serialTimeoutCounter = 0;
                // Thread.Sleep(1); // wait for 50 ms to get the parsing done and be able to update ADC voltage properly
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException(ex.Message);
            }
            catch (Exception ex)
            {
                form1.appendToRichTextBox1("Unknown exception caught in timeoutWait function, message" + ex.Message + "\r");
            } // labels get updated when the serial port has received stuff and changed stored values
        }

        // enable serial timeout 
        public void enableSerialTimeout()
        {
            serialTimeoutEnable = true;
        }

        // disable serial timeout
        public void disableSerialTimeout()
        {
            serialTimeoutEnable = false;
        }

        public void refreshSerial()
        {
            txPacket.setInstruction(TxPacket.INSTRUCTION_READ_ADC);
            txPacket.setData1(0x00);
            txPacket.setData2(0x00);
            serialPort.Write(txPacket.asByteArray(), 0, 3);
            serialTimeout = minSerialTimeout;
        }
    }
}