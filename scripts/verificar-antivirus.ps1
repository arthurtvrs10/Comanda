<# RNF09/CT21: verificar o instalador final antes de gerar hash e publicar. #>
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$Caminho)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Os cmdlets Defender usam CIM nativo do Windows PowerShell.
if ($PSVersionTable.PSEdition -eq 'Core') {
    & "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -File $PSCommandPath -Caminho $Caminho
    if ($LASTEXITCODE -ne 0) { throw 'Verificacao de antivirus falhou. Nao publique o instalador.' }
    return
}
$arquivo = (Get-Item -LiteralPath $Caminho -ErrorAction Stop)
if ($arquivo.PSIsContainer) { throw 'Informe o arquivo do instalador.' }
$status = Get-MpComputerStatus
if (-not $status.AntivirusEnabled -or -not $status.RealTimeProtectionEnabled) {
    throw 'A verificacao exige Microsoft Defender ativo. Nao publique sem uma analise de seguranca.'
}
$plataformas = Join-Path $env:ProgramData 'Microsoft\Windows Defender\Platform'
$mp = Get-ChildItem -LiteralPath $plataformas -Directory -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | ForEach-Object { Join-Path $_.FullName 'MpCmdRun.exe' } |
    Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $mp) { $mp = Join-Path $env:ProgramFiles 'Windows Defender\MpCmdRun.exe' }
if (-not (Test-Path -LiteralPath $mp)) { throw 'Microsoft Defender nao encontrado.' }

$antes = (Get-FileHash -LiteralPath $arquivo.FullName -Algorithm SHA256).Hash
$inicio = Get-Date
& $mp -Scan -ScanType 3 -File $arquivo.FullName
$codigo = $LASTEXITCODE
# MpCmdRun pode retornar zero quando removeu uma ameaca: conferir tambem
# o historico e a integridade do arquivo, sem restaurar ou liberar bloqueios.
$historico = @(Get-MpThreatDetection | Where-Object {
    ($_.InitialDetectionTime -ge $inicio.AddSeconds(-1) -or $_.LastThreatStatusChangeTime -ge $inicio.AddSeconds(-1)) -and
    @($_.Resources | Where-Object { $_ -match [regex]::Escape($arquivo.FullName) }).Count -gt 0
})
if ($codigo -ne 0 -or $historico.Count -gt 0 -or -not (Test-Path -LiteralPath $arquivo.FullName)) {
    throw 'O antivirus sinalizou ou nao conseguiu verificar o instalador. Nao publique.'
}
$depois = (Get-FileHash -LiteralPath $arquivo.FullName -Algorithm SHA256).Hash
if ($antes -ne $depois) { throw 'Arquivo alterado durante a verificacao. Nao publique.' }
[pscustomobject]@{ Arquivo=$arquivo.Name; SHA256=$depois; Defender=$status.AMProductVersion; Definicoes=$status.AntivirusSignatureVersion; Resultado='Nenhuma deteccao nesta verificacao' } | ConvertTo-Json -Compress
