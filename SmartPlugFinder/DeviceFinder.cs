using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Contrequarte.SmartPlug.Core;

namespace Contrequarte.SmartPlugFinder
{
    public class DeviceFinder
    {
        AsyncCallback asyncCallback;
        bool searching;
        static object Lock = new object();
        UdpState udpState;
        Timer timeout;
        int timeoutPeriod = 45000;
        List<SmartPlug.Core.SmartPlug> smartPlugList;
        public long SendingPort {get; set;}
        public long ListeningPort {get; set;}

        public List<SmartPlug.Core.SmartPlug> SmartPlugList
        {
            get
            {
                return smartPlugList;
            }
        }

        public DeviceFinder(long sendingPort, long listeningPort)
        {
            SendingPort = sendingPort;
            ListeningPort = listeningPort;
        }

        public void FindDevices(IPAddress ipAddressOfSender)
        {
            smartPlugList = new List<SmartPlug.Core.SmartPlug>();

            //Assuming to work with a class C IP address, so broadcast address looks like a.b.c.255
            byte[] broadcastIpAddress = ipAddressOfSender.GetAddressBytes();
            broadcastIpAddress[3] = 255;

            UdpClient listenClient = new UdpClient(54520);
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 20560);

            udpState = new UdpState(localEp, listenClient);

            asyncCallback = new AsyncCallback(ReceiveSmartPlugCallBack);

            //sending a "Hello" to all smartplugs in the network
            searching = true;
            SendHelloToPlugs(new IPAddress(broadcastIpAddress), udpState.UdpClient);
            udpState.UdpClient.BeginReceive(asyncCallback, udpState);

            timeout = new Timer(new TimerCallback(TimeCallBack), null, timeoutPeriod, 0);
            //Thread.Sleep(500); // time to wait for an reaction

            //while (listenClient.Available>0)
            //{ 
            //    Thread.Sleep(500);
            //}

            //return (IEnumerable<SmartPlug.Core.SmartPlug>)smartPlugList;
        }

        private void ReceiveSmartPlugCallBack(IAsyncResult ar)
        {
            if (searching)
            {
                // Reset the timeout
                timeout.Change(timeoutPeriod, 0);

                lock (Lock)
                {
                    UdpClient udpClient = (UdpClient)((UdpState)(ar.AsyncState)).UdpClient;
                    if (udpClient.Client != null)
                    {
                        //IPEndPoint ipEndPoint = (IPEndPoint)((UdpState)(ar.AsyncState)).EndPoint;
                        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] receivedBytes = udpClient.EndReceive(ar, ref ipEndPoint);
                        smartPlugList.Add(new SmartPlug.Core.SmartPlug(ipEndPoint.Address, GetSmartPlugDetails(receivedBytes)));

                        udpState.UdpClient.BeginReceive(asyncCallback, udpState);
                    }
                }
            }
        }

        public void TimeCallBack(object o)
        {
            lock (Lock)
            {
                // Close the connection
                udpState.UdpClient.Close();
                searching = false;
            }
        }

        public void SendHelloToPlugs(IPAddress broadcastAddress, UdpClient udpClient)
        {
             UdpClient client = udpClient;
             client.EnableBroadcast = true;
             IPEndPoint ip = new IPEndPoint(broadcastAddress, 20560);

            //Don't know, what the following byte sequence exactly means, but it's the sequence the Edimax
            //Android app is sending to discover the smart plugs in the network
             byte[] helloSmartPlugs = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x45, 0x44, 0x49, 0x4d, 0x41, 
                                        0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa1, 0xff, 0x5e };

             client.Send(helloSmartPlugs, helloSmartPlugs.Length, ip);
        }

        /// <summary>
        /// Evaluating the UDP datagram received to get model, sw version and name
        /// of a smart plug
        /// </summary>
        /// <param name="smartplugUdpDataGram">byte array sent as answer from an smart plug</param>
        /// <returns>A SmartPlugDetails object with details of the smart plug </returns>
        private SmartPlugDetails GetSmartPlugDetails(byte[] smartplugUdpDataGram)
        {
            int beginOfModelInformation = 22;
            int beginOfSoftwareVersion = 36;
            int beginOfSmartPlugName = 44;

            string model = ReturnStringValue(smartplugUdpDataGram, beginOfModelInformation);
            string softwareVersion = ReturnStringValue(smartplugUdpDataGram, beginOfSoftwareVersion);
            string smartPlugName = ReturnStringValue(smartplugUdpDataGram, beginOfSmartPlugName);

            return new SmartPlugDetails(model,softwareVersion, smartPlugName);
        }


        private string ReturnStringValue(byte[] smartplugUdpDataGram, int startAddress)
        {
            int offSet = 0;
            while(smartplugUdpDataGram[startAddress+offSet]!=0)
            {
                offSet++; 
            }
            return Encoding.ASCII.GetString(smartplugUdpDataGram, startAddress, offSet);
        }

        public static void ShowIt()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    System.Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            System.Console.WriteLine(ip.Address.ToString());
                        }
                    }
                }
            }
        }
    }
}

