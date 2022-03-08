# YT Striker

## Description
Command-line automation tool to report YouTube channels and videos. It uses Selenium WebDriver as an automation backend.

### Supported OS
* Windows
* ~~MacOS~~ (planned)

### Supported browsers
* Microsoft Edge (recommended)
* Chrome
* Firefox

### Operation Modes
1. **Report Channel** - opens channel 'About' page and reports the channel owner;
1. **Report Videos** - opens every video on the channel and reports it. The number of videos can be limited (see [Usage](#usage) section).


## Usage

### Quick Guide

1. Open browser you want to use and login/create a user profile. Recommended browser - Microsoft Edge<br>
*It is recommended to create a separate profile so as not to interfere with your main profile.*
![Adding a Browser Profile][browser-profile]

1. Navigate to YouTube and sign in. 
*It is recommeded to create a separate YouTube account.*

1. Close all tabs in the web browser and the browser itself<br>
**Important!** Make sure you have a correct profile in your web browser and you are signed in to YouTube before closing web browser.

1. Open "Targets.txt" file and add there channel names you want to report. Each channel on a separate line

1. *\[Optional\]* Enable VPN. (Some channels may be restricted to access from different countries)

1. Run `ProcessChannels_BROWSER.bat` if you want to report channel owners or `ProcessVideos_BROWSER.bat` if you want to report videos (BROWSER - the name of the web browser you want to use). It is recommended to use Edge because it is the most stable during automation.

Repeat the last three actions for each set of channels.

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


[browser-profile]: Docs/img/browser_profile.png "Adding Browser Profile"
[complaint-options]: Docs/img/complaint_options.png "Complaint options"