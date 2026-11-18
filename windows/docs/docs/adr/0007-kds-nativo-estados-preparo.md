# ADR 0007 — KDS nativo com estados de preparo

Data: 23/10/2026. Situação: aceita para a extensão solicitada pelo responsável nesta conversa. O pedido foi aplicar no Comanda desktop a organização e as funcionalidades do KDS do Food Flow POS, conforme a imagem fornecida, preservando a identidade do Comanda.

## Decisão

Ampliar a cozinha WPF existente com Pendente → Em preparo → Pronto → Entregue por item. O cartão agrupa os lançamentos de uma comanda; não cria entidade independente de pedido nem implementa todo o plano P1/P2. Ações individuais e de conjunto usam caso de uso e repositório próprios, sem alterar os totais, snapshots, pagamentos ou encerramento da comanda. Marcar pronto pode avançar diretamente de pendente; entregar exige pronto. Não há regressão automática de estado nem KDS web nesta etapa.

O estado é persistido em item_comanda por nova migração, com restrição de valores. Escritas validam em transação o ID da comanda, IDs explícitos dos itens, estados e quantidades vistos pelo operador. Conflito recusa o lote inteiro; novos itens recebidos depois da leitura não são incluídos silenciosamente. Comanda cancelada não admite atualização; comanda paga/encerrada permanece na fila enquanto houver entrega pendente. Entregar não encerra nem recebe dinheiro.

Adicionar produto após iniciar preparo cria outro item pendente, preservando o lançamento já preparado. Alterar quantidade de item iniciado/pronto/entregue é recusado; consumo adicional usa novo lançamento. Mover itens preserva preparo; copiar cria nova demanda pendente. Remoção/cancelamento existentes retiram a demanda da fila, sem novos controles de cobrança. Itens entregues deixam a fila ativa e continuam no consumo/histórico.

## Migração e continuidade

Instalação existente usa a proteção de backup antes de migração já implementada no aplicativo. Migração mantém preços, quantidades e totais; itens de comandas antigas encerradas/canceladas são tratados como entregues para não reabrir a cozinha histórica. Itens de comandas abertas começam pendentes e precisam ser conferidos na adoção do KDS. Não editar migrações anteriores. Testes usam banco descartável, sem executar migração em dados operacionais nesta tarefa.

Atualização automática local a cada três segundos enquanto o painel está visível, além das atualizações existentes do servidor. Não anunciar WebSocket. Som do Windows somente para novos IDs depois da carga inicial, com toggle de sessão. Filtros por local existente e situação preservam escopo explícito das ações. Falha mantém a última fila e desabilita ações até atualizar com sucesso. Temas, WPF/MVVM, marca e componentes existentes preservados.

RF32 → UC13 → CT27; RN30 e RN04–RN06/RN19/RN21 preservadas. Capturas WPF controladas, testes de transação/migração e regressão registrados na [spec](../../../specs/2026-10-23-cozinha-kds-desktop.md). Som físico, tela externa, toque/leitor de tela e uso prolongado no estabelecimento permanecem para homologação. Nenhum instalador ou publicação externa autorizado por esta entrega.
