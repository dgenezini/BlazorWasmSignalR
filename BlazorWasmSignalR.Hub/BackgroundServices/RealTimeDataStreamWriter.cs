using BlazorWasmSignalR.Wasm.Shared;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace BlazorWasmSignalR.SignalRServer.BackgroundServices;

public class RealTimeDataStreamWriter
{
    private readonly Dictionary<string, ChannelWriter<CurrencyStreamItem>> _currencyWriters;
    private readonly Dictionary<string, ChannelWriter<DataItem>> _variationWriters;
    private readonly Timer _timer = default!;

    private int _currentVariationValue = 50;
    private decimal _currentYenValue = RandomNumberGenerator.GetInt32(1, 3);
    private decimal _currentEuroValue = RandomNumberGenerator.GetInt32(1, 3);

    public RealTimeDataStreamWriter()
    {
        _timer = new(OnElapsedTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        _currencyWriters = new();
        _variationWriters = new();
    }

    public void AddCurrencyListener(string connectionId, ChannelWriter<CurrencyStreamItem> channelWriter)
    {
        _currencyWriters[connectionId] = channelWriter;
    }

    public void AddVariationListener(string connectionId, ChannelWriter<DataItem> channelWriter)
    {
        _variationWriters[connectionId] = channelWriter;
    }

    public void RemoveListeners(string connectionId)
    {
        _currencyWriters.Remove(connectionId);
        _variationWriters.Remove(connectionId);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void OnElapsedTime(object? state)
    {
        SendCurrencyData();
        SendVariationData();
    }

    private void SendCurrencyData()
    {
        var date = DateTime.Now;

        var yenDecimals = RandomNumberGenerator.GetInt32(-20, 20) / 100M;
        var euroDecimals = RandomNumberGenerator.GetInt32(-20, 20) / 100M;

        _currentYenValue = Math.Max(0.5M, _currentYenValue + yenDecimals);
        _currentEuroValue = Math.Max(0.5M, _currentEuroValue + euroDecimals);

        var currencyStreamItem = new CurrencyStreamItem()
        {
            Minute = date.ToString("hh:mm:ss"),
            YenValue = _currentYenValue,
            EuroValue = _currentEuroValue
        };

        foreach(var listener in _currencyWriters)
        {
            _ = listener.Value.WriteAsync(currencyStreamItem);
        }
    }

    private void SendVariationData()
    {
        var min = Math.Max(0, _currentVariationValue - 20);
        var max = Math.Min(100, _currentVariationValue + 20);

        var variationValue = new DataItem(DateTime.Now.ToString("hh:mm:ss"),
            RandomNumberGenerator.GetInt32(min, max));

        _currentVariationValue = (int)variationValue.Value;

        foreach (var listener in _variationWriters)
        {
            _ = listener.Value.WriteAsync(variationValue);
        }
    }
}
