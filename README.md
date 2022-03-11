# YT Striker
![master](https://github.com/alex89200/yt-striker/actions/workflows/master.yml/badge.svg)
![develop](https://github.com/alex89200/yt-striker/actions/workflows/develop.yml/badge.svg)

## Language
* **English**
* [Українська](Docs/README_UA.md)

## Description
Command-line automation tool to report YouTube channels and videos. It uses Selenium WebDriver as an automation backend.

### Supported OS
* Windows
* MacOS (using Mono)

### Supported browsers
* Microsoft Edge (recommended)
* Chrome
* Firefox

### Operation Modes
1. **Report Channel** - opens channel 'About' page and reports the channel owner;
1. **Report Videos** - opens every video on the channel and reports it. The number of videos can be limited (see [Usage](#usage) section).


## Usage

### Quick Guide

1. Open "targets.txt" file and add there channel URLs you want to report. Each channel on a separate line. URL must not end with `/videos`, `/about`, `/featured`, etc. Examples of correct URLs:
    * `https://www.youtube.com/c/FaktyICTVchannel`
    * `https://www.youtube.com/channel/UC9zktRjA2aI7cEZOfkDqmLg`

1. *\[Optional\]* Enable VPN. (Some channels may be restricted to access from different countries)

1. Run `ProcessChannels_BROWSER` if you want to report channel owners or `ProcessVideos_BROWSER` if you want to report videos (BROWSER - the name of the web browser you want to use). It is recommended to use Edge because it is the most stable during automation.<br><br>
MacOS users should use Terminal to run the script. Just open Terminal, drag-drop the script on it and press Return. If Mono is not installed, it will be prompted to install it first.
<br><br>

**Important!**<br>
>The first time you run this tool, it will be prompted to sign in to the YouTube profile. Program will wait for 5 minutes to let you to log in.<br>
It is not recommended to use a personal account, it is better to create a separate one.

<br>

### Advanced Guide

This tool can be run manualy through the command line interface in order to have a full control over its capabilities.

<br>

#### **Command-Line Arguments**

|      Argument       | Description                   |
| ------------------- | ----------------------------- |
| `-m`, `--mode`      | *(Default: videos)*<br>Processing mode. <br><br>**Possible values:**<br>`channel` - report author of the channel<br>`videos` - report separate videos. |
| `-b`, `--browser`   | *(Default: edge)*<br>Which browser to use.<br><br>**Possible values:**<br>`chrome`, `firefox`, `edge`. |
| `-c`, `--channel`   | *(Group: input)*<br>Name of the channel to process<br>**Important!** Either this or `-f` argument must be passed. |
| `-f`, `--file`      | *(Group: input)*<br>File which contains URLs of YouTube channels to process. Each URL must be on a separate line.<br>**Important!** Either this or `-c` argument must be passed. |
| `-l`, `--limit`     | *(Default: 30)*<br>Maximum number of videos to process on each channel. Parameter is omitted when in 'channel' mode (see -m parameter). |
| `-i`, `--complaint`     | Index of the main complaint. Numbering starts from 0. Default is 'Violence'. |
| `-o`, `--sub-complaint` | Index of the sub-complaint in a dropdown if relevant. Numbering starts from 0. Default is the first in the list. |
| `-d`, `--desc`          | *(Default: description.txt)*<br>Path to the file with text which will be used as a violation description in the report. |
| `-t`, `--timeout`       | *(Default: 60)*<br>Page loading timeout. |
| `--dry-run`           | *(Default: false)*<br>Do the full flow, but don't press the 'Submit' button in the end. |
| `-v`, `--verbose`       | *(Default: false)*<br>Set output to verbose messages. |
| `--help`              | Display a help screen. |
| `--version`           | Display version information. |

<br>

#### **Detailed Info**

* It is recommended to use Edge as a target web browser because it comes with Windows and works pretty well with Selenium (automation backend). Google Chrome is on the second place and Firefox is not recommended due to very unstable behavior during automation;

* There are two ways to provide channels to process:
  * Run this tool for each channel separately providing it as an command line argument: `-c channelName`;
  * Provide a file with a list of channels to process `-f filePath`.

  These variants may be used in both `Channels` and `Videos` processing modes.

  In case both `-c` and `-f` arguments are passed, only `-c` argument will be used.

  File with channel names must contain each channel name on a separate line;

* Description file contains text to be passed into the report dialog. Skip this option to use default 'description.txt' file or provide a path to the custom one;

* Use `--dry-run` argument to check your setup without actual reporting;

* Complaint and Sub-complaint arguments are simply indexes of the options to select (see image below). Indexing starts from zero. Skip them or pass -1 to automatically select 'Violence' related options.
![Complaint options][complaint-options]

<br>

#### **Useful Tips / Troubleshoot**
- The tool runs web browsers using the separate user profiles. These profiles are created in the `Profiles` folder of the directory where the tool is located. If you want to switch/logout from the YouTube account - simply delete this folder;
- You can not run a few copies of this tool in parallel. If you do so, you will receive an error that the web browser profile is already in use;
- The tool supports only latest versions of the web browsers (currently they are v98 and v99).

[complaint-options]: Docs/img/complaint_options.png "Complaint options"