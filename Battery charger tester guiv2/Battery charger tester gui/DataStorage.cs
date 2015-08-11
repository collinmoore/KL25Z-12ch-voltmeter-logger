using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battery_charger_tester_gui
{
    class DataStorage
    {

        private static DataStorage dataStorage;

        private Boolean verbosity = false; // set this to change the amount of data going to the serial monitor window, RichTextBox1
        // variables for storing current and past values
        /**********************************    Define number of ADC channels to measure from     **************************/
        private int numADCChannels;
        /******************************************************************************************************************/
        private UInt16[] ADCCounts;
        private decimal[] decimalValues;
        private decimal[] scalingFactors = {
                                       /* Channel 1 scaling factor */ 2.9890000000M,// 6.872852234M,  //  multiplier for 2.91V to 20V
                                       /* Channel 2 scaling factor */ 2.9840000000M,  //  multiplier for 2.91V to 6.0V
                                       /* Channel 3 scaling factor */ 1.4980000000M,  //  multiplier for 2.91V to 5.0V
                                       /* Channel 4 scaling factor */ 1.4980000000M,  //  multiplier for 2.91V to 5.0V
                                       /* Channel 5 scaling factor */ 1.9410000M, //  
                                       /* Channel 6 scaling factor */ 1.9410000M, //  
                                       /* Channel 7 scaling factor */ 1.9410000M, //  
                                       /* Channel 8 scaling factor */ 1.9410000M, //
                                       /* Channel 9 scaling factor */ 1.00000M,
                                       /* Channel 10 scaling factor */ 1.00000M,
                                       /* Channel 11 scaling factor */ 1.00000M,
                                       /* Channel 12 scaling factor */ 1.00000M,
                                       /* Channel 13 scaling factor */ 1.00000M,
                                       /* Channel 14 scaling factor */ 1.00000M,
                                       /* Channel 15 scaling factor */ 1.00000M,
                                       /* Channel 16 scaling factor */ 1.00000M,
                                    };
        private string[] units = {
                               /* Channel 1 units (V or A usually) */ "V",
                               /* Channel 2 units (V or A usually) */ "V",
                               /* Channel 3 units (V or A usually) */ "V",
                               /* Channel 4 units (V or A usually) */ "V",
                               /* Channel 5 units (V or A usually) */ "A",
                               /* Channel 6 units (V or A usually) */ "A",
                               /* Channel 7 units (V or A usually) */ "A",
                               /* Channel 8 units (V or A usually) */ "A",
                               /* Channel 9 units (V or A usually) */ "V",
                               /* Channel 10 units (V or A usually) */ "V",
                               /* Channel 11 units (V or A usually) */ "V",
                               /* Channel 12 units (V or A usually) */ "V",
                               /* Channel 13 units (V or A usually) */ "V",
                               /* Channel 14 units (V or A usually) */ "V",
                               /* Channel 15 units (V or A usually) */ "V",
                               /* Channel 16 units (V or A usually) */ "V"
                             };
        // logging rates to choose from in the drop-down
        public readonly int[] logrates = { 200, 500, 900, 1000, 2000, 5000, 10000, 20000, 30000, 60000, 120000, 300000 };
        private const decimal voltsPerCount = (decimal)(2.910 / 65535); // volts per count will be the max volts divided by the max counts

        private DataStorage()
        {
            // constructor is empty, all values are consts or declared later when size is known.
        }
        // gets instance or creates one if there is none
        public static DataStorage getInstance()
        {
            if (dataStorage == null)
            {
                dataStorage = new DataStorage();
            }
            return dataStorage;
        }

        // method to set verbosity of the GUI displays
        public void setVerbosity(Boolean verbosity)
        {
            this.verbosity = verbosity;
        }

        // method to read the verbosity of the displays
        public Boolean getVerbosity()
        {
            return this.verbosity;
        }

        // set the numADCChannels
        public void setNumADCChannels(int numADCChannels)
        {
            this.numADCChannels = numADCChannels;
            this.ADCCounts = new UInt16[numADCChannels];
            this.decimalValues = new decimal[numADCChannels];
        }

        // return the number of ADC channels
        public int getNumADCChannels()
        {
            return this.numADCChannels;
        }

        // method to set ADCCount
        public void setADCCount(int channel, UInt16 count)
        {
            ADCCounts[channel] = count;
            decimal countToDec = count;
            decimalValues[channel] = Decimal.Round(countToDec * voltsPerCount * scalingFactors[channel], 5);
        }

        // method to get ADC counts
        public UInt16 getADCCount(int channel)
        {
            return ADCCounts[channel];
        }

        // method to get decimal value for ADC channel input
        public decimal getDecimalValues(int channel)
        {
            return decimalValues[channel];
        }

        // method to get log rates
        public int[] getLogRates()
        {
            return logrates;
        }

        public string getUnits(int channel)
        {
            return this.units[channel];
        }
    }
}
