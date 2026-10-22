# Configurar backup no Google Drive

A integração direta está preparada na pré-release 1.4.0 de 22/10/2026,
nas edições normal e demonstração. Ainda não há projeto OAuth configurado
nem conta Google real validada nesta entrega. Nenhuma credencial acompanha os pacotes.
O cadastro, atendimento e backup local continuam funcionando sem internet.

## Preparar o projeto Google

O responsável pela integração precisa de acesso ao Google Cloud. Os nomes das
opções podem mudar no console; consulte o [guia oficial de OAuth para aplicativos
de computador](https://developers.google.com/identity/protocols/oauth2/native-app).

1. Crie ou selecione um projeto Google Cloud para o Varthex.
2. Habilite a Google Drive API nesse projeto.
3. Configure a tela de consentimento/Google Auth Platform com nome do aplicativo,
   público e contatos de suporte reais. Preencha os demais dados solicitados.
4. Declare o escopo `https://www.googleapis.com/auth/drive.file`, que permite
   trabalhar com arquivos criados ou autorizados para o aplicativo. Ele não concede
   acesso geral a todo o Drive. Consulte os [escopos oficiais do Google Drive](https://developers.google.com/workspace/drive/api/guides/api-specific-auth).
5. Crie um cliente OAuth do tipo **Aplicativo para computador** (Desktop app).
6. Baixe o arquivo JSON desse cliente e guarde-o fora do repositório.

Em modo de teste, pode ser necessário cadastrar usuários de teste e os tokens
podem ter validade limitada. Para distribuir em produção, configure publicação
e eventual validação exigidas pelo Google para o projeto, público e escopos.
Siga as políticas e orientações atuais do console. Preparar a integração no
Varthex não significa que o aplicativo foi aprovado pelo Google.

## Importar e autorizar no Varthex

1. Entre como **Gestor** com permissão para backup e abra **Backup**.
2. No cartão Google Drive, escolha **Conectar com o Google**. O convite também
   aparece ao entrar no perfil sem uma conta conectada.
3. Escolha **Configurar credenciais do Google…** e selecione o JSON baixado.
   Arquivo de cliente Web ou JSON inválido é recusado.
4. Após a confirmação de configuração, clique em **Conectar com o Google**.
5. No navegador, escolha a conta, confira a identidade e as permissões e autorize.
   Volte ao Varthex e aguarde a confirmação da conexão.
6. Execute **Fazer backup agora** e confira o resultado do progresso e a pasta
   `Varthex Backups` no Drive. Verifique banco, checksum e fotos do lote concluído.

O Varthex não pede sua senha Google. A senha e o consentimento são tratados no
navegador pelo Google. A autorização tem prazo de três minutos; se cancelar,
expirar ou falhar, tente conectar novamente. Sem credenciais válidas, a tela
explica que o responsável precisa configurar a integração.

**Configurar depois** libera após cinco segundos. Antes disso, X e Alt+F4 também
não fecham o convite. Adiar permite usar o sistema e o convite reaparece na
próxima entrada elegível. Durante a conexão, a ação permite cancelar após o prazo.

## Arquivos e proteção local

O JSON importado fica como `google-oauth-client.json` na raiz local de dados.
Tokens ficam em `google-drive-token.bin`, protegidos para o usuário atual do
Windows. Na edição normal, a raiz é `%LOCALAPPDATA%\VarthexComanda`; demonstração
ou configuração de raiz própria usa sua pasta correspondente. Nunca coloque o
JSON, tokens ou senha Google no Git, em capturas ou em mensagens de suporte.

O token não é portável para outro usuário Windows. Reconecte a conta nesse caso.
Se o Google revogar a autorização e recusar a renovação, o token local é removido
e será necessário conectar novamente. **Desconectar** revoga a autorização e
remove o token após sucesso; os backups já enviados continuam no Google. Se a
revogação falhar, corrija a conexão e repita a ação.

O conteúdo dos backups não recebe criptografia adicional pelo aplicativo. A
proteção do token não impede que pessoas com acesso à conta Google ou aos
arquivos de backup leiam esses arquivos.

## Conferir o resultado e recuperar

O banco, checksum e fotos disponíveis são preparados localmente antes do envio.
Cada execução remota cria uma subpasta em `Varthex Backups`. O sufixo
`(incompleto)` só desaparece quando todos os arquivos são aceitos. Um lote
incompleto exige conferir a falha e tentar novo backup; o aplicativo não retoma
automaticamente a sessão interrompida nem apaga lotes antigos do Drive.

O envio automático tem limite de trinta segundos; o manual, com progresso, dez
minutos. Falha de rede, permissão ou espaço gera aviso e mantém a cópia local.
Conectado indica autorização armazenada e não comprova que o último upload
terminou. Para OneDrive, selecionar uma pasta continua sendo uma cópia local:
confira a sincronização no aplicativo OneDrive.

Para restaurar do Google, baixe manualmente o `.db`, seu `.sha256` e o
`.fotos.zip` companheiro para a mesma pasta. Abra **Restaurar dados**, selecione
o banco e siga a confirmação. O Varthex valida a cópia e cria backup preventivo
antes de substituir a base. Se necessário, use **Restaurar somente fotos** para
o arquivo de fotos. Não há seletor nem download de backups Google dentro do app.
Faça um ensaio de restauração separado antes de depender dessa recuperação.

Os limites e evidências técnicas estão na [especificação da extensão](../../specs/2026-10-22-drive-oauth-medidas-venda.md).
