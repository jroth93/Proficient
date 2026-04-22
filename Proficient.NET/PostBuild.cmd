set sign=false  

if %1 == Release2025 set sign=true
if %1 == Release2026 set sign=true
if %1 == Release2027 set sign=true

if %1 == Publish2025 set sign=true
if %1 == Publish2026 set sign=true
if %1 == Publish2027 set sign=true

if %sign%==true "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /t "http://timestamp.digicert.com" /fd SHA256 /f "C:\Users\jroth\source\Certs\JRCert.pfx" /p "mei" %3

if exist "%2\System.*.dll" del "%2\System.*.dll" /Q
if exist "%2\Microsoft.*.dll" del "%2\Microsoft.*.dll" /Q

if %1 == Release2025 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R25"
if %1 == Release2026 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R26"
if %1 == Release2027 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R27"

if %1 == Publish2025 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R25"
if %1 == Publish2026 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R26"
if %1 == Publish2027 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R27"


