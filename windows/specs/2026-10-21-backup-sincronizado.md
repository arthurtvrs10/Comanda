# Backup e pastas sincronizadas — 21/10/2026

Modo: Operate. Extensão restrita da tela WPF de Backup, baseada na organização da referência Consumer e no sistema Windows existente. O usuário confirmou o uso das pastas dos aplicativos desktop do Google Drive e Dropbox. A implementação não configura as contas ou escolhe pastas reais na máquina do usuário.

## Composição e tema

A configuração fica à esquerda: pasta de backup local seguida de dois cartões independentes, Google Drive e Dropbox. Cada cartão mostra ícone centralizado, nome do serviço, pasta, situação e Configurar. Desconectar pasta aparece quando há configuração e remove apenas aquela preferência; os arquivos já salvos permanecem. À direita, um separador fino delimita Restaurar backup, com ações separadas para dados e somente fotos.

Segoe UI de 12 DIP no corpo e 18 DIP nos títulos; notas com altura de linha de 17 DIP. O fundo e os textos usam os recursos dinâmicos `AjudaCanvas`, `AjudaSurface`, `AjudaInk`, `AjudaMuted` e `AjudaLine`. A ação principal usa `AjudaAction` e `AjudaActionInk`. Controles mantêm cantos retos e bordas finas; botões principais desta superfície têm altura mínima de 36 DIP. Esses ajustes pertencem à tela de Backup, sem redefinir a identidade global.

O corpo tem largura lógica mínima de 680 DIP e rolagem vertical/horizontal automática. Em uma janela de 760 × 440 DIP, a rolagem preserva o conteúdo e o rodapé continua disponível. O rodapé apresenta a mensagem da operação, anunciada por região acessível, e Fazer backup agora. O estado vazio do histórico não reserva os 170 DIP da lista. No uso normal, Backups recentes aparece recolhido abaixo da configuração; em recuperação, aparece expandido no início e o rodapé oferece Restaurar dados.

Os novos vetores GoogleDrive, Dropbox, Pasta e Fotos pertencem a `IconesVarthex`, com a centralização e área segura compartilhadas. A pasta local usa 26 DIP; os cartões de serviços usam 36 DIP; as ações de restauração usam 24 DIP. Ícones são decorativos; botões e campos carregam a identificação acessível.

## Seleção e persistência das pastas

Configurar abre o seletor nativo de pastas. A pasta precisa existir e permitir gravação; a pasta do banco ativo é recusada. Cada destino pode ser alterado de forma independente. A seleção é persistida em `pastas-backup.json`, sob `AppPaths.Root`, por arquivo temporário e substituição, fora do SQLite.

O aplicativo grava as cópias de Google Drive e Dropbox em `Varthex\Backups` dentro de cada raiz selecionada. A indicação de última cópia consulta os arquivos presentes nessa pasta. O texto explica que o envio depende do aplicativo de sincronização e pede que o usuário confira nele a conclusão. Não há OAuth, autenticação de conta, API de nuvem ou confirmação de upload.

A pasta local configurada recebe o backup gerenciado diretamente. Se estiver indisponível, o serviço usa a pasta padrão e apresenta aviso. Falha de leitura das preferências também preserva a opção padrão e gera aviso. Alterar a configuração não move nem remove backups existentes.

## Criação manual e automática

O serviço cria um snapshot SQLite gerenciado e seu arquivo `.sha256`. Em seguida copia o mesmo snapshot e checksum para cada destino sincronizado configurado, incluindo o `.fotos.zip` correspondente quando disponível. A cópia prepara arquivos temporários, verifica o conteúdo e publica o `.db` por último, depois dos acompanhantes. Uma falha de um destino é reportada sem invalidar a cópia local nem impedir a tentativa no outro destino.

A completude das fotos é explícita no resultado: falha ou conjunto parcial gera aviso, preservando o backup dos dados. Um ZIP anterior só é reutilizado após conferir que seu manifesto corresponde ao conjunto atual de fotos. Um arquivo antigo não é escolhido como substituto quando a nova preparação falha. Se parte das fotos não puder ser lida, o manifesto descreve apenas o conjunto incluído, permitindo nova tentativa na próxima criação.

