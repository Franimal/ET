using SharpNeat.Core;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Algorithms.Neat
{
    public class TraderBlackBoxEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        private const double StopFitness = 100000.0;
        private ulong _evalCount;
        private bool _stopConditionSatisfied = false;

        private Simulation _simulation;

        private Coin[] coins;

        public TraderBlackBoxEvaluator(Simulation simulation)
        {
            coins = null;
            UnityThread.SetUnityValue(() => coins = UnityEngine.Object.FindObjectsOfType<Coin>());
            _simulation = simulation;
        }

        public ulong EvaluationCount {
            get {
                return _evalCount;
            }
        }

        bool IPhenomeEvaluator<IBlackBox>.StopConditionSatisfied {
            get {
                return _stopConditionSatisfied;
            }
        }

        public FitnessInfo Evaluate(IBlackBox phenome)
        {
            try
            {
                double fitness = 0;

                ISignalArray inputArr = phenome.InputSignalArray;
                ISignalArray outputArr = phenome.OutputSignalArray;

                _evalCount++;

                phenome.ResetState();

                //Allow this blackbox to be used for the lifetime of a trader.  Lifetime fitness is returned when said Trader is dead.   

                var traderPrefab = _simulation.trader;

                Trader trader = null;

                UnityThread.SetUnityValue(() =>
                {
                    //UnityEngine.Debug.Log("Starting action " + Time.realtimeSinceStartup);
                    var obj = UnityEngine.Object.Instantiate(traderPrefab).GetComponent<Trader>();
                    //UnityEngine.Debug.Log("Instantiated trader, null: " + (obj == null));
                    obj.transform.localPosition += new Vector3(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
                    //UnityEngine.Debug.Log("set position");
                    trader = obj;
                    //UnityEngine.Debug.Log(Time.realtimeSinceStartup + "  : set trader.  Null: " + trader == null);
                    //UnityEngine.Debug.Log("ending action " + Time.realtimeSinceStartup);
                });

                //Thread.Sleep(500);
                //Set its initial variables (lifetime etc) or leave default            

                //Set blackbox for trader

                //Run until trader is dead 
                while (trader.currentEnergy > 0 && trader.timeTillDeath > 0)
                {
                    //ourPosition, othersPositions, coinNames, coinPositions, coinPrices, coinHighs, coinLows, 

                    var position = Vector3.zero;
                    UnityThread.SetUnityValue(() => position = trader.transform.localPosition);

                    var rotation = Vector3.zero;
                    UnityThread.SetUnityValue(() => rotation = trader.transform.localRotation.eulerAngles);

                    //Inputs
                    inputArr[0] = position.x;
                    inputArr[1] = position.y;
                    inputArr[2] = position.z;
                    inputArr[3] = rotation.x;
                    inputArr[4] = rotation.y;
                    inputArr[5] = rotation.z;

                    var insertIndex = 6;

                    for (var i = 0; i < coins.Length; i++)
                    {
                        inputArr[insertIndex++] = coins[i].CoinPrice;
                        inputArr[insertIndex++] = coins[i].CoinHigh;
                        inputArr[insertIndex++] = coins[i].CoinLow;
                        inputArr[insertIndex++] = coins[i].LatestBuyOffer;
                        inputArr[insertIndex++] = coins[i].LatestSellOffer;
                    }

                    phenome.Activate();

                    //Determine current outputs
                    trader._nervousSystem._move = outputArr[0] > 0.5;
                    trader._nervousSystem._turnLeft = outputArr[1] > 0.5;
                    trader._nervousSystem._turnRight = outputArr[2] > 0.5;
                    trader._nervousSystem._turnUp = outputArr[3] > 0.5;
                    trader._nervousSystem._turnDown = outputArr[4] > 0.5;
                    trader._nervousSystem._tryBuy = outputArr[5] > 0.5;
                    trader._nervousSystem._trySell = outputArr[6] > 0.5;
                    trader._nervousSystem._move = outputArr[7] > 0.5;
                }

                fitness = Math.Max(trader.GetTotalCurrentValue(), 0) + trader.GetLifespan();

                UnityThread.executeInUpdate(() =>
                {
                    UnityEngine.Debug.Log($"Individuals Fitness: {fitness}");
                    UnityEngine.Object.Destroy(trader.gameObject);
                });

                //Report lifetime fitness
                return new FitnessInfo(fitness, fitness);
            }
            catch (Exception e)
            {
                UnityThread.executeInUpdate(() => UnityEngine.Debug.Log(Time.realtimeSinceStartup + " : " + e.Message + e.StackTrace));
                return FitnessInfo.Zero;
            }
        }

        public void Reset()
        {

        }
    }
}
