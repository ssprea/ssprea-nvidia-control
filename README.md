# ssprea-nvidia-control


ssprea-nvidia-control is a GUI overclocking tool for NVidia GPUs that supports both Wayland and X11

## Disclaimer

This project was created mainly to learn Avalonia UI, so it might not be perfect. Feel free to report any bugs or to give me any suggestions you might have by opening an [Issue](https://github.com/ssprea/ssprea-nvidia-control/issues/new).


## Features

- Overclock profiles
- Fan control and curve
- Power limit management
- Works on Wayland and X11

## Screenshots

![Main window](https://i.ibb.co/LzmBSCMN/Screenshot-20250212-031431.png)

![Profile creation](https://i.ibb.co/pr9bVc4J/Screenshot-20250212-031855.png)

![Curve creation](https://i.ibb.co/Q3fqT7tk/Screenshot-20250212-031920.png)
## Requirements

- NVidia proprietary driver 555+
- .NET core 9
## Installation:

### Ubuntu/Debian:

APT Repo available, follow instructions here: https://github.com/ssprea/snvctl-apt-repo/blob/main/README.md

Deb packages available in releases.
## Building:

### Install dependencies:

Ubuntu:
```
sudo apt install make dotnet-sdk-9.0
```
-------------------------

### Building:

Quick build and install script (gui & cli):

```
wget https://gist.githubusercontent.com/ssprea/d82f6fd46b15b7076df162dc66e44204/raw/2278c05805d57e33e036ffa9011ad564900cd50f/snvctl-install.sh && chmod +x ./snvctl-install.sh && ./snvctl-install.sh install
```

Uninstall: 


```
wget https://gist.githubusercontent.com/ssprea/d82f6fd46b15b7076df162dc66e44204/raw/2278c05805d57e33e036ffa9011ad564900cd50f/snvctl-install.sh && chmod +x ./snvctl-install.sh && ./snvctl-install.sh uninstall
```


### Without quick install script:

Make deb packages to install with dpkg:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make deb
```
The packages will be located in `ssprea-nvidia-control/ssprea-nvidia-control/bin/Release/deb/` for the gui and `ssprea-nvidia-control/ssprea-nvidia-control-cli/bin/Release/deb/` for the cli.

You can install them using `sudo dpkg -i <package_path>`


To install GUI and CLI:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make publish
  sudo make installall
```

Install CLI only:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make publish
  sudo make installcli
```




Without make:

```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  dotnet run --configuration Release
```

## Planned features

- Better gui

- Autorun and auto apply of profile at startup

- APT repo and flatpak 
