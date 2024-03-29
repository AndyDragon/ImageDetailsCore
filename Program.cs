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
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using MetadataExtractor;

namespace ImageDetailsCore
{
    class ThemeData
    {
        public Color BackgrounColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color CameraForegroundColor { get; set; }
        public Color LabelColor { get; set; }
        public Color BorderColor { get; set; }
        public string ImageLocation { get; set; }
    }

    class Options
    {
        public bool WarnMissingCamera { get; set; }
        public bool WarnMissingLens { get; set; }
        public bool WarnMissingValues { get; set; }
        public bool RecursiveFolders { get; set; }
        public string Theme { get; set; }
        public bool ThemeFromExt { get; set; }
        public IList<string> FolderSearchExtensions { get; set; }
        public bool UseScaling { get; set; }
        public string DefaultArtist { get; set; }
        public bool SkipDate { get; set; }
        public bool SkipOutput { get; set; }
        public IList<MapValue> Cameras { get; set; }
        public IList<MapValue> Lenses { get; set; }
        public bool DumpMetadata { get; set; }
        public string Output { get; set; }
        public int Start { get; set; }
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
        static readonly Dictionary<string, ThemeData> themeData = new()
        {
            {
                "black",
                new ThemeData
                {
                    BackgrounColor = Color.Black,
                    ForegroundColor = Color.White,
                    CameraForegroundColor = Color.White,
                    LabelColor = Color.FromRgb(128, 128, 128),
                    BorderColor = Color.White,
                    ImageLocation = "Resources/Themes/black",
                }
            },
            {
                "white",
                new ThemeData
                {
                    BackgrounColor = Color.White,
                    ForegroundColor = Color.Black,
                    LabelColor = Color.FromRgb(128, 128, 128),
                    BorderColor = Color.Black,
                    ImageLocation = "Resources/Themes/white",
                }
            },
            {
                "dark",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(40, 40, 40),
                    ForegroundColor = Color.FromRgb(215, 215, 215),
                    CameraForegroundColor = Color.FromRgb(215, 215, 215),
                    LabelColor = Color.FromRgb(128, 128, 128),
                    BorderColor = Color.FromRgb(215, 215, 215),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "light",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(215, 215, 215),
                    ForegroundColor = Color.FromRgb(40, 40, 40),
                    CameraForegroundColor = Color.FromRgb(40, 40, 40),
                    LabelColor = Color.FromRgb(128, 128, 128),
                    BorderColor = Color.FromRgb(40, 40, 40),
                    ImageLocation = "Resources/Themes/light",
                }
            },
            {
                "nikon",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(40, 40, 40),
                    ForegroundColor = Color.FromRgb(215, 215, 215),
                    CameraForegroundColor = Color.FromRgb(226, 226, 0),
                    LabelColor = Color.FromRgb(113, 113, 0),
                    BorderColor = Color.FromRgb(226, 226, 0),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "fujifilm",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(40, 40, 40),
                    ForegroundColor = Color.FromRgb(215, 215, 215),
                    CameraForegroundColor = Color.FromRgb(144, 226, 144),
                    LabelColor = Color.FromRgb(96, 113, 96),
                    BorderColor = Color.FromRgb(144, 226, 144),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "canon",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(40, 40, 40),
                    ForegroundColor = Color.FromRgb(215, 215, 215),
                    CameraForegroundColor = Color.White,
                    LabelColor = Color.FromRgb(113, 6, 6),
                    BorderColor = Color.FromRgb(215, 11, 11),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
            {
                "olympus",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(215, 215, 215),
                    ForegroundColor = Color.FromRgb(40, 40, 40),
                    CameraForegroundColor = Color.Black,
                    LabelColor = Color.FromRgb(3, 3, 64),
                    BorderColor = Color.FromRgb(6, 6, 113),
                    ImageLocation = "Resources/Themes/light",
                }
            },
            {
                "omsystem",
                new ThemeData
                {
                    BackgrounColor = Color.FromRgb(40, 40, 40),
                    ForegroundColor = Color.FromRgb(215, 215, 215),
                    CameraForegroundColor = Color.FromRgb(72, 144, 226),
                    LabelColor = Color.FromRgb(48, 96, 113),
                    BorderColor = Color.FromRgb(72, 144, 226),
                    ImageLocation = "Resources/Themes/dark",
                }
            },
        };

