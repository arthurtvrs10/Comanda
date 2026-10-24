# Regras de negócio

Regras do produto atual. Regras futuras RE01–RE10 estão no [plano de evolução](21-evolucao-atendimento.md); não alteram os fluxos publicados até uma entrega validada.

| Código | Regra |
| --- | --- |
| RN01 | Somente uma comanda aberta pode usar determinado número |
| RN02 | O número pode ser reutilizado após fechamento ou cancelamento |
| RN03 | Somente produto ativo pode ser incluído em nova comanda |
| RN04 | O item copia nome, preço por medida e unidade de venda praticados no lançamento |
| RN05 | Alterar preço ou medida atuais não modifica itens ou vendas anteriores |
| RN06 | Quantidade é positiva; UN exige inteiro, KG/L aceitam até três casas decimais |
| RN07 | Subtotal em centavos é preço por medida multiplicado pela quantidade e arredondado para inteiro, com meio centavo afastado de zero |
| RN08 | Total da comanda é a soma dos subtotais válidos |
| RN09 | Comanda vazia não pode gerar venda |
| RN10 | Comanda fechada é imutável no MVP |
| RN11 | O MVP não processa nem registra pagamento, forma, valor recebido ou troco |
| RN12 | A tela final exibe produto, quantidade, preço unitário, subtotal e total |
| RN13 | O operador somente encerra após confirmar cobrança externa |
| RN14 | Encerrar grava venda e situação da comanda atomicamente |
| RN15 | Falha no encerramento desfaz todas as gravações da operação |
| RN16 | Desativar produto ou categoria preserva o histórico |
| RN17 | Produto com histórico não pode ser excluído fisicamente |
| RN18 | Cancelar comanda com itens exige confirmação e não gera venda |
| RN19 | Restaurar backup exige confirmação e cópia preventiva da base atual |
| RN20 | Datas operacionais são persistidas em UTC e exibidas no horário de Brasília, em padrão brasileiro |
| RN21 | Uma segunda instância do aplicativo não pode acessar a mesma base enquanto a primeira estiver ativa |
| RN22 | Reiniciar o aplicativo não fecha cancela nem altera uma comanda que estava aberta |
| RN23 | Cada alteração confirmada de item deve ser persistida antes de a interface indicar sucesso |

## Invariantes de dados

RN28: nome do cliente é opcional, limitado a 80 caracteres sem controles/quebras de
linha. Espaços nas pontas são removidos; vazio remove o nome. Somente comandas
abertas podem ser renomeadas. O nome fica no histórico da sessão e não é reutilizado
automaticamente quando o mesmo número abre uma nova comanda.

RN24–RN27: prazos fixos de licença, bloqueio por licença inválida/vencida,
preservação de dados e detecção de atraso do relógio estão definidos em
[Licenciamento offline](20-licenciamento-offline.md).

- `subtotal_centavos = Round(preco_unitario_centavos * quantidade, 0, AwayFromZero)`, com quantidade lógica decimal;
- no SQLite, quantidade e contadores de impressão são inteiros em milésimos; UN exige múltiplo de 1000;
- `comanda.total_centavos = soma(item_comanda.subtotal_centavos)`;
- `venda.total_centavos = comanda.total_centavos` revalidado no encerramento;
- uma comanda possui no máximo uma venda;
- uma venda não possui pagamento no MVP;
- nenhuma comanda com status diferente de `ABERTA` recebe alterações de itens;
- nenhum número aparece em duas comandas abertas simultaneamente.
- encerramento inesperado não cria venda nem muda automaticamente o status da comanda;
- uma venda só existe após a confirmação explícita e o commit do encerramento.

## Arredondamento

Preço por medida continua em centavos inteiros. Cada subtotal é arredondado uma
única vez para centavos com `MidpointRounding.AwayFromZero`; o total soma os
subtotais já arredondados. Por exemplo, R$ 1,00 × 1,005 KG resulta em R$ 1,01.
UN não aceita fração. Ticket médio usa divisão do total em centavos pela quantidade
de vendas e deve ser apresentado com duas casas decimais.

## Extensão de backup — 22/10/2026

O backup local validado é criado antes do envio opcional ao Google Drive.
Falha de autorização, rede, espaço ou envio gera aviso e preserva essa cópia.
O lote remoto fica marcado como incompleto até a conclusão de todos os arquivos;
desconectar revoga o acesso e remove o token local após sucesso, preservando os
arquivos no Google. OneDrive continua sendo cópia em pasta sincronizada.
Não há retenção remota automática nem restauração diretamente da nuvem no aplicativo.
Regras de migração, quantidades e recuperação estão na
[especificação da extensão](../../specs/2026-10-22-drive-oauth-medidas-venda.md).

## RN29 — Atendimento móvel inicial — 23/10/2026

O desktop é a autoridade e único processo que abre o banco. Acesso remoto exige servidor iniciado e chave da sessão atual. Cada escrita tem identificação única; reenviar o mesmo corpo não duplica o commit na sessão ativa. Alterar o corpo com a mesma identificação é conflito. Sem servidor ou resposta conclusiva, não indicar sucesso nem repetir com nova identificação. Depois de reiniciar, conferir a comanda antes de reenviar. A restauração exige parar o servidor. Quantidades, itens históricos e guardas de recebimento/fechamento usam as regras existentes.

## RN30 — Preparo separado do consumo — 23/10/2026

Preparo avança Pendente → Em preparo → Pronto → Entregue; Marcar pronto permite Pendente → Pronto. Entregar exige pronto e não encerra a comanda. Encerramento financeiro não elimina entrega pendente; cancelamento retira a comanda da fila. Lote só altera os IDs/quantidades/estados conferidos, em transação, sem incluir novos lançamentos. Não agregar novo consumo a item que já iniciou preparo; criar pendente separado. Quantidade de item iniciado não é editável; mover preserva estado, copiar reinicia em pendente. Sem mudar preço/nome históricos, observação ou total ao avançar preparo. Na migração, itens antigos de comandas encerradas/canceladas são entregues; abertas começam pendentes para conferência.

## RN31 — Balcão compartilha a faixa de comandas — 23/10/2026

Balcão lista somente comandas abertas sem número de mesa, incluindo as anteriores a esta tela. Novo pedido escolhe o primeiro número livre a partir do início configurado, respeitando o máximo e toda ocupação existente. Faixa lotada recusa abertura; disputa na gravação não reaproveita o pedido de outra pessoa. A consulta filtrada do Balcão não filtra a grade de Mesas. Associar uma mesa retira o pedido da lista de Balcão na atualização; retirar a mesa o devolve. Preços, itens, pagamento, preparo e numeração histórica usam os fluxos existentes, sem nova migração.
