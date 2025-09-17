# ssprea-nvidia-control


ssprea-nvidia-control is a highly customizable GUI overclocking tool for NVidia and AMD GPUs that supports Wayland, X11 and Windows.


## Disclaimer

This project was created mainly to learn Avalonia UI, so it might not be perfect. Feel free to report any bugs or to give me any suggestions you might have by opening an [Issue](https://github.com/ssprea/ssprea-nvidia-control/issues/new).


## AMD Branch

This branch is for experimental AMD support, only a couple of features are implemented right now but feel free to test it.

Currently implemented AMD features:

Monitoring:

- Gpu core clock speed
- Gpu memory clock speed
- Gpu temperature
- Gpu memory usage


Profiles cannot be applied right now!


## Features

- Overclock profiles
- Fan control and curve
- Auto apply profile at startup
- Power limit management
- Works on Wayland and X11
- Highly customizable (See ![Customization wiki page](https://github.com/ssprea/ssprea-nvidia-control/wiki/Customization))

## Screenshots

![Main window](https://i.ibb.co/DH0M6QLm/Schermata-20250722-153534.png)

![Curve creation](https://i.ibb.co/mnhFbwc/Schermata-20250722-153726.png)

## Requirements

- NVidia proprietary driver 555+
- .NET core 9
## Installation:

### Ubuntu/Debian/Mint:

APT Repo available, follow instructions here: https://github.com/ssprea/snvctl-apt-repo/blob/main/README.md

Deb packages available in releases.

### Arch:

Package is available in the AUR: https://aur.archlinux.org/packages/ssprea-nvidia-control

```
git clone https://aur.archlinux.org/ssprea-nvidia-control.git
cd ssprea-nvidia-control
makepkg -si
```

With yay:

```
yay -S ssprea-nvidia-control
```

### Windows:

The tool should mostly work on windows without major problems, however I haven't tested it since I currently don't have a Windows machine. 
If you encounter any problems please open an [issue](https://github.com/ssprea/ssprea-nvidia-control/issues/new)



Currently these features are known to not work on Windows:
  - Startup profile
  - Every AMD GPU feature

To run it, download the source code and build manually, then add the snvctl CLI tool exe to the PATH environment variable.
After that you can just run it as an administrator.

NOTE: Windows is not the main target platform for this tool so features might be delayed and there might be many bugs.

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
