;--------------------------------
;Includes

  !include "MUI2.nsh"
  !include "InstallOptions.nsh"
  !include "fileassoc.nsh"
  !include "LogicLib.nsh"
  !include "x64.nsh"
  !include "WinVer.nsh"
  !include "DotNetVer.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Open Source Automation"
  OutFile "OSA Setup v0.4.8.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES64\OSA"
  
;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING  

  CRCCheck on
  XPStyle on 
  
  ShowInstDetails show
  
  BrandingText "OSA Installer"
  
  

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !define MUI_FINISHPAGE_RUN_TEXT "Thank you for installing Open Source Automation."
  !insertmacro MUI_PAGE_FINISH
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Install Types
  InstType "Server"
  InstType "Client"
  InstType "UIs"
  InstType /NOCUSTOM

;Installer Sections
 
; These are the programs that are needed by OSA.
Section -Prerequisites
  SectionIn 1 2 3
  SetOutPath $INSTDIR
  ${If} ${HasDotNet4.0}
    ${If} ${DOTNETVER_4_0} HasDotNetFullProfile 1
      DetailPrint "Microsoft .NET Framework 4.0 (Full Profile) available."
    ${Else}
      File "dotNetFx40_Full_setup.exe"
      ExecWait "$INSTDIR\dotNetFx40_Full_setup.exe /q /norestart"
    ${EndIf}    
  ${Else}
    File "dotNetFx40_Full_setup.exe"
    ExecWait "$INSTDIR\dotNetFx40_Full_setup.exe /q /norestart"
  ${EndIf}         
  ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x64" 'Installed'
  ${If} $0 == 1
    DetailPrint "VC++ 2011 Redist. already installed"
  ${Else}
  DetailPrint $0
    File "vc_redist.x64.exe"
    ExecWait "$INSTDIR\vc_redist.x64.exe /q"
    Delete "$INSTDIR\vc_redist.x64.exe"
    Goto endVC  
  ${EndIf}
  endVC:
  
  Delete "$INSTDIR\dotNetFx40_Full_x86_x64.exe"
   
SectionEnd

