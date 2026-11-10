
clean:
	dotnet clean
	rm packages/*

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
	mkdir -p packages
	nfpm package -p deb -t packages

rpm: publish
	mkdir -p packages
	nfpm package -p rpm -t packages


	

install:
	mkdir -p $(DESTDIR)/usr/local/bin/
	mkdir -p $(DESTDIR)/usr/share/applications

	mkdir -p $(DESTDIR)/usr/lib/systemd/user
	mkdir -p $(DESTDIR)/usr/lib/systemd/system
	mkdir -p $(DESTDIR)/usr/share/icons/hicolor/256x256/apps


	cp bin/Release/net10.0/linux-x64/publish/SLimit.Gui $(DESTDIR)/usr/local/bin/slimit-gui
	cp Assets/SLimit.desktop $(DESTDIR)/usr/share/applications/SLimit.desktop
	cp Assets/slimit-small-512.png $(DESTDIR)/usr/share/icons/hicolor/256x256/apps/slimit.png



	cp SystemdServices/snvctl-gui.service $(DESTDIR)/usr/lib/systemd/user/snvctl-gui.service


		
uninstall:
	rm -r /usr/share/slimit-gui
	rm /usr/share/applications/SLimit.desktop
	rm /usr/local/bin/slimit-gui
