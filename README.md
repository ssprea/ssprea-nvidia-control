# ssprea-nvidia-control


ssprea-nvidia-control is a highly customizable GUI overclocking tool for NVidia GPUs that supports Wayland, X11 and Windows.

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

## Future features:

- AMD Support
- GPU Undervolting
  
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

To run it, download the source code and build manually, then add the snvctl CLI tool exe to the PATH environment variable.
After that you can just run it as an administrator.

NOTE: Windows is not the main target platform for this tool so features might be delayed and there might be many bugs.

-------------------------

### Building:

To build this tool you need to install Make and .NET SDK 9.0


Build and install GUI & CLI:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make publish
  sudo make installall
```

Build and install CLI only:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make publish
  sudo make installcli
```

Build deb packages to install with dpkg:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make deb
```
The packages will be located in `ssprea-nvidia-control/ssprea-nvidia-control/bin/Release/deb/` for the gui and `ssprea-nvidia-control/ssprea-nvidia-control-cli/bin/Release/deb/` for the cli.

You can install them using `sudo dpkg -i <package_path>`


Without make:

```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  dotnet run --configuration Release
```
If you run the tool like this the CLI tool won't be found in the path so you will not be able to apply settings.
