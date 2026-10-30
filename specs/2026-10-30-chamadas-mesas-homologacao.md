# Chamadas das mesas e homologação operacional — 30/10/2026

## Entrega

O atendimento pelo celular, já autenticado pelo QR Code da sessão, passa a oferecer **Chamar atendente** na comanda. A mesa vem do vínculo salvo no computador; não é enviada livremente pelo navegador. Comandas sem mesa ou encerradas não podem gerar chamada. A solicitação é opcional, com até 120 caracteres.

A fila é persistida no SQLite pela configuração `cozinha.kanban.chamadas.v1`, lida pelo KDS. O cartão exibe mesa, texto, minutos decorridos e hora de Brasília. **Atendida** remove somente a chamada, sem alterar pedido, venda ou preparo. O KDS consulta a fila no ciclo de atualização de três segundos e avisa quando surgem chamadas com som habilitado.

Só existe uma chamada pendente por mesa. Repetições retornam a chamada existente. O navegador preserva a identificação de uma tentativa sem confirmação para reutilizá-la após falha de rede. Confirmações persistidas impedem recriar uma chamada já atendida quando a resposta original se perdeu. Limites: 300 chamadas pendentes e 10.000 identificações confirmadas; o serviço recusa novas solicitações ao atingir o limite, sem apagar histórico automaticamente. A identificação de uma tentativa não pode ser reutilizada em outra comanda.

Não é um cardápio público para clientes: o recurso integra o atendimento local existente, que usa a chave da sessão. Parar o servidor invalida o acesso e exige ler outro QR Code.

## Validação automatizada

- HTTP real: autenticação, concorrência, uma chamada por mesa, reenvio após atendimento e chave invalidada após reiniciar servidor.
- SQLite real: concorrência entre instâncias do serviço e fila preservada ao criar outro repositório.
- JavaScript real com DOM de teste: envio, autenticação, resposta perdida, mesma identificação no reenvio, mensagem preservada durante sincronização e botão desabilitado sem mesa.
- Fluxo completo: abertura HTTP, lançamento HTTP, chamada > KDS > atendida, Pendente > Preparando > Pronto > Entregue, Pix de R$ 20 + dinheiro de R$ 30 recebido em R$ 50, troco de R$ 20, venda única de R$ 50, duas formas e documentos da conta de 58/80 mm.
- Fonte isolada criada a partir do Git, adicionando somente as alterações desta entrega. Complementos, estoque e recuperação por e-mail não foram copiados.

## Ensaio no celular disponível

1. Abra a compilação de teste no computador e conecte o celular ao mesmo Wi-Fi.
2. Em Configurações > Servidor / acesso pelo celular, escolha o IP da rede e leia o QR Code atual. Não use localhost ou 127.0.0.1 no celular.
3. Abra uma comanda no computador e use Vincular mesa. No celular, abra essa mesma comanda.
4. Digite uma solicitação de teste e toque em Chamar atendente.
5. No KDS, confirme a mesa, texto e tempo em Chamadas. Repita o toque no celular: deve continuar havendo um só cartão.
6. Marque Atendida. Envie uma nova solicitação e confirme que aparece outra chamada.
7. Reinicie normalmente o aplicativo com uma chamada pendente e confirme sua permanência. Leia o novo QR Code para reconectar o celular.
8. Confirme som habilitado e volume do Windows. O sistema recomenda 80% e não altera o volume por conta própria.

## Pendências reais

O usuário confirmou celular disponível, mas não impressora nem os demais equipamentos. Impressão física em 58/80 mm e áudio em ambiente de cozinha não estão homologados. As prévias e os testes de documentos não substituem ensaio com impressora térmica. A validação no telefone depende de o usuário confirmar os passos acima. Não houve alteração automática do Firewall do Windows.

Não publicar como versão estável antes dessa conferência. A versão numérica não foi incrementada nesta preparação. Os instaladores já publicados não recebem estas mudanças automaticamente.

## Rastreabilidade

Extensão do atendimento local e do quadro KDS: RF de atendimento/preparo e continuidade descritos em `docs/docs/02-requisitos.md`, `docs/docs/03-regras-negocio.md` e `docs/docs/21-evolucao-atendimento.md`. Caso de integração executável: `FluxoOperacionalCompletoTests.CelularKdsChamadasEntregaPagamentoMistoHistoricoEImpressosUsamMesmoBanco`. As chamadas são solicitação de atendimento, independentes do estado financeiro e da entrega.
