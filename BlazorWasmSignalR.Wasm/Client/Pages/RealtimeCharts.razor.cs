using ApexCharts;
using BlazorWasmSignalR.Wasm.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;

namespace BlazorWasmSignalR.Wasm.Client.Pages;

public partial class RealtimeCharts
{
    private readonly IList<DataItem> _yenSeries = new List<DataItem>();
    private readonly IList<DataItem> _euroSeries = new List<DataItem>();
    private DataItem[] _radialData = default!;
    private ApexChart<DataItem> _radialChart = default!;
    private ApexChart<DataItem> _lineChart = default!;

    [Inject]
    private IConfiguration _configuration { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _radialData = new DataItem[1] {
            new(DateTime.Now.ToString("mm:ss"), 0)
        };

        var connection = new HubConnectionBuilder()
            .WithUrl(_configuration["RealtimeDataUrl"]!)
            .Build();

        await connection.StartAsync();

        var channelCurrencyStreamItem = await connection
            .StreamAsChannelAsync<CurrencyStreamItem>("CurrencyValues");

        var channelVariation = await connection
            .StreamAsChannelAsync<DataItem>("Variation");

        _ = ReadCurrencyStreamAsync(channelCurrencyStreamItem);
        _ = ReadVariationStreamAsync(channelVariation);
    }

    private async Task ReadCurrencyStreamAsync(ChannelReader<CurrencyStreamItem> channelCurrencyStreamItem)
    {
        // Wait asynchronously for data to become available
        while (await channelCurrencyStreamItem.WaitToReadAsync())
        {
            // Read all currently available data synchronously, before waiting for more data
            while (channelCurrencyStreamItem.TryRead(out var currencyStreamItem))
            {
                _yenSeries.Add(new(currencyStreamItem.Minute, currencyStreamItem.YenValue));
                _euroSeries.Add(new(currencyStreamItem.Minute, currencyStreamItem.EuroValue));

                await _lineChart.UpdateSeriesAsync();
            }
        }
    }

    private async Task ReadVariationStreamAsync(ChannelReader<DataItem> channelVariation)
    {
        // Wait asynchronously for data to become available
        while (await channelVariation.WaitToReadAsync())
        {
            // Read all currently available data synchronously, before waiting for more data
            while (channelVariation.TryRead(out var variation))
            {
                _radialData[0] = variation;

                await _radialChart.UpdateSeriesAsync();
            }
        }
    }

    private ApexChartOptions<DataItem> _lineChartOptions = new ApexChartOptions<DataItem>
    {
        Chart = new Chart
        {
            Animations = new()
            {
                Enabled = true,
                Easing = Easing.Linear,
                DynamicAnimation = new()
                {
                    Speed = 1000
                }
            },
            Toolbar = new()
            {
                Show = false
            },
            Zoom = new()
            { 
                Enabled = false
            }
        },
        Stroke = new Stroke { Curve = Curve.Straight },
        Xaxis = new()
        {
            Range = 12
        },
        Yaxis = new()
        {
            new()
            {
                DecimalsInFloat = 2,
                TickAmount = 5,
                Min = 0,
                Max = 5
            }
        }
    };

    private ApexChartOptions<DataItem> _radialChartOptions = new ApexChartOptions<DataItem>
    {
        PlotOptions = new()
        {
            RadialBar = new()
            {
                StartAngle = -135,
                EndAngle = 135
            }
        },
        Stroke = new()
        {
            DashArray = 4
        },
        Chart = new Chart
        {
            Animations = new()
            {
                Enabled = true,
                Easing = Easing.Linear,
                DynamicAnimation = new()
                {
                    Speed = 1100
                }
            }
        }
    };
}