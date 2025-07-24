set sign=false  

if %1 == Release2025 set sign=true

if %1 == Publish2025 set sign=true

if %sign%==true "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /t "http://timestamp.digicert.com" /fd SHA256 /f "C:\Users\jroth\source\Certs\JRCert.pfx" /p "mei" %2

if %1 == Release2025 copy %2 "C:\ProgramData\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R25"

if %1 == Publish2025 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R25"

