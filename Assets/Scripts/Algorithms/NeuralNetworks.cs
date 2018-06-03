using Assets.Scripts.Algorithms.Neat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Algorithms
{
    public class NeuralNetworks
    {
        private static NeuralNetworkInputProvider _neuralNetworkInputProvider;

        private static NeatExperiment _neatExperiment;

        public static void InitializeSharpNeat()
        {
            _neuralNetworkInputProvider = new NeuralNetworkInputProvider();

            _neatExperiment = new NeatExperiment(null);
        }
    }
}