        static void Main(string[] args)
        {
            // Look for options in user profile folder first.
            var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var options = ReadOptions(".imagedetailscore_options.json", homeFolder);
            if (options == null)
            {
                // Fall-back to the default options.
                Console.WriteLine("Using default options, user options not found");
                options = ReadDefaultOptions();
                if (options == null)
                {
                    Console.Error.WriteLine("Cannot find the Options JSON file in the application folder");
                }
            }

            if (args.Length < 1)
            {
                Console.WriteLine("Include a file name or multiple file names as parameters");
                return;
            }

            // Enumerate the files / folders and output the badges.
            var waitingForOutput = false;
            var waitingForInclude = false;
            var waitingForStart = false;
            options.Start = 0;
            foreach (var arg in args)
            {
                if (arg == "-dump")
                {
                    options.DumpMetadata = true;
                    Console.WriteLine("Turned on dumping metadata");
                }
                else if (arg == "-warn")
                {
                    options.WarnMissingValues = true;
                    Console.WriteLine("Turned on warning for missing values");
                }
                else if (arg == "-deep")
                {
                    options.RecursiveFolders = true;
                    Console.WriteLine("Turned on recursive folders");
                }
                else if (arg == "-skipout")
                {
                    options.SkipOutput = true;
                    Console.WriteLine("Skipping badge output");
                }
                else if (arg == "-start")
                {
                    waitingForStart = true;
                }
                else if (waitingForStart)
                {
                    waitingForStart = false;
                    options.Start = int.Parse(arg);
                    Console.WriteLine("Setting start to: {0}", options.Start);
                }
                else if (arg == "-include")
                {
                    waitingForInclude = true;
                }
                else if (waitingForInclude)
                {
                    waitingForInclude = false;
                    options.FolderSearchExtensions.Add(arg);
                    Console.WriteLine("Added *{0} to search pattern: {1}", arg, string.Join(", ", options.FolderSearchExtensions.Select(ext => "*" + ext)));
                }
                else if (arg == "-out")
                {
                    waitingForOutput = true;
                }
                else if (waitingForOutput)
                {
                    waitingForOutput = false;
                    options.Output = arg;
                    if (!System.IO.Directory.Exists(options.Output))
                    {
                        System.IO.Directory.CreateDirectory(options.Output);
                    }
                    Console.WriteLine("Set output location to: {0}", options.Output);
                }
                else if (waitingForInclude)
                {
                    waitingForInclude = false;
                    options.FolderSearchExtensions.Add(arg);
                    Console.WriteLine("Added *{0} to search pattern: {1}", arg, string.Join(", ", options.FolderSearchExtensions.Select(ext => "*" + ext)));
                }
                else if (File.Exists(arg))
                {
                    OutputBadge(arg, options);
                }
                else if (System.IO.Directory.Exists(arg))
                {
                    OutputFolderBadges(arg, options);
                }
                else
                {
                    Console.WriteLine("Could not load file {0}...", arg);
                }
            }

            if (options.SkipOutput)
            {
                Console.WriteLine();
            }
        }

        private static Options ReadDefaultOptions()
        {
            string optionsContents;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ImageDetailsCore.Options.json"))
            using (var streamReader = new StreamReader(stream))
            {
                optionsContents = streamReader.ReadToEnd();
            }
            var options = JsonConvert.DeserializeObject<Options>(optionsContents);
            return options;
        }

        private static Options ReadOptions(string optionsFileName, string location)
        {
            var optionsFile = System.IO.Path.Combine(location, optionsFileName);
            if (!File.Exists(optionsFile))
            {
                return null;
            }

            var options = JsonConvert.DeserializeObject<Options>(File.ReadAllText(optionsFile));
            return options;
        }

