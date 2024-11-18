installcli:
	make -C ssprea-nvidia-control-cli publish
	make -C ssprea-nvidia-control-cli install


install: installcli
	make -C ssprea-nvidia-control publish
	make -C ssprea-nvidia-control install

uninstallcli: uninstall
	make -C ssprea-nvidia-control-cli uninstall

uninstall:
	make -C ssprea-nvidia-control uninstall
