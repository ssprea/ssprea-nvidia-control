VERSION=2.0.0
PKGN=1

installcli:
	make -C  SLimit.Cli install DESTDIR=$(DESTDIR)

installdaemon:
	make -C SLimit.Daemon install DESTDIR=$(DESTDIR)

installgui:
	make -C ssprea-nvidia-control install DESTDIR=$(DESTDIR)


uninstallcli:
	make -C SLimit.Cli uninstall

uninstalldaemon:
	make -C SLimit.Daemon uninstall

uninstallgui:
	make -C ssprea-nvidia-control uninstall


publishcli:
	make -C SLimit.Cli publish

publishdaemon:
	make -C SLimit.Daemon publish

publishgui:
	make -C ssprea-nvidia-control publish

.NOTPARALLEL:
publish: publishcli publishgui publishdaemon

installall: installcli installgui installdaemon

uninstallall: uninstallgui uninstallcli uninstalldaemon

reinstallall: uninstallcli installgui

deb:
	make -C SLimit.Cli deb VERSION=$(VERSION) PKGN=$(PKGN)
	make -C SLimit.Daemon deb VERSION=$(VERSION) PKGN=$(PKGN)
	make -C ssprea-nvidia-control deb VERSION=$(VERSION) PKGN=$(PKGN)

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

