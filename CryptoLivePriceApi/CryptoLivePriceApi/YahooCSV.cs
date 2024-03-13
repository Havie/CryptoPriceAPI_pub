using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CryptoLivePriceApi
{
    public class YahooCSV
    {
        public void GenerateACSV(string currencyPair, string dateFrom, string dateTo, string settings = "daily")
        {
            string queryUrl = YahooCsvHelper.ConstructDownloadUrl(currencyPair, dateFrom, dateTo, settings);
            List<YahooPriceData> records = YahooCsvHelper.GetRecordsFromUrl<YahooPriceData>(queryUrl);

            // Save dataset as a CSV
            YahooCsvHelper.SaveAsCsv(records, $"{currencyPair}_Historical_Data.csv");

            // Save dataset as a JSON file
            YahooCsvHelper.SaveAsJson(records, $"{currencyPair}.json");

            string fullPath = Path.GetFullPath($"{currencyPair}_Historical_Data.csv");
            Console.WriteLine($"CSV file has been written to: {fullPath}");
        }

        public decimal GetPriceForUTCDate(string currencyPair, DateTime utcDateFromCSV, bool getHighestPrice = true)
        {
            try
            {
                var filteredRecords = YahooCsvHelper.GetRecordsForDate<YahooPriceData>(currencyPair, utcDateFromCSV);
                decimal targetPrice = getHighestPrice ? filteredRecords.Max(record => record.High) : filteredRecords.Min(record => record.Low);

                //Console.WriteLine($"The {(getHighestPrice ? "highest" : "lowest")} price for {utcDateFromCSV} is: {targetPrice}");
                return targetPrice;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }

        }
        public decimal GetPriceForUTCDate(string currencyPair, string utcDateFromCSV, bool getHighestPrice = true)
        {
            try
            {
                DateTime targetDate = DateTime.ParseExact(utcDateFromCSV, "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                return GetPriceForUTCDate(currencyPair, targetDate, getHighestPrice);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }
    }

    public static class YahooCsvHelper
    {
        public static string ConstructDownloadUrl(string ticker, string period1, string period2, string interval = "monthly")
        {
            try
            {
                var intervalReference = new Dictionary<string, string>
                {
                    { "daily", "1d" },
                    { "weekly", "1wk" },
                    { "monthly", "1mo" }
                };

                if (!intervalReference.TryGetValue(interval, out string _interval))
                {
                    Console.WriteLine("Interval code is incorrect");
                    return null;
                }

                long p1 = ConvertToSeconds(period1);
                long p2 = ConvertToSeconds(period2);

                return $"https://query1.finance.yahoo.com/v7/finance/download/{ticker}?period1={p1}&period2={p2}&interval={_interval}&filter=history";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static long ConvertToSeconds(string period)
        {
            DateTime dateTimeValue = DateTime.ParseExact(period, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return (long)(dateTimeValue - new DateTime(1970, 1, 1)).TotalSeconds + 86400;
        }

        public static List<T> GetRecordsFromUrl<T>(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                using (HttpContent content = response.Content)
                {
                    string csvData = content.ReadAsStringAsync().Result;

                    using (TextReader textReader = new StringReader(csvData))
                    using (CsvReader csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture))
                    {
                        return csvReader.GetRecords<T>().ToList();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<T>();
            }
        }

        public static void SaveAsCsv<T>(IEnumerable<T> records, string fileName)
        {
            using (var csvWriter = new CsvWriter(new StreamWriter(fileName), CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
            }
        }

        public static void SaveAsJson<T>(IEnumerable<T> records, string fileName)
        {
            using (StreamWriter file = File.CreateText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, records);
            }
        }

        public static IEnumerable<T> GetRecordsForDate<T>(string currencyPair, DateTime targetDate)
        {
            string singleDateStr = targetDate.ToString("yyyy-MM-dd");
            string queryUrl = ConstructDownloadUrl(currencyPair, singleDateStr, singleDateStr, "daily");
            return GetRecordsFromUrl<T>(queryUrl);
        }

        public static List<T> LoadCsvFile<T>(string fileName)
        {
            try
            {
                // Combine the current directory with the file name
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null, // Set MissingFieldFound to null to ignore missing fields

                };

                using (var reader = new StreamReader(filePath))
                //using (var csv = new CsvReader(reader, config))
                using (var csv = new CsvReader(reader, config))
                {
                    return csv.GetRecords<T>().ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading CSV file: {e.Message}");
                return new List<T>();
            }
        }

    }

    public class YahooPriceData
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class VoyagerCSVData
    {
        [Name("Timestamp (UTC)")]
        public DateTime Timestamp { get; set; }
        [Name("Type")]
        public string EntryType { get; set; }
        public long InternalID { get; set; }
        public string? Platform { get; set; }
        public string? PlatformID { get; set; }
        public string? BlockchainID { get; set; }
        public string? RecordType { get; set; }
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        [Name("AssetCostAtTime (in USD)")]
        public decimal? AssetCostAtTime { get; set; } // Make it nullable if the column may contain empty values
        public decimal? CostBasis { get; set; }
    }
}
