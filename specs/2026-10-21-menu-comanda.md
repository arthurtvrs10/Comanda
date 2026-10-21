# Menu da comanda — 21/10/2026

Modo: Operate. Extensão restrita da edição de comanda WPF, atendendo ao pedido “Vamos arrumar nosso menu de comanda com as funcionalidades, baseie-se nesse”. As três referências Consumer fornecidas mostram a composição do pedido com lateral, itens, Produtos, Imprimir e PAGAMENTO, o menu Mais opções e o menu Imprimir. Esta passagem implementa a organização e os fluxos disponíveis no produto e preserva as demais superfícies e o registro legado de `DESIGN.md`.

## Composição e fidelidade visual

A edição abre em janela nativa filha, centralizada no proprietário, inicialmente em 900 × 740 DIP, limitada à área útil do monitor, com mínimo de 800 × 580 DIP e redimensionamento habilitado. Reutiliza o cabeçalho cinza compartilhado de `AparenciaJanelaModal`, sem marca, com título contextual de mesa/comanda e pedido. A maximização segue os limites compartilhados das janelas auxiliares; essas dimensões nativas não se confundem com as dimensões de conteúdo das capturas controladas.

O topo de 54 DIP reúne número, situação Em aberto/Bloqueado, busca e Produtos. A lateral de 210 DIP contém vínculo do cliente, Mais opções, identificação e abertura do pedido, Criado por, responsável, observação e Bloquear pedido. O painel de itens tem cabeçalho de 38 DIP com contagem e Exibição completa; cada linha mostra quantidade, produto, código, subtotal e opções de quantidade/remoção. A exibição completa acrescenta operador, horário e estado da cozinha, além da observação do item. Subtotal e total ficam abaixo da lista. O rodapé de 58 DIP mantém Imprimir à esquerda e PAGAMENTO à direita.

Segoe UI, corpo de 13 DIP, informações auxiliares de 12 DIP, cantos retos, bordas finas e separação tonal seguem o sistema desktop existente. A tela reutiliza `AjudaCanvas`, `AjudaSurface`, `AjudaInk`, `AjudaMuted`, `AjudaLine`, `AjudaSoft` e `AjudaHover`, com `AjudaAction`/`AjudaActionInk` na ação de pagamento. São referências dinâmicas do tema claro/escuro, sem introduzir uma paleta paralela. Os vetores locais `Path` de 20 × 20 DIP usam escala uniforme, traço de 1,4 DIP e alinhamento central no controle. Os menus usam texto de 13 DIP, linhas de pelo menos 42 DIP, divisores, indicação de submenu, realce neutro e opacidade reduzida quando indisponíveis; a altura máxima admite rolagem.

A fidelidade está na hierarquia e na distribuição de tarefas da referência. As adaptações são controles WPF, recursos de tema e cabeçalho compartilhados, catálogo disponível no produto, nomenclatura local e confirmação nas operações destrutivas. A marca e os ativos Consumer não são reutilizados. Não se acrescentam quantidades fracionadas, taxa de serviço, cobrança online ou envio automático de mensagens.

## Produtos, teclado e estados

Produtos abre o catálogo sob demanda sobre a área de itens, preservando a lateral; Voltar ao pedido e Esc fecham esse painel. A busca continua no topo, Ctrl+F a focaliza e Enter adiciona o primeiro resultado, uma unidade por acionamento. A observação do próximo produto acompanha o lançamento. F4 abre o pagamento existente, disponível quando há itens. O estado vazio orienta usar Produtos ou a busca; a ausência de resultados tem mensagem própria. Lista e catálogo usam rolagem dentro do espaço disponível.

As opções de cada item aumentam ou diminuem quantidades inteiras e permitem remoção com o fluxo de confirmação existente. A seleção do item recém-adicionado usa `ComandaComItens.ItemAlteradoId`, retornado pelo repositório, para apontar exatamente a linha criada ou incrementada mesmo quando há produtos, preços ou observações duplicados em linhas de operadores diferentes.

Responsável e observação são salvos no atendimento. A observação aceita até 1.000 caracteres; a do próximo item, até 300. A guarda `PrepararSaidaPedido` salva a observação antes de Voltar ou abrir pagamento, inclusive pelo teclado. O fechamento da janela também tenta salvá-la. Se o salvamento falha, a saída é interrompida e o texto permanece no campo para correção ou nova tentativa.

Bloquear pedido persiste o estado e protege adição, alteração de quantidade, remoção, junção e transferência/cópia de itens, com validação também no repositório. Uma origem ou destino bloqueados recusam movimentação. Pagamento e impressão continuam disponíveis. A identificação Criado por é registrada somente na abertura de novas comandas; a ausência de autoria em dados anteriores aparece como Não informado. Cada item mantém seu próprio operador de lançamento.

## Mais opções

O menu oferece, na ordem da implementação:

