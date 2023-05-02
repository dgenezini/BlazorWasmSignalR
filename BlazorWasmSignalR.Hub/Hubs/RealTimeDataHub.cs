using BlazorWasmSignalR.SignalRServer.BackgroundServices;
using BlazorWasmSignalR.Wasm.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

namespace BlazorWasmSignalR.SignalRServer.Hubs;

public class RealTimeDataHub : Hub
{
    private readonly RealTimeDataStreamWriter _realTimeDataStreamWriter;

    public RealTimeDataHub(RealTimeDataStreamWriter realTimeDataStreamWriter)
    {
        _realTimeDataStreamWriter = realTimeDataStreamWriter;
    }

    public ChannelReader<CurrencyStreamItem> CurrencyValues(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<CurrencyStreamItem>();

        _realTimeDataStreamWriter.AddCurrencyListener(Context.ConnectionId, channel.Writer);

        return channel.Reader;
    }

    public ChannelReader<DataItem> Variation(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<DataItem>();

        _realTimeDataStreamWriter.AddVariationListener(Context.ConnectionId, channel.Writer);

        return channel.Reader;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _realTimeDataStreamWriter.RemoveListeners(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