Section Server s1
  SectionIn 1
  
  SimpleSC::StopService "OSAE" 1 30
  
  SetOutPath $INSTDIR
  
  SimpleSC::ExistsService "MySQL"
  Pop $0
  SimpleSC::ExistsService "MySQL56"
  Pop $1
  SimpleSC::ExistsService "MySQL57"
  Pop $2
  
  ${If} $0 != 0
  ${AndIf} $1 != 0
  ${AndIf} $2 != 0
    MessageBox MB_OK "*** Sorry! You MUST Install MySQL Before Installing OSA ***" IDOK lbl_ok1 
    lbl_ok1:
      GoTo lblDone
  ${Else}
    DetailPrint "MySql is already installed, Great!"
    SimpleSC::GetServiceStatus "MyService"
    Pop $0 ; returns an errorcode (<>0) otherwise success (0)
    Pop $1 ; return the status of the service (See "service_status" in the parameters)

  ${EndIf}
 
  endMysql: 
  
  SetOutPath "$INSTDIR"  
  File "..\DB\osae.sql"
  File "..\DB\0.4.6-0.4.7.sql"
  File "..\DB\0.4.7-0.4.8.sql"
  File "MySql.Data.dll"
  File "DBInstall\DBInstall\bin\Debug\DBInstall.exe"
  ExecWait 'DBInstall.exe "$INSTDIR" "Server"'
  Goto endDBInstall
  endDBInstall:   
  Delete "DBInstall.exe"
  
  SetRegView 64 
  
  
  SetOutPath "$INSTDIR"
  Delete "..\output\OSAE Manager.exe"
  Delete "..\output\OSAE Manager.exe.config"
  
  File "..\output\ICSharpCode.SharpZipLib.dll"
  File "..\output\NetworkCommsDotNetComplete.dll"
  File "..\output\log4net.dll"
  File "..\output\log4net.xml"
  File "..\output\MjpegProcessor.dll"
  File "..\output\OSAE.UI.Controls.dll"
  File "..\output\OSA.png"
  File "..\output\OSAE.Manager.exe"
  File "..\output\OSAE.Manager.exe.config"
  File "..\output\OSAE.api.dll"
  File "..\output\OSAE.api.dll.config"
  File "..\output\OSAE.Screens.exe"
  File "..\output\OSAE.Screens.exe.config"
  File "..\output\OSAEService.exe"
  File "..\output\OSAE.VR.exe"
  File "..\output\OSAEService.exe.config"
  File "..\output\ClientService.exe"
  File "..\output\ClientService.exe.config"
  File "..\output\PluginDescriptionEditor.exe"
  File "..\output\PluginDescriptionEditor.exe.config"
  File "..\output\UserControlDescriptionEditor.exe"
  File "..\output\UserControlDescriptionEditor.exe.config"
  CreateDirectory "$APPDATA\Logs"
  CreateDirectory "$INSTDIR\UserControls"
  CreateDirectory "$INSTDIR\UserControls\Weather Control"
  SetOutPath "$INSTDIR\UserControls\Weather Control"
  File "..\output\UserControls\Weather Control\install.sql"
  File "..\output\UserControls\Weather Control\Screenshot.jpg"
  File "..\output\UserControls\Weather Control\Weather.osaud"
  File "..\output\UserControls\Weather Control\Weather_Control.dll"

  CreateDirectory "$INSTDIR\UserControls\MyStateButton"
  SetOutPath "$INSTDIR\UserControls\MyStateButton"
  File "..\output\UserControls\MyStateButton\install.sql"
  File "..\output\UserControls\MyStateButton\Screenshot.jpg"
  File "..\output\UserControls\MyStateButton\MyStateButton.osaud"
  File "..\output\UserControls\MyStateButton\MyStateButton.dll"

  CreateDirectory "$INSTDIR\Plugins"
  SetOutPath "$INSTDIR\Plugins"

  CreateDirectory "$INSTDIR\Plugins\Bluetooth"
  CreateDirectory "$INSTDIR\Plugins\Email"
  CreateDirectory "$INSTDIR\Plugins\Jabber"
  CreateDirectory "$INSTDIR\Plugins\Network Monitor"
  CreateDirectory "$INSTDIR\Plugins\PowerShell"
  CreateDirectory "$INSTDIR\Plugins\Rest"
  CreateDirectory "$INSTDIR\Plugins\Script Processor"
  CreateDirectory "$INSTDIR\Plugins\Speech"
  CreateDirectory "$INSTDIR\Plugins\WUnderground"
  CreateDirectory "$INSTDIR\Plugins\Web Server"
  
  SetOutPath "$INSTDIR\Plugins\Bluetooth"
  File "..\output\Plugins\Bluetooth\Bluetooth.osapd"
  File "..\output\Plugins\Bluetooth\OSAE.Bluetooth.dll"
  File "..\output\Plugins\Bluetooth\InTheHand.Net.Personal.dll"
  File "..\output\Plugins\Bluetooth\Screenshot.jpg"
    
  SetOutPath "$INSTDIR\Plugins\Email"
  File "..\output\Plugins\Email\Email.osapd"
  File "..\output\Plugins\Email\OSAE.Email.dll"
  File "..\output\Plugins\Email\Screenshot.jpg"
  
  SetOutPath "$INSTDIR\Plugins\Jabber"
  File "..\output\Plugins\Jabber\Jabber.osapd"
  File "..\output\Plugins\Jabber\OSAE.Jabber.dll"
  File "..\output\Plugins\Jabber\agsXMPP.dll"
  File "..\output\Plugins\Jabber\Screenshot.jpg"
    
  SetOutPath "$INSTDIR\Plugins\Network Monitor"
  File "..\output\Plugins\Network Monitor\Network Monitor.osapd"
  File "..\output\Plugins\Network Monitor\OSAE.NetworkMonitor.dll"
  File "..\output\Plugins\Network Monitor\Screenshot.jpg"

  SetOutPath "$INSTDIR\Plugins\PowerShell"
  File "..\output\Plugins\PowerShell\PowerShell.osapd"
  File "..\output\Plugins\PowerShell\Google.GData.AccessControl.DLL"
  File "..\output\Plugins\PowerShell\Google.GData.Calendar.dll"
  File "..\output\Plugins\PowerShell\Google.GData.Client.dll"
  File "..\output\Plugins\PowerShell\Google.GData.Extensions.dll"
  File "..\output\Plugins\PowerShell\NMALib.dll"
  File "..\output\Plugins\PowerShell\OSAE.PowerShellProcessor.dll"
  File "..\output\Plugins\PowerShell\Screenshot.jpg"
  File "..\output\Plugins\PowerShell\System.Management.Automation.dll"

  SetOutPath "$INSTDIR\Plugins\Rest"
  File "..\output\Plugins\Rest\Rest.osapd"
  File "..\output\Plugins\Rest\OSAE.Rest.dll"
  File "..\output\Plugins\Rest\Screenshot.jpg"
  
  SetOutPath "$INSTDIR\Plugins\Script Processor"
  File "..\output\Plugins\Script Processor\Script Processor.osapd"
  File "..\output\Plugins\Script Processor\OSAE.ScriptProcessor.dll"
  File "..\output\Plugins\Script Processor\Screenshot.jpg"
  
  SetOutPath "$INSTDIR\Plugins\Speech"
  File "..\output\Plugins\Speech\Speech.osapd"
  File "..\output\Plugins\Speech\OSAE.Speech.dll"
  File "..\output\Plugins\Speech\Screenshot.jpg"
  
  SetOutPath "$INSTDIR\Plugins\WUnderground"
  File "..\output\Plugins\WUnderground\WUnderground.osapd"
  File "..\output\Plugins\WUnderground\WUnderground.dll"  
  File "..\output\Plugins\WUnderground\Screenshot.jpg"

  SetOutPath "$INSTDIR\Plugins\Web Server"
  File "..\output\Plugins\Web Server\Web Server.osapd"
  File "..\output\Plugins\Web Server\Screenshot.jpg"

  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot"
  File "..\output\Plugins\Web Server\wwwroot\*.*"


  ; Unregister website to make sure no files are in use by webserver while upgrading and to pick up any changes in how we register it now
  DetailPrint "Unregistering Website"
  ExecWait '"$PROGRAMFILES64\UltiDev\Web Server\UWS.RegApp.exe" /unreg /AppID:{58fe03ca-9975-4df2-863e-a228614258c4}'

  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\Bin"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\controls"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\controls\usercontrols"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\controls\usercontrols\MyStateButton"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\controls\usercontrols\WeatherControl"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\mobile"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\Images"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\css"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\js"
  
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap\css"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap\js"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap\img"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap\css"
  File "..\output\Plugins\Web Server\wwwroot\bootstrap\css\*.*"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap\js"
  File "..\output\Plugins\Web Server\wwwroot\bootstrap\js\*.*"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\bootstrap\img"
  File "..\output\Plugins\Web Server\wwwroot\bootstrap\img\*.*"
  
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\Bin"
  File "..\output\Plugins\Web Server\wwwroot\Bin\*.*"

  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\controls"
  File "..\output\Plugins\Web Server\wwwroot\controls\*.*"

  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\controls\usercontrols\MyStateButton"
  File "..\output\Plugins\Web Server\wwwroot\controls\usercontrols\MyStateButton\*.*"

  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\controls\usercontrols\WeatherControl"
  File "..\output\Plugins\Web Server\wwwroot\controls\usercontrols\WeatherControl\*.*"
  
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\Images"
  File "..\output\Plugins\Web Server\wwwroot\Images\*.*"
  
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\mobile"
  File "..\output\Plugins\Web Server\wwwroot\mobile\*.*"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\mobile\images"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\mobile\images"
  File "..\output\Plugins\Web Server\wwwroot\mobile\images\*.*"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\mobile"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\mobile\jquery"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\mobile\jquery"
  File "..\output\Plugins\Web Server\wwwroot\mobile\jquery\*.*"
  CreateDirectory "$INSTDIR\Plugins\Web Server\wwwroot\mobile\jquery\images"
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\mobile\jquery\images"
  File "..\output\Plugins\Web Server\wwwroot\mobile\jquery\images\*.*"
  
  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\css"
  File "..\output\Plugins\Web Server\wwwroot\css\*.*"

  SetOutPath "$INSTDIR\Plugins\Web Server\wwwroot\js"
  File "..\output\Plugins\Web Server\wwwroot\js\*.*"

  SimpleSC::ExistsService "UWS LoPriv Services"
  Pop $0
  
  ${If} $0 != 0
  SetOutPath $INSTDIR
    DetailPrint "Installing UltiDev Web Server Pro"
    File "UltiDev Web Server Setup.exe"
    ExecWait "$INSTDIR\UltiDev Web Server Setup.exe"
  ${EndIf} 


  ; Register the website 
  ExecWait '"$PROGRAMFILES64\UltiDev\Web Server\UWS.RegApp.exe" /r /AppId={58fe03ca-9975-4df2-863e-a228614258c4} /path:"$INSTDIR\Plugins\Web Server\wwwroot" "/EndPoints:http://*:8081/" /ddoc:default.aspx /appname:"Open Source Automation" /apphost=SharedLocalSystem /clr:4 /vpath:"/"'

  # Start Menu Shortcuts
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\OSA"
  createShortCut "$SMPROGRAMS\OSA\Manager.lnk" "$INSTDIR\OSAE.Manager.exe"
  createShortCut "$SMPROGRAMS\OSA\OSAE.Screens.lnk" "$INSTDIR\OSAE.Screens.exe"

  ${If} ${AtLeastWinVista}
    SetShellVarContext all
    ShellLink::SetRunAsAdministrator "$INSTDIR\OSAE.Manager.exe"
    ShellLink::SetRunAsAdministrator "$SMPROGRAMS\OSA\Manager.lnk"
    ShellLink::SetRunAsAdministrator "$INSTDIR\OSAE.Screens.exe"
    ShellLink::SetRunAsAdministrator "$SMPROGRAMS\OSA\OSAE.Screens.lnk"
    #Pop $0
  ${EndIf}          
  
  SimpleSC::InstallService "OSAE" "OSAE Service" "16" "2" "$INSTDIR\OSAEService.exe" "" "" ""
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "INSTALLDIR" "$INSTDIR"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBCONNECTION" "localhost"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBPORT" "3306"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBNAME" "osae"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBUSERNAME" "osae"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBPASSWORD" "osaePass"
  
  !insertmacro APP_ASSOCIATE "osapd" "OSA.osapd" "Plugin Description" "$INSTDIR\PluginDescriptionEditor.exe,0" "Open" "$INSTDIR\PluginDescriptionEditor.exe $\"%1$\""  
  !insertmacro APP_ASSOCIATE "osapp" "OSA.osapp" "Plugin Package" "$INSTDIR\OSAE Manager.exe,0" "Open" "$INSTDIR\OSAE Manager.exe $\"%1$\"" 
  !insertmacro UPDATEFILEASSOC    
  
  AccessControl::GrantOnFile "$INSTDIR" "(BU)" "GenericRead + GenericWrite"
  AccessControl::GrantOnFile "$APPDATA\Logs" "(S-1-5-32-545)" "FullAccess"
  AccessControl::GrantOnFile "$APPDATA\Logs\*" "(S-1-5-32-545)" "FullAccess"
      
  writeUninstaller $INSTDIR\uninstall.exe
  
  MessageBox MB_OKCANCEL "Open Source Automation has been successfully installed. Would you like to start the OSAE Service now?" IDOK lbl_ok  IDCANCEL lblDone
    lbl_ok:
      SimpleSC::StartService "OSAE" "" 30
      Pop $0
    GoTo lblDone
    lblDone:
    
