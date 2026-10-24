# Balcão e diferenciação visual da Cozinha

Pedido do responsável em 23/10/2026: novo acesso ao lado de Mesas/comandas para pedidos no balcão; distinguir o KDS por cores e destacar observações. Extensão da identidade e dos fluxos existentes, sem novo mundo visual ou rodada de conceitos.

## Contrato de direção

- THESIS: abrir e retomar pedidos sem mesa pelo Balcão e reconhecer situação e observações na cozinha sem depender apenas de cor.
- OWN-WORLD: WPF, Segoe UI, cantos retos, superfícies cinza e recursos dinâmicos da identidade Comanda por Varthex. Cores de preparo são semânticas, autorizadas neste pedido; não recolorem o fundo geral.
- STORY: Principal → Balcão → Novo pedido/lista → editor existente → produtos/observações/pagamento; KDS mantém sua fila, filtros, tempos e ações.
- FIRST VIEWPORT: Balcão ao lado de Mesas/comandas no grupo Pedido; lista com pesquisa e Novo pedido. KDS apresenta legenda, estado por cartão/item e notas legíveis destacadas.
- FORM: extensão local de interface desktop Operate. Código e controles WPF existentes; não há comp aprovado, concept roll, novos rasteres de produto ou mudança de identidade.

## Comportamento

Balcão é a visão de comandas abertas sem número de mesa, conforme o significado que já existe no sistema. Pedidos antigos sem mesa também aparecem. A tela usa uma coleção filtrada própria, sem filtrar a grade de Mesas. A criação usa o primeiro número livre na faixa configurada, incluindo ocupação por mesas, e mantém a proteção de número único na gravação. Faixa lotada recusa a criação. Não cria numeração independente, novo tipo persistido ou nova migração.

O editor existente abre mesmo sem a tela de Mesas estar carregada, preservando catálogo com fotos, quantidades, observações, operador, impressão e pagamento. Novo pedido também atende F2; Enter/duplo clique/botão abrem o selecionado. Busca por cliente, comanda ou pedido, incluindo #. Sem envio automático a terceiros ou alteração dos registros antigos.

KDS: pendente amarelo, em preparo azul, pronto verde; atraso/alta prioridade vermelho, com rótulos e tempos. O resumo do cartão usa o conjunto de itens exibido: todos prontos → Pronto; todos pendentes → Pendente; demais combinações → Em preparo. Filtros continuam limitando as ações ao que é exibido. Observação do pedido e de cada item aparecem em blocos amarelos com título, texto em destaque e filete lateral; vazio/espaços não geram bloco. O nome acessível do item inclui sua observação. Não muda preparo persistido, cobrança ou os limites de atraso existentes.

## Evidências e limites

Capturas WPF reais de fixtures com dados sintéticos em `.impeccable/review/balcao-kds`: Balcão 1280/800, claro/escuro, vazio e editor; KDS 1900/1280/800, claro/escuro, vazio/erro; Ribbon 1280/800 nos dois temas. Não são capturas da base operacional nem do mouse do usuário. Detector HTML/CSS não se aplica a WPF. Não há novo raster de produto para verificar proveniência.

Teste funcional de balcão percorre abertura, filtro, produto/observação, cliente e pagamento; outro abre o editor modal WPF sem Mesas carregadas e lança um produto. Testes KDS verificam resumo, observações, cores aplicadas e contraste mínimo 4,5:1 para texto e 3:1 para filetes nos dois temas.

## Revisão visual e documentação

Extensão local compatível com o mundo Comanda em modo Operate: fontes `BalcaoView.xaml`/`BalcaoViewModel.cs`, `CozinhaView.xaml`/`CozinhaViewModel.cs`, `PaletaComanda.cs` e `MainWindow.Ribbon.cs` preservam WPF, Segoe UI, cantos retos, superfícies neutras e temas dinâmicos. Balcão entra após Mesas / comandas no mesmo grupo Pedido e reutiliza os estilos de tabela e o editor existentes. Corpo de 12 DIP, títulos de 22 DIP e linhas de 34 DIP mantêm a densidade operacional. A nova paleta `Kds*` restringe amarelo, azul, verde e vermelho aos estados e alertas; rótulos escritos e tempos conservam o significado sem depender apenas da cor. Observações usam título funcional, texto semibold de 13 DIP e filete lateral de 4 DIP, preservando neutro o restante do corpo dos cartões.

Evidência visual: 24 PNGs de fixtures WPF com dados sintéticos em `.impeccable/review/balcao-kds`, cobrindo Balcão, editor, KDS, Ribbon, larguras e estados descritos acima. Nesta documentação foram amostrados `balcao-claro-1280.png`, `balcao-escuro-800.png`, `kds-claro-1280.png` e `kds-escuro-800.png`: ações e pesquisa permanecem visíveis; a tabela reduz o nome do cliente com reticências; o KDS conserva legenda, identificação, observações e rolagem em largura reduzida. Não houve nova captura ou execução de testes nesta etapa.

O revisor independente retornou `disposition: ship`, com persistence, fidelity, ceiling, material_fixes e keep completos e sem achado material nas fontes/capturas fornecidas. Os testes de contraste passaram para texto ≥ 4,5:1 e filetes ≥ 3:1 nos dois temas. A contagem final está na seção seguinte. O parecer e as imagens não comprovam homologação operacional, mouse físico, toque, leitor de tela ou DPI variado. Detector HTML/CSS não se aplica ao WPF; não houve novo raster de produto.

Preservados `DESIGN.md`, sua estrutura legada e decisões vigentes, sem promover regras locais a um novo sistema visual. Há deriva histórica preexistente nesse documento, incluindo referências a verde, navegação anterior e cozinha somente de consulta, posteriormente substituídas por revisões; não foi reparada nem canonizada como orientação desta extensão. As ausências preexistentes de `PRODUCT.md` e `.impeccable/design.json` também permanecem. Nenhum código, token global ou documento adicional foi alterado nesta etapa.

## Resultado final

**835 testes aprovados**, somando execuções completas dos cinco projetos: domínio 4, aplicação 135, infraestrutura 310, servidor 12 e desktop 374. A primeira regressão da solução encontrou somente a expectativa antiga de cinco comandos em Principal, agora seis com Balcão. Atualizada essa expectativa, o projeto desktop inteiro foi executado novamente e passou. Evidências em outputs/balcao-kds-regressao-final.log e outputs/balcao-kds-desktop-final.log; TRXs em backend/artifacts/balcao-kds-resultados. Dois avisos xUnit2031 anteriores à mudança permanecem em AtendimentoTecladoXamlTests.

Publicação local Release autocontida win-x64 em artifacts/balcao-kds/publish, registrada em outputs/balcao-kds-publicacao.log. Sem alteração do banco operacional, migração nova, atualização do instalador ou publicação externa. Requisitos: RF33/RN31/CT28 e refinamento RF32/CT27/RNF18.