- Trocar para…: usa o fluxo existente de mudança de comanda.
- Visualizar conta resumida: abre a conferência da conta.
- Transferir / copiar itens para outro pedido: abre seleção de itens e destino, oferecendo somente outra comanda aberta e desbloqueada. Todos os itens começam selecionados; é necessário escolher destino e pelo menos um item, e confirmar a operação. Sem destino elegível, orienta abrir outra comanda e desbloqueá-la.
- Imprimir fichas de consumação: informa quantas unidades ainda não têm ficha e fica indisponível quando não há novas.
- Enviar para WhatsApp: pede número brasileiro com DDD e abre `wa.me` com um rascunho da conta. O usuário revisa e envia no WhatsApp; a aplicação não confirma entrega.
- Recalcular pedido: refaz subtotais e total com quantidade e preço unitário já lançado, sem atualizar pelo preço atual do catálogo.
- Dados do pedido: submenu para mesa, quantidade de pessoas, prioridade e origem do cliente.
- Excluir pedido…: usa o cancelamento com confirmação existente.
- Cancelar: fecha o menu.

A transferência preserva preço, quantidade, observação, autoria e os contadores de impressão do item, retirando-o da origem. A cópia conserva os dados comerciais e a autoria, cria uma nova linha no destino e começa com pendências de cozinha e fichas. As operações atualizam os totais das duas comandas em transação; não exigem recriar o pedido nem substituir preços históricos.

## Impressão e persistência

Imprimir oferece conta, conta e bloqueio, impressão dos novos itens na cozinha, reimpressão na cozinha e Cancelar. As opções dependem dos itens e das pendências reais. Conta e bloqueio só aplica o bloqueio depois de a saída aceitar a impressão. Havendo mais de uma cozinha habilitada, o usuário escolhe uma saída; o catálogo atual não associa produtos a cozinhas, portanto não há distribuição automática entre várias saídas.

`ComprovanteConta.Visualizar` retorna `bool`: fechar a prévia ou cancelar o diálogo não produz sucesso; `true` significa que o envio foi aceito pelo mecanismo de impressão, sem atestar a saída física. Os contadores `QuantidadeCozinha` e `QuantidadeFicha` são independentes e persistentes. A impressão de novos usa a diferença entre quantidade e contador correspondente; a reimpressão de cozinha inclui todas as unidades sem consumir novamente a pendência. `RegistrarImpressao` só roda após envio aceito, e uma falha de registro avisa para conferir a saída antes de repetir. Fechar a prévia não altera contadores.

A migração `20261021222856_AddOperacoesComanda` acrescenta criador, responsável e bloqueio à comanda, e operador e contadores de cozinha/ficha aos itens; a observação do pedido reutiliza o campo existente. Dados anteriores recebem autores nulos, contadores zero e bloqueio desativado. O histórico sem informação de autoria não é preenchido com o operador que apenas o consulta. O contrato de aplicação e o repositório preservam os dados ao reabrir o atendimento.

## Evidência e limites

Os relatórios em `backend/artifacts/comanda-menu-resultados/` registram 304/304 testes Desktop, 289/289 Infrastructure, 133/133 Application e 4/4 Domain: 730 aprovados, sem falhas. A passagem cobre persistência, operações de itens e impressão, estados e os dois reparos apontados na revisão: seleção pelo ID exato e proteção da observação ao sair ou pagar. A revisão independente retornou `ship` após conferir a resolução desses dois achados; o veredito se limita a eles, sem declarar aprovação global da interface.

Foram abertas e verificadas individualmente as 14 capturas controladas válidas em `.impeccable/review/comanda-menu/`:

- `comanda-claro-800.png`, `comanda-escuro-800.png`, `comanda-claro-1180.png` e `comanda-escuro-1180.png`: conteúdo em 800 × 548 e 1180 × 688 pixels.
- `comanda-claro-800-produtos.png` e `comanda-escuro-800-produtos.png`: catálogo aberto.
- `comanda-claro-800-bloqueado.png` e `comanda-escuro-800-bloqueado.png`: pedido bloqueado.
- `comanda-claro-800-vazio.png` e `comanda-escuro-800-vazio.png`: pedido sem itens.
- `menu-opcoes-claro.png`, `menu-opcoes-escuro.png`, `menu-imprimir-claro.png` e `menu-imprimir-escuro.png`: menus completos nos dois temas.

Essas imagens comprovam o conteúdo renderizado dos cenários controlados; não comprovam a moldura HWND, impressão em hardware ou envio real pelo WhatsApp. Não se aplica detector de layout HTML a esta superfície nativa WPF. A documentação não amplia esses limites nem atribui comportamento validado a integrações externas não exercitadas.

Fontes principais: `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoView.xaml`, `AtendimentoView.xaml.cs`, `AtendimentoView.Operacoes.cs`, `AtendimentoViewModel.cs` e `AtendimentoViewModel.Operacoes.cs`; `backend/src/VarthexComanda.Desktop/MainWindow.ImpressaoPedido.cs` e `Configuracao/ComprovanteConta.cs`; `backend/src/VarthexComanda.Application/Atendimento/AdicionarItem.cs`, `ComandaComItens.cs` e `IComandaRepository.cs`; entidades `Comanda` e `ItemComanda` em Domain; `backend/src/VarthexComanda.Infrastructure/Persistence/Atendimento/EfComandaRepository.cs`, `EfComandaRepository.Operacoes.cs` e a migração `AddOperacoesComanda`.

Esta passagem apenas acrescenta a seção Menu da comanda a `DESIGN.md` e cria esta especificação. O documento legado é preservado; `PRODUCT.md` e `.impeccable/design.json` continuam ausentes, sem reparo de drift anterior ou alteração de código por esta etapa documental.
