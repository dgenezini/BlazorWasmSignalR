namespace BlazorWasmSignalR.Wasm.Shared;

public class CurrencyStreamItem
{
    public required string Minute { get; set; }
    public decimal YenValue { get; set; }
    public decimal EuroValue { get; set; }
}