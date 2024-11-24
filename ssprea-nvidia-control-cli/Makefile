
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
	mkdir -p /usr/share/ssprea-nvidia-control-cli
	cp -r bin/Release/net8.0/publish/* /usr/share/ssprea-nvidia-control-cli

	ln -s /usr/share/ssprea-nvidia-control-cli/ssprea-nvidia-control-cli /usr/local/bin/snvctl
		
uninstall:
	rm -r /usr/share/ssprea-nvidia-control-cli
	rm /usr/local/bin/snvctl