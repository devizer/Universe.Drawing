rd /q /s Universe.Drawing.Tests\bin
for /f %%i in ('datetime local') do set datetime=%%i
"C:\Program Files\7-Zip\7zG.exe" a -t7z -mx=9 -mfb=128 -md=128m -ms=on -xr!.git ^
  "C:\Users\Backups on Google Drive\Universe.Drawing (%datetime%).7z" .
