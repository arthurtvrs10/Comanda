# Histórico de pedidos

Data: 22/10/2026. Extensão da consulta de pedidos seguindo as três referências Consumer fornecidas pelo usuário. Fonte de implementação: commit `d323ea6`. Esta especificação registra o comportamento do código e as evidências reportadas pela execução principal e pela revisão independente; o documentador não executou aplicativo, testes, launcher ou operações Git.

## Escopo e rastreabilidade

Novo destino Histórico de pedidos em Principal / Vendas, separado do Histórico de vendas em Financeiro. Consulta comandas abertas, finalizadas e, quando solicitado e compatível com a situação escolhida, canceladas. Reutiliza os registros existentes, sem migração ou alteração de schema. O histórico financeiro conserva seu fluxo e sua definição de venda.

Rastreabilidade da extensão: RF30, UC11 e CT25, registrados na documentação funcional específica, fora da matriz gerada CT01–CT22. Preserva RN04, RN05, RN10, RN20, RN22 e RN23, além de RNF01, RNF07 e RNF18. Não encerra homologação humana desses contratos.

A navegação atende aos perfis de atendimento e gestor. `MainWindow` conserva as guardas de bloqueio, cozinha e restauração, e o callback de abertura usa o atendimento existente. A exibição de um comando na Ribbon não concede acesso: as regras de autenticação e encaminhamento ao login continuam sendo as do aplicativo.

## Consulta e filtros

`ConsultarHistoricoPedidos` valida o filtro e delega a `EfHistoricoPedidosRepository`. A consulta EF usa `AsNoTracking`; não grava comandas, itens, vendas ou configurações no banco. O código identificador é `Comanda.CodigoPedido ?? Comanda.Id`, distinto do número reutilizável da comanda.

O período inicial é Hoje, de 00:00 a 23:59, em Brasília. O repositório converte as datas locais com `FusoBrasilia` para UTC, usando o deslocamento vigente do aplicativo, UTC−3. De é inclusivo; Até inclui o minuto inteiro informado, implementado como limite exclusivo do minuto seguinte. A data pode ser Abertura ou Fechamento; esta última exige que exista data de fechamento no período. Horários usam HH:mm. Período invertido, datas/horários inválidos e opções inválidas retornam mensagem de validação.

| Opção | Seleção efetiva |
|---|---|
| Todos os pedidos | Todas as situações no período; cancelados dependem da opção explícita |
| Abertos | `StatusComanda.Aberta`, incluindo Em fechamento |
| Finalizados | `StatusComanda.Fechada` |
| Finalizados e pagos | Comanda fechada com uma venda `StatusVenda.Concluida` |
| Exibir pedidos cancelados | Remove a exclusão inicial de cancelados; não sobrepõe a restrição da situação escolhida |

Em fechamento deriva de `Comanda.Bloqueada` enquanto a comanda está aberta. Não cria uma nova situação persistida. Os rótulos da tabela são Em aberto, Em fechamento, Finalizado e Cancelado.

Cliente pesquisa o nome registrado. Colaborador pesquisa Criado por ou Responsável. Esses filtros são aplicados em memória após as restrições SQL de período, situação e códigos, com `StringComparison.OrdinalIgnoreCase`: ignora maiúsculas/minúsculas também nas letras acentuadas; não remove acentos. Nome do cliente aceita até 80 caracteres e colaborador até 120.

Códigos aceita números positivos, com exemplo separado por vírgula; o parser também admite ponto e vírgula, espaços e quebras de linha, além do prefixo `#`. Valida no máximo 1.000 entradas e elimina duplicatas. Não há filtros de fiado, UTM, endereço ou telefone, pois esses dados não existem no modelo atual. Origem utiliza somente `OrigemCliente` já registrada.

Os atalhos Hoje, Mês atual, Últimos 7 dias, Ontem, Mês anterior e Últimos 30 dias preenchem datas e horários completos de 00:00 a 23:59. Os intervalos de 7/30 dias incluem o dia atual; Mês atual alcança o último dia do mês. Pesquisar aplica somente após validação; Voltar/fechar mantém o filtro anterior. Atualizar repete o filtro vigente. Se a consulta falhar, a mensagem orienta nova tentativa e os resultados anteriores são preservados.

## Composição visual e operação

