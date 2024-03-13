using CryptoLivePriceApi;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
//using System.Web.Script.Serialization;

internal class Program
{
    // Assumed in \bin\Debug\net7.0\ folder
    private static readonly string[] CSVFileNames = 
    {
        "Voyager_ALL_Transactions_Parsed.csv",
    };

    private static void Main(string[] args)
    {
        //TestThings();
        YahooCSV yahooCSV = new YahooCSV();

        foreach (var CSVFileName in CSVFileNames)
        {
            List<VoyagerCSVData> voyagerData = YahooCsvHelper.LoadCsvFile<VoyagerCSVData>(CSVFileName);

            bool isValid = voyagerData != null && voyagerData.Count > 0;
            if (!isValid)
            {
                Console.WriteLine($"INVALUD CSV : {CSVFileName}");
                continue;
            }

            //int c = 2; // make this line up with the sheet #s
            foreach (var vData in voyagerData)
            {
                decimal valueUSD = 1;
                if (vData.Asset.Contains("USD") == false) //ignore all USD/USDC entries the dollar is always $1
                {
                    valueUSD = yahooCSV.GetPriceForUTCDate($"{vData.Asset}-USD", vData.Timestamp, false);
                }

                //Console.WriteLine($"{c++} had : {vData.EntryType} -> {vData.Asset} @ {valueUSD}");
                vData.AssetCostAtTime = valueUSD;
                //Calculate the cost basis?
                vData.CostBasis = (vData.AssetCostAtTime * vData.Amount);
            }

            // Update the loaded CSVFileName to write the new calculated col CostAtTime
            YahooCsvHelper.SaveAsCsv(voyagerData, CSVFileName);
        }
    }

    private static void TestThings()
    {

        //AlphaAdvantageAPI alphaAPi = new AlphaAdvantageAPI();
        //alphaAPi.Run(args); //args could be thecsv array idk

        YahooCSV yahooCSV = new YahooCSV();
        yahooCSV.GenerateACSV("BTC-USD", "2021-01-01", "2021-01-15", "daily");
        //Timestamp(UTC)
        string utc = "2022-06-01T11:59:41.000";
        var test = yahooCSV.GetPriceForUTCDate($"ADA-USD", utc, false);
        Console.WriteLine($"test was: {test}");
    }
}
