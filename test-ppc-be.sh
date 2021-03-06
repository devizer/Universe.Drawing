#!/bin/bash
set -e
work=/d2/Universe.Drawing
rm -rf $work
mkdir $work
git clone https://github.com/devizer/Universe.Drawing.git $work
xbuild $work/Universe.Drawing.sln /t:Rebuild /p:Configuration=Release /verbosity:minimal
old=`pwd`
cd $work/Universe.Drawing.Tests/bin/Release

# /opt/mono-3.2.8/bin/mono Universe.Drawing.Tests.exe

# mono --aot=keep-temps -O=all,-shared Universe.Drawing.dll || true
mono Universe.Drawing.Tests.exe


cd $old
