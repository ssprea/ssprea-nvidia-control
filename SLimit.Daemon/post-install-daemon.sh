if [ -f /etc/systemd/system/snvctl.service ]; then
    systemctl disable --now snvctl.service || true
    rm -f /etc/systemd/system/snvctl.service
fi

systemctl daemon-reload
systemctl enable --now slimitd.service
systemctl restart slimitd.service
