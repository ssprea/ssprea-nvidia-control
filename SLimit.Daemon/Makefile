
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
	mkdir -p $(DESTDIR)/usr/lib/systemd/system/
	cp bin/Release/net10.0/linux-x64/publish/SLimit.Daemon $(DESTDIR)/usr/local/bin/slimitd

	cp SystemdServices/slimitd.service $(DESTDIR)/usr/lib/systemd/system/slimitd.service
		
uninstall:
	rm /usr/local/bin/slimitd
