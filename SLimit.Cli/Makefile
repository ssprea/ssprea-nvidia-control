
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
	mkdir -p bin/Release/deb/slimit-cli_$(VERSION)-$(PKGN)_amd64/usr/local/bin
	mkdir -p bin/Release/deb/slimit-cli_$(VERSION)-$(PKGN)_amd64/DEBIAN
	echo -e 'Package: slimit-cli\nVersion: $(VERSION)-$(PKGN)\nDepends: \nArchitecture: amd64\nMaintainer: ssprea <ssprea@proton.me>\nDescription: A CLI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/slimit-cli_$(VERSION)-$(PKGN)_amd64/DEBIAN/control
	
	cp bin/Release/net10.0/linux-x64/publish/SLimit.Cli bin/Release/deb/slimit-cli_$(VERSION)-$(PKGN)_amd64/usr/local/bin/snvctl

	dpkg-deb --build --root-owner-group bin/Release/deb/slimit-cli_$(VERSION)-$(PKGN)_amd64

install:
	mkdir -p $(DESTDIR)/usr/local/bin/
	cp bin/Release/net10.0/linux-x64/publish/SLimit.Cli $(DESTDIR)/usr/local/bin/snvctl
		
uninstall:
	rm /usr/local/bin/snvctl
