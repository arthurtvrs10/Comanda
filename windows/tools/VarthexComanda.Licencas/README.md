# Varthex Licenças 1.1.0 — Windows

Emissor offline para o responsável pelo VComanda. Compatível com o protocolo de ativação do VComanda 1.4.7. Não instalar no computador do cliente nem distribuir a chave privada.

## Utilizar

1. Extraia todo o ZIP e abra VarthexComanda.Licencas.exe. O programa Windows continua sendo um assistente no terminal.
2. Informe o caminho do PEM privado original. O emissor verifica a correspondência com a chave pública do VComanda.
3. Cole os 64 caracteres do computador ou o conteúdo do QR de ativação e informe o cliente.
4. Escolha 1, 2 ou 3 meses; 0 para vitalícia; 4 para dias (1 a 36.500); ou 5 para o último dia de uso.
5. Para meses/dias, informe se é renovação antecipada e o vencimento atual mostrado na licença, à meia-noite. Se já venceu, o prazo parte de hoje. A licença nova começa hoje e pode substituir imediatamente a anterior.
6. Confira o cliente, computador e datas. Digite SIM somente quando a emissão estiver autorizada. Qualquer outra resposta cancela sem ler a chave privada nem emitir a licença.
7. Copie a licença para o cliente. Uma cópia fica em %LOCALAPPDATA%\VarthexLicencas\emitidas.

Datas no horário de Brasília (UTC−3). O último dia escolhido é incluído integralmente; o vencimento técnico é às 00:00 do dia seguinte. Não é necessário alterar o VComanda para aceitar prazos personalizados, pois o protocolo de licença permanece na versão 1.

A chave privada não acompanha o executável. Não foi gerada, substituída ou utilizada uma chave de produção nesta atualização.

## Compilar

scripts/publicar-emissor.ps1 gera a publicação Windows autocontida. Use VarthexComanda.Licencas.exe --ajuda para instruções rápidas. O comando inicializar continua disponível para criar um novo par sem substituir arquivos; uma nova chave pública requer recompilar também o VComanda e o APK. Não use esse comando para uma renovação de licença.

## Validação desta atualização

25 testes .NET de emissão e validação aprovados. Sete licenças sintéticas Java validadas no Windows, seis licenças sintéticas .NET verificadas em Java e QR PNG do Windows decodificado por ZXing. Seis fluxos reais do executável conferidos e cancelados sem chave de produção.
