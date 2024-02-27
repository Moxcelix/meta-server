using System.Net;
using System.Net.Sockets;

namespace MetaServer
{
    public class Client
    {
        private readonly TcpClient _tcp;
        private readonly Stream _stream;

        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }
        public EndPoint? EndPoint { get; }

        public Client(TcpClient tcp)
        {
            _tcp = tcp;

            _stream = tcp.GetStream();

            Reader = new StreamReader(_stream);
            Writer = new StreamWriter(_stream);
            EndPoint = _tcp.Client.RemoteEndPoint;
        }

        public void Close()
        {
            _tcp.Close();
        }
    }
}
