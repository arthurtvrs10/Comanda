# Executar no PowerShell 7. Não publica serviços nem ativa planos pagos.
[CmdletBinding()]
param([string]$ApiUrl)
$ErrorActionPreference='Stop'
$pastaSegredos=Join-Path $PSScriptRoot '.secrets'
$arquivoPrivado=Join-Path $pastaSegredos 'signing-private.pem'
if(Test-Path -LiteralPath $arquivoPrivado){throw 'Já existem chaves nesta pasta. Preserve-as; não gere novas chaves sobre uma instalação existente.'}
if($ApiUrl){
    $enderecoApi=[Uri]$ApiUrl
    if(-not $enderecoApi.IsAbsoluteUri -or $enderecoApi.Scheme -ne 'https' -or $enderecoApi.UserInfo -or $enderecoApi.Query -or $enderecoApi.Fragment){throw 'Informe o endereço HTTPS do Worker, sem credenciais ou parâmetros.'}
}
New-Item -ItemType Directory -Path $pastaSegredos -Force | Out-Null
$chaveAssinatura=[Security.Cryptography.RSA]::Create(3072)
try {
    [IO.File]::WriteAllText($arquivoPrivado,$chaveAssinatura.ExportPkcs8PrivateKeyPem())
    $chavePublica=$chaveAssinatura.ExportSubjectPublicKeyInfoPem()
    [IO.File]::WriteAllText((Join-Path $pastaSegredos 'signing-public.pem'),$chavePublica)
    $segredoCodigo=[Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32)).ToLowerInvariant()
    [IO.File]::WriteAllText((Join-Path $pastaSegredos 'otp-pepper.txt'),$segredoCodigo)
    if($ApiUrl){
        $configPublica=@{ApiUrl=$ApiUrl.TrimEnd('/')+'/';ChavePublicaPem=$chavePublica}|ConvertTo-Json
        [IO.File]::WriteAllText((Join-Path $pastaSegredos 'recuperacao-email.json'),$configPublica)
    }
} finally {$chaveAssinatura.Dispose();$segredoCodigo=$null}
Write-Output 'Chaves criadas em .secrets (ignorada pelo Git). Guarde uma cópia segura. Nenhum segredo foi exibido e nenhum serviço foi publicado.'
