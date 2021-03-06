﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace ThermalSensorTest
{
    public class MySerialPort 
    {
        public SerialPort serialPort { get; set; }
        public MySerialPort(string portName, int baudRate, bool writeData, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            try
            {
                serialPort = new SerialPort();
                serialPort.PortName = Utils.SetPortName(portName);
                serialPort.BaudRate = Utils.SetPortBaudRate(baudRate);
                serialPort.Parity = parity;
                serialPort.ReadTimeout = Timeout.Infinite;
                serialPort.DataBits = dataBits;
                serialPort.StopBits = stopBits;
                serialPort.Handshake = handshake;
                serialPort.Open();
                Console.WriteLine($"Port {serialPort.PortName} opened");
                Console.WriteLine($"Port settings: \r\n" +
                    $"BaudRate:{serialPort.BaudRate} \r\n" +
                    $"Parity:{serialPort.Parity} \r\n" +
                    $"DataBits: {serialPort.DataBits} \r\n" +
                    $"StopBits: {serialPort.StopBits} \r\n" +
                    $"Handshake: {serialPort.Handshake} \r\n" +
                    $"Waiting data...");
                serialPort.DataReceived += SerialPort_DataReceived;
                if (writeData)
                {
                    var writeThread = new Thread(Write);
                    writeThread.Start();
                }
                //ModbusRequest();
                //Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                serialPort?.Close();
                serialPort?.Dispose();
            }
        }
       
        void Write()
        {
            while (true)
            {
                try
                {
                    serialPort.WriteLine("Hello!");
                    //Console.WriteLine($"WriteThread{Thread.CurrentThread.ManagedThreadId}");
                    //Console.WriteLine($"{_serialPort.BytesToWrite}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        void SerialPort_DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);
            StringBuilder sb = new StringBuilder();
            foreach (var d in data)
            {
                sb.Append($"0x{d.ToString("X2")} ");
            }
            var str = sb.ToString();
            //var str = Encoding.UTF8.GetString(data);
            //var str = serialPort.ReadLine();
            Console.WriteLine(str);
        }   
    }
}
