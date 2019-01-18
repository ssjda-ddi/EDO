#define MyAppName 'Easy DDI Organizer'
#define MyAppVersion GetFileVersion('..\EDO\bin\Release\EDO.exe')
#define MyAppProductVersion GetStringFileInfo('..\EDO\bin\Release\EDO.exe', 'ProductVersion')
#define MyAppPublisher 'SSJDA'

[Setup]
OutputDir=out
SourceDir=.
AppendDefaultDirName = no
AppName={#MyAppName}
AppVerName={#MyAppName} {#MyAppProductVersion}
AppVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
VersionInfoProductTextVersion={#MyAppProductVersion}
OutputBaseFilename=EDO-{#MyAppProductVersion}
AppPublisher={#MyAppPublisher}
AppCopyright={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\EDO.exe
Compression=lzma2
SolidCompression=yes

; downloading and installing dependencies will only work if the memo/ready page is enabled (default and current behaviour)
DisableReadyPage=no
DisableReadyMemo=no

; supported languages
#include "scripts\lang\english.iss"
#include "scripts\lang\japanese.iss"

[Files]
Source: "..\EDO\bin\Release\EDO.exe"; DestDir: "{app}"
Source: "..\EDO\bin\Release\EDO.exe.config"; DestDir: "{app}"
Source: "..\EDO\bin\Release\SpssLib.dll"; DestDir: "{app}"
Source: "..\EDO\bin\Release\WPFLocalization.dll"; DestDir: "{app}"
Source: "..\EDO\bin\Release\en\EDO.resources.dll"; DestDir: "{app}\en"
Source: "..\EDO\DDI\*"; Destdir: "{app}\DDI"; Flags: recursesubdirs

[Icons]
Name: "{group}\Easy DDI Organizer"; Filename: "{app}\EDO.exe"
Name: "{group}\Uninstall Easy DDI Organizer"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\EDO.exe"; Description: "Easy DDI Organizer‚ð‹N“®"; Flags: nowait postinstall skipifsilent

[CustomMessages]
DependenciesDir=MyProgramDependencies

#include "scripts\products.iss"

#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"


#include "scripts\products\dotnetfx35sp1.iss"
#include "scripts\products\dotnetfx47.iss"
#include "scripts\products\openxmlsdk20.iss"


[UninstallDelete]
Type: files ; Name: "{app}\DocumentFormat.OpenXml.dll"
Type: files ; Name: "{app}\RibbonControlsLibrary.dll"
Type: files ; Name: "{app}\Microsoft.Windows.Shell.dll"

[Code]
function InitializeSetup(): boolean;
begin
  dotnetfx35sp1();
  dotnetfx47(70);
  openxmlsdk20();
  Result := true;
end;

procedure CopyDLLFiles();
var
  FromPath: String;
  ToPath: String;
begin
  FromPath := ExpandConstant('{pf}{\}') + 'Open XML SDK\V2.0\lib\DocumentFormat.OpenXml.dll';
  ToPath := ExpandConstant('{app}{\}') + 'DocumentFormat.OpenXml.dll';
  FileCopy(FromPath, ToPath, True);
end;


procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) then 
  begin
    CopyDLLFiles();
  end;
end;

