[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$PastaPublicacaoValida)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Publicacao-Guardas.ps1')
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$testesBase=Join-Path $repo 'artifacts/testes-publicacao'
$teste=Join-Path $testesBase ([guid]::NewGuid().ToString('N'))
if(Test-Path -LiteralPath $teste){throw 'Use uma pasta de teste nova; não sobrescrever testes anteriores automaticamente.'}
New-Item -ItemType Directory -Force $teste | Out-Null
$commit=(git -C $repo rev-parse refs/heads/codex/release-atual).Trim()
$nomes=@('windows/src/VarthexComanda.Desktop/Atendimento/AtendimentoView.xaml','windows/src/VarthexComanda.Desktop/Rede/JanelaServidor.xaml','windows/src/VarthexComanda.Servidor/EnderecosServidor.cs','windows/src/VarthexComanda.Desktop/Backup/PastasOneDrive.cs','windows/src/VarthexComanda.Infrastructure/Persistence/Migrations/20261029190000_AddTipoPedido.cs','windows/src/VarthexComanda.Desktop/VarthexComanda.Desktop.csproj')
foreach($nome in $nomes){$alvo=Join-Path $teste $nome;New-Item -ItemType Directory -Force (Split-Path $alvo) | Out-Null;Copy-Item -LiteralPath (Join-Path $repo $nome) -Destination $alvo}
foreach($nome in @('scripts','demo','demo-restaurante')){New-Item -ItemType Directory -Force (Join-Path $teste $nome)|Out-Null}
function Falha([string]$Nome,[scriptblock]$Acao){$falhou=$false;try{& $Acao | Out-Null}catch{$falhou=$true};if(-not $falhou){throw "Proteção falhou: $Nome"};Write-Host "PASSOU: $Nome"}
$versao=Get-VersaoPublicacao $teste
$manifesto=[ordered]@{Versao=$versao;Commit=$commit;Repositorio=$repo;Arquivos=(Get-MapaFonte $teste)}
$manifesto | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $teste 'fonte-publicacao.json') -Encoding UTF8
Assert-FontePublicacao $teste | Out-Null;Write-Host 'PASSOU: fonte íntegra aceita'
$arquivo=Join-Path $teste $nomes[0];$original=Get-Content $arquivo -Raw
Set-Content $arquivo 'ResponsavelPedido' -Encoding UTF8
Falha 'campo aprovado removido' {Assert-FontePublicacao $teste}
[IO.File]::WriteAllText($arquivo,$original.TrimEnd()+[Environment]::NewLine,[Text.UTF8Encoding]::new($true))
# Restore exact bytes before the next isolated mutation.
Copy-Item -LiteralPath (Join-Path $repo $nomes[0]) -Destination $arquivo -Force
Add-Content $arquivo '<!-- alteração depois da preparação -->'
Falha 'fonte alterada' {Assert-FontePublicacao $teste}
Copy-Item -LiteralPath (Join-Path $repo $nomes[0]) -Destination $arquivo -Force
Set-Content (Join-Path $teste 'windows/src/extra.cs') '// inesperado'
Falha 'arquivo não identificado adicionado' {Assert-FontePublicacao $teste}
Remove-Item -LiteralPath (Join-Path $teste 'windows/src/extra.cs')
$manifesto.Commit='0'*40;$manifesto | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $teste 'fonte-publicacao.json') -Encoding UTF8
Falha 'commit antigo' {Assert-FontePublicacao $teste}
$manifesto.Commit=$commit;$manifesto | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $teste 'fonte-publicacao.json') -Encoding UTF8
Falha 'workspace sem manifesto' {Assert-FontePublicacao $repo}
$publicacao=Join-Path $teste 'pacote';New-Item -ItemType Directory -Force $publicacao|Out-Null
foreach($nome in @('VarthexComanda.exe','VarthexComanda.dll','VarthexComanda.Domain.dll','VarthexComanda.Application.dll','VarthexComanda.Infrastructure.dll','VarthexComanda.Servidor.dll')){Copy-Item -LiteralPath (Join-Path $PastaPublicacaoValida $nome) -Destination $publicacao}
$fonte=[pscustomobject]@{Versao=$versao;Commit=$commit}
Write-ProvenienciaPublicacao $publicacao $fonte
Assert-PublicacaoPronta $publicacao $fonte;Write-Host 'PASSOU: pacote identificado aceito'
$fonte.Commit='f'*40;Falha 'pacote de outro commit' {Assert-PublicacaoPronta $publicacao $fonte};$fonte.Commit=$commit
Add-Content (Join-Path $publicacao 'VarthexComanda.dll') 'alterado'
Falha 'binário alterado' {Assert-PublicacaoPronta $publicacao $fonte}
$resolvido=(Resolve-Path -LiteralPath $teste).Path
if(-not $resolvido.StartsWith([IO.Path]::GetFullPath($testesBase)+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Pasta de teste fora do destino permitido.'}
Remove-Item -LiteralPath $resolvido -Recurse -Force
Write-Host '9 verificações de publicação aprovadas. Cópias dos binários de teste removidas.'
