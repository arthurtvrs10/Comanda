# Prioridades, localizador de produtos e backup modal — 21/10/2026

Modo: Operate. Extensão restrita da edição de comanda e do Backup WPF. O usuário pediu cor no número da comanda e em Em aberto, prioridade alta vermelha, mediana amarela e normal verde, número do pedido em negrito, alternâncias no lugar de caixas de seleção, Produtos em outra tela com categorias e modos lista/fotos conforme duas referências Consumer, e Fazer backup agora em modal com barra de progresso.

Esta especificação substitui somente o catálogo sobreposto descrito em `2026-10-21-menu-comanda.md` e a página de progresso embutida de `2026-10-21-progresso-backup-onedrive.md`, além de registrar os novos tratamentos de prioridade e alternância. As demais regras de pedido, persistência, criação e restauração de backups continuam existentes.

## Prioridade e alternâncias do pedido

A prioridade aparece no número da comanda e no fundo da situação Em aberto; quando bloqueado, o texto continua Bloqueado. Clicar na situação abre um menu com as três opções, bolinha colorida e indicação textual da opção atual. Tooltip e nome acessível identificam a prioridade e a ação de alterar. O código do pedido é destacado separadamente em negrito, como `#102`, por `NumeroPedidoDestacado`.

| Rótulo visível | Valor existente de `PrioridadeComanda` | Fundo/indicador | Número no tema claro | Número no tema escuro |
| --- | --- | --- | --- | --- |
| Normal | `Normal` | `PrioridadeNormal` #2D8050 | `PrioridadeNumeroNormal` #286D44 | `PrioridadeNumeroNormal` #80D59F |
| Mediana | `Alta` | `PrioridadeMediana` #F2CB59 | `PrioridadeNumeroMediana` #8B6900 | `PrioridadeNumeroMediana` #F2CB59 |
| Alta | `Urgente` | `PrioridadeAlta` #B6322C | `PrioridadeNumeroAlta` #B6322C | `PrioridadeNumeroAlta` #FF9993 |

O status verde/vermelho usa texto branco; o amarelo usa #302500. As variantes do número preservam a leitura sobre o fundo de cada tema. O mapeamento é de apresentação: não altera os valores persistidos, não cria migração e mantém a ordenação de prioridade existente. Os rótulos dos demais pontos atualizados, incluindo a grade e Dados do pedido, usam a mesma nomenclatura.

`AlternanciaCompacta` é um estilo de `CheckBox` com trilho arredondado de 38 × 20 DIP, pino circular de 14 DIP e altura mínima de controle de 32 DIP. Desligado usa recursos neutros; ligado usa `AjudaAction` e `AjudaActionInk`, com pino à direita. Hover realça a borda, `FocoVisivel` indica teclado e desabilitado reduz a opacidade a 0,5. O estilo se aplica a Bloquear pedido e Exibição completa. A mudança é visual; o bloqueio continua persistido e protegido pelas regras existentes, enquanto Exibição completa mantém o estado do modelo da sessão.

## Localizar produto

Produtos abre uma nova janela modal `LocalizarProdutoView`, filha da janela do pedido, centralizada no proprietário e fora da barra de tarefas. Começa em 900 × 740 DIP, mínimo de 740 × 520 DIP, com redimensionamento e cabeçalho cinza compartilhado de `AparenciaJanelaModal`, sem marca. A abertura respeita `PodeAlterarPedido` e salva a observação pendente antes de prosseguir. A comanda ativa e o modelo do pedido continuam sendo os mesmos; fechar o localizador retorna à edição.

A composição adapta as duas referências Consumer a WPF, Segoe UI de 12 DIP, cantos retos, bordas finas e recursos dinâmicos neutros `AjudaCanvas`, `AjudaSurface`, `AjudaInk`, `AjudaMuted`, `AjudaLine`, `AjudaSoft` e `AjudaHover`. Não incorpora marca ou imagens Consumer. O topo contém busca pelo nome, Lista, Fotos e Atualizar. Lista/Fotos são controles de seleção exclusiva com vetores próprios e foco de teclado. A lateral de 172 DIP mostra Todas e as categorias reais carregadas no pedido; a categoria selecionada recebe realce neutro e seminegrito. Todas remove o filtro de categoria e preserva a busca digitada. Atualizar recarrega as categorias e o catálogo disponível, com retorno textual de sucesso ou falha.

