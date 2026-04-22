set sign=false  

if %1 == Release2019 set sign=true
if %1 == Release2020 set sign=true
if %1 == Release2021 set sign=true
if %1 == Release2022 set sign=true
if %1 == Release2023 set sign=true
if %1 == Release2024 set sign=true

if %1 == Publish2019 set sign=true
if %1 == Publish2020 set sign=true
if %1 == Publish2021 set sign=true
if %1 == Publish2022 set sign=true
if %1 == Publish2023 set sign=true
if %1 == Publish2024 set sign=true

if %sign%==true "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /t "http://timestamp.digicert.com" /fd SHA256 /f "C:\Users\jroth\source\Certs\JRCert.pfx" /p "mei" %3

if %1 == Release2019 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R19"
if %1 == Release2020 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R20"
if %1 == Release2021 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R21"
if %1 == Release2022 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R22"
if %1 == Release2023 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R23"
if %1 == Release2024 copy %2 "%AppData%\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\R24"

if %1 == Publish2019 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R19"
if %1 == Publish2020 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R20"
if %1 == Publish2021 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R21"
if %1 == Publish2022 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R22"
if %1 == Publish2023 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R23"
if %1 == Publish2024 copy %2 "Z:\Revit\Proficient\Proficient.bundle\Contents\R24"

