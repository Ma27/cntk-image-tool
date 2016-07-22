using System;
using System.IO;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ImageProcessor.Imaging.Formats;

namespace ImageToRGBArray
{
    /// <summary>
    /// Custom exception class for the image parser.
    /// </summary>
    class ParsePixelException : Exception
    {
        /// <summary>
        /// Constructor.
        /// NOTE: this exception requires an exception message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ParsePixelException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// ImagePixelProcessor is a class which resizes an image and extracts the appropriate pixel format from the image.
    /// </summary>
    internal class ImagePixelProcessor
    {
        /// <summary>
        /// The path to the image.
        /// </summary>
        private readonly string ImagePath;

        /// <summary>
        /// Conditional flag whether to calculate a per-pixel mean for each channel or not.
        /// </summary>
        private readonly bool ComputePerPixelMean;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="imagePath">The path to the image to process.</param>
        public ImagePixelProcessor(string imagePath, bool computePerPixelMean = false)
        {
            if (!File.Exists(imagePath))
            {
                throw new ParsePixelException(string.Format("Unable to locate file with path '{0}'!", imagePath));
            }

            ImagePath = imagePath;
            ComputePerPixelMean = computePerPixelMean;
        }

        /// <summary>
        /// Extracts the image pixels as list.
        /// </summary>
        /// <returns>A float list containing the pixels as list.</returns>
        public List<float> GetPixelsAsList()
        {
            ImageFactory factory = new ImageFactory(false);
            factory.Load(ImagePath).Format(CreateImageFormat()).Resize(CreateResizeConfig());

            Bitmap bitmap = new Bitmap(factory.Image);
            List<float> result = new List<float>();

            int width = bitmap.Width;
            int height = bitmap.Height;

            for (int c = 0; c < 3; c++)
            {
                float mean = ComputePerPixelMean ? GetPerPixelMean(c, width, height, bitmap) : (float) 0;
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        if (ComputePerPixelMean)
                        {
                            result.Add(ExtractValueFromColor(c, bitmap.GetPixel(w, h)) - mean);
                            continue;
                        }

                        result.Add(ExtractValueFromColor(c, bitmap.GetPixel(w, h)));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builder which assembles the ResizeLayer object that creates the configuration for the ImageResizer.
        /// </summary>
        /// <returns>The assembled configuration object.</returns>
        private ResizeLayer CreateResizeConfig()
        {
            return new ResizeLayer(new Size(224, 224), ResizeMode.Crop, AnchorPosition.Center);
        }

        /// <summary>
        /// Extracts the appropriate channel information from the color object.
        /// </summary>
        /// <param name="c">Counter.</param>
        /// <param name="color">The color object.</param>
        /// <returns>The appropriate channel value (either red, green or blue).</returns>
        private float ExtractValueFromColor(int c, Color color)
        {
            return c == 0 ? color.R : c == 1 ? color.G : color.B;
        }

        /// <summary>
        /// Builder for the image format.
        /// </summary>
        /// <returns>The appropriate image format.</returns>
        private FormatBase CreateImageFormat()
        {
            Regex regex = new Regex("^(.*)\\.(.*)(?<extension>(gif|bmp|jp?(e)?g|png))$");
            Match match = regex.Match(ImagePath); // TODO error condition?

            switch (match.Groups["extension"].Value)
            {
                case "gif":
                    return new GifFormat();
                case "jpeg":
                case "jpg":
                    return new JpegFormat();
                case "png":
                    return new PngFormat();
                default:
                    return new BitmapFormat();
            }
        }

        /// <summary>
        /// Computes the per-pixel mean of a channel for the given image.
        /// </summary>
        /// <param name="channelPointer">The pointer which points to a certain color channel.</param>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="bitmap">The actual image.</param>
        /// <returns></returns>
        private float GetPerPixelMean(int channelPointer, int width, int height, Bitmap bitmap)
        {
            float counter = 0;
            float amount = 0;
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    float channelValue = ExtractValueFromColor(channelPointer, bitmap.GetPixel(w, h));
                    counter++;
                    amount += channelValue;
                }
            }

            return amount / counter;
        }
    }
}
