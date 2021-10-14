using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Console;

WriteLine("Press enter to connect");
ReadLine();

var port = 9000;
var endpoint = new IPEndPoint(IPAddress.Loopback, port);

var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
socket.Connect(endpoint);

await using NetworkStream networkStream = new (socket, true);

var msg = "Hello world";

var buffer = Encoding.UTF8.GetBytes(msg);
await networkStream.WriteAsync(buffer);

var response = new byte[1024];

await networkStream.ReadAsync(response);

var responseStr = Encoding.UTF8.GetString(response);

WriteLine($"Response: {responseStr}");

ReadLine();