/****************************************************************************

MIT License

Copyright (c) 2019 AndyDragon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

****************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace ImageDetailsCore
{
    class ThemeData
    {
        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; set; }
        public Brush CameraForegroundBrush { get; set; }
        public Brush LabelBrush { get; set; }
        public Brush BorderBrush { get; set; }
        public string ImageLocation { get; set; }
    }

    class Options
    {
        public bool WarnMissingCamera { get; set; }
        public bool WarnMissingLens { get; set; }
        public bool RecursiveFolders { get; set; }
        public string Theme { get; set; }
        public bool ThemeFromExt { get; set; }
        public IList<string> FolderSearchExtensions { get; set; }
        public bool UseScaling { get; set; }
        public string DefaultArtist { get; set; }
        public bool SkipDate { get; set; }
        public IList<MapValue> Cameras { get; set; }
        public IList<MapValue> Lenses { get; set; }
    }

    class MapValue
    {
        public string Source { get; set; }
        public string Serial { get; set; }
        public string Display { get; set; }
        public string Camera { get; set; }
    }

    class Program
    {
        static readonly Dictionary<string, ThemeData> themeData = new Dictionary<string, ThemeData>
        {
            {
                "black",
                new ThemeData
                {
                    BackgroundBrush = Brushes.Black,
                    ForegroundBrush = Brushes.White,
                    CameraForegroundBrush = Brushes.White,
                    LabelBrush = new SolidBrush(Color.FromArgb(128, 128, 128)),
                    BorderBrush = Brushes.White,
                    ImageLocation = "Resources/Themes/black",
                }
            },
            {
                "white",
                new ThemeData
                {
                    BackgroundBrush = Brushes.White,
                    ForegroundBrush = Brushes.Black,
                    LabelBrush = new SolidBrush(Color.FromArgb(128, 128, 128)),
                    BorderBrush = Brushes.Black,
                    ImageLocation = "Resources/Themes/white",
                }
            },
            {
                "dark",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    CameraForegroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    LabelBrush = new SolidBrush(Color.FromArgb(128, 128, 128)),
                    BorderBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "light",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    CameraForegroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    LabelBrush = new SolidBrush(Color.FromArgb(128, 128, 128)),
                    BorderBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    ImageLocation = "Resources/Themes/light",
                }
            },
            {
                "nikon",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    CameraForegroundBrush = new SolidBrush(Color.FromArgb(226, 226, 0)),
                    LabelBrush = new SolidBrush(Color.FromArgb(113, 113, 0)),
                    BorderBrush = new SolidBrush(Color.FromArgb(226, 226, 0)),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "fujifilm",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    CameraForegroundBrush = new SolidBrush(Color.FromArgb(144, 226, 144)),
                    LabelBrush = new SolidBrush(Color.FromArgb(96, 113, 96)),
                    BorderBrush = new SolidBrush(Color.FromArgb(144, 226, 144)),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "canon",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    CameraForegroundBrush = new SolidBrush(Color.White),
                    LabelBrush = new SolidBrush(Color.FromArgb(113, 6, 6)),
                    BorderBrush = new SolidBrush(Color.FromArgb(215, 11, 11)),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "olympus",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    CameraForegroundBrush = new SolidBrush(Color.Black),
                    LabelBrush = new SolidBrush(Color.FromArgb(3, 3, 64)),
                    BorderBrush = new SolidBrush(Color.FromArgb(6, 6, 113)),
                    ImageLocation = "Resources/Themes/light",
                }
            },
            {
                "omsystem",
                new ThemeData
                {
                    BackgroundBrush = new SolidBrush(Color.FromArgb(40, 40, 40)),
                    ForegroundBrush = new SolidBrush(Color.FromArgb(215, 215, 215)),
                    CameraForegroundBrush = new SolidBrush(Color.FromArgb(72, 144, 226)),
                    LabelBrush = new SolidBrush(Color.FromArgb(48, 96, 113)),
                    BorderBrush = new SolidBrush(Color.FromArgb(72, 144, 226)),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
        };

        static void Main(string[] args)
        {
            var location = Path.GetDirectoryName(Assembly.GetEntryAssembly().GetFiles()[0].Name);

            // Look for options in user profile folder first.
            var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var options = ReadOptions(".imagedetailscore_options.json", homeFolder);
            if (options == null)
            {
                // Fall-back to the default options.
                Console.WriteLine("Using default options, user options not found");
                options = ReadOptions("Options.json", location);
                if (options == null)
                {
                    Console.Error.WriteLine("Cannot find the Options JSON file in the application folder");
                }
            }

            if (args.Count() < 1)
            {
                Console.WriteLine("Include a file name or multiple file names as parameters");
                return;
            }

            // Enumerate the files / folders and output the badges.
            foreach (var arg in args)
            {
                if (File.Exists(arg))
                {
                    OutputBadge(arg, location, options);
                }
                else if (Directory.Exists(arg))
                {
                    OutputFolderBadges(arg, location, options);
                }
                else
                {
                    Console.WriteLine("Could not load file {0}...", arg);
                }
            }
        }

        private static Options ReadOptions(string optionsFileName, string location)
        {
            var optionsFile = Path.Combine(location, optionsFileName);
            if (!File.Exists(optionsFile))
            {
                return null;
            }

            var options = JsonConvert.DeserializeObject<Options>(File.ReadAllText(optionsFile));
            return options;
        }

        private static void OutputFolderBadges(string folder, string assemblyLocation, Options options)
        {
            var searchOptions = options.RecursiveFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var file in Directory.GetFiles(folder, "*.*", searchOptions))
            {
                if (options.FolderSearchExtensions.Any(extension => string.Equals(Path.GetExtension(file), extension, StringComparison.OrdinalIgnoreCase)))
                {
                    OutputBadge(file, assemblyLocation, options);
                }
            }
        }

        private static void OutputBadge(string file, string assemblyLocation, Options options)
        {
            Func<string, string, TagLocator> Locator = (directory, tag) => new TagLocator { Directory = directory, Tag = tag };

            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(file);
                // Change the following line to dump the EXIF data to the console.
                var dumpExif = false;
                if (dumpExif)
                {
                    foreach (var directory in directories)
                        foreach (var tag in directory.Tags)
                            Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                    return;
                }

                var camera = GetStringValue(directories, new[] {
                    Locator("Exif IFD0", "Model"),
                    Locator("Exif SubIFD", "Model"),
                }) ?? "unknown";

                var cameraSerial = GetStringValue(directories, new[] {
                    Locator("Exif IFD0", "Serial Number"),
                    Locator("Exif SubIFD", "Body Serial Number"),
                    Locator("Olympus Equipment", "Serial Number"),
                }) ?? "unknown";

                var lens = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Lens Model"),
                    Locator("Nikon Makernote", "Lens"),
                }) ?? "n/a";

                var lensSerial = GetStringValue(directories, new[] {
                    Locator("Exif IFD0", "Lens Serial Number"),
                    Locator("Exif SubIFD", "Lens Body Serial Number"),
                    Locator("Olympus Equipment", "Lens Serial Number"),
                }) ?? "unknown";

                var focalLength = (GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Focal Length"),
                    Locator("Exif SubIFD", "Focal Length 35"),
                }) ?? "n/a").Replace(" mm", "mm");

                var ISO = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "ISO Speed Ratings"),
                }) ?? "n/a";

                var shutterSpeed = (GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Exposure Time"),
                }) ?? "n/a").Replace(" sec", "s");

                var aperture = (GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "F-Number"),
                }) ?? "n/a").Replace("f/", "ƒ/");

                var exposureBias = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Exposure Bias Value"),
                }) ?? "n/a";

                var exposureProgram = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Exposure Program"),
                }) ?? "n/a";
                var exposureMode = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Exposure Mode"),
                }) ?? "n/a";

                var whiteBalanceMode = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "White Balance Mode"),
                }) ?? "n/a";

                var whiteBalance = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "White Balance"),
                }) ?? "n/a";

                var dateTimeOriginal = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Date/Time Original"),
                }) ?? "n/a";

                var artist = GetStringValue(directories, new[] {
                    Locator("Exif IFD0", "Artist"),
                    Locator("Exif SubIFD", "Artist"),
                }) ?? options.DefaultArtist ?? "n/a";
                if (artist.Length == 0) {
                    artist = options.DefaultArtist ?? "n/a";
                }

                if (camera == "unknown")
                {
                    Console.WriteLine("ERR: Could not find camera for {0}", Path.GetFileName(file));
                    return;
                }

                // Fix up camera
                var cameraMapping = options.Cameras.FirstOrDefault(map =>
                {
                    // Camera can be mapped using serial number.
                    if (!string.IsNullOrEmpty(map.Serial))
                    {
                        return map.Source.Trim() == camera.Trim() && map.Serial.Trim() == cameraSerial.Trim();
                    }
                    // Fallback for no serial number specified.
                    return map.Source.Trim() == camera.Trim();
                });
                if (cameraMapping != null)
                {
                    camera = cameraMapping.Display;
                }
                else
                {
                    if (options.WarnMissingCamera)
                    {
                        Console.WriteLine("WARN: Could not map camera '{0}' s/n '{1}' in {2}", camera, cameraSerial, Path.GetFileName(file));
                    }
                }

                // Fix up lens
                var lensMapping = options.Lenses.FirstOrDefault(map =>
                {
                    // Lens can be mapped using camera, serial number or both.
                    if (!string.IsNullOrEmpty(map.Serial) && !string.IsNullOrEmpty(map.Camera))
                    {
                        return map.Source.Trim() == camera.Trim() && map.Serial.Trim() == lensSerial.Trim() && map.Camera.Trim() == camera.Trim();
                    }
                    if (!string.IsNullOrEmpty(map.Camera))
                    {
                        return map.Source.Trim() == lens.Trim() && map.Camera.Trim() == camera.Trim();
                    }
                    if (!string.IsNullOrEmpty(map.Serial))
                    {
                        return map.Source.Trim() == camera.Trim() && map.Serial.Trim() == lensSerial.Trim();
                    }
                    // Fallback for no serial number or camera specified.
                    return map.Source.Trim() == lens.Trim();
                });
                if (lensMapping != null)
                {
                    lens = lensMapping.Display;
                }
                else
                {
                    if (options.WarnMissingCamera)
                    {
                        Console.WriteLine("WARN: Could not map lens '{0}' s/n '{1}' in {2}", lens, lensSerial, Path.GetFileName(file));
                    }
                }
                lens = lens.Replace("f/", "ƒ/").Replace("F/", "ƒ/").Replace("F_", "ƒ/");

                // Fix up shutter speed
                if (shutterSpeed.Contains("."))
                {
                    var decimalValue = double.Parse(shutterSpeed[0..^1]);
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
                if (whiteBalance.Length > 24)
                {
                    int position = whiteBalance.Substring(0, 24).LastIndexOf(' ');
                    if (position >= 0)
                    {
                        whiteBalance = whiteBalance.Substring(0, position);
                    }
                }

                // Fix up exposure prgm
                if (exposureProgram == "Unknown (0)")
                {
                    exposureProgram = exposureMode;
                }
                if (exposureProgram == "n/a")
                {
                    exposureProgram = exposureMode;
                }
                if (exposureProgram.Length > 24)
                {
                    int position = exposureProgram.Substring(0, 24).LastIndexOf(' ');
                    if (position >= 0)
                    {
                        exposureProgram = exposureProgram.Substring(0, position);
                    }
                }

                if (!themeData.ContainsKey(options.Theme))
                {
                    options.Theme = "black";
                }

                var theme = themeData[options.Theme];
                if (options.ThemeFromExt) {
                    if (System.IO.Path.GetExtension(file).ToLower() == ".nef")
                    {
                        theme = themeData["nikon"];
                    }
                    else if (System.IO.Path.GetExtension(file).ToLower() == ".orf")
                    {
                        if (camera.StartsWith("OM SYSTEM"))
                        {
                            theme = themeData["omsystem"];
                        }
                        else
                        {
                            theme = themeData["olympus"];
                        }
                    }
                    else if (System.IO.Path.GetExtension(file).ToLower() == ".raf")
                    {
                        theme = themeData["fujifilm"];
                    }
                    else if (System.IO.Path.GetExtension(file).ToLower() == ".cr2")
                    {
                        theme = themeData["canon"];
                    }
                }
                var imageLocation = Path.Combine(assemblyLocation, theme.ImageLocation);

                var drawingScale = options.UseScaling ? 4 : 1;

                using var bitmap = new Bitmap(480 * drawingScale, 300 * drawingScale, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.Clear(Color.Transparent);

                    // Draw the plaque
                    DrawRoundedRect(graphics, drawingScale, 2, 2, 476, 296, theme.BackgroundBrush, 13, 13);
                    DrawRoundedRect(graphics, drawingScale, 12, 12, 456, 276, theme.BorderBrush, 9, 9);
                    DrawRoundedRect(graphics, drawingScale, 15, 15, 450, 270, theme.BackgroundBrush, 8, 8);

                    // Draw the camera
                    DrawValue(graphics, theme, drawingScale, 14, 32, 28, 33, Path.Combine(imageLocation, @"camera.png"), "CAMERA", camera, FontStyle.Bold, theme.CameraForegroundBrush);

                    // Draw the lens
                    DrawValue(graphics, theme, drawingScale, 11, 28, 28, 78, Path.Combine(imageLocation, @"lens.png"), "LENS", lens);

                    // Draw the focal length
                    DrawValue(graphics, theme, drawingScale, 11, 28, 28, 118, Path.Combine(imageLocation, @"ruler.png"), "FOCAL LENGTH", focalLength);

                    // Draw the ISO
                    DrawValue(graphics, theme, drawingScale, 11, 28, 240, 118, Path.Combine(imageLocation, @"film.png"), "ISO", ISO);

                    // Draw the exposure
                    DrawValue(graphics, theme, drawingScale, 11, 28, 28, 158, Path.Combine(imageLocation, @"aperture.png"), "EXPOSURE", string.Format("{0} @ {1}", shutterSpeed, aperture));

                    // Draw the exposure bias
                    DrawValue(graphics, theme, drawingScale, 11, 28, 240, 158, Path.Combine(imageLocation, @"bias.png"), "EXPOSURE BIAS", exposureBias);

                    // Draw the white balance
                    DrawValue(graphics, theme, drawingScale, 11, 28, 28, 198, Path.Combine(imageLocation, @"whitebalance.png"), "WHITE BALANCE", whiteBalance);

                    // Draw the exposure program
                    DrawValue(graphics, theme, drawingScale, 11, 28, 240, 198, Path.Combine(imageLocation, @"exposure.png"), "EXPOSURE PROGRAM", exposureProgram);

                    if (options.SkipDate)
                    {
                        // Draw the date
                        DrawValue(graphics, theme, drawingScale, 11, 28, 28, 238, Path.Combine(imageLocation, @"artist.png"), "ARTIST", artist);
                    }
                    else
                    {
                        // Draw the date
                        DrawValue(graphics, theme, drawingScale, 11, 28, 28, 238, Path.Combine(imageLocation, @"calendar.png"), "DATE", dateTimeOriginal);

                        // Draw the time
                        DrawValue(graphics, theme, drawingScale, 11, 28, 240, 238, Path.Combine(imageLocation, @"artist.png"), "ARTIST", artist);
                    }
                }
                var output = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "_info.png");
                if (options.UseScaling)
                {
                    using var result = ResizeImage(bitmap, 480 * 2, 300 * 2);
                    result.Save(output, ImageFormat.Png);
                }
                else
                {
                    bitmap.Save(output, ImageFormat.Png);
                }

                Console.WriteLine("Processed {0}...", Path.GetFileName(file));
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to process {0}: {1}", Path.GetFileName(file), exception.Message);
            }
        }

        class TagLocator
        {
            public string Directory { get; set; }
            public string Tag { get; set; }
        }

        private static string GetStringValue(IEnumerable<MetadataExtractor.Directory> directories, IEnumerable<TagLocator> tagLocators)
        {
            foreach (var locator in tagLocators)
            {
                var locatedDirectories = directories.Where(directory => directory.Name == locator.Directory);
                foreach (var directory in locatedDirectories)
                {
                    if (directory != null)
                    {
                        var tag = directory.Tags.FirstOrDefault(tag => tag.Name == locator.Tag);
                        if (tag != null)
                        {
                            return tag.Description;
                        }
                    }
                }
            }
            return null;
        }

        private static void DrawValue(Graphics graphics, ThemeData theme, int scale, int fontSize, int imageSize, int x, int y, string image, string label, string value, FontStyle fontStyle = FontStyle.Regular, Brush valueBrush = null)
        {
            using (var resImage = new Bitmap(image))
            {
                graphics.DrawImage(resImage, x * scale, y * scale, imageSize * scale, imageSize * scale);
            }
            using (var font = new Font("Arial", 7 * scale, FontStyle.Bold))
            {
                graphics.DrawString(label, font, theme.LabelBrush, (x + 36) * scale, y * scale);
            }
            using (var font = new Font("Candara", fontSize * scale, fontStyle))
            {
                graphics.DrawString(value, font, valueBrush ?? theme.ForegroundBrush, (x + 36) * scale, (y + 10) * scale);
            }
        }

        private static void DrawRoundedRect(Graphics graphics, int scale, int left, int top, int width, int height, Brush brush, int radiusX, int radiusY)
        {
            radiusX *= scale;
            radiusY *= scale;
            int diameterX = 2 * radiusX;
            int diameterY = 2 * radiusY;
            left *= scale;
            top *= scale;
            width *= scale;
            height *= scale;

            graphics.FillRectangle(brush, left + radiusX, top, width - diameterX, height);
            graphics.FillRectangle(brush, left, top + radiusY, width, height - diameterY);
            graphics.FillEllipse(brush, left, top, diameterX, diameterY);
            graphics.FillEllipse(brush, left + width - diameterY, top, diameterX, diameterY);
            graphics.FillEllipse(brush, left, top + height - diameterY, diameterX, diameterY);
            graphics.FillEllipse(brush, left + width - diameterY, top + height - diameterY, diameterX, diameterY);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.Clear(Color.Transparent);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }
    }
}
