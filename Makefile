installcli:
	make -C ssprea-nvidia-control-cli publish
	make -C ssprea-nvidia-control-cli install


installgui: installcli
	make -C ssprea-nvidia-control publish
	make -C ssprea-nvidia-control install

uninstallcli: uninstallgui
	make -C ssprea-nvidia-control-cli uninstall

uninstallgui:
	make -C ssprea-nvidia-control uninstall

reinstallall: uninstallcli installgui

