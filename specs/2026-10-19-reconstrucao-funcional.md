# Reconstrução funcional — primeira entrega

Data: 19/10/2026.

## Objetivo e referência

Evoluir o VComanda no fluxo autorizado: produtos, mesas/comandas, lançamento de consumo e fechamento. A base já implementa produtos, categorias, comandas, quantidades, totais, encerramento e histórico. Esta entrega acrescenta identificação de mesa e instruções de consumo à implementação própria existente.

O Consumer foi identificado anteriormente por arquivos e metadados. Não houve observação de seus fluxos, descompilação, execução, importação de dados ou verificação de equivalência. Esta entrega não representa uma reconstrução integral daquele produto.

## Requisitos e decisões desta entrega

| Código | Comportamento |
| --- | --- |
| RFREC01 | Definir, alterar ou remover a mesa de uma comanda aberta. |
| RFREC02 | Mostrar a mesa na grade, no atendimento e na consulta da cozinha. |
| RFREC03 | Informar observação antes de adicionar o próximo produto; mostrar no consumo, cozinha e histórico. |
| RNREC01 | Mesa opcional, de 1 a 999; vazio representa balcão. Compartilhamento da mesa não une comandas nem totais. |
| RNREC02 | Observação opcional, até 300 caracteres, com espaços externos removidos; vazio equivale a ausência de observação. |
| RNREC03 | Agrupar somente produto, preço e observação iguais. O preço e o nome históricos continuam preservados. |
| RNREC04 | Limpar a observação após gravação bem-sucedida. Em falha, preservar o texto e não confirmar sucesso. |
| RNREC05 | Comanda encerrada não permite alteração de mesa. Alterar mesa não altera itens ou total. |
| RNREC06 | Dados antigos recebem mesa nula; preservar itens e totais por migração aditiva com backup preventivo já existente. |

As escolhas de mesa compartilhada, limite numérico e observação por próximo produto são padrões iniciais de implementação para validação no estabelecimento. Mesa não possui entidade de reserva/ocupação própria nesta entrega. Instruções podem ser corrigidas removendo a linha e lançando-a novamente; não há histórico de correções de pedido.

## Validação

| Código | Evidência automatizada |
| --- | --- |
| CTREC01 | EfComandaRepositoryTests.MesaPersistePodeSerCompartilhadaELiberadaSemAlterarConsumo: persistência, mesa compartilhada, remoção e total preservado. |
| CTREC02 | EfComandaRepositoryTests.MesaInvalidaOuComandaEncerradaNaoPodeSerAlterada: limites e bloqueio após fechamento. |
| CTREC03 | EfComandaRepositoryTests.ObservacoesDiferentesSeparamItensEPersistemAposEncerramento: normalização, agrupamento, separação e histórico após fechamento. |
| CTREC04 | AdicionarItemTests.ObservacaoLongaNaoRegistraConsumo: limite sem lançamento parcial. |
| CTREC05 | AtendimentoViewModelTests: validação da mesa, atualização da grade e aplicação/limpeza da observação com seleção correta da linha. |
| CTREC06 | CarregamentoDeXamlTests.AtendimentoComMesaEObservacaoRenderizaEmDuasResolucoes: tela real WPF em 1280×780 e 800×600 e consulta da cozinha. |
| CTREC07 | Suíte existente de atualização, recuperação, backup, catálogo, histórico e encerramento. |

As prévias de teste usam dados fictícios e ficam em `artifacts/reconstrucao-funcional/previas`. Execução dos testes requer comunicação local do executor de testes; o isolamento desta sessão impediu essa comunicação, e a validação foi executada com a permissão apropriada.

Resultado: 107 testes da aplicação, 4 do domínio, 265 da infraestrutura e 250 da interface. Compilação sem avisos ou erros. Na inspeção visual, a janela de 800×600 exigiu reduzir a largura do catálogo e reorganizar o aviso de saída para preservar a leitura das instruções dos itens. A cozinha continua uma consulta local, sem estados de preparo.

Não aplicar a migração manualmente na base de produção: a inicialização normal já verifica migrações e faz o backup preventivo. Não foi alterado o banco operacional nesta sessão.

## Uso

1. Cadastre categoria e produtos no catálogo existente.
2. Abra uma comanda pelo número.
3. Use “Balcão · definir mesa” para informar a mesa, se aplicável.
4. Digite uma observação antes de selecionar o produto quando precisar de instrução específica.
5. Confira as linhas, quantidades e total. A cozinha consulta mesa e observações.
6. Conclua o lançamento para manter a comanda aberta ou use o fechamento existente após a cobrança.
7. Consulte a venda e as observações no histórico.

## Limites

Pedido independente numerado, fila com estados de preparo/entrega, impressão, integração de pagamentos, emissão fiscal, delivery e rede local não são entregas deste incremento. Não houve alteração de licenciamento nem publicação de instalador. A validação automatizada não substitui o ensaio no estabelecimento.
