// libs
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;


// namespace
namespace connection
{

    // class for connection send file
    class ConnectionSend
    {
        // variables
        IPAddress ip;
        int port;

        string type;
        IPEndPoint endPoint;
        TcpClient tcpClient;
        UdpClient udpClient;
        NetworkStream stream;


        // constructor
        public ConnectionSend(string ipAddr, int port, string type, string filePath)
        {
            // variables
            ip = IPAddress.Parse(ipAddr);
            this.port = port;
            this.type = type;

            // open connection
            Open();

            if (type.ToLower() == "tcp")
            {
                SendTCP(filePath);
            }
            else if (type.ToLower() == "udp")
            {
                SendUDP(filePath);
            }
        }

        // method for open connection
        public void Open()
        {
            switch (type.ToLower())
            {
                // for TCP connection
                case "tcp":
                    // connection init
                    endPoint = new IPEndPoint(ip, port);
                    tcpClient = new TcpClient();
                    tcpClient.Connect(endPoint);

                    // get connection stream
                    stream = tcpClient.GetStream();
                    break;

                // for UDP connection
                case "udp":
                    endPoint = new IPEndPoint(ip, port);
                    udpClient = new UdpClient();
                    break;
            }
        }


        // method for send file via TCP
        public void SendTCP(string filePath)
        {
            // file to send
            byte[] fileToSend = File.ReadAllBytes(filePath);

            // send data
            stream.Write(fileToSend);
        }

        // method for send file via UDP
        public void SendUDP(string filePath)
        {
            // send file
            byte[] fileToSend = File.ReadAllBytes(filePath);

            // loop for read 1000 byte for iteration from file
            int byteCounter;
            for (int counter = 0, nextItem = 0; nextItem < fileToSend.Length; nextItem += 1000)
            {
                byteCounter = 0;
                byte[] buffer = new byte[1000];
                for (int i = 0; i < 1000 && i + nextItem < fileToSend.Length; i++)
                {
                    buffer[i] = fileToSend[nextItem + i];
                    byteCounter++;
                }
                Console.WriteLine($"Package number {++counter}");
                udpClient.Send(buffer, byteCounter, endPoint);
            }

            // send package with 0 bytes. Its signal for end file
            udpClient.Send(new byte[] { }, 0, endPoint);
        }
    }

    class ConnectionReceive
    {
        // variables
        IPAddress ip;
        int port;
        string type;
        IPEndPoint endPoint;
        TcpListener tcpListener;
        TcpClient tcpClient;
        UdpClient udpClient;
        NetworkStream stream;
        int amountBytes;
        byte[] bufferTcp = new byte[1024 * 16];
        byte[] bufferUdp;



        // constructor
        public ConnectionReceive(int port, string type, string filePath)
        {
            // variables
            ip = IPAddress.Any;
            this.port = port;
            this.type = type;


            // open connection
            Open();

            if (type.ToLower() == "tcp")
            {
                ReceiveTCP(filePath);
            }
            else if (type.ToLower() == "udp")
            {
                // message
                Console.WriteLine("Awaiting for file...");
                ReceiveUDP(filePath);
            }
        }

        // method for open connection
        public void Open()
        {
            switch (type.ToLower())
            {
                // for TCP connection
                case "tcp":
                    // connection init
                    endPoint = new IPEndPoint(ip, port);
                    tcpListener = new TcpListener(endPoint);
                    tcpListener.Start();

                    // message
                    Console.WriteLine("Awaiting for file...");

                    // await for clients
                    tcpClient = tcpListener.AcceptTcpClient();

                    // get connection stream
                    stream = tcpClient.GetStream();
                    break;

                // for UDP connection
                case "udp":
                    // connection init
                    endPoint = new IPEndPoint(ip, port);
                    udpClient = new UdpClient(endPoint);
                    break;
            }
        }


        // method for receive file via TCP
        public void ReceiveTCP(string filePath)
        {
            // open file to write
            FileStream fileStream = new FileStream(filePath, FileMode.Append);

            // loop to get data and write to file
            while ((amountBytes = stream.Read(bufferTcp, 0, bufferTcp.Length)) != 0)
            {
                fileStream.Write(bufferTcp, 0, amountBytes);
                Console.WriteLine($"Read {amountBytes} bytes, In file {fileStream.Length} bytes");
            }

            // close file
            fileStream.Close();

            // print message
            Message(filePath);
        }

        // method for receive file via UDP
        public void ReceiveUDP(string filePath)
        {
            // open file to write
            FileStream fileStream = new FileStream(filePath, FileMode.Append);
            int counter = 0;
            // loop to get data and write to file
            while (true)
            {
                // receive data
                bufferUdp = udpClient.Receive(ref endPoint);

                // stop loop if buffer 0
                if (bufferUdp.Length == 0)
                {
                    break;
                }

                // write file
                fileStream.Write(bufferUdp, 0, bufferUdp.Length);
                Console.WriteLine($"Read {++counter} packages with {fileStream.Length} bytes");
            }

            // close file
            fileStream.Close();

            // print message
            Message(filePath);
        }

        // method for print message
        private void Message(string filePath)
        {
            Console.WriteLine($"File was saved to {filePath}");
        }
    }


}