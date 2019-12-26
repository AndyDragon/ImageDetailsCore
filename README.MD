# ImageDetailsCore

The ImageDetailsCore project is a .NET Core console application which uses MetadataExtractor and System.Drawing to load the EXIF data from an image file and produces an image details badge PNG (in the same location using the _info.png suffix). This can be used to show the details about photographs on social media platforms as a nice way to display the camera settings.

## Sample

![alt text](docs/Sample_info.png "Simple image badge")

The information shown includes the camera, lens, focal length, ISO, shutter speed and aperture F-stop, EV bias, white balance program, exposure program, date/time and artist.

| Property        | Source EXIF                                     |
| --------------- | ----------------------------------------------- |
| Camera          | IFD0/Model or SubIFD/Model                      |
| Lens            | SubIFD/Lens Model                               |
| Focal Length    | SubIFD/Focal Length or SubIFD/Focal Length 35   |
| ISO             | SubIFD/ISO Speed Ratings                        |
| Shutter         | SubIFD/Exposure Time                            |
| Aperture        | SubIFD/F-Number                                 |
| EV Bias         | SubIFD/Exposure Bias Value                      |
| White Balance   | SubIFD/White Balance Mode + SubIFD/White Balance|
| Exposure Prgm   | SubIFD/Exposure Program                         |
| Date            | SubIFD/Date/Time Original                       |
| Artist          | IFD0/Artist or SubIFD/Artist                    |

Additionally, the Camera and Lens properties are then mapped from a source string to a finalized string using the Resources/Options.json file. This file currently has the data I use for various cameras and lenses to produce cleaner text. Without changing the source code or recompiling, mappings can be added, removed or modified in the file to do the same for your equipment. The code will emit a warning whenever a camera or lens string is not found. To skip the warning output, change the 'warnMissingCamera' and 'warnMissingLens' options to `false`.

If the camera (Model EXIF tag) is not found in the tags, the badge will not be created and an error message will be emitted.

## Options file

The Resources/Options.json file contains a few options for the tool.

> `warnMissingCamera` and `warnMissingLens`

Boolean flags to turn on and off warning about missing camera and lens mapping in the `cameras` and `lenses` options.

> `recursiveFolders`

Boolean flag set to `true` to recursively handle folders when the application is passed a folder as an argument or set to `false` to only process files in the root folder when passed a folder as an argument.

> `folderSearchExtensions` 

An array of strings of extensions for files to be processed when a folder is passed as an argument. Only files matching the extensions will be processed.

> `cameras` and `lenses`

Two arrays of mapping objects with a `source` string and a `display` string. Whenever a camera is found in the EXIF data that matches the `source` property of an entries in the `cameras` array, the camera is changed to the `display` property. Same for `lenses`. This is used to produce a more pleasing (and consistent) output.  eg:

``` json
  "cameras": [
    { "source": "NIKON Z 7", "display": "NIKON Z7" },
  ],
  "lenses": [
    { "source": "24-70 mm F_4.0", "display": "NIKKOR Z 24-70mm f/4 S" },
  ]
```

If the camera is `NIKON Z 7`, the badge will show `NIKON Z7` instead. If the lens is `24-70 mm F_4.0`, the badge will show `NIKKOR Z 24-70mm f/4 S`.

## Using the application

Once built (VS 2019 or VS 2019 for Mac to build), simply run the following command:

```
ImageDetailsCore [full path to the image file][full path to folder of images][...additional full paths]
```

On Windows, you can drop files or folders right on the ImageDetailsCore.exe file. On the Mac, I created an Automator script with a Run Shell Script that runs the ImageDetailsCore application with 'accepts files' and then shows a message with the output. The downside to this is that the ImageDetailsCore output needs to be in a fixed location and a fully qualified path is required in the run shell script task.

## Dependencies

The application is currently using System.Drawing which means that the libgdiplus library is required for Linux and MacOS.

For the MacOS, I used homebrew to install the library:

```
brew install mono-libgdiplus
```

Likely for your Linux distro, a similar 'apt', 'yum' or other package installer should be able to install the libgdiplus component.

## Building

If you have the .NET Core SDK installed on your machine, you can also build just by changing directory to the location of the .SLN file and running dotnet:

```
dotnet build -c Release
```

This will build the release configuration of the application.

Alternatively, you can load the .SLN file using VS 2019 or VS 2019 for Mac and build the solution.