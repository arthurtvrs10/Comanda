# Fluxo de pagamento adaptado à operação

Solicitação de 22/10/2026: referências de pagamento e orientação posterior para ajustar às necessidades do Comanda. Extensão aprovada para recebimentos manuais parciais/mistos, troco em dinheiro e divisão calculada por pessoas. Cobrança permanece externa. Fiado/clientes, taxas/descontos e divisão por item não fazem parte desta etapa.

Extensão de RF12/RF19, CT09/CT10/CT12 e RNF07/RNF18/CT17: dinheiro em centavos, itens e preços históricos preservados. Recebimentos positivos em dinheiro, Pix, débito e crédito, sem NSU, autorização, bandeira ou dados de cartão. Rejeita excedente e mais de duas casas decimais. Troco separa valor recebido do valor aplicado. Rateio distribui os centavos restantes sem alterar a conta.

Cada recebimento é salvo na comanda antes do fechamento. Voltar/reabrir não apaga registros. Remover exige confirmação, mantém registro cancelado no JSON e não faz devolução externa. Com recebimento ativo, editar itens e cancelar a comanda são bloqueados até corrigir os registros. Fechamento incompleto é rejeitado no repositório. Histórico exibe as formas combinadas e a conta impressa detalha recebimentos e saldo.

Migração AddRecebimentosComandaVenda adiciona pagamentos_json em comanda e venda, padrão [] para registros antigos. Sem alterar dados históricos ou totais. Backup antes de migração segue o inicializador existente. Retorno a versão anterior requer backup anterior. Não executar migração manualmente na base do usuário durante o desenvolvimento.

Validação: testes com SQLite real de reabertura, fechamento misto, troco, remoção lógica, idempotência e rejeição de excedente, além de ViewModels, temas, impressão e teclado. Resultados em backend/artifacts/pagamento-resultados; logs em outputs/pagamento-misto-completo.log. Impressão física e equipamento do caixa ainda não homologados.

Resultado final: 786 testes em Release aprovados (4 Domain, 134 Application, 303 Infrastructure, 345 Desktop). Documentação validada; SQLite CLI ausente, testes Infrastructure usam SQLite real. A primeira passagem encontrou expectativas antigas de persistência/rastreabilidade e um mock sem o novo método de recebimento; foram atualizados, sem ignorar testes.


## Extensão — seleção de itens (23/10/2026)

Autorizada pelo usuário após a referência de “Selecionar para dividir”. A conferência continua recolhida até o toggle esquerdo ser ativado. Dentro dela, o operador ativa a seleção e marca linhas inteiras; o valor é calculado automaticamente. A divisão igual por pessoas permanece disponível. Não há fracionamento da quantidade de uma mesma linha nesta etapa.

O recebimento armazena os IDs dos itens no JSON existente, preservando compatibilidade com registros anteriores. O repositório valida pertencimento à comanda, soma dos subtotais e ausência de itens já pagos. Itens pagos ficam indisponíveis ao reabrir; cancelar o registro libera a seleção. Recebimentos avulsos continuam abatendo o saldo global, e uma seleção acima do saldo é bloqueada e explicada. Valores monetários permanecem em centavos.


## Fluxo conforme referências — 23/10/2026

Tela principal com conferência opcional à esquerda, formas no centro e resumo de totais/pagamentos à direita. Clicar em uma das quatro formas existentes (dinheiro, Pix, débito e crédito) abre o lançamento separado, com valor, atalhos, observação opcional e confirmação do recebimento. Dinheiro inclui recebido e troco; atalhos monetários preenchem recebido em dinheiro e o valor nas demais formas, com Shift para somar. A cobrança permanece externa.

Salvar grava o recebimento imediatamente e retorna ao resumo. A seleção de itens preserva o valor calculado. A lista mostra forma, observação, pago, recebido e troco; excluir continua exigindo confirmação e não faz estorno externo. O rodapé apresenta pago, saldo e progresso. As ações de finalizar ficam desabilitadas até quitar tudo; “Finalizar e imprimir” primeiro conclui a venda e depois abre a impressão, evitando registrar novamente se a impressão falhar. Sem fiado, taxas, descontos ou bandeiras sem suporte operacional nesta etapa.

Validação adicional: teclado abre o lançamento sem confirmar recebimento; janela exige confirmação explícita, persiste observação e troco, atualiza progresso e cancela sem registrar. Capturas nos dois temas e larguras existentes.
