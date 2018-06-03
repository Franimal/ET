using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Algorithms.Neat
{
    public class NeatExperiment
    {

        private IGenomeFactory<NeatGenome> _genomeFactory;
        private List<NeatGenome> _genomeList;
        private NeatEvolutionAlgorithm<NeatGenome> _ea;

        private NeatEvolutionAlgorithmParameters _eaParams;
        private NeatGenomeParameters _neatGenomeParams;    
        private NetworkActivationScheme _activationScheme;
        private ParallelOptions _parallelOptions;

        private string _name;
        private int _populationSize;
        private int _specieCount;
        private string _complexityRegulationStr;
        private int? _complexityThreshold;
        private string _description;
        private int _inputCount;
        private int _outputCount;

        private Simulation _simulation;

        public NeatExperiment(Simulation simulation)
        {
            _simulation = simulation;

            _name = "Trader";
            _populationSize = 100;
            _specieCount = 1;
            _activationScheme = NetworkActivationScheme.CreateAcyclicScheme();
            _complexityRegulationStr = "Relative";
            _complexityThreshold = 10;
            _description = "Generate trader neural network";
            _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 2 };

            UnityThread.SetUnityValue(() => _inputCount = 6 + UnityEngine.Object.FindObjectsOfType<Coin>().Length * 5);

            _outputCount = 8;

            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = _specieCount;

            _neatGenomeParams = new NeatGenomeParameters();
            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;
            //_neatGenomeParams.ActivationFn = LeakyReLU.__DefaultInstance;
            _neatGenomeParams.ActivationFn = SharpNeat.Network.SReLU.__DefaultInstance;
            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            _genomeFactory = CreateGenomeFactory();
            
            // Create an initial population of randomly generated genomes.
            _genomeList = _genomeFactory.CreateGenomeList(_populationSize, 0);
            
            // Create evolution algorithm and attach update event.
            _ea = CreateEvolutionAlgorithm(_genomeFactory, _genomeList);
            // _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);
            
            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
        }

        public IGenomeFactory<NeatGenome> CreateGenomeFactory()
        {
            return new NeatGenomeFactory(_inputCount, _outputCount, _neatGenomeParams);
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
        {
            return CreateEvolutionAlgorithm(_populationSize);
        }

        public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
        {
            return new NeatGenomeDecoder(_activationScheme);
        }


        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize)
        {
            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();

            // Create an initial population of randomly generated genomes.
            List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

            // Create evolution algorithm.
            return CreateEvolutionAlgorithm(genomeFactory, genomeList);
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weight difference.
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, _parallelOptions);
            
            // Create complexity regulation strategy.
            IComplexityRegulationStrategy complexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);
            
            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);
            
            // Create IBlackBox evaluator.
            var evaluator = new TraderBlackBoxEvaluator(_simulation);
            
            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = CreateGenomeDecoder();
            
            // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
            IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, _parallelOptions);
            
            // Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
            // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
            //  IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
            //                                                                         innerEvaluator,
            //                                                                        SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_PeriodicReevaluation(5));

            // Initialize the evolution algorithm.
            ea.Initialize(innerEvaluator, genomeFactory, genomeList);

            // Finished. Return the evolution algorithm
            return ea;
        }
    }
}
