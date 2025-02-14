; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Warpinator"
#define MyAppVersion "1.0"
#define MyAppPublisher "slowscript"
#define MyAppURL "https://github.com/slowscript/warpinator-windows"
#define MyAppExeName "Warpinator.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{46B90016-8ABB-4BE2-9B04-C8FFA56BE7FB}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
;AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline
OutputBaseFilename=warpinator-setup_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
;UninstallDisplayName=Warpinator
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "sendtointegration"; Description: "Add to Send-to menu"; GroupDescription: "Desktop integration"

[Files]
Source: "Warpinator\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\BouncyCastle.Cryptography.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Common.Logging.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Common.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Google.Protobuf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Grpc.Core.Api.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Grpc.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\grpc_csharp_ext.x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\grpc_csharp_ext.x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Makaretu.Dns.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Makaretu.Dns.Multicast.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\NaCl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\QRCoder.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\SimpleBase.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\System.Net.IPNetwork.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\Warpinator.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\zlib.net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Warpinator\bin\Release\cs\Warpinator.resources.dll"; DestDir: "{app}\cs"; Flags: ignoreversion
Source: "Warpinator\bin\Release\de\Warpinator.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "Warpinator\bin\Release\es\Warpinator.resources.dll"; DestDir: "{app}\es"; Flags: ignoreversion
Source: "Warpinator\bin\Release\it\Warpinator.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "Warpinator\bin\Release\zh-CN\Warpinator.resources.dll"; DestDir: "{app}\zh-CN"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKA; Subkey: "Software\Classes\warpinator"; ValueType: "string"; ValueData: "URL:warpinator"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\warpinator"; ValueType: "string"; ValueName: "URL Protocol"; ValueData: ""
Root: HKA; Subkey: "Software\Classes\warpinator\DefaultIcon"; ValueType: "string"; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\warpinator\shell\open\command"; ValueType: "string"; ValueData: """{app}\{#MyAppExeName}"" -ConnectTo ""%1"""

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{usersendto}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: sendtointegration
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
