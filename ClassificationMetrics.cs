using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ImageToRGBArray
{
    /// <summary>
    /// Metric analysation for images to classify
    /// </summary>
    internal class ClassificationMetrics
    {
        /// <summary>
        /// Model to use for the metrics.
        /// </summary>
        private readonly string TrainingModel;

        /// <summary>
        /// Target dir of models to analyse.
        /// </summary>
        private readonly string TargetDir;

        /// <summary>
        /// Strict mode means that everything will be rated as failure which is not the first displayed result.
        /// </summary>
        private readonly bool Strict;

        /// <summary>
        /// The expected wordnet ID.
        /// </summary>
        private readonly string ID;

        /// <summary>
        /// The path to the trainmap file.
        /// </summary>
        private readonly string TrainMap;

        /// <summary>
        /// Target directory of the images to classify
        /// </summary>
        /// <param name="trainingModel">The model to use.</param>
        /// <param name="targetDir">The target directory of the images to validate.</param>
        /// <param name="id">The expected wordnet ID.</param>
        /// <param name="strict">Whether to use the strict mode or not.</param>
        public ClassificationMetrics(string trainingModel, string targetDir, string id, string trainMap, bool strict = false)
        {
            // TODO validate stuff here :P

            TrainingModel = trainingModel;
            TargetDir = targetDir;
            ID = id;
            Strict = strict;
            TrainMap = trainMap;
        }

        /// <summary>
        /// Runs the metricts and returns the success percentage.
        /// </summary>
        /// <returns>The success percentage.</returns>
        public float RunMetrics()
        {
            CNTKModelEvaluator evaluator = new CNTKModelEvaluator(TrainingModel);
            string[] files = Directory.GetFiles(TargetDir, string.Format("n{0}_*.jpeg", Convert.ToString(ID)));
            float fileAmount = files.Length;
            float counter = 0;

            foreach (string file in files)
            {
                ImagePixelProcessor processor = new ImagePixelProcessor(file, true);
                ClassDetector detector = new ClassDetector(
                    evaluator.Evaluate(processor.GetPixelsAsList()),
                    TrainMap
                );

                IEnumerable<string> wordnetIds = detector.GetWordnetIds();

                if (IsPresent(wordnetIds))
                {
                    counter++;
                }
            }

            return (counter / fileAmount) * 100;
        }

        /// <summary>
        /// Checks whether a wordnet is present in the resultset.
        /// </summary>
        /// <param name="wordnetIds">The list of wordnet IDs to validate.</param>
        /// <returns>The result.</returns>
        private bool IsPresent(IEnumerable<string> wordnetIds)
        {
            if (Strict)
            {
                return ID == wordnetIds.First();
            }

            return wordnetIds.Take(5).Contains(ID);
        }
    }
}
