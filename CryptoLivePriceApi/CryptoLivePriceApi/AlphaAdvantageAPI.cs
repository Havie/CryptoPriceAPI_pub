
using System.Net;
using System.Text.Json;
//using System.Web.Script.Serialization;

namespace CryptoLivePriceApi
{
    internal class AlphaAdvantageAPI
    {
        private const string API_KEY = "V733UHAMY89NFMKO";
        public void Run(string[] args)
        {

            string symbol = "IBM";
            // replace the "demo" apikey below with your own key from https://www.alphavantage.co/support/#api-key
            //string QUERY_URL = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=IBM&interval=5min&apikey={API_KEY}";
            //string QUERY_URL = $"https://www.alphavantage.co/query?function=SYMBOL_SEARCH&keywords=BTC&apikey={API_KEY}";
            string QUERY_URL = $"https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=BTC&to_currency=USD&apikey={API_KEY}";
            Uri queryUri = new Uri(QUERY_URL);

            using (WebClient client = new WebClient())
            {
                // -------------------------------------------------------------------------
                // if using .NET Core (System.Text.Json)
                // using .NET Core libraries to parse JSON is more complicated. For an informative blog post
                // https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/

                dynamic json_data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(client.DownloadString(queryUri));
                // do something with the json_data
                Console.WriteLine(json_data);
                ParseJsonData(json_data);

            }
        }

        private static void ParseJsonData(dynamic json_data)
        {
            if (json_data != null)
            {
                foreach (var kvp in json_data)
                {
                    Console.WriteLine($"Key: {kvp.Key}");

                    // Check the type of the JsonElement to decide how to handle it
                    switch (kvp.Value.ValueKind)
                    {
                        case JsonValueKind.Object:
                            Console.WriteLine("This is a JSON object.");
                            // You can further parse the object if necessary
                            //ValueKind = Object : "{        "1. From_Currency Code": "BTC",        "2. From_Currency Name": "Bitcoin",        "3. To_Currency Code": "USD",        "4. To_Currency Name": "United States Dollar",        "5. Exchange Rate": "71760.80000000",        "6. Last Refreshed": "2024-03-12 16:04:08",        "7. Time Zone": "UTC",        "8. Bid Price": "71760.79000000",        "9. Ask Price": "71760.80000000"    }"

                            break;
                        case JsonValueKind.Array:
                            Console.WriteLine("This is a JSON array.");
                            // You can iterate through the array if necessary
                            break;
                        case JsonValueKind.String:
                            Console.WriteLine($"String value: {kvp.Value.GetString()}");
                            break;
                            // Handle other types (Number, True, False, etc.) as needed
                    }
                }
            }
        }
    }
}
