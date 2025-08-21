VERSION=1.1.2

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
	make -C ssprea-nvidia-control-cli deb VERSION=$(VERSION)
	make -C ssprea-nvidia-control deb VERSION=$(VERSION)

appimage: OUTDIR ?= packages/AppImage/AppDir
appimage: DESTDIR ?= ../$(OUTDIR)
appimage: publish installall
	mkdir -p $(OUTDIR)
	cp ssprea-nvidia-control/Assets/app-icon.png $(OUTDIR)/ssprea-nvidia-control.png
	cp ssprea-nvidia-control/Assets/ssprea-nvidia-control.desktop $(OUTDIR)/ssprea-nvidia-control.desktop
	touch $(OUTDIR)/AppRun
	echo -e '#!/bin/bash\nexport PATH="$$APPDIR/usr/local/bin:$$PATH"\nexec "$$APPDIR/usr/local/bin/snvctl-gui" "$$@"' > $(OUTDIR)/AppRun
	chmod +x $(OUTDIR)/AppRun
	[ -f appimagetool-x86_64.AppImage ] || wget https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage
	chmod +x ./appimagetool-x86_64.AppImage
	ARCH=x86_64 ./appimagetool-x86_64.AppImage $(OUTDIR) ssprea-nvidia-control-$(VERSION).AppImage

