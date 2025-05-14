
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
	mkdir -p bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64/usr/local/bin
	mkdir -p bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64/usr/share/ssprea-nvidia-control-cli
	mkdir -p bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64/DEBIAN
	echo 'Package: ssprea-nvidia-control-cli\nVersion: 1.0.3\nDepends: dotnet-runtime-9.0\nArchitecture: amd64\nMaintainer: ssprea <ssprea@noreply.users.github.com>\nDescription: A CLI tool to manage NVidia GPUs overclock, fans and power limit. Supports both Wayland and X11' > bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64/DEBIAN/control
	
	cp -r bin/Release/net9.0/publish/* bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64/usr/share/ssprea-nvidia-control-cli
	ln -sf /usr/share/ssprea-nvidia-control-cli/ssprea-nvidia-control-cli bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64/usr/local/bin/snvctl

	dpkg-deb --build --root-owner-group bin/Release/deb/ssprea-nvidia-control-cli_1.0.3-1_amd64

install:
	mkdir -p $(DESTDIR)/usr/share/ssprea-nvidia-control-cli
	cp -r bin/Release/net9.0/publish/* $(DESTDIR)/usr/share/ssprea-nvidia-control-cli

	mkdir -p $(DESTDIR)/usr/local/bin/
	ln -s /usr/share/ssprea-nvidia-control-cli/ssprea-nvidia-control-cli $(DESTDIR)/usr/local/bin/snvctl
		
uninstall:
	rm -r /usr/share/ssprea-nvidia-control-cli
	rm /usr/local/bin/snvctl