        private static void OutputFolderBadges(string folder, Options options)
        {
            var searchOptions = options.RecursiveFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var file in System.IO.Directory.GetFiles(folder, "*.*", searchOptions).Order())
            {
                if (options.FolderSearchExtensions.Any(extension => string.Equals(System.IO.Path.GetExtension(file), extension, StringComparison.OrdinalIgnoreCase)))
                {
                    // Skip any images before the start index.
                    if (options.Start > 0)
                    {
                        try
                        {
                            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(file);
                            var fileNameIndex = fileNameWithoutExt[^4..];
                            var index = int.Parse(fileNameIndex);
                            if (index < options.Start)
                            {
                                continue;
                            }
                        }
                        catch
                        {
                            // Ignore errors
                        }
                    }
                    OutputBadge(file, options);
                }
            }
        }

        private static void OutputBadge(string file, Options options)
        {
            static TagLocator Locator(string directory, string tag) => new() { Directory = directory, Tag = tag };
            static RawTagLocator RawLocator(string directory, Int32 tag) => new() { Directory = directory, Tag = tag };

            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
                if (options.DumpMetadata)
                {
                    foreach (var directory in directories)
                    {
                        foreach (var tag in directory.Tags)
                        {
                            Console.WriteLine($"{directory.Name} - {tag.Name} (0x{tag.Type:x4}) = {tag.Description}");
                        }
                    }
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
                    Locator("Exif SubIFD", "Lens Specification"),
                    Locator("Olympus Equipment", "Lens Type"),
                    Locator("Olympus Equipment", "Lens Model"),
                }) ?? "n/a";

                var lensSerial = GetStringValue(directories, new[] {
                    Locator("Exif IFD0", "Lens Serial Number"),
                    Locator("Exif SubIFD", "Lens Body Serial Number"),
                    Locator("Olympus Equipment", "Lens Serial Number"),
                }) ?? "unknown";

                var focalLength = (GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Focal Length"),
                }) ?? "n/a").Replace(" mm", "mm");

                var focalLength35 = (GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Focal Length 35"),
                }) ?? "n/a").Replace(" mm", "mm");

                var iso = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "ISO Speed Ratings"),
                }) ?? "n/a";
                var nikonIsoInfo = GetStringValue(directories, new[] {
                    Locator("Nikon Makernote", "ISO Info"),
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

                var afMode = GetStringValue(directories, new[] {
                    Locator("Nikon Makernote", "AF Type"),
                    Locator("Fujifilm Makernote", "Focus Mode"),
                    Locator("Olympus Camera Settings", "Focus Mode"),
                }) ?? "n/a";

                var shootingMode = GetStringValue(directories, new[] {
                    Locator("Nikon Makernote", "Shooting Mode"),
                    Locator("Olympus Camera Settings", "Drive Mode"),
                }) ?? "n/a";
                if (shootingMode == "n/a" && camera == "GFX100S")
                {
                    var driveMode = GetInt32Value(directories, new[] {
                        RawLocator("Fujifilm Makernote", 0x1103),
                    });
                    switch (driveMode & 0x00ff)
                    {
                        case 0:
                            shootingMode = "Single";
                            break;
                        case 1:
                            shootingMode = string.Format("Continuous Low ({0}fps)", (driveMode >> 24) & 0x00ff);
                            break;
                        case 2:
                            shootingMode = string.Format("Continuous High ({0}fps)", (driveMode >> 24) & 0x00ff);
                            break;
                    }
                    var bracketing = GetStringValue(directories, new[]
                    {
                        Locator("Fujifilm Makernote", "Auto Bracketing"),
                    }) ?? "n/a";
                    if (bracketing != "n/a" && bracketing != "Off")
                    {
                        if (shootingMode == "n/a")
                        {
                            shootingMode = "Auto Bracketing";
                        }
                        else
                        {
                            shootingMode += ", Auto Bracketing";
                        }
                    }
                }

                var dateTimeOriginal = GetStringValue(directories, new[] {
                    Locator("Exif SubIFD", "Date/Time Original"),
                }) ?? "n/a";

                var artist = GetStringValue(directories, new[] {
                    Locator("Exif IFD0", "Artist"),
                    Locator("Exif SubIFD", "Artist"),
                }) ?? options.DefaultArtist ?? "n/a";
                if (artist.Length == 0)
                {
                    artist = options.DefaultArtist ?? "n/a";
                }

                if (camera == "unknown")
                {
                    if (options.SkipOutput)
                    {
                        Console.WriteLine();
                    }
                    Console.WriteLine("ERR: Could not find camera for {0}", System.IO.Path.GetFileName(file));
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
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        if (camera == "None")
                        {
                            camera = "n/a";
                        }
                        Console.WriteLine("WARN: Could not map camera '{0}' s/n '{1}' in {2}", camera, cameraSerial, System.IO.Path.GetFileName(file));
                        if (camera.Length == 0)
                        {
                            camera = "n/a";
                        }
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
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        if (lens == "None")
                        {
                            lens = "n/a";
                        }
                        if (lens == "n/a")
                        {
                            Console.WriteLine("WARN: Could not find lens in {0}", System.IO.Path.GetFileName(file));
                        }
                        else
                        {
                            Console.WriteLine("WARN: Could not map lens '{0}' s/n '{1}' in {2}", lens, lensSerial, System.IO.Path.GetFileName(file));
                        }
                        if (lens.Length == 0)
                        {
                            lens = "n/a";
                        }
                    }
                }
                lens = lens.Replace("f/", "ƒ/").Replace("F/", "ƒ/").Replace("F_", "ƒ/");

                // Fix up shutter speed
                if (shutterSpeed.Contains('.'))
                {
                    var decimalValue = double.Parse(shutterSpeed[0..^1]);
                    if (decimalValue < 1)
                    {
                        shutterSpeed = string.Format("1/{0}s", 1 / decimalValue);
                    }
                }

                // Fix up aperature
                if (aperture == "ƒ/0" || aperture == "ƒ/0.0")
                {
                    aperture = "n/a";
                }

                // Fix up focal length
                if (focalLength == "Unknown")
                {
                    focalLength = "n/a";
                }
                if (focalLength35 == "Unknown")
                {
                    focalLength35 = "n/a";
                }
                if (focalLength35 == "n/a" && camera == "OM SYSTEM OM-1")
                {
                    focalLength35 = (float.Parse(focalLength[..^2]) * 2).ToString(".#") + "mm";
                }
                if (focalLength != "n/a" && focalLength35 != "n/a")
                {
                    var focalLengthValue = float.Parse(focalLength[..^2]);
                    var focalLength35Value = float.Parse(focalLength35[..^2]);
                    // If the camera specified a focal length within a small margin either way, consider them
                    // the same to avoid silly display of same value.
                    if (focalLength35Value > focalLengthValue - 0.95 && focalLength35Value < focalLengthValue + 0.95)
                    {
                        focalLength35 = focalLength;
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
                    int position = whiteBalance[..24].LastIndexOf(' ');
                    if (position >= 0)
                    {
                        whiteBalance = whiteBalance[..position];
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
                    int position = exposureProgram[..24].LastIndexOf(' ');
                    if (position >= 0)
                    {
                        exposureProgram = exposureProgram[..position];
                    }
                }

                // Fix up exposure bias
                if (exposureBias.Contains("/-"))
                {
                    exposureBias = "-" + exposureBias.Replace("/-", "/");
                }
                if (exposureBias.Contains("/100 EV"))
                {
                    var parts = exposureBias[..^3].Split("/");
                    if (parts.Length == 2)
                    {
                        try
                        {
                            var value = float.Parse(parts[0]) / float.Parse(parts[1]);
                            exposureBias = value.ToString("0.## EV");
                        }
                        catch
                        {
                            // Ignore errors
                        }
                    }
                }
                var exposureBiasReplacements = new Dictionary<string, string>
                {
                    { "0.33 EV", "1/3 EV" },
                    { "-0.33 EV", "-1/3 EV" },
                    { "0.67 EV", "2/3 EV" },
                    { "-0.67 EV", "-2/3 EV" },
                    { "1.33 EV", "1 1/3 EV" },
                    { "-1.33 EV", "-1 1/3 EV" },
                    { "1.67 EV", "1 2/3 EV" },
                    { "-1.67 EV", "-1 2/3 EV" },
                    { "2.33 EV", "2 1/3 EV" },
                    { "-2.33 EV", "-2 1/3 EV" },
                    { "2.67 EV", "2 2/3 EV" },
                    { "-2.67 EV", "-2 2/3 EV" },
                    { "3.33 EV", "2 1/3 EV" },
                    { "-3.33 EV", "-2 1/3 EV" },
                    { "3.67 EV", "2 2/3 EV" },
                    { "-3.67 EV", "-2 2/3 EV" },
                };
                foreach (var key in exposureBiasReplacements.Keys)
                {
                    if (exposureBias == key)
                    {
                        exposureBias = exposureBiasReplacements[key];
                        break;
                    }
                }

                // Fix up ISO
                if (iso == "n/a" && nikonIsoInfo != "n/a")
                {
                    var parts = nikonIsoInfo.Split(' ', StringSplitOptions.TrimEntries);
                    if (parts.Length == 14)
                    {
                        var code2 = int.Parse(parts[4]) * 256 + int.Parse(parts[5]);
                        //Console.WriteLine("Code 2: {0}", code2);
                        var isoCode2s = new Dictionary<int, string>
                        {
                            { 0x0, "Off" },
                            { 0x101, "Hi 0.3" },
                            { 0x102, "Hi 0.5" },
                            { 0x103, "Hi 0.7" },
                            { 0x104, "Hi 1.0" },
                            { 0x105, "Hi 1.3" },
                            { 0x106, "Hi 1.5" },
                            { 0x107, "Hi 1.7" },
                            { 0x108, "Hi 2.0" },
                            { 0x201, "Lo 0.3" },
                            { 0x202, "Lo 0.5" },
                            { 0x203, "Lo 0.7" },
                            { 0x204, "Lo 1.0" },
                        };
                        if (isoCode2s.ContainsKey(code2))
                        {
                            iso = isoCode2s[code2];
                        }
                        else
                        {
                            var code = int.Parse(parts[1]) * 256 + int.Parse(parts[2]);
                            var isoCodes = new Dictionary<int, string>
                            {
                                { 0x0, "Off" },
                                { 0x101, "Hi 0.3" },
                                { 0x102, "Hi 0.5" },
                                { 0x103, "Hi 0.7" },
                                { 0x104, "Hi 1.0" },
                                { 0x105, "Hi 1.3" },
                                { 0x106, "Hi 1.5" },
                                { 0x107, "Hi 1.7" },
                                { 0x108, "Hi 2.0" },
                                { 0x109, "Hi 2.3" },
                                { 0x10a, "Hi 2.5" },
                                { 0x10b, "Hi 2.7" },
                                { 0x10c, "Hi 3.0" },
                                { 0x10d, "Hi 3.3" },
                                { 0x10e, "Hi 3.5" },
                                { 0x10f, "Hi 3.7" },
                                { 0x110, "Hi 4.0" },
                                { 0x111, "Hi 4.3" },
                                { 0x112, "Hi 4.5" },
                                { 0x113, "Hi 4.7" },
                                { 0x114, "Hi 5.0" },
                                { 0x201, "Lo 0.3" },
                                { 0x202, "Lo 0.5" },
                                { 0x203, "Lo 0.7" },
                                { 0x204, "Lo 1.0" },
                            };
                            if (isoCodes.ContainsKey(code))
                            {
                                iso = isoCodes[code];
                            }
                        }
                    }
                }

                // Fix up shooting mode
                shootingMode = RemoveStringTerms(shootingMode, new List<string>()
                {
                    "pc control",
                    ">shot ",
                });

                if (options.WarnMissingValues)
                {
                    if (focalLength == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'focal length' in {0}", System.IO.Path.GetFileName(file));
                    }
                    if (iso == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'iso' in {0}", System.IO.Path.GetFileName(file));
                    }
                    if (shutterSpeed == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'shutter speed' in {0}", System.IO.Path.GetFileName(file));
                    }
                    if (aperture == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'aperture' in {0}", System.IO.Path.GetFileName(file));
                    }
                    if (exposureBias == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'exposure bias' in {0}", System.IO.Path.GetFileName(file));
                    }
                    if (whiteBalance == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'white balance' in {0}", System.IO.Path.GetFileName(file));
                    }
                    if (exposureProgram == "n/a")
                    {
                        if (options.SkipOutput)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine("WARN: missing 'exposure program' in {0}", System.IO.Path.GetFileName(file));
                    }
                }

                if (options.SkipOutput)
                {
                    Console.Write(".");
                    return;
                }

                if (!themeData.ContainsKey(options.Theme))
                {
                    options.Theme = "black";
                }

                var theme = themeData[options.Theme];
                if (options.ThemeFromExt)
                {
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
                    else if (System.IO.Path.GetExtension(file).ToLower() == ".jpg"
                        || System.IO.Path.GetExtension(file).ToLower() == ".jpeg"
                        || System.IO.Path.GetExtension(file).ToLower() == ".dng"
                        || System.IO.Path.GetExtension(file).ToLower() == ".heic")
                    {
                        if (camera.StartsWith("OLYMPUS"))
                        {
                            theme = themeData["olympus"];
                        }
                        else if (camera.StartsWith("OM SYSTEM"))
                        {
                            theme = themeData["omsystem"];
                        }
                        else if (camera.StartsWith("NIKON"))
                        {
                            theme = themeData["nikon"];
                        }
                        else if (camera.StartsWith("CANON"))
                        {
                            theme = themeData["canon"];
                        }
                        else if (camera.StartsWith("FUJI"))
                        {
                            theme = themeData["fujifilm"];
                        }
                        else if (camera.StartsWith("APPLE"))
                        {
                            theme = themeData["light"];
                        }
                        else
                        {
                            theme = themeData["black"];
                        }
                    }
                }

                var imageLocation = "ImageDetailsCore." + theme.ImageLocation.Replace("/", ".");

                var drawingScale = options.UseScaling ? 4 : 1;

                using (var image = new Image<Rgba32>(480 * drawingScale, 300 * drawingScale))
                {
                    image.Mutate(imageContext =>
                    {
                        // Draw the plaque.
                        imageContext = DrawRoundedRect(imageContext, drawingScale, image, 2, 13, theme.BackgrounColor);
                        imageContext = DrawRoundedRect(imageContext, drawingScale, image, 12, 9, theme.BorderColor);
                        imageContext = DrawRoundedRect(imageContext, drawingScale, image, 15, 7, theme.BackgrounColor);

                        // Draw the camera.
                        imageContext = DrawValue(imageContext, drawingScale, 14, 32, 28, 33, imageLocation + ".camera.png", "CAMERA", camera, FontStyle.Bold, theme.CameraForegroundColor, theme.LabelColor);

                        // Draw the lens
                        imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 73, imageLocation + ".lens.png", "LENS", lens, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                        // Draw the focal length
                        if (focalLength35 != "n/a" && focalLength35 != focalLength)
                        {
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 106, imageLocation + ".ruler.png", "FOCAL LENGTH", string.Format("{0} ({1} @ 35mm)", focalLength, focalLength35), FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }
                        else
                        {
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 106, imageLocation + ".ruler.png", "FOCAL LENGTH", focalLength, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }

                        // Draw the ISO
                        imageContext = DrawValue(imageContext, drawingScale, 11, 28, 220, 106, imageLocation + ".film.png", "ISO", iso, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                        // Draw the exposure
                        if (aperture == "n/a")
                        {
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 139, imageLocation + ".aperture.png", "EXPOSURE", string.Format("{0}", shutterSpeed), FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }
                        else
                        {
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 139, imageLocation + ".aperture.png", "EXPOSURE", string.Format("{0} @ {1}", shutterSpeed, aperture), FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }

                        // Draw the exposure bias
                        imageContext = DrawValue(imageContext, drawingScale, 11, 28, 220, 139, imageLocation + ".bias.png", "EXPOSURE BIAS", exposureBias, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                        // Draw the white balance
                        imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 172, imageLocation + ".whitebalance.png", "WHITE BALANCE", whiteBalance, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                        // Draw the exposure program
                        imageContext = DrawValue(imageContext, drawingScale, 11, 28, 220, 172, imageLocation + ".exposure.png", "EXPOSURE PROGRAM", exposureProgram, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                        // Draw the AF mode
                        imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 205, imageLocation + ".focus.png", "FOCUS MODE", afMode, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                        if (shootingMode != "n/a")
                        {
                            // Draw the Shooting mode
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 220, 205, imageLocation + ".shutter.png", "SHOOTING MODE", shootingMode, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }

                        if (options.SkipDate)
                        {
                            // Draw the date
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 238, imageLocation + ".artist.png", "ARTIST", artist, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }
                        else
                        {
                            // Draw the date
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 28, 238, imageLocation + ".calendar.png", "DATE", dateTimeOriginal, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);

                            // Draw the time
                            imageContext = DrawValue(imageContext, drawingScale, 11, 28, 220, 238, imageLocation + ".artist.png", "ARTIST", artist, FontStyle.Regular, theme.ForegroundColor, theme.LabelColor);
                        }
                    });
                    var output = System.IO.Path.Combine(options.Output ?? System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileNameWithoutExtension(file) + "_info.png");
                    var layer = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileNameWithoutExtension(file) + ".png");
                    if (File.Exists(layer))
                    {
                        using var layerImage = Image.Load(layer);
                        using var resultImage = new Image<Rgba32>(500 * drawingScale, 320 * drawingScale);
                        resultImage.Mutate(imageContext =>
                        {
                            imageContext = imageContext.Fill(theme.BackgrounColor);

                            var scaleX = (float)resultImage.Width / layerImage.Width;
                            var scaleY = (float)resultImage.Height / layerImage.Height;
                            var resultScale = Math.Max(scaleX, scaleY);
                            var newLayerImageWidth = layerImage.Width * resultScale;
                            var left = (resultImage.Width - newLayerImageWidth) / 2;
                            var newLayerImageHeight = layerImage.Height * resultScale;
                            var top = (resultImage.Height - newLayerImageHeight) / 2;
                            layerImage.Mutate(layerImageContext =>
                            {
                                layerImageContext = layerImageContext.Resize((int)(newLayerImageWidth * 1.1), (int)(newLayerImageHeight * 1.1));
                            });
                            imageContext = imageContext.DrawImage(
                                layerImage,
                                new Point(0, 0),
                                new Rectangle((layerImage.Width - resultImage.Width) / 2, (layerImage.Height - resultImage.Height) / 2, resultImage.Width, resultImage.Height),
                                1f);

                            imageContext = imageContext.GaussianBlur(30);

                            imageContext = imageContext.DrawImage(image, new Point(10 * drawingScale, 10 * drawingScale), 0.85f);
                        });
                        resultImage.SaveAsPng(output);
                    }
                    else
                    {
                        image.SaveAsPng(output);
                    }
                };

                Console.WriteLine("Processed {0}...", System.IO.Path.GetFileName(file));
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to process {0}: {1}", System.IO.Path.GetFileName(file), exception.Message);
            }
        }

        private static string RemoveStringTerms(string stringValue, IEnumerable<string> terms)
        {
            var stringTerms = stringValue.Split(',', StringSplitOptions.TrimEntries);
            foreach (var term in terms)
            {
                if (term[0] == '>')
                {
                    stringTerms = stringTerms.Where(part => !part.ToLower().StartsWith(term[1..])).ToArray<string>();
                }
                else if (term[0] == '<')
                {
                    stringTerms = stringTerms.Where(part => !part.ToLower().EndsWith(term[1..])).ToArray<string>();
                }
                else
                {
                    stringTerms = stringTerms.Where(part => !part.ToLower().Equals(term)).ToArray<string>();
                }
            }
            stringValue = string.Join(", ", stringTerms);
            if (string.IsNullOrEmpty(stringValue))
            {
                stringValue = "n/a";
            }
            return stringValue;
        }

        private static IImageProcessingContext DrawValue(
            IImageProcessingContext imageContext,
            int drawingScale,
            int fontSize,
            int imageSize,
            int x,
            int y,
            string image,
            string label,
            string value,
            FontStyle fontStyle,
            Color color,
            Color labelColor)
        {
            float scale(float value) => drawingScale * value;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(image))
            using (var valueImage = Image.Load(stream))
            {
                valueImage.Mutate(x => x.Resize((int)scale(imageSize), (int)scale(imageSize)));
                imageContext = imageContext.DrawImage(valueImage, new Point((int)scale(x), (int)scale(y)), 0.8f);
            }

            var labelTextOptions = new RichTextOptions(SystemFonts.CreateFont("Arial", scale(7), FontStyle.Bold))
            {
                Origin = new PointF(scale(x + 36), scale(y)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            imageContext = imageContext.DrawText(labelTextOptions, label, labelColor);

            var valueTextOptions = new RichTextOptions(SystemFonts.CreateFont("Candara", scale(fontSize), fontStyle))
            {
                Origin = new PointF(scale(x + 36), scale(y + 10)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            imageContext = imageContext.DrawText(valueTextOptions, value, color);

            return imageContext;
        }

        private static IImageProcessingContext DrawRoundedRect(
            IImageProcessingContext imageContext,
            int drawingScale,
            Image<Rgba32> image,
            int offset,
            int cornerRadius,
            Color color)
        {
            float scale(float value) => drawingScale * value;
            foreach (IPath path in BuildCorners(image.Width - (int)scale(offset * 2), image.Height - (int)scale(offset * 2), scale(cornerRadius)))
            {
                imageContext = imageContext.Fill(color, path.Translate(scale(offset), scale(offset)));
            }
            return imageContext;
        }

        class TagLocator
        {
            public string Directory { get; set; }
            public string Tag { get; set; }
        }

        class RawTagLocator
        {
            public string Directory { get; set; }
            public Int32 Tag { get; set; }
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
                        if (tag != null && !string.IsNullOrEmpty(tag.Description))
                        {
                            return tag.Description;
                        }
                    }
                }
            }
            return null;
        }

        private static int GetInt32Value(IEnumerable<MetadataExtractor.Directory> directories, IEnumerable<RawTagLocator> tagLocators)
        {
            foreach (var locator in tagLocators)
            {
                var locatedDirectories = directories.Where(directory => directory.Name == locator.Directory);
                foreach (var directory in locatedDirectories)
                {
                    if (directory != null)
                    {
                        try
                        {
                            return directory.GetInt32(locator.Tag);
                        }
                        catch
                        {
                            return int.MaxValue;
                        }
                    }
                }
            }
            return int.MaxValue;
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rectHoriz = new RectangularPolygon(-0.5f, cornerRadius - 0.5f, imageWidth, imageHeight - cornerRadius * 2);
            var rectVert = new RectangularPolygon(cornerRadius - 0.5f, -0.5f, imageWidth - cornerRadius * 2, imageHeight);
            var cornerTopLeft = new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius);
            var rightPos = imageWidth - cornerTopLeft.Bounds.Width;
            var bottomPos = imageHeight - cornerTopLeft.Bounds.Height;
            var cornerTopRight = cornerTopLeft.Translate(rightPos, 0);
            var cornerBottomLeft = cornerTopLeft.Translate(0, bottomPos);
            var cornerBottomRight = cornerTopLeft.Translate(rightPos, bottomPos);
            return new PathCollection(rectHoriz, rectVert, cornerTopLeft, cornerTopRight, cornerBottomLeft, cornerBottomRight);
        }
    }
}
