using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ImageToRGBArray
{
    /// <summary>
    /// Custom exception class for the image parser.
    /// </summary>
    class DetectorException : Exception
    {
        /// <summary>
        /// Constructor.
        /// NOTE: this exception requires an exception message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public DetectorException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Detects a class for the given offset.
    /// </summary>
    internal class ClassDetector
    {
        /// <summary>
        /// The class offset.
        /// </summary>
        private IEnumerable<int> Offsets;

        /// <summary>
        /// The trainmapdata.
        /// </summary>
        private string TrainMapData;

        /// <summary>
        /// The wordnetdata.
        /// </summary>
        private string WordNetData;

        /// <summary>
        /// The path to the wordnet file.
        /// </summary>
        private string WordNetPath;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="offset">The offset of the class to lookup.</param>
        /// <param name="mapFilePath">The path to the training map.</param>
        /// <param name="wordnetFilePath">The path to the wordnet info file.</param>
        public ClassDetector(IEnumerable<int> offset, string mapFilePath, string wordnetFilePath = null)
        {
            CheckInputFile(mapFilePath, string.Format("The map file path {0} can't be located!", mapFilePath));

            Offsets = offset;
            TrainMapData = File.ReadAllText(mapFilePath);
            WordNetPath = wordnetFilePath;
        }

        /// <summary>
        /// Executes the detection.
        /// </summary>
        /// <returns>The class name in human-readable version.</returns>
        public IEnumerable<string> GetCLassifications()
        {
            return Offsets.Select((int offset) => MatchName(FindWordnetId(offset)));
        }

        /// <summary>
        /// Getter for bare wordnet IDs.
        /// </summary>
        /// <returns>The wordnet IDs.</returns>
        public IEnumerable<string> GetWordnetIds()
        {
            return Offsets.Select((int offset) => FindWordnetId(offset));
        }

        /// <summary>
        /// Parses the wordnet ID from the train_map.txt file.
        /// </summary>
        /// <param name="offset">Offset of the class to match.</param>
        /// <returns>Returns the wordnet ID converted to a string (needed in the WordNet RegEx to be available as string)</returns>
        private string FindWordnetId(int offset)
        {
            Regex trainMapRegex = new Regex(string.Format("train\\.zip\\@\\/n(?<id>(\\d+))\\/([\\w\\d]+)\\.(\\w+)([ ]+|[ \\t]+)(({0}))", Convert.ToString(offset)));

            return Convert.ToString(trainMapRegex.Match(TrainMapData).Groups["id"].Value);
        }

        /// <summary>
        /// Matches a WordNet name when the ID is given.
        /// </summary>
        /// <param name="wordnetId">The wordnet ID.</param>
        /// <returns>The actual wordnet name.</returns>
        private string MatchName(string wordnetId)
        {
            CheckInputFile(WordNetPath, string.Format("The wordnet file path {0} can't be located!", WordNetPath));
            if (string.IsNullOrEmpty(WordNetData))
            {
                WordNetData = File.ReadAllText(WordNetPath);
            }

            Regex wordnetRegex = new Regex(string.Format("({0}) ([0-9][1-9]|[1-9][0-9]) (\\w)([\\d ]+)(?<term>(\\w+))?( (\\d) (?<synonym>([A-z]+)))?", wordnetId));
            Match wordMatch = wordnetRegex.Match(WordNetData);

            string term = wordMatch.Groups["term"].Value;
            var synonym = wordMatch.Groups["synonym"].Value;

            if (!string.IsNullOrEmpty(synonym))
            {
                return string.Concat(term, " (", synonym, ")");
            }

            return term;
        }

        /// <summary>
        /// Checks an input file for its existance and throws an error if it can't be located.
        /// </summary>
        /// <param name="fileName">The filename to check.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        private void CheckInputFile(string fileName, string exceptionMessage)
        {
            if (!File.Exists(fileName))
            {
                throw new DetectorException(exceptionMessage);
            }
        }
    }
}