A lista tem categoria, código, nome, preço, Adicionar e Personalizar. Linhas e cabeçalho medem 42 DIP; células herdam o alinhamento vertical central compartilhado. Números e preços ficam à direita, textos à esquerda. A coluna de nome ocupa o espaço restante, com mínimo de 130 DIP, reticências e tooltip do nome completo. Em largura mínima, a tabela usa sua rolagem para manter as ações acessíveis.

Fotos dispõe cartões em grade com quebra de linha e rolagem vertical. Cada quadro interno mede 156 × 238 DIP e reserva 100 DIP para a imagem; as alturas iguais mantêm preço e ações alinhados. Mostra código, nome, preço, Adicionar e Personalizar. A imagem vem de `FotoArquivoParaImagemConverter`, usando `Produto.FotoArquivo` e o `DiretorioFotos` já configurado. Quando não há foto carregável, aparece o placeholder vetorial neutro. Esta implementação não gera imagens de produtos nem grava novos arquivos de imagem no diretório de fotos do usuário. A evidência controlada usa uma imagem sintética identificada como produto de teste, sem representar uma foto comercial real.

O rodapé mostra contagem de resultados, mensagens de operação e Voltar ao pedido. A ausência de produtos orienta alterar busca ou categoria. A busca e o filtro pertencem ao modelo ativo; a lista mantém a seleção pelo código quando possível e seleciona o primeiro resultado quando necessário.

## Lançamento e teclado

Adicionar, o botão de soma na lista e F11 lançam uma unidade do produto selecionado com a observação do próximo item. Enter abre Personalizar para o selecionado, exceto quando a origem é um botão. O formulário permite quantidade inteira de 1 a 999 e observação de até 300 caracteres, com validação e mensagem de erro. Adicionar ao pedido fecha a personalização apenas após sucesso; Cancelar descarta o formulário. Enter/F11 ignoram repetição automática da tecla. Ctrl+F focaliza e seleciona a busca; Esc ou Voltar ao pedido fecha o localizador.

`LocalizarProdutoViewModel.Lancar` delega a `AtendimentoViewModel.AdicionarProduto`, que usa o caso de uso existente de adição. O item alterado continua selecionado pelo `ItemAlteradoId` retornado pelo resultado, preservando o contrato de identificação exata em produtos, preços e observações duplicados. O lançamento valida a possibilidade de alteração, conserva o pedido ativo, atualiza os itens e limpa a observação do próximo lançamento após sucesso. O antigo painel de catálogo sobre a lista de itens deixa de compor o menu da comanda; os demais menus e operações permanecem.

## Progresso do backup em modal

`BackupView` mantém a configuração e deixa de conter a página de progresso. Tanto Fazer backup agora quanto o atalho Criar backup em Configurações chamam `CriarBackupComProgressoAsync`. O método abre `JanelaProgressoBackup` por `ShowDialog`, com a janela de configuração de backup como proprietária. Ela começa em 660 × 510 DIP, mínimo de 460 × 340 DIP, centralizada, com cabeçalho cinza compartilhado e corpo neutro `Ajuda*`. A configuração continua presente ao fundo e reaparece ao terminar e fechar o progresso.

`ProgressoBackupView` reúne estado textual, indicador, barra de 9 DIP de altura, registros roláveis e Voltar. A barra usa `IsIndeterminate` ligado a `EmAndamento`; não mostra porcentagem prevista. `ValorBarraProgresso` vale 100 apenas em sucesso ou aviso e zero em falha. A barra cheia significa término da operação, inclusive quando há aviso; o título e os registros distinguem o resultado. Os estados existentes permanecem: Aguarde, fazendo o backup…, Backup concluído!, Backup concluído com avisos e Falha ao fazer o backup, com indicadores correspondentes.

Os registros conservam as etapas reais do serviço, os horários de Brasília e os caminhos efetivamente gravados, com rolagem automática e quebra de caminhos longos. Avisos usam seminegrito e texto neutro. Os pontos animam por opacidade apenas quando as animações da área de cliente do Windows estão habilitadas; o texto identifica a execução mesmo sem animação. Não há porcentagem simulada, atraso artificial nem etapas de upload inventadas. A barra indeterminada pode ser capturada sem segmento visível conforme a fase da animação; a verificação de estado confere `IsIndeterminate` diretamente.