Extensão nativa Windows/WPF do sistema Consumer existente, modo Operate, sem nova identidade visual. Segoe UI de 12 DIP, `Language="pt-BR"`, cantos retos e recursos dinâmicos `AjudaCanvas`, `AjudaSurface`, `AjudaInk`, `AjudaMuted`, `AjudaLine` e `AjudaSoft`. A referência orienta hierarquia e distribuição; não incorpora marca, imagem ou ativo externo.

A barra contém resumo clicável do filtro/Pesquisar, Atualizar, Mais colunas, Exportar Excel e Imprimir, com vetores de 22 DIP. Usa quebra de linha; o resumo tem reticências e tooltip com o filtro completo. A tabela é somente leitura, com cabeçalhos de altura mínima de 36 DIP, linhas de 34 DIP, rolagem horizontal/vertical e seleção azul `#176BD1` com texto branco. Cabeçalhos têm indicação de ordenação; o DataGrid mantém a ordenação nativa e permite reorganizar e redimensionar colunas.

Quantidade e total monetário aparecem abaixo da tabela. Somam os registros carregados pelo filtro, inclusive abertos e cancelados quando selecionados; não são receita de vendas concluídas. Sem resultados, a tela informa Nenhum pedido encontrado para os filtros selecionados e desabilita exportação/impressão.

Pesquisar usa janela de 500 × 635 DIP, mínimo de 440 × 480, com corpo rolável e rodapé Voltar/Pesquisar. Exibir / ocultar colunas usa 350 × 590 DIP, mínimo de 320 × 400, árvore rolável e rodapé Voltar/Aplicar. Ambos reutilizam `DialogoCompacto`, recursos neutros e o cabeçalho cinza compartilhado sem marca. As dimensões são unidades lógicas WPF, não garantia de tamanho físico em qualquer escala de tela.

Atalhos: Ctrl+F abre pesquisa, F5 atualiza, Ctrl+E exporta e Ctrl+P abre a prévia de impressão. Enter na linha selecionada, duplo clique em uma linha ou Abrir apresenta os detalhes. Os campos têm nomes de automação e rótulos associados; os diálogos mantêm ação padrão e cancelamento do padrão compartilhado. Não foi executada homologação com teclado físico ou leitor de tela nesta extensão.

## Colunas e preferências locais

| Grupo em Mais colunas | Campos disponíveis |
|---|---|
| Dados do cliente | Cliente |
| Totais do pedido | Total, Tot. itens |
| Pagamento | Pagamento |
| Outras informações | Código, Data abertura, Status, Criado por, Tipo, Data fechamento, Comanda, Mesa, Responsável, Origem, Pessoas, Observação |

Inicialmente são visíveis Código, Data abertura, Status, Criado por, Cliente, Tipo, Data fechamento e Total. Código permanece obrigatório, com controle desabilitado e explicação. Tot. itens é a contagem de linhas de itens registradas, não a soma das quantidades. Tipo distingue Mesa/Balcão; pagamento representa a forma da venda concluída relacionada, quando houver. Ausências de autoria, origem e pagamento usam Não informado/Não informada, sem inventar cadastros.

Agrupar por oferece Criado por, Origem, Tipo e Pagamento, com cabeçalhos de grupo e contagem. Auto ajustar colunas reutiliza `AlternanciaCompacta`: ligado distribui a largura proporcionalmente; desligado usa as larguras definidas para cada campo. A ordenação e a reorganização atuais da tabela são respeitadas pelas saídas.

`historico-pedidos.json` fica na raiz de dados selecionada por `AppPaths`, separando os perfis normal e demonstração. Persiste visibilidade, agrupamentos e ajuste automático, sem tocar no banco. Não persiste o filtro, as larguras manuais nem a ordem arrastada das colunas. A escrita usa temporário e substituição; falha ao salvar preserva a aplicação visual e informa que a preferência não pôde ser salva. JSON ilegível retorna ao padrão com registro de aviso.

## Detalhes e saída dos dados

Abrir busca os detalhes da comanda e mostra cabeçalho, autoria, cliente, tipo/mesa, pagamento, total e itens registrados com nome, quantidade, medida, observação e subtotal. Os itens utilizam os dados armazenados no pedido, preservando o snapshot do lançamento. A janela é somente leitura, de 850 × 540 DIP, mínimo de 650 × 400. Abrir comanda é oferecido apenas se o registro continua aberto e existe callback de atendimento. Não reabre nem altera pedidos finalizados ou cancelados.

