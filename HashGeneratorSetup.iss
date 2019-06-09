[Setup]
AppId={{FC8E5633-9325-4D18-8BE6-9B4BE3A10A48}
SetupMutex=Global\FC8E5633-9325-4D18-8BE6-9B4BE3A10A48
AppMutex=Global\AA679F6E-D860-47B8-B8C4-733FE6B107DF
AppCopyright=Copyright (c) 2019 Philippe Coulombe
AppPublisher=Philippe Coulombe
AppVersion=2.3.0.0
VersionInfoVersion=2.3.0.0
AppVerName=Hash Generator 2.3
AppName=Hash Generator
DefaultDirName={commonpf}\Hash Generator
UninstallDisplayIcon={app}\HashGenerator.exe
OutputBaseFilename=HashGeneratorSetup
OutputDir=.
LicenseFile=LICENSE
DisableProgramGroupPage=yes
DisableDirPage=yes
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=6.1.7601
WizardSizePercent=120,100

[Files]
Source: "LICENSE"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "HashGenerator\bin\x64\Release\CsharpHelpers.dll"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "HashGenerator\bin\x64\Release\HashGenerator.exe"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "HashGenerator\bin\x64\Release\HashGenerator.exe.config"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "HashGenerator\bin\x64\Release\System.Windows.Interactivity.dll"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion
Source: "x64\Release\HashGeneratorExt.dll"; DestDir: {app}; Flags: restartreplace uninsrestartdelete ignoreversion regserver

[Icons]
Name: "{commonprograms}\Hash Generator"; Filename: "{app}\HashGenerator.exe"
Name: "{commondesktop}\Hash Generator"; Filename: "{app}\HashGenerator.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; Flags: unchecked

[Run]
Filename: "{app}\HashGenerator.exe"; Description: "{cm:LaunchProgram,Hash Generator}"; Flags: nowait postinstall skipifsilent unchecked

[Code]
procedure InitializeWizard();
begin
    WizardForm.LicenseMemo.Font.Name := 'Consolas';
end;
