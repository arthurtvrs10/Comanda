# Cozinha (KDS) nativa do Comanda

Data: 23/10/2026. Pedido humano: aplicar ao Comanda desktop o KDS do Food Flow POS mostrado no anexo, conforme a identidade Comanda. Extensão Operate do sistema aprovado, sem nova identidade/comp ou biblioteca visual. RF32 → UC13 → CT27; RN30. Decisão operacional e arquitetural em docs/docs/adr/0007-kds-nativo-estados-preparo.md.

## Direção e contrato de interface

Referências: anexo do usuário, C:/Users/arthu/foodflow-pos/src/components/KDSView.tsx e trecho de API do server/index.js, somente leitura. Nenhum banco do Food Flow consultado ou alterado. Preservar WPF/MVVM, Segoe UI, recursos dinâmicos Ajuda*/Accent/Prioridade*, fundos cinza, cantos retos e controles da identidade Comanda. Cartões com local/mesa, comanda e código de pedido quando existentes, cliente, espera, itens, quantidades UN/KG/L, observações e estados. Sem preços ou ações de cobrança na cozinha.

Topo com contagem de comandas/lançamentos, filtros por local e situação, Atualizar e toggle Som de novos itens. Grade ajusta largura dos cartões ao painel; em janela compacta usa duas colunas e rolagem geral, com rolagem própria para cartão longo. Espera parte do item ativo mais antigo, nunca negativa: atenção aos 10 minutos e atraso aos 20. Prioridade existente também sinaliza atenção; texto nomeia alerta além da cor. Marcar pronto usa Accent e Entregue usa a ação neutra, mantendo cores da marca. Rodapé informa a última leitura e o intervalo real de três segundos, sem afirmar WebSocket.

Clique no item avança Pendente → Em preparo → Pronto → Entregue. Marcar pronto no cartão pode pular a etapa de preparo; Entregue do cartão exige todos os itens exibidos prontos. As duas ações usam somente os IDs do cartão filtrado, com quantidade e estado esperados. Entregues deixam a fila. Filtro de local conserva o comportamento existente: itens sem vínculo específico aparecem nos locais selecionados. Filtros e objetos dos cartões permanecem entre leituras quando os dados não mudam. Falha conserva a última fila e desabilita alterações até nova consulta bem-sucedida; primeira falha tem estado indisponível, sem afirmar fila vazia válida.

Som do Windows somente para novos IDs após a carga inicial, respeitando toggle de sessão; não toca ao trocar filtro ou repetir consulta. Áudio não impede consulta. Timer só consulta enquanto o painel está visível e para no Unloaded. Consultas e gravações periódicas usam tarefa fora da thread WPF; não há novo transporte de rede para KDS. Atualização imediata do servidor existente continua integrada.

## Persistência e integridade

Novo StatusPreparo por item, nova migração 20261023232145_AddStatusPreparoCozinha com check de valores. Migrações anteriores preservadas. Consulta inclui comandas não canceladas com entrega pendente, mesmo encerradas; não encerra ou recebe pagamentos pelo KDS. Preço, nome, observação, quantidade e total não mudam ao avançar preparo. Operações em lote verificam associação, estado e quantidade em transação, com recusa integral se algum dado mudou. Não incorporam item que chegou depois do snapshot.

Não agregar novo consumo a item já iniciado/pronto/entregue; criar novo pendente. Quantidade de item iniciado não pode ser alterada, salvo repetição sem mudança; consumo adicional usa novo item. Mover preserva preparo; copiar inicia nova demanda. Remoção/cancelamento existentes deixam de aparecer na fila. Não há reversão de preparo ou histórico de transições nesta etapa. Cartões agrupam comanda, sem entidade independente de pedido de P1.

Migração conserva preços, unidades, quantidades e totais. Itens antigos de comandas encerradas/canceladas são tratados como entregues, evitando reabrir vendas antigas. Comandas abertas começam pendentes e precisam de conferência na adoção. O aplicativo já exige backup preventivo antes de migrar base existente. Nesta tarefa somente bases descartáveis foram migradas. Nenhuma instância operacional foi encerrada para trocar binários.

## Qualidade e evidências