Exportar Excel usa seleção de destino nativa e produz `.xlsx` com a aba Pedidos, apenas colunas visíveis na ordem atual e pedidos na ordenação exibida. Código, contagens e total são células numéricas; datas são texto em dd/MM/yyyy HH:mm. Texto utiliza `inlineStr`, inclusive valores iniciados por `=`, sem gerar fórmulas. A gravação conclui em temporário antes de substituir o destino, com limpeza e mensagem de erro em falha; cancelar a seleção não escreve o arquivo.

Imprimir cria `FlowDocument` com título, filtro, tabela e totais das mesmas colunas/pedidos. Abre prévia de 1000 × 680 DIP, mínimo de 600 × 400, e exige Selecionar impressora e imprimir para apresentar o diálogo nativo, configurado em paisagem. O fluxo não envia impressão ao abrir a prévia. A estrutura do documento foi testada; nenhuma impressão física foi executada.

## Evidências e revisão

A execução principal reportou regressão completa aprovada em Debug, com 762 testes distintos: 4 Domain, 133 Application, 303 Infrastructure e 322 Desktop. Logs em `outputs/historico-pedidos-regressao.log` e TRX em `backend/artifacts/historico-pedidos-regressao/`. Os focados foram 8 Infrastructure e 4 Desktop, registrados em `backend/artifacts/historico-pedidos-resultados/historico-sqlite.trx` e `historico-desktop.trx`; a recaptura passou 1/1 em `historico-recaptura.trx`. Focados e recaptura repetem testes e não ampliam o total distinto.

A revisão independente abriu as três referências fornecidas em arquivos temporários de clipboard e as oito capturas abaixo em `.impeccable/review/historico-pedidos/`:

| Captura | Estado conferido |
|---|---|
| `historico-False-1180.png` | Histórico claro, largura ampla |
| `historico-True-800.png` | Histórico escuro, largura estreita |
| `historico-pesquisar-False.png` | Pesquisa clara |
| `historico-pesquisar-True.png` | Pesquisa escura |
| `historico-colunas-False.png` | Mais colunas clara |
| `historico-colunas-True.png` | Mais colunas escura |
| `historico-agrupado-False.png` | Histórico agrupado claro |
| `historico-vazio-True.png` | Histórico vazio escuro |

São capturas nativas de fixtures descartáveis, sem ImageGen. A primeira captura agrupada estava prematura; foi refeita após `ApplicationIdle` e aprovada, sem mudança de interface. A revisão retornou `ship`, sem achados materiais, restrita ao histórico e aos dois modais de pesquisa/colunas. A pasta local de revisão não é versionada e não substitui as fotos publicadas no README. Não foi aplicado detector HTML/CSS à superfície WPF.

O arquivo local `historico-exportado.xlsx`, na mesma pasta de revisão, foi aberto pela execução principal com openpyxl: aba Pedidos, duas linhas incluindo cabeçalho e quatro colunas. A linha de dados contém código 2 numérico, data 22/10/2026 09:01 textual, cliente `=HYPERLINK("teste") 🧾` textual e total 12,34 numérico; não há nós de fórmula. O teste do `FlowDocument` verificou duas linhas de tabela, sem acionar impressora.

## Limites e continuidade

Esta extensão não republica nem substitui a pré-release 1.4.0 existente. Os metadados da compilação de desenvolvimento ainda indicam 1.4.0; esse número não demonstra que os instaladores anteriores contêm o novo histórico. A execução principal abriu e ativou a janela de `backend/artifacts/historico-pedidos/bin/VarthexComanda.Desktop/debug/VarthexComanda.exe` e confirmou em captura o título 1.4.0 e o acesso visível Histórico de pedidos em Principal / Vendas. Não autenticou a sessão nessa conferência. A instância publicada anterior já estava fechada, sem necessidade de encerrar trabalho. Isso não comprova instalação, impressão física ou teste em equipamento do estabelecimento.

A execução principal confirmou aprovação de `docs/scripts/validate.sh`, com a etapa da CLI SQL ignorada por ausência de `sqlite3`; a regressão Infrastructure aprovada inclui SQLite real. A leitura dos TRX pela execução principal confirmou 4/133/303/322 testes e nenhuma falha. Não houve alteração de código após a revisão `ship`. As evidências acima pertencem aos testes e à revisão reportados, sem nova execução pelo documentador. O trabalho documental desta etapa limita-se a este arquivo e ao acréscimo correspondente em `DESIGN.md`. Preserva o formato legado e as ausências preexistentes de `PRODUCT.md` e `.impeccable/design.json`; não repara drift nem amplia a aprovação visual aos outros módulos.
