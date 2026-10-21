# Backup — progresso e OneDrive — 21/10/2026

Modo: Operate. Extensão restrita da tela WPF de Backup, solicitada pelo usuário a partir de referências visuais: aviso com losango vermelho junto à restauração, OneDrive no lugar de Dropbox, progresso dentro da mesma janela e remoção do histórico visível. Esta especificação substitui as regras de Dropbox e Backups recentes de `2026-10-21-backup-sincronizado.md`; as demais garantias de criação e restauração permanecem.

## Configuração e restauração

A configuração mantém a pasta local e os dois cartões independentes à esquerda, agora Google Drive e OneDrive, com a restauração à direita. Segoe UI de 12 DIP, títulos de 18 DIP, superfícies e textos neutros `Ajuda*`, bordas finas, cantos retos e ações verdes seguem a implementação anterior. A janela usa o cabeçalho compartilhado de 30 DIP documentado em `2026-10-21-cabecalhos-modais.md`.

O ícone de OneDrive é uma nuvem vetorial azul original em `IconesVarthex`, normalizada no contorno seguro de 28 × 28 dentro do quadro de 32 × 32 e apresentada em 36 DIP no cartão. O aviso de restauração ganha o vetor Alerta, losango branco com contorno e exclamação vermelhos, apresentado em 24 DIP à esquerda das duas notas. Elas informam a substituição dos dados, cópia preventiva e reinício, e lembram que produtos com fotos precisam também do backup de fotos.

Não há Expander, tabela, estado vazio de histórico ou botão de restaurar registro selecionado na interface. Restaurar dados e Restaurar somente fotos continuam abrindo seus seletores de arquivo. No modo de recuperação, criação e alteração de pastas permanecem indisponíveis e o rodapé oferece Restaurar dados. A lista e os comandos legados do modelo continuam por compatibilidade interna, sem controles visíveis correspondentes.

As mensagens reais de ausência ou falha de consulta dos backups orientam usar a ação visível Restaurar dados; as quatro capturas de recuperação foram renovadas com essas mensagens do modelo, sem substituição pelo cenário de captura.

## Página de progresso

Fazer backup agora substitui o corpo de configuração pelo progresso na mesma `BackupView`, sem abrir outra janela. A página tem estado centralizado no topo, lista rolável de etapas e Voltar à direita do rodapé. O título inicial é Aguarde, fazendo o backup…; o resultado diferencia Backup concluído!, Backup concluído com avisos e Falha ao fazer o backup.

O estado de andamento usa três pontos com animação de opacidade entre 0,35 e 1, com duração de 0,65 segundo, reversão e repetição enquanto ocupado. A animação só começa quando `SystemParameters.ClientAreaAnimation` está habilitado; o estado continua identificado por texto quando ela está desabilitada. O término substitui os pontos por marca verde de sucesso, triângulo de aviso ou X de falha. As linhas de aviso usam texto neutro seminegrito, preservando contraste nos dois temas.

Cada linha apresenta horário de Brasília no formato `HH:mm:ss > mensagem`. Os registros vêm de etapas reais do serviço: início do backup local, snapshot salvo com caminho efetivo, preparação e resultado das fotos, início e resultado da cópia em cada pasta configurada. Quando há ZIP correspondente, seu caminho real é mostrado; ausência de fotos e incompletude têm mensagens próprias. Cópia em pasta externa legada, quando solicitada, também gera registro real. A lista rola para a última linha adicionada e quebra caminhos longos dentro da largura disponível.

`EtapaBackup` transporta data, mensagem e sinalização de aviso por `IProgress<EtapaBackup>`. `IBackupService` e `CriarBackupManual` repassam esse acompanhamento. O modelo executa o trabalho fora da interface e publica as atualizações da coleção observável no contexto da interface. Não há porcentagens simuladas, espera artificial nem mensagens de etapas não executadas.

## Navegação e concorrência

Voltar fica desabilitado durante a criação e só retorna à configuração após o término. O estado ocupado também impede criação duplicada e preserva a proteção existente de fechamento da janela de backup e da janela principal. Uma nova criação limpa o registro e reinicia o estado da página.

