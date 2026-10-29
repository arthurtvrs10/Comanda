# Protótipo de fluxo de pagamento — 28/10/2026

Pedido: melhorar, prototipar e validar a qualidade do pagamento. Na confirmação de escopo, o usuário apontou **o fluxo** como problema principal.

## Direção

Superfície de operação para o atendente do caixa, preservando o tema desktop do Comanda. Recebimentos continuam manuais: o sistema registra o que foi recebido fora dele, sem processar Pix ou cartão.

Hipótese: **saldo → forma → valor → confirmar recebimento → finalizar quando quitado**, na mesma tela, reduz a necessidade de alternar janelas e interpretar totais repetidos. A confirmação do recebimento é uma ação explícita, separada da finalização da venda. Divisão e observação ficam disponíveis sob demanda.

O experimento não substitui `EncerramentoView`, não muda as regras publicadas e não é uma nova release. O módulo de complementos também continua fora da publicação. Não houve commit ou push deste protótipo.

## O que foi prototipado

- Saldo e orientação do próximo passo no topo. Ao quitar, o destaque passa a mostrar o total recebido, evitando apresentar zero como se fosse o valor recebido.
- Dinheiro, Pix, débito e crédito junto do valor na própria tela. Atalhos Alt+1 a Alt+4 escolhem a forma; digitar ou escolher uma forma nunca registra por si só.
- Confirmação do recebimento sempre visível no rodapé, inclusive na largura de 820 px. O conteúdo secundário pode rolar; a ação principal e a finalização permanecem acessíveis.
- Troco separado do valor aplicado à conta. Recebido vazio significa dinheiro exato, com explicação visível.
- Histórico visível e saldo atualizado após cada confirmação. A próxima parcela exige escolher a forma novamente, impedindo que o segundo clique registre automaticamente o saldo restante.
- Calculadora de divisão do **saldo restante** por pessoas, com distribuição exata de centavos. Ela sugere valores, sem fingir identificar ou quitar pessoas automaticamente.
- Divisão por linhas inteiras de itens, preservando a regra atual. Linhas pagas não são selecionadas novamente.
- Correção de registro com confirmação no próprio histórico. Remover um registro não faz devolução externa e preserva a auditoria já existente.
- Finalização liberada somente após recebimentos registrados quitarem a conta. Uma nova execução do comando não duplica a venda.
- Falha de gravação ou conflito de saldo reconsulta a comanda e pede conferência, sem reenvio automático.

## Como experimentar

Executável separado: `backend/artifacts/pagamento-prototipo-build/VarthexComanda.PagamentoPrototipo.exe`.

O programa abre uma base SQLite fictícia em uma nova pasta `vcomanda-pagamento-prototipo-*` no diretório temporário do Windows. Não consulta a base, a licença, os clientes ou os pedidos reais da instalação. A inicialização operacional do aplicativo e seu servidor não são acionados. Não há impressão nem cobrança real no protótipo.

Na barra de simulação, escolha conta de R$ 32,00 ou R$ 100,01, reinicie a simulação ou alterne os temas. Voltar ao pedido e continuar o pagamento recarrega os recebimentos gravados na base fictícia. Reiniciar cria outro pedido fictício, sem apagar dados do estabelecimento.

## Validação técnica concluída

**42 testes aprovados:** 29 testes de desktop (protótipo, fluxo existente, ViewModel e homologação de recebimentos mistos) e 13 testes de persistência e falhas de pagamento/encerramento. Os 13 casos do protótipo fazem parte dos 29; não são contados novamente.

Os testes novos usam SQLite real em base descartável. Cobrem:

| Cenário | Resultado verificado |
| --- | --- |
| Forma escolhida, sem confirmação | Nenhum recebimento gravado |
| R$ 10,00 Pix e R$ 22,00 dinheiro, recebido R$ 50,00 | Saldo zero; troco R$ 28,00; dois registros; uma venda |
| Duplo clique após parcela parcial | Apenas a primeira parcela é registrada; uma nova forma deve ser escolhida |
| Vazio, zero, negativo, texto, três casas decimais e valor acima do saldo | Registro bloqueado e base preservada |
| Dinheiro insuficiente, excedente e recebido exato | Bloqueio, cálculo de troco e valor exato corretos |
| R$ 100,01 dividido entre três | R$ 33,34 + R$ 33,34 + R$ 33,33, sem perder centavos |
| Voltar/reabrir após receber R$ 10,00 | Pago R$ 10,00 e saldo R$ 22,00 preservados |
| Corrigir registro | Confirmação obrigatória; cancelamento auditado; itens preservados |
| Selecionar itens já pagos | Seleção rejeitada |
| Pessoa sem permissão | Nenhum recebimento ou venda gravado |
| Outro atendimento registra antes da confirmação | Saldo reconsultado; tentativa excedente não gravada nem reenviada |
| Finalizar novamente | Uma única venda |

