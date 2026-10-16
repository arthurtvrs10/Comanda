$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$folder = Join-Path $repo 'artifacts/android-deps'
$jar = Join-Path $folder 'core-3.5.3.jar'
New-Item -ItemType Directory -Force $folder | Out-Null
if (-not (Test-Path $jar)) {
    Invoke-WebRequest 'https://repo.maven.apache.org/maven2/com/google/zxing/core/3.5.3/core-3.5.3.jar' -OutFile $jar
}
if ((Get-FileHash $jar -Algorithm SHA256).Hash -ne '8D8064C1636FDAEF7189DD9055C7D59950A8940A12F2293956446EC3C109FD82') {
    throw 'A biblioteca ZXing não corresponde ao checksum esperado. Remova o arquivo em artifacts/android-deps e tente novamente.'
}
Write-Output $jar
