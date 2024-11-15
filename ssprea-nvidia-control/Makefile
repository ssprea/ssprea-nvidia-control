
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

install:
	mkdir -p /usr/share/ssprea-nvidia-control
	mkdir -p /usr/share/applications/
	cp -r bin/Release/net8.0/publish/* /usr/share/ssprea-nvidia-control
	cp Assets/ssprea-nvidia-control.desktop /usr/share/applications/ssprea-nvidia-control.desktop

	ln -s /usr/share/ssprea-nvidia-control/ssprea-nvidia-control /usr/local/bin
		
uninstall:
	rm -r /usr/share/ssprea-nvidia-control
	rm /usr/share/applications/ssprea-nvidia-control.desktop
	rm /usr/local/bin/ssprea-nvidia-control
			