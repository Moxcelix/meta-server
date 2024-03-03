using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    private readonly TcpListener _listener;
    private readonly List<TcpClient> _clients = new List<TcpClient>();
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
            TcpClient client = _listener.AcceptTcpClient();
            lock (_lock)
            {
                _clients.Add(client);
            }

            Console.WriteLine("Новый клиент подключен: " + client.Client.RemoteEndPoint);

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        if(obj == null)
        {
            return;
        }

        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //Console.WriteLine("Получено сообщение от клиента " + client.Client.RemoteEndPoint + ": " + message);

                lock (_lock)
                {
                    foreach (TcpClient c in _clients)
                    {
                        if (c.Client.RemoteEndPoint != client.Client.RemoteEndPoint)
                        {
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            c.GetStream().Write(data, 0, data.Length);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка чтения данных от клиента " + client.Client.RemoteEndPoint + ": " + e.Message);
        }
        finally
        {
            lock (_lock)
            {
                _clients.Remove(client);
            }
            Console.WriteLine("Клиент отключен: " + client.Client.RemoteEndPoint);
            client.Close();
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();

        foreach (TcpClient client in _clients)
        {
            client.Close();
        }
    }
}