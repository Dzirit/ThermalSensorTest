using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace ThermalSensorTest
{
    public class MySerialReader2
    {
        static bool _continue;
        static SerialPort _serialPort;
        public MySerialReader2(string portName, int baudRate, bool writeData, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            try
            {
                Thread writeThread = new Thread(Write);
                Thread readThread = new Thread(Read);
                
                // Create a new SerialPort object with default settings.
                _serialPort = new SerialPort();

                // Allow the user to set the appropriate properties.
                _serialPort.PortName = Utils.SetPortName(portName);
                _serialPort.BaudRate = Utils.SetPortBaudRate(baudRate);
                //_serialPort.Parity = SetPortParity(parity);
                //_serialPort.DataBits = SetPortDataBits(dataBits);
                //_serialPort.StopBits = SetPortStopBits(stopBits);
                //_serialPort.Handshake = SetPortHandshake(handshake);

                // Set the read/write timeouts
                _serialPort.ReadTimeout = Timeout.Infinite;
                _serialPort.WriteTimeout = Timeout.Infinite;
                _serialPort.Open();
                _continue = true;
                Console.WriteLine($"Port {_serialPort.PortName} opened.");
                _serialPort.WriteLine("Hello!");
                //Console.WriteLine($"MainThread{Thread.CurrentThread.ManagedThreadId}");
                writeThread.Start();
                readThread.Start();
                
                //if (writeData)
                //{
                //    Write();
                //}
                //Read();
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                _serialPort.Close();
                _serialPort.Dispose();
            }
            
        }
        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    //Console.WriteLine($"ReadThread{Thread.CurrentThread.ManagedThreadId}");
                    string message = _serialPort.ReadLine();
                    Console.WriteLine(message);
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e);
                }
            }
        }
        public static void Write()
        {
            while (_continue)
            {
                try
                {
                    _serialPort.WriteLine("Hello!!!!");
                    //Console.WriteLine($"WriteThread{Thread.CurrentThread.ManagedThreadId}");
                    //Console.WriteLine($"{_serialPort.BytesToWrite}");
                }
                catch (Exception e)
                { 
                    Console.WriteLine(e);
                }
            }
        }
        
    }
}