QUALITY BAR: tarefa de cozinha completa com persistência e guardas, identidade do desktop preservada, texto legível e ações alcançáveis em 800×600/1280×780/1900×900 nos dois temas; sem confundir entrega com cobrança, sem fingir atualização WebSocket. Contexto Impeccable já carregado na sessão; referências Operate/craft-floor foram aplicadas sem reparar drift preexistente de PRODUCT.md/design.json.

Sete CozinhaKdsTests de SQLite/migração/transação aprovados, log outputs/kds-persistencia.log. Cinco CozinhaViewModelTests e um CozinhaKdsXamlTests aprovados, log outputs/kds-desktop.log; cobrem filtros/escopo, som inicial/repetido/mudo, erros, conflitos e estados. Duas rodadas próprias de inspeção; na correção conjunta foram acrescentados nomes textuais de atenção/atraso e estado inicial indisponível. Testes não emitem som físico; verificam callback.

Capturas WPF reais por RenderTargetBitmap, dados fictícios controlados, em .impeccable/review/kds-desktop/: kds-{claro,escuro}-{1280,800,1900}.png, kds-vazio-{claro,escuro}.png e kds-erro-{claro,escuro}.png. Dez arquivos da rodada final foram abertos individualmente; são o corpo real WPF, sem moldura do aplicativo nem screenshot de uma sessão operacional. Não há raster de produto ou marca criado. Detector HTML/CSS não se aplica ao WPF e não foi executado nesta etapa.

Regressão completa Debug aprovada: 4 Domain + 134 Application + 310 Infrastructure + 362 Desktop + 12 Servidor = 822 testes distintos; log outputs/kds-regressao.log e TRX em backend/artifacts/kds-regressao-resultados/. Os sete testes de persistência e seis de desktop acima integram esse total, sem dupla contagem. Há dois avisos xUnit2031 preexistentes em AtendimentoTecladoXamlTests. Build do aplicativo sem avisos/erros. Publicação local Release win-x64 autocontida concluída em artifacts/kds-desktop/publish, log outputs/kds-publicacao.log; não muda instaladores ou release externa. Validador documental aprovado (outputs/kds-docs-validacao.log); SQL do validador foi pulado por falta do sqlite3 CLI, mas os testes usam SQLite real.

Áudio físico, toque/leitor de tela, monitores/escala diferentes e homologação prolongada no estabelecimento permanecem pendentes. Não declarar P1/P2/P3 completos nem KDS web entregue. O navegador desabilitado nas configurações não foi contornado nem necessário para as capturas nativas.

## Parecer final e registro do sistema

Revisão independente final com disposição `ship`, sem achados materiais ou correções adicionais requeridas. O revisor abriu individualmente os dez PNGs finais obrigatórios: kds-{claro,escuro}-{1280,800,1900}.png e kds-{vazio,erro}-{claro,escuro}.png, em .impeccable/review/kds-desktop/. O parecer cobre o corpo WPF renderizado de forma controlada com dados fictícios e as fontes/contrato amostrados; não atesta uma sessão operacional, áudio físico, toque, leitor de tela completo, outras escalas ou piloto no estabelecimento.

TYPE: Segoe UI e hierarquia incumbente; MATERIAL: superfícies neutras, bordas simples, cantos retos e Accent da marca; GROUND: temas e recursos dinâmicos Comanda preservados. A composição em cartões e os estados de preparo são a adaptação funcional autorizada do KDS de referência ao desktop. O registro durável foi acrescentado a DESIGN.md sem substituir seções ou tokens: cartão/itens, filtros, espera e alertas textuais de 10/20 minutos, erro que bloqueia mudanças, som de sessão, transições persistidas por item sem alterar totais e ações limitadas aos IDs/estados/quantidades do snapshot exibido.

A evidência de entrega permanece a regressão de 822 testes distintos em Debug, publicação local Release autocontida e validação documental já registradas acima. Não há detector HTML/CSS aplicável ao WPF, ImageGen ou novo raster de produto. Não declarar KDS web, WebSocket ou P1/P2/P3 completos, novo instalador/release externa, migração de base operacional ou encerramento de instância real. PRODUCT.md e .impeccable/design.json continuam ausências preexistentes, e o formato legado de DESIGN.md permanece sem reparação lateral.
