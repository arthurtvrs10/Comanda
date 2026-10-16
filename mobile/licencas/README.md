# Varthex Licenças — Android

Emissor offline para uso do responsável pelo Varthex Comanda. Android 8.0 ou superior, sem permissão de internet. A versão 1.1.0 está em desenvolvimento/testes; os downloads públicos anteriores permanecem inalterados.

## Ativar um computador

1. Instale o APK gerado em `artifacts/android-licencas/Varthex-Licencas-1.1.0.apk` no celular do responsável.
2. Configure PIN, senha ou padrão de bloqueio no Android. O aplicativo usa a confirmação de credencial do sistema para proteger a emissão.
3. Toque em **Importar chave privada (.pem)** ou **Colar chave privada**, informe o PEM original e confirme o desbloqueio do aparelho. A correspondência com a chave pública do Windows é verificada antes de salvar. O PEM não acompanha o APK.
4. No Windows, abra a tela de ativação. No celular, use **Escanear QR Code** e autorize a câmera. O código da máquina será preenchido; também é possível colá-lo manualmente.
5. Informe o cliente e escolha 1, 2, 3 meses, Vitalícia, **Personalizado: quantidade de dias** ou **Personalizado: data de vencimento**.
6. Confira a data exata apresentada, marque a autorização, toque em **Gerar chave de ativação**, confira o resumo e confirme a credencial do aparelho.
7. Use **Copiar chave** ou **Compartilhar chave** e cole o código no Windows. O QR transfere apenas a identificação do computador; não ativa automaticamente nem transfere a licença de volta.

## Validade e renovação

- Dias: de 1 a 36.500, contados a partir de hoje, às 00:00 (UTC−3). Um dia libera até o fim do dia atual.
- Data escolhida: inclui todo o dia selecionado; o vencimento técnico é às 00:00 do dia seguinte.
- Meses: preservam a regra de calendário anterior, com vencimento exclusivo às 00:00.
- Renovação antecipada: marque a opção e informe o vencimento atual. Meses ou dias são somados a essa data se ela for futura; se já venceu, contam de hoje.
- A nova licença começa hoje, inclusive em uma renovação antecipada, para permitir substituir a licença imediatamente sem interromper o uso.
- A interface informa o último dia de uso e o instante exato de vencimento em horário de Brasília.

## Chave protegida

O PEM é importado uma vez, criptografado com AES-GCM e armazenado em `noBackupFilesDir`. A chave AES fica no Android Keystore e requer autenticação do aparelho. Cada emissão abre a confirmação de credencial; o período criptográfico de autenticação é de 30 segundos. A chave RSA descriptografada não é mantida como sessão desbloqueada.

**Bloquear emissor** limpa a operação e o resultado da sessão. **Remover chave deste aparelho** remove o registro e a chave do Keystore. Reinstalação, troca de aparelho ou invalidação da chave pelo Android exigem nova importação. Mantenha o PEM original guardado fora do aplicativo. O arquivo original selecionado no celular não é apagado automaticamente.

O app não faz backup nem envia a chave. Capturas de tela e a prévia de aplicativos recentes são bloqueadas. Interrupção do processo durante a autenticação cancela a operação; nenhuma chave privada é gravada no estado de restauração da tela.

## Compilar

JDK 21, Android SDK Platform 35 e Build Tools 35.0.0. Não precisa de Gradle ou Android Studio.

```powershell
.\mobile\licencas\build.ps1 -Sdk 'C:\caminho\android-sdk' -Java 'C:\caminho\jdk-21'
```

O script incorpora [ZXing Core 3.5.3](https://github.com/zxing/zxing), Apache-2.0, com SHA-256 fixado em `dependencies.ps1`. A primeira compilação precisa baixar o JAR; o aplicativo instalado lê QR Codes offline sem depender de outro app ou de um módulo baixado pelo Google Play. A captura usa a API nativa de câmera do Android. O Windows gera o QR com [QRCoder](https://github.com/Shane32/QRCoder).

O APK e SHA-256 ficam em `artifacts/android-licencas/`. Preserve `artifacts/segredos/android-emissor.p12` e seu arquivo de senha para assinar atualizações. Essa chave de assinatura do APK é diferente da chave de emissão de licenças. Nenhum segredo deve entrar no repositório.

Protocolo do QR: `varthex://ativacao?v=1&maquina=<64 caracteres hexadecimais maiúsculos>`. O leitor aceita somente esse formato, sem parâmetros extras ou versões desconhecidas. O QR não contém chave privada, cliente ou autorização.

## Validação

```powershell
.\mobile\licencas\test.ps1
# Opcional: verificar a imagem real produzida pelo teste QrAtivacaoTests do Windows
.\mobile\licencas\test.ps1 -QrImage artifacts/ativacao-preview/ativacao-qr.png
```

Os testes usam somente chaves sintéticas. Verificam os quatro planos antigos, dias personalizados, data escolhida, renovação antecipada, ano bissexto, entradas inválidas, limite de vencimento e rejeição em outra máquina. As licenças Java são verificadas pelo código real `LicencaOffline` do Windows. O teste opcional decodifica com ZXing a imagem PNG gerada por QRCoder.

### Conferência pendente em aparelho Android

APK compilado e assinatura verificada. Câmera e Android Keystore ainda precisam de validação em aparelho/emulador antes de publicação:

- Instalar sobre o APK anterior, sem perder dados; importar uma chave de testes compatível com a chave pública de uma compilação de homologação.
- Salvar a chave, encerrar o processo, reabrir e emitir após desbloquear; cancelar a autenticação não deve emitir.
- Negar a câmera, cancelar e voltar ao preenchimento manual; ler a tela do Windows em diferentes orientações e condições de luz.
- Bloquear/remover a chave, remover o bloqueio do aparelho e verificar a recuperação por nova importação.
- Confirmar que a tela de conferência, datas, teclado, cópia e compartilhamento funcionam no aparelho.

Não usar chave privada de produção em testes automatizados nem incluí-la em capturas ou logs.
