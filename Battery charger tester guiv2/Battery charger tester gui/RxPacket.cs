using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battery_charger_tester_gui
{
    class RxPacket
    {
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_ZERO = 0x00;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_ONE = 0x01;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TWO = 0x02;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_THREE = 0x03;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_FOUR = 0x04;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_FIVE = 0x05;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_SIX = 0x06;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_SEVEN = 0x07;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_EIGHT = 0x08;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_NINE = 0x09;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TEN = 0x0A;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_ELEVEN = 0x0B;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TWELVE = 0x0C;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_THIRTEEN = 0x0D;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_FOURTEEN = 0x0E;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_FIFTEEN = 0x0F;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_SIXTEEN = 0x10;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_SEVENTEEN = 0x11;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_EIGHTEEN = 0x12;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_NINETEEN = 0x13;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TWENTY = 0x14;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TWENTY_ONE = 0x15;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TWENTY_TWO = 0x16;
        public const int RX_INSTRUCTION_READ_ADC_CHANNEL_TWENTY_THREE = 0x17;

        public const int RX_INSTRUCTION_FAILED_ADC_READ = 0x20;

        public const int RX_INSTRUCTION_READ_DUTY_CYCLE = 0X30;
        
        public const int RX_INSTRUCTION_SET_DUTY_CYCLE = 0X40;

        public const int RX_INSTRUCTION_HANDSHAKE = 0x99;

        public const int RX_INSTRUCTION_UNKNOWN_COMMAND = 0xAA;
        
        public const int RX_INSTRUCTION_EOF = 0xFF;

        public const int RX_DATA1_FAILED_ADC_READ = 0xF0;
        public const int RX_DATA1_INCORRECT_DUTY_CYCLE_SETTING = 0xF4;
        // handshake data1 is number of ADC channels
        public const int RX_DATA1_EOF = 0xFF;

        public const int RX_DATA2_FAILED_ADC_READ = 0xF0;
        public const int RX_DATA2_HANDSHAKE = 0x37;
        public const int RX_DATA2_EOF = 0xFF;

        private Byte instruction;
        private Byte data1;
        private Byte data2;

        public RxPacket()
        {

        }
        public RxPacket(Byte instruction, Byte data1, Byte data2)
        {
            checkInstruction(instruction);
            this.instruction = instruction;
            this.data1 = data1;
            this.data2 = data2;
        }

        private void checkInstruction(Byte instruction)
        {
            switch (instruction)
            {
                case RX_INSTRUCTION_READ_ADC_CHANNEL_ZERO:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_ONE:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_TWO:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_THREE:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_FOUR:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_FIVE:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_SIX:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_SEVEN:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_EIGHT:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_NINE:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_TEN:
                case RX_INSTRUCTION_READ_ADC_CHANNEL_ELEVEN:
                case RX_INSTRUCTION_READ_DUTY_CYCLE:
                case RX_INSTRUCTION_SET_DUTY_CYCLE:
                case RX_INSTRUCTION_HANDSHAKE:
                case RX_INSTRUCTION_UNKNOWN_COMMAND:
                case RX_INSTRUCTION_EOF:
                    break;
                default:
                    throw new Exception("Invalid Instruction "+instruction.ToString("x")+" for RxPacket.\r");
            }
        }

        // set the field instruction on a packet.
        public void setInstruction(Byte instruction)
        {
            checkInstruction(instruction);
            this.instruction = instruction;
        }

        // set the data1 field of a packet
        public void setData1(Byte data1)
        {
            this.data1 = data1;
        }

        // set the data2 field of a packet
        public void setData2(Byte data2)
        {
            this.data2 = data2;
        }

        // returns the Byte instruction of a packet
        public Byte getInstruction()
        {
            return this.instruction;
        }

        // returns the Byte data1 of a packet
        public Byte getData1()
        {
            return this.data1;
        }

        // returns the Byte data2 of a packet
        public Byte getData2()
        {
            return this.data2;
        }

        public Byte[] asByteArray()
        {
            return new Byte[] { this.instruction, this.data1, this.data2 };
        }
    }

}
