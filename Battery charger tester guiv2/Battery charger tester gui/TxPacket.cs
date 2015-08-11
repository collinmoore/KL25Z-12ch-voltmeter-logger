using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battery_charger_tester_gui
{
    class TxPacket
    {
        public const int INSTRUCTION_READ_ADC = 0x00;
        
        public const int INSTRUCTION_READ_DUTY_CYCLE = 0x30;
        
        public const int INSTRUCTION_SET_DUTY_CYCLE = 0x40;
        
        public const int INSTRUCTION_HANDSHAKE = 0x99;

        public const int DATA1_HANDSHAKE = 0x13;

        public const int DATA2_HANDSHAKE = 0x37;

        private Byte instruction, data1, data2;

        public TxPacket()
        {

        }
        public TxPacket(Byte instruction, Byte data1, Byte data2)
        {
            checkInstruction(instruction);
            this.instruction = instruction;
            this.data1 = data1;
            this.data2 = data2;
        }

        // check if the instruction for setting or constructing is a valie instcution
        private void checkInstruction(Byte instruction)
        {
            switch (instruction)
            {
                case INSTRUCTION_READ_ADC:
                case INSTRUCTION_READ_DUTY_CYCLE:
                case INSTRUCTION_SET_DUTY_CYCLE:
                case INSTRUCTION_HANDSHAKE:
                    break;
                default:
                    throw new Exception("Invalid instruction for TxPacket.");
            }
        }

        // set instruction field of a packet
        public void setInstruction(Byte instruction)
        {
            checkInstruction(instruction);
            this.instruction = instruction;
        }

        // set data1 field of a packet
        public void setData1(Byte data1)
        {
            this.data1 = data1;
        }

        // set data2 field of a packet
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

        // return a byte array of all fields of the packet.
        public Byte[] asByteArray()
        {
            return new Byte[] { instruction, data1, data2 };
        }

    }
}
