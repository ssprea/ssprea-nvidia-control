installcli:
	make -C ssprea-nvidia-control-cli publish
	make -C ssprea-nvidia-control-cli install


installgui:
	make -C ssprea-nvidia-control publish
	make -C ssprea-nvidia-control install

uninstallcli:
	make -C ssprea-nvidia-control-cli uninstall

uninstallgui:
	make -C ssprea-nvidia-control uninstall


installall: installcli installgui

uninstallall: uninstallgui uninstallcli

reinstallall: uninstallcli installgui

deb:
	make -C ssprea-nvidia-control-cli deb
	make -C ssprea-nvidia-control deb
