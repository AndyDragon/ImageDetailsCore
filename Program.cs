using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using MetadataExtractor;
using Newtonsoft.Json;

namespace ImageDetailsCore
{
    class MapValue
    {
        public string Source { get; set; }
        public string Display { get; set; }
    }

    class CameraMap
    {
        public IList<MapValue> Cameras { get; set; }
    }

    class LensMap
    {
        public IList<MapValue> Lenses { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().GetFiles()[0].Name);

            if (args.Count() < 1)
            {
                Console.WriteLine("Include a file name or multiple file names as parameters");
                return;
            }

            foreach (var arg in args)
            {
                if (System.IO.File.Exists(arg))
                {
                    try
                    {
                        OutputBadge(arg, location);
                        Console.WriteLine("Processed {0}...", System.IO.Path.GetFileName(arg));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Failed to process {0}: {1}", System.IO.Path.GetFileName(arg), exception.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Could not load file {0}...", arg);
                }
            }
        }

        private static void OutputBadge(string file, string assemblyLocation)
        {
            var cameraMap = JsonConvert.DeserializeObject<CameraMap>(System.IO.File.ReadAllText(System.IO.Path.Combine(assemblyLocation, @"Resources/CameraMap.json")));
            var lensMap = JsonConvert.DeserializeObject<LensMap>(System.IO.File.ReadAllText(System.IO.Path.Combine(assemblyLocation, @"Resources/LensMap.json")));

            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(file);
            var camera = GetStringValue(directories, "Exif IFD0", "Model") ?? GetStringValue(directories, "Exif SubIFD", "Model") ?? "unknown";
            var lens = GetStringValue(directories, "Exif SubIFD", "Lens Model") ?? "n/a";
            var focalLength = (GetStringValue(directories, "Exif SubIFD", "Focal Length") ?? GetStringValue(directories, "Exif SubIFD", "Focal Length 35") ?? "n/a").Replace(" mm", "mm");
            var ISO = GetStringValue(directories, "Exif SubIFD", "ISO Speed Ratings") ?? "n/a";
            var shutterSpeed = (GetStringValue(directories, "Exif SubIFD", "Exposure Time") ?? "n/a").Replace(" sec", "s");
            var aperture = (GetStringValue(directories, "Exif SubIFD", "F-Number") ?? "n/a").Replace("f/", "ƒ/");
            var exposureBias = GetStringValue(directories, "Exif SubIFD", "Exposure Bias Value") ?? "n/a";
            var exposureProgram = GetStringValue(directories, "Exif SubIFD", "Exposure Program") ?? "n/a";
            var whiteBalanceMode = GetStringValue(directories, "Exif SubIFD", "White Balance Mode") ?? "n/a";
            var whiteBalance = GetStringValue(directories, "Exif SubIFD", "White Balance") ?? "n/a";
            var dateTimeOriginal = GetStringValue(directories, "Exif SubIFD", "Date/Time Original") ?? "n/a";
            var artist = GetStringValue(directories, "Exif IFD0", "Artist") ?? GetStringValue(directories, "Exif SubIFD", "Artist") ?? "n/a";

            if (camera == "unknown")
            {
                Console.WriteLine("ERR: Could not find camera for {0}", System.IO.Path.GetFileName(file));
                return;
            }

            // Fix up camera
            var cameraMapping = cameraMap.Cameras.FirstOrDefault(map => map.Source == camera.Trim());
            if (cameraMapping != null)
            {
                camera = cameraMapping.Display;
            }
            else
            {
                Console.WriteLine("WARN: Could not map camera '{0}' in {1}", camera, System.IO.Path.GetFileName(file));
            }

            // Fix up lens
            var lensMapping = lensMap.Lenses.FirstOrDefault(map => map.Source == lens.Trim());
            if (lensMapping != null)
            {
                lens = lensMapping.Display;
            }
            else
            {
                Console.WriteLine("WARN: Could not map lens '{0}' in {1}", lens, System.IO.Path.GetFileName(file));
            }
            lens = lens.Replace("f/", "ƒ/").Replace("F/", "ƒ/").Replace("F_", "ƒ/");

            // Fix up shutter speed
            if (shutterSpeed.Contains("."))
            {
                var decimalValue = double.Parse(shutterSpeed.Substring(0, shutterSpeed.Length - 1));
                if (decimalValue < 1)
                {
                    shutterSpeed = string.Format("1/{0}s", 1 / decimalValue);
                }
            }

            // Fix up white balance
            if (whiteBalanceMode == "Auto white balance" && whiteBalance == "Unknown")
            {
                whiteBalance = whiteBalanceMode;
            }
            if (whiteBalance == "n/a")
            {
                whiteBalance = whiteBalanceMode;
            }

            using var bitmap = new Bitmap(480, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.Clear(Color.Transparent);

                // Draw the plaque
                DrawRoundedRect(graphics, 2, 2, 476, 296, Brushes.Black, 12, 12);
                DrawRoundedRect(graphics, 12, 12, 456, 276, Brushes.White, 9, 9);
                DrawRoundedRect(graphics, 15, 15, 450, 270, Brushes.Black, 8, 8);

                // Draw the camera
                DrawValue(graphics, 14, 32, 28, 33, System.IO.Path.Combine(assemblyLocation, @"Resources/camera.png"), "CAMERA", camera, FontStyle.Bold);

                // Draw the lens
                DrawValue(graphics, 11, 28, 28, 78, System.IO.Path.Combine(assemblyLocation, @"Resources/lens.png"), "LENS", lens);

                // Draw the focal length
                DrawValue(graphics, 11, 28, 28, 118, System.IO.Path.Combine(assemblyLocation, @"Resources/ruler.png"), "FOCAL LENGTH", focalLength);

                // Draw the ISO
                DrawValue(graphics, 11, 28, 240, 118, System.IO.Path.Combine(assemblyLocation, @"Resources/film.png"), "ISO", ISO);

                // Draw the exposure
                DrawValue(graphics, 11, 28, 28, 158, System.IO.Path.Combine(assemblyLocation, @"Resources/aperture.png"), "EXPOSURE", string.Format("{0} @ {1}", shutterSpeed, aperture));

                // Draw the exposure bias
                DrawValue(graphics, 11, 28, 240, 158, System.IO.Path.Combine(assemblyLocation, @"Resources/bias.png"), "EXPOSURE BIAS", exposureBias);

                // Draw the white balance
                DrawValue(graphics, 11, 28, 28, 198, System.IO.Path.Combine(assemblyLocation, @"Resources/whitebalance.png"), "WHITE BALANCE", whiteBalance);

                // Draw the exposure program
                DrawValue(graphics, 11, 28, 240, 198, System.IO.Path.Combine(assemblyLocation, @"Resources/exposure.png"), "EXPOSURE PROGRAM", exposureProgram);

                // Draw the date
                DrawValue(graphics, 11, 28, 28, 238, System.IO.Path.Combine(assemblyLocation, @"Resources/calendar.png"), "DATE", dateTimeOriginal);

                // Draw the time
                DrawValue(graphics, 11, 28, 240, 238, System.IO.Path.Combine(assemblyLocation, @"Resources/artist.png"), "ARTIST", artist);
            }
            var output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileNameWithoutExtension(file) + "_info.png");
            bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
        }

        private static string GetStringValue(IEnumerable<Directory> directories, string directoryName, string tagName)
        {
            var directory = directories.FirstOrDefault(directory => directory.Name == directoryName);
            if (directory != null)
            {
                var tag = directory.Tags.FirstOrDefault(tag => tag.Name == tagName);
                if (tag != null)
                {
                    return tag.Description;
                }
            }
            return null;
        }

        private static void DrawValue(Graphics graphics, int fontSize, int imageSize, int x, int y, string image, string label, string value, FontStyle fontStyle = FontStyle.Regular)
        {
            using (var resImage = new Bitmap(image))
            {
                graphics.DrawImage(resImage, x, y, imageSize, imageSize);
            }
            using (var font = new Font("Arial", 7, FontStyle.Bold))
            {
                graphics.DrawString(label, font, Brushes.Gray, x + 36, y);
            }
            using (var font = new Font("Candara", fontSize, fontStyle))
            {
                graphics.DrawString(value, font, Brushes.White, x + 36, y + 10);
            }
        }

        private static void DrawRoundedRect(Graphics graphics, int left, int top, int width, int height, Brush color, int radiusX, int radiusY)
        {
            int diameterX = 2 * radiusX;
            int diameterY = 2 * radiusY;
            graphics.FillRectangle(color, left + radiusX, top, width - diameterX, height);
            graphics.FillRectangle(color, left, top + radiusY, width, height - diameterY);
            graphics.FillEllipse(color, left, top, diameterX, diameterY);
            graphics.FillEllipse(color, left + width - diameterY, top, diameterX, diameterY);
            graphics.FillEllipse(color, left, top + height - diameterY, diameterX, diameterY);
            graphics.FillEllipse(color, left + width - diameterY, top + height - diameterY, diameterX, diameterY);
        }
    }
}
