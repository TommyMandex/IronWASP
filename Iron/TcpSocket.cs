using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace IronWASP
{
    public class TcpSocket
    {
        string intServerIp = "";
        int intPort = 0;
        TcpClient intClient = null;
        NetworkStream NetStream = null;

        public TcpSocket(string _ServerIp, int _Port)
        {
            this.intServerIp = _ServerIp;
            this.intPort = _Port;
            this.intClient = new TcpClient(_ServerIp, _Port);
            this.NetStream = this.intClient.GetStream();
        }

        public void Write(string Data)
        {
            this.Write(Encoding.UTF8.GetBytes(Data));
        }

        public void Write(byte[] Data)
        {
            this.NetStream.Write(Data, 0, Data.Length);
        }

        public byte[] Read()
        {
            byte[] Data = new byte[this.intClient.Available];
            if (Data.Length > 0)
            {
                this.NetStream.Read(Data, 0, Data.Length);
            }
            return Data;
        }

        public string ReadString()
        {
            return Encoding.UTF8.GetString(Read());
        }

        public byte[] WaitAndRead()
        {
            while (true)
            {
                if (IsDataAvailable)
                {
                    return Read();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public string WaitAndReadString()
        {
            return Encoding.UTF8.GetString(WaitAndRead());
        }

        public bool IsDataAvailable
        {
            get
            {
                return this.intClient.Available > 0;
            }
        }


        public void Close()
        {
            try
            {
                this.NetStream.Close();
            }
            catch { }
            try
            {
                this.intClient.Close();
            }
            catch { }
        }

    }
}
