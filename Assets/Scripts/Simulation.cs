using Assets.Scripts.Algorithms.Neat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Simulation : MonoBehaviour {

    public float TimeScale;

    public float PriceUpdateInterval;

    public GameObject trader;

    public float secondsToWaitBeforeRunning = 5;
    private float secondsLeftToWait;
    private bool startedExperiment = false;

    private ICurrencyInfoProvider CurrencyInfoProvider { get; set; }

    private NeatExperiment _neatExperiment;

    private List<Action<Trader>> spawnActions = new List<Action<Trader>>();

    private Thread simulationThread;

    void Start()
    {
        Time.timeScale = TimeScale;

        //////////////// Currency Information Provider Setup \\\\\\\\\\\\\\\\\\
        var realtimeCurrencyProvider = gameObject.AddComponent<RealtimeCurrencyInfoProvider>();

        realtimeCurrencyProvider.SetUpdateIntervalInSeconds(PriceUpdateInterval);

        CurrencyInfoProvider = realtimeCurrencyProvider;
        ////////////////////////////////////////////////////////////////////////

        secondsLeftToWait = secondsToWaitBeforeRunning;
    }

    void StartExperiment()
    {
        UnityThread.initUnityThread();

        simulationThread = new Thread(() => {
            _neatExperiment = new NeatExperiment(this);
        });

        simulationThread.Start();
    }

    void OnDestroy()
    {
        simulationThread.Abort();
    }
	
	// Update is called once per frame
	void Update () {

        if(TimeScale != Time.timeScale)
        {
            Time.timeScale = TimeScale;
        }

        secondsLeftToWait = secondsToWaitBeforeRunning - Time.realtimeSinceStartup;

        if (!startedExperiment && secondsLeftToWait < 0)
        {
            startedExperiment = true;
            StartExperiment();
        }

        if (spawnActions.Count > 0)
        {
            UnityThread.executeInUpdate(() => UnityEngine.Debug.Log("Spawning trader"));
            spawnActions[0](InstantiateTrader());
            spawnActions.RemoveAt(0);
        }
	}

    public void SpawnTrader(Action<Trader> action)
    {
        spawnActions.Add(action);
    }

    public Trader InstantiateTrader()
    {
        var obj = Instantiate(trader).GetComponent<Trader>();

        obj.transform.localPosition += new Vector3(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));

        return obj;
    }
}
