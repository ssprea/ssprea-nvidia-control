
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
	mkdir -p bin/Release/deb/slimit-daemon_$(VERSION)-$(PKGN)_amd64/usr/local/bin
	mkdir -p bin/Release/deb/slimit-daemon_$(VERSION)-$(PKGN)_amd64/DEBIAN
	echo -e 'Package: slimit-daemon\nVersion: $(VERSION)-$(PKGN)\nDepends: \nArchitecture: amd64\nMaintainer: ssprea <ssprea@proton.me>\nDescription: Daemon tool for SLimit utility' > bin/Release/deb/slimit-daemon_$(VERSION)-$(PKGN)_amd64/DEBIAN/control
	
	cp bin/Release/net9.0/linux-x64/publish/SLimit.Daemon bin/Release/deb/slimit-daemon_$(VERSION)-$(PKGN)_amd64/usr/local/bin/slimitd

	dpkg-deb --build --root-owner-group bin/Release/deb/slimit-daemon_$(VERSION)-$(PKGN)_amd64

install:
	mkdir -p $(DESTDIR)/usr/local/bin/
	cp bin/Release/net10.0/linux-x64/publish/SLimit.Daemon $(DESTDIR)/usr/local/bin/slimitd
		
uninstall:
	rm /usr/local/bin/slimitd