O atalho Criar backup das configurações abre essa mesma janela e inicia o fluxo no evento `Loaded`. Uma guarda local impede iniciar duas vezes por repetição desse evento; o modelo também recusa criação enquanto ocupado. Quando o atalho inclui a pasta externa legada, ela segue pelo mesmo fluxo de acompanhamento.

## OneDrive e preservação dos arquivos

`pastas-backup.json`, sob `AppPaths.Root`, passa a ter a propriedade independente `OneDrive`, junto de `Local` e `GoogleDrive`. A chave antiga `Dropbox` é ignorada na leitura e não é migrada ou reinterpretada como OneDrive. Pastas e arquivos anteriormente gravados permanecem; as preferências local e Google Drive continuam disponíveis. É necessário selecionar a pasta real já sincronizada pelo aplicativo OneDrive.

O destino continua sendo `Varthex\Backups` sob a raiz escolhida. O aplicativo registra a cópia gravada nessa pasta, enquanto a sincronização remota depende do aplicativo desktop. Não há autenticação de conta, upload direto ou confirmação de envio à nuvem.

O serviço mantém a ordem local primeiro, snapshot SQLite, checksum e ZIP de fotos correspondente. As cópias sincronizadas continuam preparando e verificando os acompanhantes antes de publicar o banco, preservando o backup local e a tentativa no outro destino se uma pasta falhar. Os avisos de fotos incompletas e a reutilização condicionada ao manifesto permanecem. As garantias existentes de validação, cópia preventiva, integridade e reinício na restauração de dados, e de preparação, limites e recuperação na restauração somente de fotos, não foram alteradas nesta extensão.

As referências servem à apresentação visual. Não existe fluxo de dados XML no produto; não foram criados restauração XML ou registros de progresso XML fictícios.

## Evidência e limites

Passaram 297 testes Desktop, 284 Infrastructure e 133 Application, totalizando 714. A execução completa final da suíte Desktop passou após uma falha intermitente anterior, fora do escopo, em Atendimento/Enter; não houve alteração desse comportamento nesta tarefa. As verificações da extensão usam dados e pastas controlados, sem conectar contas reais de nuvem do usuário.

As 14 capturas em `.impeccable/review/backup-progresso/` foram abertas e verificadas individualmente:

- `backup-claro-760.png`, `backup-escuro-760.png`, `backup-claro-1100.png` e `backup-escuro-1100.png`: configuração em 760 × 440 e 1100 × 630 pixels;
- `backup-recuperacao-claro-760.png`, `backup-recuperacao-escuro-760.png`, `backup-recuperacao-claro-1100.png` e `backup-recuperacao-escuro-1100.png`: recuperação em 760 × 440 e 1100 × 440 pixels;
- `progresso-sucesso-False-andamento.png`, `progresso-sucesso-False-concluido.png`, `progresso-sucesso-True-andamento.png` e `progresso-sucesso-True-concluido.png`: andamento e conclusão nos dois temas, em 760 × 460 pixels;
- `progresso-aviso-False-concluido.png` e `progresso-falha-False-concluido.png`: resultados de aviso e falha, em 760 × 460 pixels.

São telas nativas Windows; não se aplica detector de layout HTML. A revisão independente retornou `ship` após verificar a correção da única observação material: as mensagens reais de recuperação passaram a indicar o botão visível Restaurar dados. O veredito dessa conferência cobre esse item; não representa aprovação global da interface nem comprova sincronização remota.

Fontes principais: `backend/src/VarthexComanda.Desktop/Backup/BackupView.xaml`, `BackupView.xaml.cs` e `BackupViewModel.cs`; `backend/src/VarthexComanda.Desktop/IconesVarthex.cs` e `MainWindow.xaml.cs`; `backend/src/VarthexComanda.Application/Backup/EtapaBackup.cs`, `IBackupService.cs` e `CriarBackupManual.cs`; `backend/src/VarthexComanda.Infrastructure/Backup/EfBackupService.cs` e `PreferenciasBackup.cs`. As demais seções e o formato legado de `DESIGN.md` permanecem. Esta passagem não cria `PRODUCT.md`, não gera `.impeccable/design.json` e não repara documentação fora do escopo.
