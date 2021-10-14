using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Console;


void Start(int port = 9000)
{
    var endPoint = new IPEndPoint(IPAddress.Loopback, port);
    var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    socket.Bind(endPoint);
    socket.Listen(128);

    _ = Task.Run(() => DoEcho(socket));
}

async Task DoEcho(Socket socket)
{
    while (true)
    {
        var clientSocket = await Task.Factory.FromAsync(
            socket.BeginAccept,
            socket.EndAccept,
            null
        ).ConfigureAwait(false);
        WriteLine("ECHO SERVER :: CLIENT CONNECTED");

        await using var stream = new NetworkStream(clientSocket, true);
        var buffer = new byte[1024];

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer).ConfigureAwait(false);
            if (bytesRead == 0) break;

            var received = Encoding.UTF8.GetString(buffer);
            WriteLine($"Received: {received}");
            await stream.WriteAsync(buffer).ConfigureAwait(false);
        }
    }
}


Start();
WriteLine("Echo Server running...");
ReadLine();
