using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermalSensorTest.Modbus
{
    class ModbusRTUMsg
    {
        public byte SlaveAddr = 0;
        public byte FuncCode = 0;
        public ushort SubFunction = 0;
        public ushort StartReg = 0;
        public ushort RegCount = 0;
        public byte RegByteCount = 2;
        public List<ushort> Data = new List<ushort>();
        public ushort CRC16 = 0xFFFF;

        public void MsgClear()
        {
            SlaveAddr = 0;
            FuncCode = 0;
            StartReg = 0;
            RegCount = 0;
            RegByteCount = 2;
            Data.Clear();
            CRC16 = 0xFFFF;
        }
    }
}