Revisão visual em duas passagens, nos temas claro/escuro e larguras 820/1120, com estados inicial, dinheiro com troco, parcial e quitado. Prévias em `backend/artifacts/pagamento-prototipo-tests/previas-pagamento-prototipo/`.

Comandos de referência:

```powershell
dotnet test backend/tests/VarthexComanda.Desktop.Tests/VarthexComanda.Desktop.Tests.csproj --no-restore -m:1 -p:UseSharedCompilation=false -p:OutDir=C:/Users/arthu/Desktop/VComanda/backend/artifacts/pagamento-prototipo-tests/ --filter 'FullyQualifiedName~PagamentoPrototipoTests|FullyQualifiedName~PagamentoFluxoTests|FullyQualifiedName~PagamentosMistosTests|FullyQualifiedName~EncerramentoViewModelTests'
dotnet vstest backend/artifacts/pagamento-prototipo-tests/VarthexComanda.Infrastructure.Tests.dll '--TestCaseFilter:FullyQualifiedName~Pagamento|FullyQualifiedName~Encerramento|FullyQualifiedName~Recebimento'
```

A compilação reutiliza as dependências existentes. A consulta de vulnerabilidades do NuGet apresentou NU1900 por indisponibilidade da origem nesta sessão; isso não foi tratado como uma auditoria de dependências aprovada.

## Validação com pessoas — pendente

Os testes automáticos comprovam comportamento, não facilidade de uso no caixa. Antes de integrar, executar sem orientar a pessoa sobre onde clicar:

1. Conta de R$ 32,00, recebida integralmente em Pix. Meta do protótipo: escolher Pix, confirmar recebimento, finalizar — três ações, sem outra janela.
2. Conta de R$ 32,00 em dinheiro com R$ 50,00 entregues. Pedir que a pessoa diga quanto devolver antes de concluir.
3. Receber R$ 10,00 em Pix e o restante em dinheiro; voltar ao pedido no meio e retomar.
4. Dividir R$ 100,01 entre três, verificando os centavos. O número de pessoas é ajustado para as que ainda vão pagar quando se recalcula o saldo restante.
5. Registrar uma forma errada e corrigir, observando se a pessoa entende que o sistema não realiza estorno.

Registrar tempo até concluir, quantidade de ações, pedidos de ajuda, tentativas de finalizar antes de quitar e qualquer registro acidental. Metas: zero registro involuntário, zero perda de centavos e entendimento do próximo passo sem explicação nos cenários comuns. Não há medição humana de tempo ou taxa de sucesso concluída nesta etapa.

Também ficam pendentes uso por teclado/leitor de tela em equipamento do caixa, impressão física, integração com os comandos de impressão existentes e tratamento visual de gravações lentas. A tela publicada deve permanecer até a aprovação do fluxo e a homologação dessas integrações.

## Variação B — fluxo guiado

Solicitação posterior: criar outra variação, preservando a primeira para comparar.

- **Modelo A — direto:** forma, valor e histórico na mesma tela; confirmação do recebimento sem etapa intermediária.
- **Modelo B — por etapas:** escolher forma → informar valor → conferir forma/valor/troco → confirmar recebimento. Após uma parcela, apresenta o recebimento salvo e oferece receber o restante. Depois de quitar, a ação passa a finalizar o pedido.
- O resumo e os recebimentos ficam à esquerda; a área principal apresenta somente a tarefa da etapa atual. É possível voltar uma etapa ou trocar a forma antes de registrar.
- Conferir não grava. Somente a confirmação explícita usa a mesma gravação validada do modelo A. Correção confirmada de uma conta quitada retorna à escolha da forma.
- O botão principal rejeita o segundo clique de um duplo clique e repetição de Enter/Espaço, para não atravessar duas etapas com um único gesto involuntário.
- O seletor A/B na barra de simulação troca o modelo sem criar outro pedido nem apagar recebimentos. Reiniciar simulação continua sendo uma ação separada.

Executável atualizado para comparação: `backend/artifacts/pagamento-variacao-b-build/VarthexComanda.PagamentoPrototipo.exe`, abrindo no modelo B. A primeira compilação permanece disponível em sua pasta original.

Validação desta variação: **32 testes de desktop aprovados**, incluindo os testes anteriores e três casos novos: recebimento misto guiado com conferência sem gravação antecipada, preservação/correção ao alternar os modelos e renderização das etapas em 820/1120 px nos dois temas. Os 13 testes de persistência da etapa anterior continuam registrados acima; não foram contados novamente nesta execução. Prévias em `backend/artifacts/pagamento-variacoes-tests/previas-pagamento-prototipo/b-*.png`.

Hipótese para comparar com pessoas: A pode exigir menos ações no pagamento comum; B pode ajudar quem precisa conferir uma coisa por vez. No Pix integral, A propõe três ações e B quatro, sem outra janela. Ainda não há resultado de teste de usabilidade com pessoas para declarar uma variação superior. Aplicar os mesmos cenários e registrar erros, pedidos de ajuda, tempo e preferência antes de integrar ao sistema publicado.
