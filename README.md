
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


```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  make publish
  sudo make install
```

Without make:

```bash
  git clone https://github.com/ssprea/ssprea-nvidia-control.git
  cd ssprea-nvidia-control
  dotnet run --configuration Release
```



If you get an "NVML_ERROR_NO_PERMISSION" error in the console when applying a profile, try running the tool with sudo.
