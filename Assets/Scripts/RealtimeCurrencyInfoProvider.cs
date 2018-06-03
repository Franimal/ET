using Assets.Scripts.Bittrex;
using Aurora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class RealtimeCurrencyInfoProvider : MonoBehaviour, ICurrencyInfoProvider
{
    private float _priceUpdateInterval;
    private float _lastPriceUpdate;

    private Coin[] Coins { get; set; }

    private BittrexWrapper Api { get; set; }

    private Action _updateAction;

    private bool completedLastUpdate = true;

    private void UpdatePrices(MarketSummary[] marketSummaries, Dictionary<string, OrderBook> orderBooks)
    {
        foreach(var coin in Coins)
        {
            var market = marketSummaries.FirstOrDefault(ms => ms.MarketName == coin.CoinName);
            var book = orderBooks[coin.CoinName];

            if (coin.LatestSellOffer != (float)book.Buy[0].Rate)
            {
                //Debug.Log($"{coin.CoinName} Updated.  Price (in btc): {(float)market.Last} Latest Sell Offer: {(float)book.Sell[0].Rate} Latest Buy Offer: {(float)book.Buy[0].Rate}");
            }

            coin.CoinPrice = (float)market.Last;
            coin.CoinHigh = (float)market.High;
            coin.CoinLow = (float)market.Low;

            coin.LatestSellOffer = (float)book.Sell[0].Rate;
            coin.LatestBuyOffer = (float)book.Buy[0].Rate;
        }
    }

    void Awake()
    {
        /// Find all coins active in the scene.
        Coins = FindObjectsOfType<Coin>();

        Api = new BittrexWrapper("c18983bdefc341c3ac7b2b915b85f5ee", "8c9926193b7a46b5a440a9c74d9c6f5b");
        Api.Connect();

        _updateAction = (async () => {

            completedLastUpdate = false;

            var marketSummaries = await Task.Run(() => Api.GetMarketSummaries());

            var orderBooks = new Dictionary<string, OrderBook>();

            for (var i = 0; i < Coins.Length; i++)
            {
                var book = await Task.Run(() => Api.GetOrderBook(Coins[i].CoinName.Split('-')[0], Coins[i].CoinName.Split('-')[1]));
                orderBooks.Add(Coins[i].CoinName, book);
            }

            UnityThread.executeInUpdate(() => UpdatePrices(marketSummaries, orderBooks));

            completedLastUpdate = true;
        });

        Task.Run(_updateAction);
    }

    void Update()
    {
        ///Update coin information if enough time has passed (real time, not game time which may be sped up or slowed down)
        if (Time.realtimeSinceStartup - _lastPriceUpdate >= _priceUpdateInterval)
        {
            _lastPriceUpdate = Time.realtimeSinceStartup;
            
            if (completedLastUpdate)
            {
                Task.Run(_updateAction);
            }
        }
    }

    internal void SetUpdateIntervalInSeconds(float priceUpdateInterval)
    {
        _priceUpdateInterval = priceUpdateInterval;
    }
}
