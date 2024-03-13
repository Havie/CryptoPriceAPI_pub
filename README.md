# CryptoAPI
A simple hacky **.NET** application I wrote in an afternoon that can parse a **CSV** file and fill the price for a given date for a crypto currency and USD.
It uses an API call to _ _yahoo finance_ _ which I adapted from a python tutorial.

**This app is by no means robust or complete but it works for what I needed it to and was pretty fun. I may turn this into something someday.**

### How It Works
Starts by looking the CSV located at `\CryptoLivePriceApi\CryptoLivePriceApi\bin\Debug\net7.0\Voyager_ALL_Transactions_Parsed.csv`,
it finds its **UTC Time** for each entry and fills in the **Price in USD** at that time in a new column, then the cost basis based on the # of shares.

It also supports multiple csv's with the same rigid format, just need to add them to the array at the top of the program.cs

Also makes us of the nuget packages:
* CsvHelper
* Newtonsoft.Json