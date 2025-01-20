
# ssprea-nvidia-control


ssprea-nvidia-control is a GUI overclocking tool for NVidia GPUs that supports both Wayland and X11


## Features

- Overclock profiles
- Fan control and curve
- Power limit management
- Works on Wayland, X11 and Windows


## Requirements

- NVidia proprietary driver 555+
- .NET core 8

## Building and installing

Linux quick install (gui & cli):

```
wget https://gist.githubusercontent.com/ssprea/d82f6fd46b15b7076df162dc66e44204/raw/2278c05805d57e33e036ffa9011ad564900cd50f/snvctl-install.sh && chmod +x ./snvctl-install.sh && ./snvctl-install.sh install
```

Uninstall: 


```
wget https://gist.githubusercontent.com/ssprea/d82f6fd46b15b7076df162dc66e44204/raw/2278c05805d57e33e036ffa9011ad564900cd50f/snvctl-install.sh && chmod +x ./snvctl-install.sh && ./snvctl-install.sh uninstall
```


To install GUI and CLI:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  sudo make installgui
```

Install CLI only:
```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  sudo make installcli
```


Without make:

```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  dotnet run --configuration Release
```



If you get an "NVML_ERROR_NO_PERMISSION" error in the console when applying a profile, try running the tool with sudo.
