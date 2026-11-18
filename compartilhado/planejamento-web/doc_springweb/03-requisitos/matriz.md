# Requisitos e rastreabilidade
Situação: proposta para estimativa. Cada linha liga capacidade C, história H, sprint S, teste T e futuro registro de homologação (P01 ou P02). RF01 e RF12 descrevem atividades assistidas do piloto. A confirmação de comportamento legado está no inventário.

| RF / origem | Ator e ação | Aceite Dado / Quando / Então | Rastro |
| --- | --- | --- | --- |
| RF01 / C01 | Fundador pode ouvir caixa e garçom e contar correções em dez pedidos | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Linha de base e método de identificação registrados | H01 · S01 · T01 · P01 |
| RF02 / C02 | Gestor pode ativar local e atribuir perfil de atendente, caixa ou gestor | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Atendente de A recebe recusa ao consultar conta de B | H02 · S02 · T02 · P01 |
| RF03 / C03 | Gestor pode cadastrar categoria, foto, preço, UN/KG e desativar produto | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: 0,250 kg a R$ 40/kg soma R$ 10 e preço histórico permanece | H03 · S02 · T03 · P01 |
| RF04 / C04 | Garçom pode abrir conta pelo código da pessoa, com mesa e nome opcionais | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Duas pessoas na mesma mesa recebem contas distintas; código ocupado retorna conflito | H04 · S03 · T04 · P01 |
| RF05 / C05 | Garçom pode lançar, corrigir quantidade, remover item e anotar observações | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Reenvio após resposta perdida produz um item; outra conta permanece intacta | H05 · S03 · T05 · P01 |
| RF06 / C06 | Caixa pode registrar pagamento externo, parcial ou misto, conferir troco e finalizar | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Receber 18 de Ana encerra só Ana; Bia segue com 25; repetição mantém uma venda | H06 · S04 · T06 · P01 |
| RF07 / C07 | Caixa pode emitir conta, cozinha e fichas; reimprimir com identificação | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Falha no papel permite reimpressão sem outra venda nem novo envio à produção | H07 · S05 · T07 · P01 |
| RF08 / C08 | Gestor pode filtrar pedidos e vendas por turno e conferir recebimentos | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Exportação e histórico reproduzem totais, formas e operador originais | H08 · S05 · T08 · P01 |
| RF09 / C09 | Atendente pode usar Local (Mesa), Salão, Balcão, Para levar e Delivery, painel de senhas e acesso móvel | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Cinco tipos persistem ao reabrir; dois terminais atendem com WAN desligada | H09 · S06 · T09 · P01 |
| RF10 / C10 | Gestor pode configurar estabelecimento, limites, numeração, acesso local e edição | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Base normal vazia e demos isoladas; reiniciar preserva configuração e catálogo | H10 · S06 · T10 · P01 |
| RF11 / C11 | Administrador pode importar cópia SQLite, restaurar backup e retomar com conciliação | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Contagens, totais e recebimentos conciliam; segundo ensaio não duplica registros | H11 · S07 · T11 · P01 |
| RF12 / C12 | Fundador e caixa pode acompanhar turno, treinar operador e decidir continuidade | Dado um ambiente autorizado com massa do cenário, quando o ator executa a ação indicada, então: Ata registra autonomia, saldos e decisão do local com evidência | H12 · S08 · T12 · P02 |

## Regras de negócio
| ID | Regra e exemplo | Exceção ou tratamento |
|---|---|---|
| RN01 | Um número ativo por estabelecimento identifica uma comanda. Ana e Bia podem ocupar a mesma mesa. | Reutilização após encerramento ganha novo ID histórico. Contas de grupo importadas preservam a origem. |
| RN02 | Preço e quantidade registrados no item determinam subtotal em centavos. | Para KG, conferir arredondamento contra amostras do legado; bloquear migração diante de diferença. |
| RN03 | Só fechar quando saldo for zero. R$ 10 em Pix + R$ 8 aplicados em dinheiro quitam R$ 18. | Troco é diferença entre recebido e aplicado em dinheiro. Excesso aplicado é recusado. |
| RN04 | Recebimento ativo bloqueia alteração de itens. | Caixa pode cancelar o registro manual com confirmação e trilha; devolução externa é tratada separadamente. |
| RN05 | Operador autenticado é autor dos lançamentos. | Correção mantém autoria e motivo; conta fechada conserva seus registros. |

## Requisitos não funcionais propostos
| ID | Cenário e meta | Medição e aceite |
|---|---|---|
| RNF01 | Duas sessões, 100 repetições da mesma operação, um efeito financeiro | Dev compara contagens e saldo em PostgreSQL antes/depois. |
| RNF02 | WAN desligada por 30 min, LAN e servidor disponíveis, concluir as cinco tarefas do ensaio | Caixa compara diário e vendas; perda de LAN deve exibir estado incerto e recuperar pelo mesmo identificador. |
| RNF03 | Zero acesso entre locais e zero alterações de preço por garçom | Gestor revisa testes com dois locais e três papéis, inclusive acesso direto por ID. |
| RNF04 | Busca e atualização visível p95 até 2 s em LAN com 10 sessões | Dev mede 100 operações em equipamento do piloto e registra configuração. |
| RNF05 | Cinco tarefas por teclado e por celular; quatro de cinco sem ajuda | Operador registra conclusão, foco, erros e tempo no P01. |
| RNF06 | Recuperação proposta em 60 min e perda máxima de 15 min em pane | Restaurar cópia em outra máquina, medir tempos e conciliar diário do intervalo. Aprovação do dono pendente. |
| RNF07 | Cada fechamento rastreável por operador, local, ID de operação e venda | Caixa correlaciona dez vendas, sem dados de cartão nos eventos. |

## Casos de uso e permissões
UC1. Garçom autenticado inicia atendimento ao receber o primeiro pedido. Procura código, verifica contexto da pessoa e abre conta. Código ativo leva à conferência; código ocupado por outra pessoa exige seleção explícita. Resultado: conta aberta identificada.
UC2. Garçom seleciona pessoa e produto ativo, confere quantidade, confirma e recebe saldo. Produto indisponível exige nova escolha. Edição concorrente exige recarregar. Resultado: item vinculado à conta e autoria persistida.
UC3. Caixa autenticado localiza conta, lê itens em voz de conferência se necessário, confirma recebimento externo e encerra em saldo zero. Resposta perdida consulta a mesma operação. Saldo positivo mantém aberta. Resultado: uma venda e um comprovante reimprimível.
UC4. Gestor prepara catálogo, operadores, configuração e relatórios; administrador autorizado ensaia restauração em instância isolada. Garçom acessa atendimento e consulta de sua loja, caixa também registra recebimentos e fecha, gestor administra as permissões. Definir segregação final com o dono.

Glossário: pessoa = participante atendido sem exigir cadastro civil; comanda = registro de consumo com identidade própria; mesa = localização opcional; venda = fotografia financeira final; recebimento = registro de valor confirmado externamente; turno = janela operacional acordada com a loja.