SectionEnd

Section Client s2
  SectionIn 2
 
  SetRegView 64 
  
  SimpleSC::StopService "OSAE Client" 1 30
  
  SetOutPath "$INSTDIR"
  Delete "..\output\OSAE Manager.exe"
  Delete "..\output\OSAE Manager.exe.config"
  
  File "..\output\ICSharpCode.SharpZipLib.dll"
  File "..\output\NetworkCommsDotNetComplete.dll"
  File "..\output\log4net.dll"
  File "..\output\log4net.xml"
  File "..\output\MjpegProcessor.dll"
  File "..\output\OSAE.UI.Controls.dll"
  File "..\output\OSA.png"
  File "..\output\MySql.Data.dll"
  File "..\output\OSAE.Manager.exe"
  File "..\output\OSAE.Manager.exe.config"
  File "..\output\OSAE.api.dll"
  File "..\output\OSAE.api.dll.config"
  File "..\output\OSAE.Screens.exe"
  File "..\output\OSAE.Screens.exe.config"
  File "..\output\OSAE.VR.exe"
  File "..\output\ClientService.exe"
  File "..\output\ClientService.exe.config"
  File "..\output\PluginDescriptionEditor.exe"
  File "..\output\PluginDescriptionEditor.exe.config"
  File "..\output\UserControlDescriptionEditor.exe"
  File "..\output\UserControlDescriptionEditor.exe.config"
  File "..\output\OSAE.VR.exe"

  CreateDirectory "$INSTDIR\UserControls"
  CreateDirectory "$INSTDIR\Plugins"
  SetOutPath "$INSTDIR\Plugins"
  CreateDirectory "$INSTDIR\Plugins\Speech"
  SetOutPath "$INSTDIR\Plugins\Speech"
  File "..\output\Plugins\Speech\Speech.osapd"
  File "..\output\Plugins\Speech\OSAE.Speech.dll"
  File "..\output\Plugins\Speech\Screenshot.jpg"
  
  SetOutPath "$INSTDIR\Plugins\Bluetooth"
  File "..\output\Plugins\Bluetooth\Bluetooth.osapd"
  File "..\output\Plugins\Bluetooth\OSAE.Bluetooth.dll"
  File "..\output\Plugins\Bluetooth\InTheHand.Net.Personal.dll"
  File "..\output\Plugins\Bluetooth\Screenshot.jpg"

  # Start Menu Shortcuts
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\OSA"
  createShortCut "$SMPROGRAMS\OSA\Manager.lnk" "$INSTDIR\OSAE.Manager.exe"
  createShortCut "$SMPROGRAMS\OSA\OSAE.Screens.lnk" "$INSTDIR\OSAE.Screens.exe"

  ${If} ${AtLeastWinVista}
    SetShellVarContext all
    ShellLink::SetRunAsAdministrator "$INSTDIR\OSAE.Manager.exe"
    ShellLink::SetRunAsAdministrator "$SMPROGRAMS\OSA\Manager.lnk"
    ShellLink::SetRunAsAdministrator "$INSTDIR\OSAE.Screens.exe"
    ShellLink::SetRunAsAdministrator "$SMPROGRAMS\OSA\OSAE.Screens.lnk"
    #Pop $0
  ${EndIf}  
  
  SimpleSC::InstallService "OSAE Client" "OSAE Client Service" "16" "2" "$INSTDIR\ClientService.exe" "" "" ""
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "INSTALLDIR" "$INSTDIR"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBNAME" "osae"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBUSERNAME" "osae"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBPASSWORD" "osaePass"
  
  !insertmacro APP_ASSOCIATE "osapd" "OSA.osapd" "Plugin Description" "$INSTDIR\PluginDescriptionEditor.exe,0" "Open" "$INSTDIR\PluginDescriptionEditor.exe $\"%1$\""  
  !insertmacro APP_ASSOCIATE "osapp" "OSA.osapp" "Plugin Package" "$INSTDIR\PluginInstaller.exe,0" "Open" "$INSTDIR\OSAE Manager.exe $\"%1$\"" 
  !insertmacro UPDATEFILEASSOC    
  
  AccessControl::GrantOnFile \
    "$INSTDIR" "(BU)" "GenericRead + GenericWrite"
  
  writeUninstaller $INSTDIR\uninstall.exe
  
  SetOutPath $INSTDIR
  File "DBInstall\DBInstall\bin\Debug\DBInstall.exe"
  ExecWait 'DBInstall.exe "$INSTDIR" "Client"'
  Goto endDBInstall2
  endDBInstall2:   
  Delete "DBInstall.exe"
  
  MessageBox MB_OKCANCEL "Open Source Automation has been successfully installed. Would you like to start the OSAE Service now?" IDOK lbl_ok  IDCANCEL lblDone
    lbl_ok:
      SimpleSC::StartService "OSAE Client" "" 30
      Pop $0
    GoTo lblDone
    lblDone:
