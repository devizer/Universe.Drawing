#!/bin/bash
work=$HOME/.tmp/Universe.Drawing
rm -rf $work
mkdir $work
git clone https://github.com/devizer/Universe.Drawing.git $work
xbuild $work/Universe.Drawing.sln /t:Rebuild /p:Configuration=Release /verbosity:minimal
xbuild $work/Universe.Drawing.sln /t:Rebuild /p:Configuration=Debug   /verbosity:minimal
old=`pwd`
cd $work/Universe.Drawing.Tests/bin/Release
mono --optimize=all Universe.Drawing.Tests.exe
cd $old


