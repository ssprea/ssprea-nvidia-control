
clean:
	dotnet clean 

restore:
	dotnet restore

build: clean restore
	dotnet build --configuration Release

publish: build
	dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeAllContentForSelfExtract=true

	
run: build
	dotnet run --configuration Release

deb: publish
	mkdir -p bin/Release/deb/ssprea-nvidia-control-cli_$(VERSION)-1_amd64/usr/local/bin
	mkdir -p bin/Release/deb/ssprea-nvidia-control-cli_$(VERSION)-1_amd64/usr/share/ssprea-nvidia-control-cli
	mkdir -p bin/Release/deb/ssprea-nvidia-control-cli_$(VERSION)-1_amd64/DEBIAN
	echo -e 'Package: ssprea-nvidia-control-cli\nVersion: $(VERSION)\nDepends: dotnet-runtime-9.0\nArchitecture: amd64\nMaintainer: ssprea <ssprea@proton.me>\nDescription: A CLI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/ssprea-nvidia-control-cli_$(VERSION)-1_amd64/DEBIAN/control
	
	cp -r bin/Release/net9.0/publish/ssprea-nvidia-control-cli bin/Release/deb/ssprea-nvidia-control-cli_$(VERSION)-1_amd64/usr/local/bin/snvctl

	dpkg-deb --build --root-owner-group bin/Release/deb/ssprea-nvidia-control-cli_$(VERSION)-1_amd64

install:
	mkdir -p $(DESTDIR)/usr/local/bin/
	cp bin/Release/net9.0/publish/ssprea-nvidia-control-cli $(DESTDIR)/usr/local/bin/snvctl
		
uninstall:
	rm -r /usr/share/ssprea-nvidia-control-cli
	rm /usr/local/bin/snvctl