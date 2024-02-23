public class Programm
{
    public static void Main(string[] args)
    {
        string ipAddress = "127.0.0.1";
        int port = 8082;

        Server server = new Server(ipAddress, port);
        server.Start();

        Console.WriteLine("Нажмите Enter для завершения работы сервера.");
        Console.ReadLine();

        server.Stop();
    }
}