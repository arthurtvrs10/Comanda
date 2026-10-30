#ifndef AppVersion
  #error AppVersion obrigatoria
#endif

[Setup]
AppId={{29F04F80-42DE-4E8F-8996-3DC6EAA147A8}
AppName=Comanda Demonstração Restaurante
AppVersion={#AppVersion}
AppPublisher=Varthex
DefaultDirName={localappdata}\Programs\VarthexComandaDemoRestaurante
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir={#OutputPath}
OutputBaseFilename=VarthexComanda-Demo-Restaurante-{#AppVersion}-Setup-win-x64
SetupIconFile=..\backend\src\VarthexComanda.Desktop\Assets\VarthexComanda.ico
UninstallDisplayIcon={app}\VarthexComanda.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
UninstallDisplayName=Comanda Demonstração Restaurante

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho Comanda Demonstração Restaurante"; GroupDescription: "Atalhos:"

[Files]
Source: "{#PublishPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\demo-restaurante\catalogo.json"; DestDir: "{app}\demo-restaurante"; Flags: ignoreversion
Source: "..\demo-restaurante\README.md"; DestDir: "{app}\demo-restaurante"; Flags: ignoreversion
Source: "..\demo-restaurante\fotos\*.png"; DestDir: "{app}\demo-restaurante\fotos"; Flags: ignoreversion
Source: "..\demo-restaurante\edicao-demonstracao-restaurante.txt"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\demo-restaurante\fontes.json"; DestDir: "{app}\demo-restaurante"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\Comanda Demonstração Restaurante"; Filename: "{app}\VarthexComanda.exe"; Parameters: "--demonstracao-restaurante"
Name: "{userdesktop}\Comanda Demonstração Restaurante"; Filename: "{app}\VarthexComanda.exe"; Parameters: "--demonstracao-restaurante"; Tasks: desktopicon

[Run]
Filename: "{app}\VarthexComanda.exe"; Parameters: "--demonstracao-restaurante"; Description: "Abrir Comanda Demonstração Restaurante"; Flags: nowait postinstall skipifsilent