Fazer backup agora executa a operação fora da interface, mostra andamento e impede ações concorrentes. A rotina automática utiliza as mesmas preferências na abertura, uma vez por dia, e no encerramento. Avisos de cópias automáticas ficam registrados. A configuração externa legada continua com seu comportamento existente no encerramento.

## Restauração dos dados e recuperação

Restaurar dados permite selecionar um `.db`; manter o `.fotos.zip` junto do backup permite recuperar também as fotos pelo fluxo existente. A restauração real preserva validação de checksum, esquema e integridade, confirmação da substituição, cópia preventiva da base atual e reinício após sucesso. Um arquivo inválido não substitui a base ativa.

Com banco corrompido, a lista é construída a partir dos arquivos das pastas disponíveis: padrão, local configurada e `Varthex\Backups` das duas raízes sincronizadas. As preferências independem do banco. Criar backup e alterar pastas ficam indisponíveis nesse modo; selecionar um arquivo externo continua possível. A listagem de disco não substitui a validação feita ao restaurar.

O escopo cobre os backups SQLite e de fotos já suportados pelo produto. Não foi criado fluxo XML, pois não existe um fluxo de dados XML correspondente na aplicação.

## Restauração somente de fotos

A ação seleciona um `.fotos.zip` e confirma a substituição de fotos com o mesmo nome, preservando pedidos, produtos e demais dados. O arquivo é integralmente validado e preparado em pasta temporária antes de modificar a biblioteca atual. Aceita apenas entradas de imagem com nomes simples; recusa caminhos e entradas inválidas. Os limites são 5.000 entradas, 10 MiB por foto e 2 GiB de conteúdo extraído no total, conferidos também durante a leitura.

Antes da cópia, todas as fotos atuais são guardadas na pasta padrão de backups como `fotos-antes-restauracao-{guid}.fotos.zip`. Se a cópia das novas fotos falhar, o serviço tenta restaurar os nomes afetados a partir dessa cópia e remover os arquivos recém-incluídos. Fotos não presentes no ZIP selecionado permanecem. O SQLite não é modificado e não há reinício por essa ação.

A operação é assíncrona, exibe mensagem de andamento e bloqueia novos comandos enquanto executa. O fechamento é impedido durante criação manual e restauração de fotos para preservar a conclusão da operação.

## Evidência e limites

Após a implementação e as duas correções materiais de revisão, passaram as suítes Desktop (291), Infrastructure (282) e Application (133), totalizando 706 testes. A revisão independente retornou `ship` na conferência das duas correções: completude das fotos e ação de recuperação imediatamente disponível. Esse veredito cobre os dois itens avaliados; não representa aprovação global da interface nem comprova envio remoto pelos aplicativos de nuvem.

As oito capturas WPF foram verificadas individualmente nos temas claro e escuro: uso normal em 760 × 440 e 1100 × 630 pixels; recuperação em 760 × 440 e 1100 × 440 pixels. Arquivos em `.impeccable/review/backup/`:

- `backup-claro-760.png` e `backup-claro-1100.png`;
- `backup-escuro-760.png` e `backup-escuro-1100.png`;
- `backup-recuperacao-claro-760.png` e `backup-recuperacao-claro-1100.png`;
- `backup-recuperacao-escuro-760.png` e `backup-recuperacao-escuro-1100.png`.

São capturas nativas de Windows; detectores de layout web não se aplicam. Não foi realizada conexão real a Google Drive ou Dropbox nem verificação de upload na conta do usuário. É necessário que o usuário selecione suas próprias pastas já sincronizadas.

Fontes principais: `backend/src/VarthexComanda.Desktop/Backup/BackupView.xaml`, `BackupView.xaml.cs` e `BackupViewModel.cs`; `backend/src/VarthexComanda.Infrastructure/Backup/EfBackupService.cs` e `PreferenciasBackup.cs`; `backend/src/VarthexComanda.Application/Backup/IBackupFotosService.cs` e `CriarBackupAutomatico.cs`. A documentação anterior de `DESIGN.md` foi preservada. Esta passagem não cria `PRODUCT.md`, não gera `.impeccable/design.json` e não repara a documentação legada fora da extensão solicitada.
