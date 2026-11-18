# Inventário de paridade
Fonte oficial consultada em 17/11/2026: codex/release-atual, commit 11d69a204008a9bfb201a16bda748eb99df7b0fa. Reconstrução via git show <SHA>:<caminho>. README.md e downloads/RELEASE-1.4.10.md identificam a pré-release e os limites de homologação. Os testes citados são referências do Windows. A execução e a equivalência web precisam de evidência própria.

| Capacidade | Perfil | Base e dependência | Teste legado ou referência | Destino |
| --- | --- | --- | --- | --- |
| C01 Descoberta do turno | Fundador | Comanda.Numero e NomeCliente opcional | Atendimento/DefinirNomeClienteTests | H01 / S01 |
| C02 Acesso por estabelecimento | Gestor | AcessoGestorTests | PermissoesColaboradoresTests | H02 / S02 |
| C03 Catálogo de venda | Gestor | Catalogo/AlterarProdutoTests | Catalogo/DefinirFotoProdutoTests | H03 / S02 |
| C04 Comanda individual | Garçom | Atendimento/AbrirComandaTests | Configuracao/NumeracaoComandasTests | H04 / S03 |
| C05 Consumo por pessoa | Garçom | Atendimento/AdicionarItemTests | Atendimento/AlterarQuantidadeTests | H05 / S03 |
| C06 Recebimentos e fechamento | Caixa | Atendimento/EncerrarComandaTests | Homologacao/PagamentosMistosTests | H06 / S04 |
| C07 Impressão e produção aprovada | Caixa | Configuracao/LayoutTicketTests | Configuracao/PreferenciasImpressaoTests | H07 / S05 |
| C08 Históricos e exportação | Gestor | Atendimento/ListarVendasPorDataTests | Atendimento/HistoricoViewModelTests | H08 / S05 |
| C09 Modalidades e operação local | Atendente | Configuracao/PainelSenhasTests | Homologacao/OperacaoOfflinePorConstrucaoTests | H09 / S06 |
| C10 Configuração e edições | Gestor | Configuracao/LimiteComandasTests | DemonstracaoRestauranteTests | H10 / S06 |
| C11 Importação e recuperação | Administrador | Backup/RestaurarBackupTests | Backup/OneDrivePadraoTests | H11 / S07 |
| C12 Piloto e transição | Fundador e caixa | Checklist do estabelecimento | Homologacao/CriteriosDeTelaComBancoRealTests | H12 / S08 |

Os nomes dos testes correspondem a arquivos .cs em backend/tests/VarthexComanda.Application.Tests ou VarthexComanda.Desktop.Tests. A regra de titularidade web é proposta. Em backend/src/VarthexComanda.Domain/Comanda.cs existem Numero, NomeCliente, NumeroMesa opcional e QuantidadePessoas. A origem física do código continua aberta em docs/homologacao/pendencias.md (QV01). Não atribuir retroativamente pessoas distintas a uma conta antiga de grupo.

**Conflito histórico.** QV03 descreve quantidades inteiras. ItemComanda.cs na fonte atual usa decimal e UnidadeVenda, e a edição Restaurante traz exemplos UN/KG. Para pagamentos, specs/2026-10-22-fluxo-pagamentos.md evoluiu de rateio por pessoas até seleção de itens, com confirmação externa e persistência dos recebimentos. Esse rateio não comprova consumo individual desde a abertura.

Normal, demonstração Lanchonete e demonstração Restaurante têm instalações e bases distintas. Normal começa vazia; as demos têm 12 e 16 produtos fictícios respectivamente. As capacidades operacionais da matriz aplicam-se às três edições; importar e distribuir catálogos precisa respeitar a edição. Considerar também temas/teclado, recuperação e licença offline aprovadas, QR/link móvel, cópias locais e destinos sincronizados configurados. Para paridade, testar a base existente antes de ativar qualquer mudança. Chamadas das mesas, KDS recente, complementos/estoque, recuperação por e-mail e transferência de licença por código curto ficam fora da publicação até decisão individual conforme AGENTS.md.
