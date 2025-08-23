
clean:
	dotnet clean 

restore:
	dotnet restore

build: clean restore
	dotnet build --configuration Release

publish: build
	dotnet publish --configuration Release -r linux-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeAllContentForSelfExtract=true
      
      
	
run: build
	dotnet run --configuration Release

deb: publish
	mkdir -p bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/usr/local/bin
	mkdir -p bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/usr/share/applications
	mkdir -p bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/usr/share/ssprea-nvidia-control
	mkdir -p bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/DEBIAN
	echo -e 'Package: ssprea-nvidia-control\nVersion: $(VERSION)-$(PKGN)\nDepends: \nArchitecture: amd64\nMaintainer: ssprea <ssprea@proton.me>\nDescription: A GUI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/DEBIAN/control
	
	cp bin/Release/net9.0/linux-x64/publish/ssprea-nvidia-control bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/usr/local/bin/snvctl-gui
	cp Assets/ssprea-nvidia-control.desktop bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64/usr/share/applications/ssprea-nvidia-control.desktop

	dpkg-deb --build --root-owner-group bin/Release/deb/ssprea-nvidia-control_$(VERSION)-$(PKGN)_amd64


	

install:
	mkdir -p $(DESTDIR)/usr/local/bin/
	mkdir -p $(DESTDIR)/usr/share/applications

	cp bin/Release/net9.0/linux-x64/publish/ssprea-nvidia-control $(DESTDIR)/usr/local/bin/snvctl-gui
	cp Assets/ssprea-nvidia-control.desktop $(DESTDIR)/usr/share/applications/ssprea-nvidia-control.desktop

		
uninstall:
	rm -r /usr/share/ssprea-nvidia-control
	rm /usr/share/applications/ssprea-nvidia-control.desktop
	rm /usr/local/bin/snvctl-gui