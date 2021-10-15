using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Console;

(byte[] header, byte[] body) Encode<T>(T message)
{
    var body = JsonSerializer.SerializeToUtf8Bytes(message);
    var header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));
    return (header, body);
}

T Decode<T>(byte[] body) => JsonSerializer.Deserialize<T>(body);

async Task<byte[]> ReadBytesOfNet(NetworkStream stream, int numberOfBytes)
{
    var buffer = new byte[numberOfBytes];
    var readedBytes = 0;
    while (readedBytes < numberOfBytes)
    {
        var newBytes = await stream.ReadAsync(buffer, readedBytes, numberOfBytes - readedBytes);
        if (newBytes == 0) break;
        readedBytes += newBytes;
    }
    return buffer;
}

async Task SendAsync<T>(NetworkStream stream, T message)
{
    var (header, body) = Encode(message);
    await stream.WriteAsync(header);
    await stream.WriteAsync(body);
}

async Task<T> Receive<T>(NetworkStream stream)
{
    var header = await ReadBytesOfNet(stream, 4);
    var bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header));
    var bodyBytes = await ReadBytesOfNet(stream, bodyLength);
    return Decode<T>(bodyBytes);
}

WriteLine("Press enter to connect");
ReadLine();

IPEndPoint endpoint = new(IPAddress.Loopback, 9000);
Socket socket = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
socket.Connect(endpoint);
await using NetworkStream networkStream = new(socket, true);

MyMessage message = new("Hello world", 42);
WriteLine($"Sending: {message}");

await SendAsync(networkStream, message);

var response = await Receive<MyMessage>(networkStream);
WriteLine($"Received: {response}");

ReadLine();


record MyMessage(string StringProperty, int IntProperty);