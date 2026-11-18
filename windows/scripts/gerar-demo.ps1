<# Gera a edição com catálogo fictício (RF03, RNF09, CT21). #>
[CmdletBinding()]
param([string]$Compilador, [string]$Saida, [string]$PublicacaoExistente,[ValidateSet("Lanchonete","Restaurante")][string]$Perfil="Lanchonete")
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'Publicacao-Guardas.ps1')
$fonteConfirmada=Assert-FontePublicacao $raiz
$versao = (Select-Xml -Path "$raiz/windows/src/VarthexComanda.Desktop/VarthexComanda.Desktop.csproj" -XPath '//Version').Node.InnerText.Trim()
$saida = if ($Saida) { $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Saida) } else { Join-Path $raiz "artifacts/demo-$versao" }
$publicacao = Join-Path $saida 'publish/win-x64'
if (-not $Compilador) {
    $Compilador = @((Join-Path $raiz 'artifacts/tools/InnoSetup7/ISCC.exe'), (Join-Path $raiz 'artifacts/tools/InnoSetup/ISCC.exe'), "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe", "$env:ProgramFiles\Inno Setup 7\ISCC.exe") |
        Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $Compilador -or -not (Test-Path -LiteralPath $Compilador)) { throw 'Informe o caminho do Inno Setup em -Compilador.' }
if ($PublicacaoExistente) {
    $publicacao = (Resolve-Path -LiteralPath $PublicacaoExistente).Path
    if (-not (Test-Path -LiteralPath (Join-Path $publicacao 'VarthexComanda.exe'))) { throw 'Publicação existente sem executável.' }
} else {
    & (Join-Path $PSScriptRoot 'publicar.ps1') -Saida $saida
    if ($LASTEXITCODE -ne 0) { throw 'Publicação falhou.' }
}
Assert-PublicacaoPronta $publicacao $fonteConfirmada
# O instalador demo adiciona apenas o catálogo e o marcador da edição. Nunca inclui dados locais.
$indevidos = Get-ChildItem -LiteralPath $publicacao -File -Recurse | Where-Object {
    $_.Extension -in '.db','.sqlite','.sqlite3','.credencial' -or $_.Name -match 'google-oauth-client|google-drive-token|estado\.dat|colaboradores\.json|edicao-demonstracao\.txt'
}
if ($indevidos) { throw 'Publicação contém dados locais ou marcador de edição. Gere uma publicação limpa.' }
[void](New-Item -ItemType Directory -Force -Path $saida)
& $Compilador "/DAppVersion=$versao" "/DPublishPath=$publicacao" "/DOutputPath=$saida" (Join-Path $PSScriptRoot $(if($Perfil -eq "Restaurante"){"instalador-demo-restaurante.iss"}else{"instalador-demo.iss"}))
if ($LASTEXITCODE -ne 0) { throw 'Compilação do instalador demo falhou.' }
$prefixo = if($Perfil -eq "Restaurante"){"VarthexComanda-Demo-Restaurante"}else{"VarthexComanda-Demo"}
$instalador = Join-Path $saida "$prefixo-$versao-Setup-win-x64.exe"
& (Join-Path $PSScriptRoot 'verificar-antivirus.ps1') -Caminho $instalador
$hash = (Get-FileHash -LiteralPath $instalador -Algorithm SHA256).Hash
"$hash  $([IO.Path]::GetFileName($instalador))" | Set-Content -LiteralPath "$instalador.sha256" -Encoding ascii
Write-Host "Instalador de demonstração: $instalador"
