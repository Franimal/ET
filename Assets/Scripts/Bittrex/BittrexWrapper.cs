using Aurora.Models;
using Bittrex;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.Bittrex
{
    class BittrexWrapper
    {

        private string ApiKey { get; set; }
        private string SecretKey { get; set; }

        private BittrexClient Client { get; set; }

        public BittrexWrapper(string apikey, string secretKey)
        {
            ApiKey = apikey;
            SecretKey = secretKey;
        }

        public void RunTestMethod()
        {
            //GetTradeHistory("btc", "ltc");
        }

        public Task<PriceData[]> GetHistoricalData(string baseCurrency, string testCurrency)
        {
            return GetHistoricalData(baseCurrency + "-" + testCurrency);
        }

        public Task<PriceData[]> GetHistoricalData(string marketName)
        {
            //https://bittrex.com/Api/v2.0/pub/market/GetTicks?marketName=BTC-XMR&tickInterval=day

            var parameters = new NameValueCollection();

            parameters.Add("marketName", marketName);
            parameters.Add("tickInterval", "day");

            return MakeArrayRequest<PriceData>(BittrexApi.Public, "GetTicks", parameters, "api/v2.0/pub/market/");
        }

        public Task<OrderBook> GetOrderBook(string baseCurrency, string testCurrency)
        {
            var parameters = new NameValueCollection();

            parameters.Add("market", baseCurrency + "-" + testCurrency);
            parameters.Add("depth", "50");
            parameters.Add("Type", "both");

            return MakeObjectRequest<OrderBook>(BittrexApi.Public, "getorderbook", parameters);
        }

        public Task<TradeHistory[]> GetTradeHistory(string baseCurrency, string testCurrency)
        {
            var parameters = new NameValueCollection();

            parameters.Add("market", baseCurrency + "-" + testCurrency);
            parameters.Add("count", "100");

            return MakeArrayRequest<TradeHistory>(BittrexApi.Public, "getmarkethistory", parameters, null);
        }

        public Task<MarketSummary[]> GetMarketSummaries()
        {
            return MakeArrayRequest<MarketSummary>(BittrexApi.Public, "getmarketsummaries", null, null);
        }

        public Task<GetBalancesQuery.Balance[]> GetBalancesAsync()
        {
            return Client.GetBalancesAsync();
        }

        public Task<GetMarketsQuery.Market[]> GetMarkets()
        {
            return Client.GetMarketsAsync();
        }

        private Task<T> MakeObjectRequest<T>(BittrexApi api, string method, NameValueCollection parameters)
        {
            return Task.Run(() =>
            {
                string relativeUrl = "api/v1.1";

                switch (api)
                {
                    case BittrexApi.Account:
                        relativeUrl = Path.Combine(relativeUrl, "account");
                        break;
                    case BittrexApi.Market:
                        relativeUrl = Path.Combine(relativeUrl, "market");
                        break;
                    case BittrexApi.Public:
                        relativeUrl = Path.Combine(relativeUrl, "public");
                        break;
                }

                relativeUrl = Path.Combine(relativeUrl, method);

                BittrexRequest request = new BittrexRequest()
                {
                    Api = api,
                    RelativeUrl = relativeUrl,
                    Parameters = parameters == null ? new NameValueCollection() : parameters
                };

                BittrexResponse<T> response = null;

                try
                {
                    response = Client.SendRequestAsync<T>(request).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    return default(T);
                }

                if (!response.Success)
                {
                    Console.WriteLine(response.Message);
                }

                return response.Result;
            });
        }

        private Task<T[]> MakeArrayRequest<T>(BittrexApi api, string method, NameValueCollection parameters, string customRelativeUrl)
        {
            // Console.WriteLine("Making request: " + method);

            return Task.Run(() =>
            {
                string relativeUrl = null;

                if (customRelativeUrl == null)
                {
                    relativeUrl = "api/v1.1";

                    switch (api)
                    {
                        case BittrexApi.Account:
                            relativeUrl = Path.Combine(relativeUrl, "account");
                            break;
                        case BittrexApi.Market:
                            relativeUrl = Path.Combine(relativeUrl, "market");
                            break;
                        case BittrexApi.Public:
                            relativeUrl = Path.Combine(relativeUrl, "public");
                            break;
                    }

                    relativeUrl = Path.Combine(relativeUrl, method);
                }
                else
                {
                    relativeUrl = customRelativeUrl;
                    relativeUrl = Path.Combine(relativeUrl, method);
                }

                BittrexRequest request = new BittrexRequest()
                {
                    Api = api,
                    RelativeUrl = relativeUrl,
                    Parameters = parameters == null ? new NameValueCollection() : parameters
                };

                BittrexResponse<JsonArray> response = null;

                response = Client.SendRequestAsync<JsonArray>(request).GetAwaiter().GetResult();

                if (!response.Success)
                {
                    Console.WriteLine(response.Message);
                    Console.ReadLine();
                }

                if (response.Result == null)
                {
                    Console.WriteLine("Request failed: " + method + " Parameters: ");
                    parameters.AllKeys.ToList().ForEach(key => { Console.WriteLine(key + " , " + parameters.Get(key)); });
                    return null;
                }

                var objects = response.Result.Select(item => JsonConvert.DeserializeObject<T>(item.ToString())).ToArray();

                //var dictionaries = response.Result.Select(item => JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ToString())).ToArray();
                //dictionaries.First().Keys.ToList().ForEach(key => Console.WriteLine(key));

                return objects;
            });
        }

        private Task<List<OpenOrderSet>> MakeOpenOrderRequest<T>(BittrexApi api, string method, NameValueCollection parameters)
        {
            // Console.WriteLine("Making request: " + method);

            return Task.Run(() =>
            {
                string relativeUrl = "api/v1.1";

                switch (api)
                {
                    case BittrexApi.Account:
                        relativeUrl = Path.Combine(relativeUrl, "account");
                        break;
                    case BittrexApi.Market:
                        relativeUrl = Path.Combine(relativeUrl, "market");
                        break;
                    case BittrexApi.Public:
                        relativeUrl = Path.Combine(relativeUrl, "public");
                        break;
                }

                relativeUrl = Path.Combine(relativeUrl, method);

                BittrexRequest request = new BittrexRequest()
                {
                    Api = api,
                    RelativeUrl = relativeUrl,
                    Parameters = parameters == null ? new NameValueCollection() : parameters
                };

                BittrexResponse<List<OpenOrderSet>> response = null;

                response = Client.SendRequestAsync<List<OpenOrderSet>>(request).GetAwaiter().GetResult();

                if (!response.Success)
                {
                    Console.WriteLine(response.Message);
                    Console.ReadLine();
                }

                if (response.Result == null)
                {
                    Console.WriteLine("Request failed: " + method + " Parameters: ");
                    parameters.AllKeys.ToList().ForEach(key => { Console.WriteLine(key + " , " + parameters.Get(key)); });
                    return null;
                }

                //var objects = response.Result.Select(item => JsonConvert.DeserializeObject<T>(item.ToString())).ToList();

                //var dictionaries = response.Result.Select(item => JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ToString())).ToArray();
                //dictionaries.First().Keys.ToList().ForEach(key => Console.WriteLine(key));

                return response.Result;
            });
        }

        public bool Connect()
        {
            Client = new BittrexClient(ApiKey, SecretKey);
            return Client != null;
        }

        public string GetReadableName()
        {
            return "Bittrex";
        }

        public Task<OpenOrder[]> GetExistingOrders(string marketName)
        {
            var parameters = new NameValueCollection();

            if (marketName != null)
            {
                parameters.Add("market", marketName.ToUpper());
            }

            return MakeArrayRequest<OpenOrder>(BittrexApi.Market, "getopenorders", parameters, null);
        }

        public Task<OrderStatus> Cancel(string uuid)
        {
            var parameters = new NameValueCollection();

            parameters.Add("uuid", uuid);

            return MakeObjectRequest<OrderStatus>(BittrexApi.Market, "cancel", parameters);
        }

        public Task<TradeRequestModel> Sell(BittrexWrapper api, Coin coin, double amount, double value)
        {
            //Console.WriteLine();
            //Console.WriteLine("About to sell coin: " + coin.MarketName + " Amount: " + amount + ", Value: " + value);
            //Console.WriteLine();
            ////Console.WriteLine("Y to confirm, any other key to abort trade.");
            //string resp = "Y";
            //
            //if (!resp.Equals("Y"))
            //{
            //    return null;
            //}
            //
            //var parameters = new NameValueCollection();
            //
            //parameters.Add("market", coin.MarketName.ToUpper());
            //parameters.Add("quantity", "" + amount);
            //parameters.Add("rate", "" + value);
            //
            //return MakeObjectRequest<TradeRequestModel>(BittrexApi.Market, "selllimit", parameters);
            throw new NotImplementedException();
        }

        public Task<TradeRequestModel> Buy(BittrexWrapper api, Coin coin, double amount, double value)
        {
            //Console.WriteLine("About to buy coin: " + coin.MarketName + " Amount: " + amount + ", Value: " + value);
            ////Console.WriteLine("Y to confirm, any other key to abort trade.");
            //string resp = "Y";
            //
            //if (!resp.Equals("Y"))
            //{
            //    return null;
            //}
            //
            //var parameters = new NameValueCollection();
            //
            //parameters.Add("market", coin.MarketName.ToUpper());
            //parameters.Add("quantity", "" + amount);
            //parameters.Add("rate", "" + value);
            //
            //return MakeObjectRequest<TradeRequestModel>(BittrexApi.Market, "buylimit", parameters);
            throw new NotImplementedException();
        }
    }
}

