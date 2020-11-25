using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermalSensorTest.Modbus
{
    class ModbusRTUMaster
    {
        byte slaveaddr;
        byte funccode;
        ushort startreg;
        ushort regcount;
        byte databytes = 2;
        List<byte> payload;
        ushort crc16;

        public List<byte> Msg;

        // Class Constructors
        public ModbusRTUMaster()
        {
            slaveaddr = funccode = databytes;
            startreg = regcount = crc16 = 0;
            payload = new List<byte>();
        }

        public ModbusRTUMaster(byte p_ToAddr, byte p_FuncCode, ushort p_StartReg)
        {
            slaveaddr = p_ToAddr;
            funccode = p_FuncCode;
            startreg = p_StartReg;
        }


        public ModbusRTUMaster(byte p_SlaveAddr, byte p_FuncCode, ushort p_StartReg, ushort p_RegCount, List<byte> p_Payload)
        {
            slaveaddr = p_SlaveAddr;
            funccode = p_FuncCode;
            startreg = p_StartReg;
            regcount = p_RegCount;
            payload = p_Payload.ToList();
        }


        // Property Initializers
        public byte SlaveAddr { get => slaveaddr; set => slaveaddr = value; }
        public byte FuncCode { get => funccode; set => funccode = value; }
        public ushort StartReg { get => startreg; set => startreg = value; }
        public ushort RegCount { get => regcount; set => regcount = value; }
        public ushort DataBytes => databytes;
        public ushort CRC16 => crc16;
        public ushort MsgSize => (ushort)Msg.Count();

        public void ClearData()
        {
            funccode = 0;
            startreg = 0;
            regcount = 0;
            databytes = 0;
            payload.Clear();
            crc16 = 0;
        }

        public void ClearAll()
        {
            slaveaddr = 0;
            ClearData();
        }

        public List<byte> CreateMessage(byte p_SlaveAddr, byte p_FuncCode, ushort p_StartReg, ushort p_RegCount, List<byte> p_Payload)
        {
            slaveaddr = p_SlaveAddr;
            funccode = p_FuncCode;
            startreg = p_StartReg;
            regcount = p_RegCount;

            return CreateMessage(p_Payload);
        }

        public List<byte> CreateMessage(List<byte> p_Payload)
        {
            Msg = new List<byte>();

            Msg.Add(slaveaddr);                         // Add slave address to overall message
            Msg.Add(funccode);                          // Add function code to overall message
            Msg.Add((byte)(startreg >> 8));             // Add starting register upper byte
            Msg.Add((byte)(startreg & 0x00FF));         // Add starting register lower byte

            if (funccode != 0x08)
            {
                Msg.Add((byte)(regcount >> 8));             // Add register count upper byte
                Msg.Add((byte)(regcount & 0x00FF));         // Add register count lower byte
            }

            if (funccode == 0x10)
                Msg.Add(GetNumDataBytes(p_Payload));    // Add number of data bytes in the data payload

            // Add data payload to overall message - skip for read register requests
            if ((funccode == 0x08) || (funccode == 0x10))
                for (int i = 0; i < p_Payload.Count; i++)
                    Msg.Add(p_Payload[i]);

            crc16 = CalcModbusRTUCRC16(Msg);            // Calculate the Modbus RTU CRC-16 value

            // Modbus RTU CRC16 is Big-Endian format So lower byte is added first
            Msg.Add((byte)(crc16 & 0x00FF));
            Msg.Add((byte)(crc16 >> 8));

            return Msg;
        }
        public List<byte> CreateMessage()
        {
            Msg = new List<byte>();

            Msg.Add(slaveaddr);                         // Add slave address to overall message
            Msg.Add(funccode);                          // Add function code to overall message
            Msg.Add((byte)(startreg >> 8));             // Add starting register upper byte
            Msg.Add((byte)(startreg & 0x00FF));         // Add starting register lower byte

            if (funccode != 0x08)
            {
                Msg.Add((byte)(regcount >> 8));             // Add register count upper byte
                Msg.Add((byte)(regcount & 0x00FF));         // Add register count lower byte
            }

            crc16 = CalcModbusRTUCRC16(Msg);            // Calculate the Modbus RTU CRC-16 value

            // Modbus RTU CRC16 is Big-Endian format So lower byte is added first
            Msg.Add((byte)(crc16 & 0x00FF));
            Msg.Add((byte)(crc16 >> 8));

            return Msg;
        }
        public byte ExtractMessage(List<byte> p_FullMsg)
        {
            ushort InCRC = 0, CalcCRC16 = 0;
            ModbusRTUMsg TmpMsg = new ModbusRTUMsg();

            /* First verify message has a valid CRC in relation to its data */
            // Extract CRC-16 value
            InCRC = (ushort)((p_FullMsg[p_FullMsg.Count - 1] << 8) | (p_FullMsg[p_FullMsg.Count - 2]));

            // Strip CRC-16 off the full message
            p_FullMsg.RemoveRange(p_FullMsg.Count - 1, 2);

            // Calculate the CRC-16 based on the received data minus the last two bytes (received CRC-16)
            CalcCRC16 = CalcModbusRTUCRC16(p_FullMsg);
            if (InCRC != CalcCRC16)
                return 0xFF;

            /* Extract the received message and put each data byte into it's correct ModbusRTUMsg fields */
            TmpMsg.CRC16 = InCRC;               // Store CRC-16 value that was previously extracted from the full message
            TmpMsg.SlaveAddr = p_FullMsg[0];    // Store the slave address
            TmpMsg.FuncCode = p_FullMsg[1];     // Store the function code

            // Store the different byte locations of the overall message based on the type of message it is
            switch (TmpMsg.FuncCode)
            {
                case 0x03: // Read register response from slave
                    TmpMsg.RegByteCount = p_FullMsg[2];
                    for (int i = 3; i < (i + TmpMsg.RegByteCount); i += 2)
                        TmpMsg.Data.Add((ushort)((p_FullMsg[i] << 8) | p_FullMsg[i + 1]));
                    break;
                case 0x08: // Loopback response from slave
                    TmpMsg.SubFunction = (ushort)((p_FullMsg[2] << 8) | p_FullMsg[3]);
                    TmpMsg.RegCount = (ushort)((p_FullMsg[4] << 8) | p_FullMsg[5]);
                    break;

                case 0x10: // Write register response from slave
                    TmpMsg.StartReg = (ushort)((p_FullMsg[2] << 8) | p_FullMsg[3]);
                    TmpMsg.RegCount = (ushort)((p_FullMsg[4] << 8) | p_FullMsg[5]);
                    break;
                default:
                    return 0xF0; // Unknown function code
            }

            return 1;
        }


        // Private Helper Functions
        private byte GetNumDataBytes(List<byte> p_Payload)
        {
            return (byte)p_Payload.Count();
        }

        private ushort CalcModbusRTUCRC16(List<byte> p_DataBuffer)
        {
            ushort CRCResult = 0xFFFF, XORVal = 0xA001, XOR = 0x0000;

            for (int i = 0; i < p_DataBuffer.Count; i++)
            {
                CRCResult ^= p_DataBuffer[i];
                for (int j = 0; j < 8; j++)
                {
                    XOR = (ushort)(CRCResult & 0x0001);
                    CRCResult >>= 1;

                    if (XOR > 0)
                        CRCResult ^= XORVal;
                }
            }

            return CRCResult;
        }
    }
}
