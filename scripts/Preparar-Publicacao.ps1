[CmdletBinding()]
param([switch]$SomentePreparar,[string]$Compilador)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Publicacao-Guardas.ps1')
$repo=(Resolve-Path -LiteralPath (Split-Path $PSScriptRoot -Parent)).Path
$commit=(git -C $repo rev-parse refs/heads/codex/release-atual)
if($LASTEXITCODE -ne 0){throw 'Fonte única ausente: configure codex/release-atual com o último código aprovado.'}
$commit=$commit.Trim()
$ultima=git -C $repo tag --list 'v[0-9]*' --sort=-version:refname | Where-Object {$_ -match '^v\d+\.\d+\.\d+$'} | Select-Object -First 1
if($ultima){git -C $repo merge-base --is-ancestor "$ultima^{}" $commit;if($LASTEXITCODE -ne 0){throw "Fonte antiga: codex/release-atual não contém a última release $ultima. Integre os ajustes atuais antes de publicar."}}
$base=[IO.Path]::GetFullPath((Join-Path $repo 'artifacts/release-atual'))
$fonte=Join-Path $base 'fonte'
New-Item -ItemType Directory -Force $base | Out-Null
foreach($pasta in @((Join-Path $repo 'artifacts'),$base)){if((Get-Item -LiteralPath $pasta).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Destino gerado não pode ser um link de diretório.'}}
if(Test-Path -LiteralPath $fonte){
    $resolvido=(Resolve-Path -LiteralPath $fonte).Path
    if(-not $resolvido.StartsWith($base+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or -not(Test-Path -LiteralPath (Join-Path $fonte 'fonte-publicacao.json'))){throw 'Recusado substituir uma pasta sem identificação de fonte gerada.'}
    if(Get-ChildItem -LiteralPath $fonte -Recurse -Attributes ReparsePoint -ErrorAction SilentlyContinue){throw 'A fonte gerada contém links de diretório. Remova-os manualmente antes de continuar.'}
    Remove-Item -LiteralPath $resolvido -Recurse -Force
}
$zip=Join-Path $base 'fonte.zip'
git -C $repo archive $commit --format=zip -o $zip
if($LASTEXITCODE -ne 0){throw 'Falha ao exportar o commit atual.'}
Expand-Archive -LiteralPath $zip -DestinationPath $fonte
$versao=Get-VersaoPublicacao $fonte
if($ultima -and [version]$versao -lt [version]$ultima.Substring(1)){throw 'A versão da fonte é anterior à última release.'}
Assert-ContratoAtual $fonte
[ordered]@{Versao=$versao;Commit=$commit;Repositorio=$repo;Arquivos=(Get-MapaFonte $fonte)} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $fonte 'fonte-publicacao.json') -Encoding UTF8
Write-Host "Fonte única: codex/release-atual · $commit · versão $versao"
if($SomentePreparar){Write-Host "Fonte preparada em $fonte";return}
$saida=Join-Path $base 'pacotes'
& (Join-Path $fonte 'scripts/gerar-instalador.ps1') -Saida $saida -Compilador $Compilador
if($LASTEXITCODE -ne 0){throw 'Falha ao gerar o instalador.'}
foreach($perfil in @('Lanchonete','Restaurante')){& (Join-Path $fonte 'scripts/gerar-demo.ps1') -Saida $saida -PublicacaoExistente (Join-Path $saida 'publish/win-x64') -Perfil $perfil -Compilador $Compilador;if($LASTEXITCODE -ne 0){throw "Falha ao gerar $perfil"}}
Write-Host "Pacotes atuais em $saida. Confira testes e hashes antes de enviar à release."
