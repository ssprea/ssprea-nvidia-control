
clean:
	dotnet clean 

restore:
	dotnet restore

build: clean restore
	dotnet build --configuration Release

publish: build
	dotnet publish
	
run: build
	dotnet run --configuration Release

deb: publish
	mkdir -p bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/usr/local/bin
	mkdir -p bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/usr/share/applications
	mkdir -p bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/usr/share/ssprea-nvidia-control
	mkdir -p bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/DEBIAN
	echo -e 'Package: ssprea-nvidia-control\nVersion: 1.1.0\nDepends: dotnet-runtime-9.0\nArchitecture: amd64\nMaintainer: ssprea <ssprea@proton.me>\nDescription: A GUI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/DEBIAN/control
	
	cp -r bin/Release/net9.0/linux-x64/publish/* bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/usr/share/ssprea-nvidia-control
	cp Assets/ssprea-nvidia-control.desktop bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/usr/share/applications/ssprea-nvidia-control.desktop
	ln -sf /usr/share/ssprea-nvidia-control/ssprea-nvidia-control bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64/usr/local/bin/snvctl-gui

	dpkg-deb --build --root-owner-group bin/Release/deb/ssprea-nvidia-control_1.1.0-1_amd64

install:
	mkdir -p $(DESTDIR)/usr/share/ssprea-nvidia-control
	mkdir -p $(DESTDIR)/usr/share/applications/
	cp -r bin/Release/net9.0/linux-x64/publish/* $(DESTDIR)/usr/share/ssprea-nvidia-control
	cp Assets/ssprea-nvidia-control.desktop $(DESTDIR)/usr/share/applications/ssprea-nvidia-control.desktop

	mkdir -p $(DESTDIR)/usr/local/bin/
	ln -sf /usr/share/ssprea-nvidia-control/ssprea-nvidia-control $(DESTDIR)/usr/local/bin/snvctl-gui
		
uninstall:
	rm -r /usr/share/ssprea-nvidia-control
	rm /usr/share/applications/ssprea-nvidia-control.desktop
	rm /usr/local/bin/snvctl-gui