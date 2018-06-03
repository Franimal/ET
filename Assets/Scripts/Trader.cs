using Assets.Scripts.Algorithms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trader : MonoBehaviour
{
    public float NetValue;

    public float currentEnergy = 1000; //IN BTC as all markets are BTC-SOMETHING

    public float totalValue = 0;

    public float energyLossPerSecond = 30;

    public float timeTillDeath = 100;
    private float _ageRandomness = 2;

    public float turnSpeed = 0.1f;
    public float moveSpeed = 0.1f;

    public float amountToTradeAtOnce = 1;

    private float _timeCreated;
    private float lifespan;

    private Rigidbody _rb;

    private Vector3 _moveForce = new Vector3(0, 0, 1f);

    private Coin _currentCoin;

    public NervousSystem _nervousSystem { get; private set; } = new NervousSystem();

    private Dictionary<string, float> CoinBalances { get; set; } = new Dictionary<string, float>();

    private Coin[] Coins { get; set; }

    private float lastTradeTime;
    private float noTradeEnergyRemovalTime;
    private float energyLostForNotTrading;

    private Material material;
    private Color originalColor;
    private Color enterColor = new Color(1, 1, 1, 1);
    private Color buyColor = new Color(0, 1, 0, 1);
    private Color sellColor = new Color(1, 0, 0, 1);


    // Use this for initialization
    void Start()
    {
        material = GetComponent<Renderer>().material;
        originalColor = material.color;

        Coins = FindObjectsOfType<Coin>();

        noTradeEnergyRemovalTime = timeTillDeath / 10f;

        timeTillDeath += Random.Range(-_ageRandomness, _ageRandomness);
        _timeCreated = Time.time;
        _rb = transform.GetComponent<Rigidbody>();        
    }

    // Update is called once per frame
    void Update()
    {
        lifespan = Time.time - _timeCreated;
        currentEnergy -= energyLossPerSecond * Time.deltaTime;
        timeTillDeath -= Time.deltaTime;

        //Punish for not trading often enough
        if(Time.time - lastTradeTime >= noTradeEnergyRemovalTime)
        {
            currentEnergy -= energyLossPerSecond * 10f;
            energyLostForNotTrading += energyLossPerSecond * 10f;
            lastTradeTime = Time.time;
        }

        var value = currentEnergy;
 
        foreach (var coinBalance in CoinBalances)
        {
            Debug.Log($"{coinBalance.Key} balance: {coinBalance.Value}");
            value += coinBalance.Value * Coins.FirstOrDefault(coin => coin.CoinName == coinBalance.Key)?.LatestBuyOffer ?? 0;
        }

        totalValue = value;

        NetValue = totalValue + lifespan * energyLossPerSecond + energyLostForNotTrading;

        if (currentEnergy <= 0 || timeTillDeath <= 0)
        {
            Dead();
        }

        if (_nervousSystem.ShouldTurnLeft())
        {
            TurnLeft();
        } 

        if (_nervousSystem.ShouldTurnRight())
        {
            TurnRight();
        } 

        if (_nervousSystem.ShouldTurnUp())
        {
            TurnUp();
        } 

        if (_nervousSystem.ShouldTurnDown())
        {
            TurnDown();
        } 

        if (_nervousSystem.ShouldMove())
        {
            Move();
        } 

        if (_nervousSystem.ShouldMoveBackwards())
        {
            MoveBackwards();
        } 

        if (_nervousSystem.ShouldTryBuy())
        {
            TryBuy();
        } else

        if (_nervousSystem.ShouldTrySell())
        {
            TrySell();
        }
    }

    private void ReportLifetimeFitness()
    {
        //If 
       // var lifespan = Time.realtimeSinceStartup - _timeCreated;

       // var maxEnergyAchieved = ;

        //var numberOfTimesReproductiveThresholdReached = ;

        //var meanEnergyOverLifetime = ; 
        
    }

    private void Dead()
    {
        //ReportLifetimeFitness();
        //Destroy(gameObject);
    }

    public float GetLifespan()
    {
        return lifespan;
    }

    public float GetTotalCurrentValue()
    {
        return totalValue;
    }

    private float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    private void TurnLeft()
    {
        // transform.Rotate(0, -turnSpeed * Time.deltaTime, 0);
        _rb.AddRelativeForce(Vector3.left * Time.deltaTime * moveSpeed, ForceMode.Force);
    }

    private void TurnRight()
    {
        //transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
        _rb.AddRelativeForce(Vector3.right * Time.deltaTime * moveSpeed, ForceMode.Force);
    }

    private void TurnUp()
    {
        // transform.Rotate(0, 0, -turnSpeed * Time.deltaTime);
        _rb.AddRelativeForce(Vector3.up * Time.deltaTime * moveSpeed, ForceMode.Force);
    }

    private void TurnDown()
    {
        //transform.Rotate(0, 0, turnSpeed * Time.deltaTime);
        _rb.AddRelativeForce(Vector3.down * Time.deltaTime * moveSpeed, ForceMode.Force);
    }

    private void Move()
    {
        _rb.AddRelativeForce(Vector3.forward * Time.deltaTime * moveSpeed, ForceMode.Force);
    }

    private void MoveBackwards()
    {
        _rb.AddRelativeForce(Vector3.back * Time.deltaTime * moveSpeed, ForceMode.Force);
    }

    private void TryBuy()
    {
        if (_currentCoin == null)
        {
            return;
        }

        lastTradeTime = Time.time;

        material.color = buyColor;

        if (!CoinBalances.ContainsKey(_currentCoin.CoinName))
        {
            CoinBalances.Add(_currentCoin.CoinName, amountToTradeAtOnce);
        }
        else
        {
            CoinBalances[_currentCoin.CoinName] = CoinBalances[_currentCoin.CoinName] + amountToTradeAtOnce;
        }

        var fee = _currentCoin.LatestSellOffer * amountToTradeAtOnce * 0.0025f;

        currentEnergy -= (_currentCoin.LatestSellOffer * amountToTradeAtOnce + fee);
    }

    private void TrySell()
    {
        if (_currentCoin == null)
        {
            return;
        }

        if (CoinBalances.ContainsKey(_currentCoin.CoinName) && CoinBalances[_currentCoin.CoinName] > 0)
        {
            lastTradeTime = Time.time;

            material.color = sellColor;

            CoinBalances[_currentCoin.CoinName] = CoinBalances[_currentCoin.CoinName] - amountToTradeAtOnce;

            var fee = _currentCoin.LatestBuyOffer * amountToTradeAtOnce * 0.0025f;

            currentEnergy += (_currentCoin.LatestBuyOffer * amountToTradeAtOnce - fee);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        var coin = collider.gameObject.GetComponent<Coin>();

        if (coin != null)
        {
            material.color = enterColor; 
            _currentCoin = coin;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        material.color = originalColor;
        var coin = collider.gameObject.GetComponent<Coin>();

        if (coin != null)
        {
            _currentCoin = null;
        }
    }
}
