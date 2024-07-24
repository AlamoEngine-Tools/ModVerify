# ModVerify: A Mod Verification Tool

ModVerify is a command-line tool designed to verify mods for the game Star Wars: Empire at War and its expansion Forces of Corruption.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Options](#options)
- [Available Checks](#available-checks)

## Installation

Download the latest release from the [releases page](https://github.com/AlamoEngine-Tools/ModVerify/releases). There are two versions of the application available. 

1. `ModVerify.exe` is the default version. Use this if you simply want to verify your mods. This version only works on Windows.
2. `ModVerify-NetX.zip` is the cross-platform app. It works on Windows and Linux and is most likely the version you want to use to include it in some CI/CD scenarios.

You can place the files anywhere on your system, eg. your Desktop. There is no need to place it inside a mod's directory. 

*Note: Both versions have the exact same feature set. They just target a different .NET runtime. Linux and CI/CD support is not fully tested yet. Current priority is on the Windows-only version.*

## Usage

Simply run the executable file `ModVerify.exe`. 

When given no specific argument through the command line, the app will ask you which game or mod you want to verify. When the tool is done, it will write the verification results into new files next to the executable. 

A `.JSON` file lists all found issues. Into seperate `.txt` files the same errors get grouped by a category of the finding. The text files may be easier to read, while the json file is more useful for 3rd party tool processing.   

## Options

You can also run the tool with command line arguments to adjust the tool to your needs. 

To see all available options, open the command line and type:

```bash
ModVerify.exe --help
```

Here is a list of the most relevant options: 

### `--path`
Specifies a path that shall be analyzed. **There will be no user input required when using this option**

### `--output`
Specified the output path where analysis result shall be written to.

### `--baseline`
Specifies a baseline file that shall be used to filter out known errors. You can download the [FoC baseline](focBaseline.json) which includes all errors produced by the vanilla game.

### `--createBaseline`
If you want to create your own baseline, add this option with a file path such as `myModBaseline.json`. 

### Example
This is an example run configuration that analyzes a specific mod, uses a the FoC basline and writes the output into a dedicated directory: 

```bash
ModVerify.exe --path "C:\My Games\FoC\Mods\MyMod" --output "C:\My Games\FoC\Mods\MyMod\verifyResults" --baseline focBaseline.json
```


## Available Checks

The following verifiers are currently implemented: 

### For SFX Events: 
- Checks whether coded preset exists
- Checks the referenced samples for validity (bit rate, sample size, channels, etc.)
- Duplicates


### For GameObjects
- Checks the referenced models for validity (textures, particles and shaders)
- Duplicates


