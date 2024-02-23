using System.Net;
using System.Net.Sockets;
using System.Text;


public class Server
{
    private readonly TcpListener _listener;
    private readonly List<TcpClient> _clients = [];
    private bool _isRunning = true;

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

            _clients.Add(client);

            Console.WriteLine("Новый клиент подключен: " + client.Client.RemoteEndPoint);

            Thread clientThread = new Thread(HandleClient);

            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено сообщение от клиента " + client.Client.RemoteEndPoint + ": " + message);

                // Рассылаем сообщение всем клиентам
                foreach (TcpClient c in _clients)
                {
                    if (c != client)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        c.GetStream().Write(data, 0, data.Length);
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
            _clients.Remove(client);
            client.Close();
            Console.WriteLine("Клиент отключен: " + client.Client.RemoteEndPoint);
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
