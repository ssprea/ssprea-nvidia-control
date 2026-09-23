
clean:
	dotnet clean 

restore:
	dotnet restore

build: clean restore
	dotnet build --configuration Release

.PHONY: publish

publish: 
	dotnet clean
	dotnet publish --configuration Release -r linux-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeAllContentForSelfExtract=true
      
      
	
run: build
	dotnet run --configuration Release

deb: publish
	mkdir -p bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/usr/local/bin
	mkdir -p bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/usr/share/applications
	mkdir -p bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/usr/share/slimit-gui
	mkdir -p bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/DEBIAN
	echo -e 'Package: slimit-gui\nVersion: $(VERSION)-$(PKGN)\nDepends: \nArchitecture: amd64\nMaintainer: ssprea <ssprea@proton.me>\nDescription: A Unified GPUs management tool to overclock, control fans and power limit. Supports both Wayland and X11' > bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/DEBIAN/control
	
	cp bin/Release/net10.0/linux-x64/publish/SLimit.Gui bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/usr/local/bin/slimit-gui
	cp Assets/SLimit.desktop bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64/usr/share/applications/SLimit.desktop

	dpkg-deb --build --root-owner-group bin/Release/deb/slimit-gui_$(VERSION)-$(PKGN)_amd64


	

install:
	mkdir -p $(DESTDIR)/usr/local/bin/
	mkdir -p $(DESTDIR)/usr/share/applications

	mkdir -p $(DESTDIR)/usr/lib/systemd/user
	mkdir -p $(DESTDIR)/usr/lib/systemd/system



	cp bin/Release/net10.0/linux-x64/publish/SLimit.Gui $(DESTDIR)/usr/local/bin/slimit-gui
	cp Assets/SLimit.desktop $(DESTDIR)/usr/share/applications/SLimit.desktop

	cp SystemdServices/snvctl-gui.service $(DESTDIR)/usr/lib/systemd/user/snvctl-gui.service


		
uninstall:
	rm -r /usr/share/slimit-gui
	rm /usr/share/applications/SLimit.desktop
	rm /usr/local/bin/slimit-gui
