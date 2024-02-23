using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server(string ipAddress, int port)
{
    private readonly TcpListener _listener = new (IPAddress.Parse(ipAddress), port);
    private readonly List<TcpClient> _clients = [];
    private bool _isRunning = true;

    public void Start()
    {
        _listener.Start();

        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        while (_isRunning)
        {
            TcpClient client = _listener.AcceptTcpClient();

            _clients.Add(client);

            Console.WriteLine("Новый клиент подключен: " + client.Client.RemoteEndPoint);

            Thread clientThread = new Thread(start: HandleClient);

            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        var buffer = new byte[1024];
        var bytesRead = 0;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено сообщение от клиента " +
                    client.Client.RemoteEndPoint + ": " + message);

                foreach (var c in _clients)
                {
                    if (c.Client.RemoteEndPoint == client.Client.RemoteEndPoint)
                    {
                        continue;
                    }

                    var data = Encoding.UTF8.GetBytes(message);
                    c.GetStream().Write(data, 0, data.Length);
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine("Ошибка чтения данных от клиента " + client.Client.RemoteEndPoint + ": " + exception.Message);
        }
        finally
        {
            _clients.Remove(client);

            Console.WriteLine("Клиент отключен: " + client.Client.RemoteEndPoint);

            client.Close();
        }
    }

    public void Stop()
    {
        _isRunning = false;

        foreach (TcpClient client in _clients)
        {
            client.Close();
        }

        _listener.Stop();
    }
}
