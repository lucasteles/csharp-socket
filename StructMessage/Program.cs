using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static System.Console;

(byte[] header, byte[] body) Encode<T>(T message) where T : struct
{
    var size = Marshal.SizeOf(message);
    var body = new byte[size];
    var ptr = Marshal.AllocHGlobal(size);
    Marshal.StructureToPtr(message, ptr, true);
    Marshal.Copy(ptr, body, 0, size);
    Marshal.FreeHGlobal(ptr);
    var header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));
    return (header, body);
}

T Decode<T>(byte[] body) where T : struct
{
    var size = Marshal.SizeOf<T>();
    var ptr = Marshal.AllocHGlobal(size);
    Marshal.Copy(body, 0, ptr, size);
    var response = (T)Marshal.PtrToStructure(ptr, typeof(T));
    Marshal.FreeHGlobal(ptr);
    return response;
}

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

async Task SendAsync<T>(NetworkStream stream, T message) where T : struct
{
    var (header, body) = Encode(message);
    var package = new byte[header.Length + body.Length];
    Buffer.BlockCopy(header, 0,package, 0, header.Length);
    Buffer.BlockCopy(body, 0,package, header.Length, body.Length);
    await stream.WriteAsync(package);
}

async Task<T> Receive<T>(NetworkStream stream) where T : struct
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

var message = new MyMessage{ IntProperty  = 42, StringProperty =  "Hello world"};
WriteLine($"Sending: {message}");

await SendAsync(networkStream, message);

var response = await Receive<MyMessage>(networkStream);
WriteLine($"Received: {response}");

ReadLine();

[StructLayout(LayoutKind.Sequential,CharSet = CharSet.Unicode)]
struct  MyMessage
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
    public string StringProperty;
    public int IntProperty;

    public override string ToString() => $"Message({StringProperty}, {IntProperty})";

}
