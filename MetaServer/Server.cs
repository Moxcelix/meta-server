using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MetaServer
{
    public class Server
    {
        private readonly TcpListener _listener;
        private readonly List<Client> _clients = new List<Client>();
        private bool _isRunning = true;
        private readonly object _lock = new object();

        public Server(string ipAddress, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (_isRunning)
            {
                TcpClient tcpClient = _listener.AcceptTcpClient();

                var client = new Client(tcpClient);

                lock (_lock)
                {
                    _clients.Add(client);
                }

                Console.WriteLine("Новый клиент подключен: " + client.EndPoint);

                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        private void HandleClient(object? obj)
        {
            if (obj == null)
            {
                return;
            }

            var client = (Client)obj;

            //byte[] buffer = new byte[1024];
            //int bytesRead;

            try
            {
                //while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                while (true)
                {
                    //string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var message = client.Reader.ReadLine();

                    Console.WriteLine("Получено сообщение от клиента " + client.EndPoint + ": " + message);

                    lock (_lock)
                    {
                        foreach (var c in _clients)
                        {
                            if (c.EndPoint == client.EndPoint)
                            {
                                continue;
                            }

                            if (string.IsNullOrEmpty(message))
                            {
                                continue;
                            }

                            //byte[] data = Encoding.UTF8.GetBytes(message);

                            //c.GetStream().Write(data, 0, data.Length);

                            c.Writer.WriteLine(message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка чтения данных от клиента " + client.EndPoint + ": " + e.Message);
            }
            finally
            {
                lock (_lock)
                {
                    _clients.Remove(client);
                }
                Console.WriteLine("Клиент отключен: " + client.EndPoint);
                client.Close();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();

            foreach (var client in _clients)
            {
                client.Close();
            }
        }
    }
}