#!/bin/bash
work=$HOME/.tmp/Universe.Drawing-Native
rm -rf $work
mkdir $work
git clone https://github.com/devizer/Universe.Drawing.git $work
xbuild $work/Universe.Drawing.sln /t:Rebuild /p:Configuration=Release /verbosity:minimal
old=`pwd`
cd $work/Universe.Drawing.Tests/bin/Release


rm -f Universe.Drawing.Tests-*
# exit 0;
export CC="gcc"
MONO_PATH=. mkbundle --keeptemp -c -o Universe.Drawing.Tests.c --static --deps Universe.Drawing.Tests.exe Universe.Drawing.dll
MONO_PATH=. mkbundle --keeptemp -o Universe.Drawing.Tests-static --static --deps Universe.Drawing.Tests.exe Universe.Drawing.dll
MONO_PATH=. mkbundle --keeptemp -o Universe.Drawing.Tests-dynamic --deps Universe.Drawing.Tests.exe Universe.Drawing.dll
export CC="gcc -O3"
MONO_PATH=. mkbundle --keeptemp -o Universe.Drawing.Tests-static-optimized --static --deps Universe.Drawing.Tests.exe Universe.Drawing.dll




cd $old


