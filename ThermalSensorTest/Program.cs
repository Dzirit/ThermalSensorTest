using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace ThermalSensorTest
{
    class Program
    {
        
        [STAThread]
        public static void Main()
        {         
            try
            {
                var config = new ConfigurationBuilder()
                 .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                 .Build();
                var defaultPortName = config["PortName"];
                int defaultPortBaudRate = int.Parse(config["PortBaudRate"]);
                bool writeData = bool.Parse(config["WriteData"]);
                Parity defaultParity = (Parity)int.Parse(config["Parity"]);
                int defaultDataBits= int.Parse(config["DataBits"]);
                StopBits defaultStopBits = (StopBits)int.Parse(config["StopBits"]);
                Handshake defaultHandshake= (Handshake)int.Parse(config["Handshake"]);
                int chooser = int.Parse(config["WhichReaderUse"]);

                if (chooser==1)
                {
                    var listener = new MySerialReader(defaultPortName,
                                                        defaultPortBaudRate,
                                                        writeData,
                                                        defaultParity,
                                                        defaultDataBits,
                                                        defaultStopBits,
                                                        defaultHandshake
                                                        );
                }
                else
                {
                    var listener2 = new MySerialReader2(defaultPortName,
                                                        defaultPortBaudRate,
                                                        writeData,
                                                        defaultParity,
                                                        defaultDataBits,
                                                        defaultStopBits,
                                                        defaultHandshake
                                                        );
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
