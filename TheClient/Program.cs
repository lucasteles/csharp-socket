using System.Net;
using System.Net.Sockets;
using static System.Text.Encoding;
using static System.Console;

WriteLine("Press enter to connect");
ReadLine();

IPEndPoint endpoint = new(IPAddress.Loopback, 9000);
Socket socket = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
socket.Connect(endpoint);

await using NetworkStream networkStream = new(socket, true);
var messageBuffer = UTF8.GetBytes("Hello world");
await networkStream.WriteAsync(messageBuffer);

var responseBuffer = new byte[1024];
await networkStream.ReadAsync(responseBuffer);

WriteLine($"Response: {UTF8.GetString(responseBuffer)}");

ReadLine();