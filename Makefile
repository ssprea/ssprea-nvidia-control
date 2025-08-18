installcli:
	make -C ssprea-nvidia-control-cli install DESTDIR=$(DESTDIR)


installgui:
	make -C ssprea-nvidia-control install DESTDIR=$(DESTDIR)

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
	make -C ssprea-nvidia-control-cli deb VERSION=1.1.1
	make -C ssprea-nvidia-control deb VERSION=1.1.1