SectionEnd


Section UIOnly s3
  SectionIn 3
 
  SetRegView 64 
    
  SetOutPath "$INSTDIR"
  
  File "..\output\ICSharpCode.SharpZipLib.dll"
  File "..\output\NetworkCommsDotNetComplete.dll"
  File "..\output\log4net.dll"
  File "..\output\log4net.xml"
  File "..\output\MjpegProcessor.dll"
  File "..\output\OSAE.UI.Controls.dll"
  File "..\output\OSA.png"
  File "..\output\MySql.Data.dll"
  File "..\output\OSAE.api.dll"
  File "..\output\OSAE.api.dll.config"
  File "..\output\OSAE.Screens.exe"
  File "..\output\OSAE.VR.exe"
  File "..\output\ClientService.exe"
  File "..\output\ClientService.exe.config"
  File "..\output\PluginDescriptionEditor.exe"
  File "..\output\OSAE.VR.exe"

  CreateDirectory "$INSTDIR\UserControls"
  CreateDirectory "$INSTDIR\UserControls\Weather Control"
  SetOutPath "$INSTDIR\UserControls\Weather Control"
  File "..\output\UserControls\Weather Control\install.sql"
  File "..\output\UserControls\Weather Control\Screenshot.jpg"
  File "..\output\UserControls\Weather Control\Weather.osaud"
  File "..\output\UserControls\Weather Control\Weather_Control.dll"

  CreateDirectory "$INSTDIR\UserControls\MyStateButton"
  SetOutPath "$INSTDIR\UserControls\MyStateButton"
  File "..\output\UserControls\MyStateButton\install.sql"
  File "..\output\UserControls\MyStateButton\Screenshot.jpg"
  File "..\output\UserControls\MyStateButton\MyStateButton.osaud"
  File "..\output\UserControls\MyStateButton\MyStateButton.dll"

  # Start Menu Shortcuts
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\OSA"
  createShortCut "$SMPROGRAMS\OSA\OSAE.Screens.lnk" "$INSTDIR\OSAE.Screens.exe"

  ${If} ${AtLeastWinVista}
    SetShellVarContext all
    ShellLink::SetRunAsAdministrator "$INSTDIR\OSAE.Screens.exe"
    ShellLink::SetRunAsAdministrator "$SMPROGRAMS\OSA\OSAE.Screens.lnk"
    #Pop $0
  ${EndIf}  
  
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "INSTALLDIR" "$INSTDIR"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBNAME" "osae"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBUSERNAME" "osae"
  WriteRegStr HKLM "SOFTWARE\OSAE\DBSETTINGS" "DBPASSWORD" "osaePass"
  
  !insertmacro APP_ASSOCIATE "osapd" "OSA.osapd" "Plugin Description" "$INSTDIR\PluginDescriptionEditor.exe,0" "Open" "$INSTDIR\PluginDescriptionEditor.exe $\"%1$\""  
  !insertmacro APP_ASSOCIATE "osapp" "OSA.osapp" "Plugin Package" "$INSTDIR\PluginInstaller.exe,0" "Open" "$INSTDIR\OSAE Manager.exe $\"%1$\"" 
  !insertmacro UPDATEFILEASSOC    
  
  AccessControl::GrantOnFile \
    "$INSTDIR" "(BU)" "GenericRead + GenericWrite"
  
  writeUninstaller $INSTDIR\uninstall.exe
SectionEnd


;--------------------------------
;Uninstaller Section

Section "Uninstall"
  SectionIn 1 2

  Delete "$INSTDIR\Uninstall.exe"
  Delete "$SMPROGRAMS\OSA\Manager.lnk"
  Delete "$SMPROGRAMS\OSA\Uninstall.lnk"
  Delete "$SMPROGRAMS\OSA\OSAE.Screens.lnk"
  Delete "$SMPROGRAMS\OSA\DevTools.lnk"
  RMDir /r "$SMPROGRAMS\OSA"
  !insertmacro APP_UNASSOCIATE "osapd" "OSA.osapd"
  !insertmacro APP_UNASSOCIATE "osapp" "OSA.osapp"
  Delete $INSTDIR\*.*
  RMDir /r $INSTDIR
  SimpleSC::RemoveService "OSAE"
  Pop $0
  SimpleSC::RemoveService "OSAE Client"
  Pop $1

SectionEnd


!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${s1} "Install the Open Source Automation server.  Install this on your main machine"
  !insertmacro MUI_DESCRIPTION_TEXT ${s2} "Client serivce for Open Source Automation.  Install on client machines."
!insertmacro MUI_FUNCTION_DESCRIPTION_END