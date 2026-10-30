Set-StrictMode -Version Latest

function Get-VersaoPublicacao([string]$Raiz) {
    $xml=[xml](Get-Content -LiteralPath (Join-Path $Raiz 'backend/src/VarthexComanda.Desktop/VarthexComanda.Desktop.csproj') -Raw)
    $versao=($xml.SelectSingleNode("//Version")).InnerText
    $versao=$versao.Trim()
    if($versao -notmatch '^\d+\.\d+\.\d+$'){throw 'Versão de publicação inválida.'}
    foreach($campo in @('AssemblyVersion','FileVersion','InformationalVersion')) {
        $valor=($xml.SelectSingleNode("//$campo")).InnerText
        $esperado=if($campo -eq 'InformationalVersion'){$versao}else{"$versao.0"}
        if($valor -ne $esperado){throw "Metadados de versão divergentes: $campo. Corrija antes de publicar."}
    }
    return $versao
}
function Get-MapaFonte([string]$Raiz) {
    $mapa=[ordered]@{}
    foreach($pasta in @('backend/src','scripts','demo','demo-restaurante')) {
        Get-ChildItem -LiteralPath (Join-Path $Raiz $pasta) -File -Recurse | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Sort-Object FullName | ForEach-Object {
            $relativo=[IO.Path]::GetRelativePath($Raiz,$_.FullName).Replace('\','/')
            $mapa[$relativo]=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    }
    return $mapa
}
function Assert-ContratoAtual([string]$Raiz) {
    $tela=Get-Content -LiteralPath (Join-Path $Raiz 'backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoView.xaml') -Raw
    foreach($nome in @('ClientePedidoTexto','TipoPedidoSelecionado','ObservacaoPedido')) {if($tela -notmatch ('x:Name="'+$nome+'"')){throw "A fonte perdeu um campo aprovado: $nome"}}
    if($tela -match 'x:Name="ResponsavelPedido"|<Button Tag="cliente"'){throw 'A fonte reintroduziu Responsável manual ou Vincular cliente.'}
    $servidor=Get-Content -LiteralPath (Join-Path $Raiz 'backend/src/VarthexComanda.Desktop/Rede/JanelaServidor.xaml') -Raw
    if($servidor -notmatch 'x:Name="LinkCompleto"'){throw 'A fonte perdeu o link completo do QR Code.'}
    foreach($caminho in @('backend/src/VarthexComanda.Servidor/EnderecosServidor.cs','backend/src/VarthexComanda.Desktop/Backup/PastasOneDrive.cs','backend/src/VarthexComanda.Infrastructure/Persistence/Migrations/20261029190000_AddTipoPedido.cs')){if(-not(Test-Path -LiteralPath (Join-Path $Raiz $caminho))){throw "A fonte perdeu uma alteração publicada: $caminho"}}
}
function Assert-FontePublicacao([string]$Raiz) {
    $arquivo=Join-Path $Raiz 'fonte-publicacao.json'
    if(-not(Test-Path -LiteralPath $arquivo)){throw 'Publicação bloqueada: use scripts/Preparar-Publicacao.ps1 na fonte principal. Não gere instaladores de cópias antigas ou do desenvolvimento.'}
    $fonte=Get-Content -LiteralPath $arquivo -Raw | ConvertFrom-Json
    $commit=git -C $fonte.Repositorio rev-parse refs/heads/codex/release-atual
    if($LASTEXITCODE -ne 0 -or $commit.Trim() -ne $fonte.Commit){throw 'A fonte de publicação ficou desatualizada. Execute Preparar-Publicacao novamente.'}
    if((Get-VersaoPublicacao $Raiz) -ne $fonte.Versao){throw 'Versão diferente da fonte preparada.'}
    Assert-ContratoAtual $Raiz
    $atual=Get-MapaFonte $Raiz
    $esperado=@($fonte.Arquivos.PSObject.Properties)
    if($atual.Count -ne $esperado.Count){throw 'Arquivos adicionados ou removidos da fonte preparada. Commite na fonte atual e prepare novamente.'}
    foreach($item in $esperado){if(-not $atual.Contains($item.Name) -or $atual[$item.Name] -ne $item.Value){throw "Fonte alterada depois da preparação: $($item.Name). Prepare novamente a partir do commit atual."}}
    return $fonte
}
function Write-ProvenienciaPublicacao([string]$Destino,$Fonte) {
    $hashes=[ordered]@{}
    foreach($nome in @('VarthexComanda.exe','VarthexComanda.dll','VarthexComanda.Domain.dll','VarthexComanda.Application.dll','VarthexComanda.Infrastructure.dll','VarthexComanda.Servidor.dll')){$hashes[$nome]=(Get-FileHash -LiteralPath (Join-Path $Destino $nome) -Algorithm SHA256).Hash}
    [ordered]@{Versao=$Fonte.Versao;Commit=$Fonte.Commit;GeradoEmUtc=[DateTime]::UtcNow.ToString('O');Arquivos=$hashes} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Destino 'publicacao.json') -Encoding UTF8
}
function Assert-PublicacaoPronta([string]$Destino,$Fonte) {
    $arquivo=Join-Path $Destino 'publicacao.json'
    if(-not(Test-Path -LiteralPath $arquivo)){throw 'Publicação sem identificação do commit. Gere novamente pela fonte atual.'}
    $pacote=Get-Content -LiteralPath $arquivo -Raw | ConvertFrom-Json
    if($pacote.Commit -ne $Fonte.Commit -or $pacote.Versao -ne $Fonte.Versao){throw 'O pacote é de outro commit ou versão. Não reutilize publicações antigas.'}
    $versao=(Get-Item -LiteralPath (Join-Path $Destino 'VarthexComanda.exe')).VersionInfo.FileVersion
    if($versao -ne "$($Fonte.Versao).0"){throw 'Executável com versão diferente do pacote.'}
    foreach($item in $pacote.Arquivos.PSObject.Properties){if((Get-FileHash -LiteralPath (Join-Path $Destino $item.Name) -Algorithm SHA256).Hash -ne $item.Value){throw "Binário divergente: $($item.Name)"}}
}
