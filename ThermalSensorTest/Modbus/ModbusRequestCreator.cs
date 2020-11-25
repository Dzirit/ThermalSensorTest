using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermalSensorTest.Modbus
{
    class ModbusRequestCreator
    {
        public ModbusRequestCreator()
        {
            ModbusRTUMaster master = new ModbusRTUMaster();
            master.SlaveAddr = SetAddress(1);
            master.StartReg = SetStartRegister(0);
            master.RegCount = SetRegistersCount(1);
            Console.WriteLine($"Choose modbus function(default - 0x03):");
            Console.WriteLine("1. Read holding reg - 0x03");
            Console.WriteLine("2. Write holding reg - 0x10");
            var key = Console.ReadKey().Key;
            if (key== ConsoleKey.NumPad2 || key==ConsoleKey.D2)
            {
                master.FuncCode = 0x10;
                var msg = SetMessage();
                var payload = CreateDataPayload(msg);
                var request = master.CreateMessage(payload).ToArray();
                Program.MySerialPort.serialPort.Write(request, 0, request.Length);
            }
            else
            {
                master.FuncCode = 0x03;
                var request = master.CreateMessage().ToArray();
                Program.MySerialPort.serialPort.Write(request, 0, request.Length);
            }
            
            //var byteRequest = master.CreateMessage();
            //var stringRequest = CreateModbusRTUDataString(master);
            //Program.MySerialPort.serialPort.WriteLine(stringRequest);
            Console.Read();
        }

        public byte SetAddress(int defaultAddress)
        {
            Console.Write($"Slave address(default:{defaultAddress}):");
            var address = Console.ReadLine();
            if (address == "")
            {
                address = defaultAddress.ToString();
            }
            return Convert.ToByte(address,16);
        }
        public byte SetStartRegister(int defaultStartRegister)
        {
            Console.Write($"Start reg(default:{defaultStartRegister}):");
            var register = Console.ReadLine();
            if (register == "")
            {
                register = defaultStartRegister.ToString();
            }
            return Convert.ToByte(register, 16);
        }

        public byte SetRegistersCount(int defaultRegistersCount)
        {
            Console.Write($"Count reg(default:{defaultRegistersCount}):");
            var registersCount = Console.ReadLine();
            if (registersCount == "")
            {
                registersCount = defaultRegistersCount.ToString();
            }
            return Convert.ToByte(registersCount,16);
        }
        public string SetMessage()
        {
            Console.Write($"Write msg in hex without 0x through a space:");
            var msg = Console.ReadLine();
            return msg;
        }
        private List<byte> CreateDataPayload(string p_Buffer)
        {
            string[] hexBuffer;
            List<byte> retVal = new List<byte>();

            p_Buffer = p_Buffer.Trim();
            hexBuffer = p_Buffer.Split(' ');
            foreach (String hexStr in hexBuffer)
            {
                byte hexVal = Convert.ToByte(hexStr, 16);
                retVal.Add(hexVal);
            }

            return retVal;
        }
        public string CreateModbusRTUDataString(ModbusRTUMaster p_Msg)
        {
            string RetVal = "";

            for (int i = 0; i < p_Msg.MsgSize; i++)
                RetVal += ("0x" + p_Msg.Msg[i].ToString("X2") + " ");

            return RetVal;
        }
    }
    
}
