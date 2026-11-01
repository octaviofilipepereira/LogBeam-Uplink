#!/bin/bash
# Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
# © 2026 Octávio Filipe Pereira Gonçalves
# Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)
#
# Cria o pacote .deb do LogBeam Uplink para Linux a partir de uma pasta publicada:
#   dotnet publish src/LogBeam.Linux/LogBeam.Linux.csproj -c Release -r linux-x64 --self-contained true \
#     -p:DebugType=none -p:PublishReadyToRun=true -o <pasta>
# Uso: packaging/linux/build-deb.sh <pasta-publicada> <pasta-de-saída>
set -e
PUB=${1:?pasta publicada em falta}
OUT=${2:?pasta de saída em falta}
AQUI=$(cd "$(dirname "$0")" && pwd)
RAIZ=$(cd "$AQUI/../.." && pwd)
VER=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$RAIZ/Directory.Build.props")
D=$(mktemp -d)
trap 'rm -rf "$D"' EXIT

mkdir -p "$D/DEBIAN" "$D/opt/logbeam-uplink" "$D/usr/bin" "$D/usr/share/applications"
cp -r "$PUB"/. "$D/opt/logbeam-uplink/"
chmod -R u=rwX,go=rX "$D/opt/logbeam-uplink"
chmod 755 "$D/opt/logbeam-uplink/logbeam-uplink"
ln -s ../../opt/logbeam-uplink/logbeam-uplink "$D/usr/bin/logbeam-uplink"
install -m 644 "$AQUI/logbeam-uplink.desktop" "$D/usr/share/applications/logbeam-uplink.desktop"
for n in 48 64 128 256; do
  mkdir -p "$D/usr/share/icons/hicolor/${n}x${n}/apps"
  install -m 644 "$AQUI/icons/logbeam-$n.png" "$D/usr/share/icons/hicolor/${n}x${n}/apps/logbeam-uplink.png"
done

cat > "$D/DEBIAN/control" <<EOF
Package: logbeam-uplink
Version: $VER
Section: hamradio
Priority: optional
Architecture: amd64
Maintainer: Octávio Filipe Pereira Gonçalves <ct7bfv@gmail.com>
Depends: libc6, libgcc-s1, libstdc++6, zlib1g, libssl3t64 | libssl3, libice6, libsm6, libx11-6, libfontconfig1, libnotify-bin, xdg-utils
Homepage: https://logbeam.org
Installed-Size: $(du -sk "$D" | cut -f1)
Description: Sends QSOs from your logging programs to LogBeam and ClubLog
 LogBeam Uplink receives QSOs from N1MM+, Not1MM, WSJT-X, JTDX and Log4OM
 and sends them to your LogBeam logbooks and to ClubLog in real time.
EOF

mkdir -p "$OUT"
dpkg-deb --root-owner-group --build "$D" "$OUT/logbeam-uplink_${VER}_amd64.deb"
sha256sum "$OUT/logbeam-uplink_${VER}_amd64.deb"