A guarda no `Loaded` inicia o trabalho uma vez por janela; a guarda de reentrância do modelo permanece. Voltar fica desabilitado enquanto ocupado. O fechamento é bloqueado antes do início no primeiro `Loaded` e durante o trabalho. Ao concluir, Voltar ou fechar encerra o modal, chama `VoltarConfiguracao` e mantém a janela proprietária de configuração. Nova execução limpa os registros anteriores. O atalho continua encaminhando a pasta externa legada quando configurada.

Permanecem o backup local primeiro, Google Drive/OneDrive como pastas já sincronizadas, e os fluxos de restauração existentes. O registro de cópia em pasta não comprova upload ou sincronização remota, e esta extensão não acrescenta autenticação de nuvem.

## Evidência, fontes e limites

`backend/artifacts/localizador-resultados/localizador.trx` registra 307/307 testes Desktop executados e aprovados, sem falhas. A execução apresentou dois avisos xUnit2031 preexistentes. Infrastructure, Application e Domain não foram reexecutadas nesta passagem; os totais históricos das especificações anteriores não representam testes novos desta extensão. A revisão independente final retornou `ship`, sem achados materiais no escopo desta extensão; não é aprovação global do sistema.

Foram abertas e conferidas individualmente 28 capturas controladas válidas em `.impeccable/review/localizador-prioridades/`, onde `False` indica tema claro e `True`, escuro:

- Oito `localizador-{False,True}-{740,1100}-{lista,fotos}.png`: lista e fotos em 740 × 520 e 1100 × 740 pixels.
- Duas `localizador-{False,True}-vazio.png`: ausência de resultados nos dois temas.
- Seis `prioridade-{False,True}-{Normal,Alta,Urgente}.png`: as três prioridades em conteúdo de 800 × 548 pixels. Os nomes de arquivo usam os valores existentes do enum; `Alta` corresponde à UI Mediana e `Urgente`, à UI Alta.
- Duas `prioridade-menu-{False,True}.png`: opções do menu nos dois temas.
- Seis `progresso-sucesso-{False,True}-{andamento,concluido,compacto}.png`: andamento, sucesso e composição compacta nos dois temas, em 660 × 510 e 460 × 340 pixels.
- Quatro `progresso-{aviso,falha}-False-{concluido,compacto}.png`: aviso e falha no tema claro, nos dois tamanhos.

As capturas de novas janelas incluem seus cabeçalhos nativos; as de prioridade registram a área de conteúdo do pedido. A imagem de produto de teste comprova o carregamento pelo conversor junto do placeholder para produto sem foto. Não se aplica detector de layout HTML a esta interface WPF. A verificação não inclui impressão em equipamento físico, sincronização remota real ou autenticação automatizada. Ao finalizar, o executável atualizado foi aberto e sua tela principal foi conferida, com acesso bloqueado aguardando autenticação pelo usuário; a evidência das novas funcionalidades vem dos testes e cenários controlados.

Fontes conferidas:

- `backend/src/VarthexComanda.Desktop/App.xaml` e `TemaInterface.cs`: estilo de alternância, estados e recursos de prioridade/contraste.
- `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoView.xaml`, `AtendimentoView.xaml.cs` e `AtendimentoView.Operacoes.cs`: composição, abertura da janela, status e menus.
- `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoViewModel.cs`, `AtendimentoViewModel.Operacoes.cs` e `ComandaSlotItem.cs`: nomenclatura, estado do pedido, lançamento e seleção.
- `backend/src/VarthexComanda.Desktop/Atendimento/LocalizarProdutoView.xaml`, `LocalizarProdutoView.xaml.cs` e `LocalizarProdutoViewModel.cs`: categorias, busca, lista/fotos, personalização e teclado; `Catalogo/FotoArquivoParaImagemConverter.cs`: leitura de imagens existentes.
- `backend/src/VarthexComanda.Desktop/Backup/BackupView.xaml`, `BackupView.xaml.cs`, `BackupViewModel.cs`, `ProgressoBackupView.xaml` e `ProgressoBackupView.xaml.cs`, além de `MainWindow.xaml.cs`: configuração preservada, abertura modal, barra, início único, fechamento e atalho.

Esta etapa documental apenas acrescenta a seção estreita correspondente ao fim de `DESIGN.md` e cria esta especificação. O restante do documento legado e as demais especificações permanecem preservados. Não cria `PRODUCT.md` ou `.impeccable/design.json`, não repara drift anterior e não altera código ou commits.
