using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.Extensions.Configuration;
using ThermalSensorTest.Modbus;

namespace ThermalSensorTest
{
    static class Program
    {
        public static MySerialPort MySerialPort { get; set; }
        public static IConfigurationRoot config;
        [STAThread]
        public static void Main()
        {         
            try
            {
                config = new ConfigurationBuilder()
                 .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                 .Build();
                var defaultPortName = config["PortName"];
                int defaultPortBaudRate = int.Parse(config["PortBaudRate"]);
                bool writeData = bool.Parse(config["WriteData"]);
                Parity defaultParity = (Parity)int.Parse(config["Parity"]);
                int defaultDataBits= int.Parse(config["DataBits"]);
                StopBits defaultStopBits = (StopBits)int.Parse(config["StopBits"]);
                Handshake defaultHandshake= (Handshake)int.Parse(config["Handshake"]);
                MySerialPort = new MySerialPort(defaultPortName,
                                                        defaultPortBaudRate,
                                                        writeData,
                                                        defaultParity,
                                                        defaultDataBits,
                                                        defaultStopBits,
                                                        defaultHandshake
                                                        );
                if (MySerialPort.serialPort.IsOpen)
                {
                    var request = new ModbusRequestCreator();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
    }
}
