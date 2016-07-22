using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.MSR.CNTK.Extensibility.Managed;

namespace ImageToRGBArray
{
    /// <summary>
    /// Evaluator service for .MODEL files.
    /// </summary>
    internal class CNTKModelEvaluator
    {
        /// <summary>
        /// The model managed by the evaluator.
        /// </summary>
        private readonly IEvaluateModelManagedF Model;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path to the file containing a trained model.</param>
        /// <param name="threads">The amount of threads to use.</param>
        public CNTKModelEvaluator(string path, int threads = 1)
        {
            Model = new IEvaluateModelManagedF();
            Model.Init(string.Format("numCPUThreads={0}", threads));
            Model.CreateNetwork(string.Format("modelPath=\"{0}\"", path));
        }

        /// <summary>
        /// Simple evaluation helper.
        /// </summary>
        /// <param name="list">The list of RGBA data.</param>
        /// <returns>The new float list.</returns>
        public IEnumerable<int> Evaluate(List<float> list)
        {
            return GetBestFiveMatches(Model.Evaluate(
                new Dictionary<string, List<float>>() { { Model.GetNodeDimensions(NodeGroup.Input).First().Key, list } },
                Model.GetNodeDimensions(NodeGroup.Output).First().Key
            ));
        }

        /// <summary>
        /// Searches the best five matches from the given float list.
        /// </summary>
        /// <param name="list">A float list containing results for all 1000 trained classes.</param>
        /// <returns>The indexes (^=class offset) of the best five matches.</returns>
        private IEnumerable<int> GetBestFiveMatches(List<float> list)
        {
            return list.OrderByDescending(item => item).Take(5).Select((float value) => list.IndexOf(value));
        }
    }
}
