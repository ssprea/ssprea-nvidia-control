using System.Net.Http;
using System.Net.Sockets;
using Grpc.Net.Client;
using SLimit.Contracts;

namespace sspreaNvidiaControl.Models;

public class DaemonSession
{
    private string _socketPath;

    public GpuControl.GpuControlClient Client { get; }
    
    public DaemonSession(string socketPath)
    {
        _socketPath = socketPath;
        var handler = new SocketsHttpHandler
        {
            UseProxy = false,

            ConnectCallback = async (_, cancellationToken) =>
            {
                var socket = new Socket(
                    AddressFamily.Unix,
                    SocketType.Stream,
                    ProtocolType.Unspecified);

                try
                {
                    await socket.ConnectAsync(
                        new UnixDomainSocketEndPoint(socketPath),
                        cancellationToken);

                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };

        var channel = GrpcChannel.ForAddress(
            "http://localhost",
            new GrpcChannelOptions
            {
                HttpHandler = handler
            });


        Client = new GpuControl.GpuControlClient(channel);
    }
}