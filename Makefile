installcli:
	make -C ssprea-nvidia-control-cli install


installgui:
	make -C ssprea-nvidia-control install

uninstallcli:
	make -C ssprea-nvidia-control-cli uninstall

uninstallgui:
	make -C ssprea-nvidia-control uninstall


publishcli:
	make -C ssprea-nvidia-control-cli publish

publishgui:
	make -C ssprea-nvidia-control publish

publish: publishcli publishgui

installall: installcli installgui

uninstallall: uninstallgui uninstallcli

reinstallall: uninstallcli installgui

deb:
	make -C ssprea-nvidia-control-cli deb
	make -C ssprea-nvidia-control deb
