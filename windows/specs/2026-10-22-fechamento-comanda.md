# Comanda em fechamento

Data: 22/10/2026. Extensão restrita à operação WPF existente: identificar a comanda como Em fechamento em amarelo e reduzir a interrupção percebida ao alternar esse estado. Preserva o modo Operate, os temas, Segoe UI, cantos retos, composição e regras de itens e pagamento.

## Comportamento final

`Bloqueada` permanece como estado interno persistido. Cabeçalho, alternância lateral e cartão da grade exibem Em fechamento; o menu usa Imprimir conta e iniciar fechamento. Cabeçalho e cartão recebem `PrioridadeMediana` (#F2CB59) com texto #302500; o número usa `PrioridadeNumeroMediana`, próprio para contraste por tema. O amarelo de fechamento sobrepõe inclusive a prioridade persistida Urgente, exibida como Alta, sem modificá-la. Ao reabrir, a apresentação volta à prioridade da comanda.

Ao clicar na alternância, a guarda existente salva a observação pendente antes de aguardar `DefinirFechamentoAsync`. Somente a escrita `DefinirBloqueio` é executada em `Task.Run`; o repositório cria seu contexto EF pela fábrica. `SalvandoFechamento` impede duplicação e desabilita edição, pagamento e saída enquanto a mensagem informa Salvando fechamento… ou Reabrindo comanda…. O salvamento prévio da observação continua seguindo o caminho anterior; esta mudança não torna todas as operações assíncronas.

Sucesso aplica os dados retornados à comanda atual, substitui uma entrada correspondente em `ComandasAbertas` e atualiza `Bloqueada` em um único slot observável. Esse caminho não limpa nem recria `Itens` ou `Slots` e não reconsulta a lista de comandas nem os itens. A atualização lateral evita reconstruir o seletor de responsável quando comanda e responsável são iguais. O caminho síncrono acionado pela impressão também usa essa atualização localizada. Falha mantém o estado persistido anterior, mostra erro e restaura a alternância; os controles voltam a responder ao término. Depois de salvo, Em fechamento continua impedindo alterações nos itens e permitindo pagamento e impressão.

## Fontes conferidas

- `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoView.xaml`: cores, texto, acessibilidade e estado desabilitado do painel.
- `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoView.Operacoes.cs`: guarda de observação, alternância aguardada, responsável e linguagem do menu.
- `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoViewModel.cs`: guardas de saída/pagamento e criação dos slots com estado persistido.
- `backend/src/VarthexComanda.Desktop/Atendimento/AtendimentoViewModel.Operacoes.cs`: escrita assíncrona, exclusão de duplicação, atualização localizada e falha.
- `backend/src/VarthexComanda.Desktop/Atendimento/ComandaSlotItem.cs`: estado observável e situação do cartão.
- `backend/src/VarthexComanda.Desktop/MainWindow.ImpressaoPedido.cs`: início do fechamento após envio aceito da conta e erro específico.
- `backend/src/VarthexComanda.Infrastructure/Persistence/Atendimento/EfComandaRepository.Operacoes.cs`: contexto EF por operação e escrita de `Bloqueada`.

## Evidências e limites

Build em `backend/artifacts/comanda-fechamento`. Resultado focado: 11/11 aprovados em `backend/artifacts/comanda-fechamento-resultados/fechamento.trx`, cobrindo `MenuComandaTests`, `OperacoesComandaRegressaoTests` e o teste de prioridade do localizador. A suíte global não foi reexecutada nesta passagem.

Dez capturas válidas em `.impeccable/review/comanda-fechamento/`, abertas individualmente em uma rodada própria: `comanda-{claro,escuro}-800-bloqueado.png` (duas; nomes legados, conteúdo Em fechamento), `grade-fechamento-{claro,escuro}.png` (duas), `comanda-{claro,escuro}-800-salvando.png` (duas), `menu-imprimir-{claro,escuro}.png` (duas) e `comanda-{claro,escuro}-1180.png` (duas normais). A avaliação usa conteúdo de 800 × 548 e 1180 × 688, sem nova moldura nativa. Outras imagens na pasta não ampliam essa evidência.

A fixture de salvamento comprova o estado visual do componente. Separadamente, o teste com escrita lenta controlada comprova retorno antes de concluir, guarda contra duplicação, tratamento de falha, preservação de item e slot e ausência de consultas de recarga. Não há benchmark real, promessa de latência zero, teste de hardware ou impressão física comprovada.

A revisão independente efetiva abriu as dez capturas e as fontes e retornou `ship` restrito, sem achados materiais e sem fallback. Por ser WPF, não há detector HTML. A versão de `backend/artifacts/comanda-fechamento` foi aberta e conferida na janela principal, bloqueada e aguardando autenticação pelo usuário; isso não comprova teste interativo autenticado das novas funções. O registro não representa aprovação global da interface. O legado de `DESIGN.md` foi preservado; `PRODUCT.md` e `.impeccable/design.json`, ausentes antes, não foram criados nem tiveram seu desvio reparado.
