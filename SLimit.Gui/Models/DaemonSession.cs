using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Serilog;
using SLimit.Contracts;

namespace SLimit.Gui.Models;

public class DaemonSession
{
    private string _socketPath;

    public GpuControl.GpuControlClient? Client { get; private set; }
    public bool IsConnected { get; private set; }
    
    public DaemonSession(string socketPath)
    {
        _socketPath = socketPath;
        
    }

    public async Task ConnectAsync()
    {
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
                        new UnixDomainSocketEndPoint(_socketPath),
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
        IsConnected = true;
        Log.Information("Connected to daemon! ");
    }
}