using System;
using System.IO;
using System.Collections.Generic;

namespace ImageToRGBArray
{
    /// <summary>
    /// Program main class.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point into the Console Application.
        /// </summary>
        /// <param name="args">Arguments comming from the CLI ordered as an array</param>
        static void Main(string[] args)
        {
            ClassificationMetrics metrics = CreateMetricObject();
            Console.WriteLine(string.Format(
                "The match percentage is at {0}",
                string.Concat(metrics.RunMetrics(), "%")
            ));

            Console.ReadKey();
        }

        /// <summary>
        /// Creates the metrics object.
        /// </summary>
        /// <returns>The configured metric measurement object.</returns>
        private static ClassificationMetrics CreateMetricObject()
        {
            return new ClassificationMetrics(
                "CNTK model",
                "images to test",
                "wordnet db",
                "map file",
                AskStrictMode()
            );
        }

        /// <summary>
        /// Asks whether to run metrics in strict or non-strict module.
        /// </summary>
        /// <returns>Bool whether strict is turned on or off.</returns>
        private static bool AskStrictMode()
        {
            Console.WriteLine("Run metrics with strict mode (y/N)?");

            return Console.ReadLine() == "y";
        }
    }
}
