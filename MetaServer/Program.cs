using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPServer
{
    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();
    private bool isRunning = true;

    public TCPServer(string ipAddress, int port)
    {
        listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        while (isRunning)
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client);

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
                foreach (TcpClient c in clients)
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
            clients.Remove(client);
            client.Close();
            Console.WriteLine("Клиент отключен: " + client.Client.RemoteEndPoint);
        }
    }

    public void Stop()
    {
        isRunning = false;
        foreach (TcpClient client in clients)
        {
            client.Close();
        }
        listener.Stop();
    }

    public static void Main(string[] args)
    {
        string ipAddress = "127.0.0.1";
        int port = 8888;

        TCPServer server = new TCPServer(ipAddress, port);
        server.Start();

        Console.WriteLine("Нажмите Enter для завершения работы сервера.");
        Console.ReadLine();

        server.Stop();
    }
}