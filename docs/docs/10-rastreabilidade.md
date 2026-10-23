# Rastreabilidade

Matriz do MVP preservada. A cadeia EV → RE → UCE → CTE e a etapa de cada entrega estão no [plano de evolução](21-evolucao-atendimento.md). Não atribuir aprovação aos testes planejados.

Identificação por nome: RF29 → RN28/RNF13 → CT24 (`DefinirNomeClienteTests`,
`EfComandaRepositoryTests` e `AtendimentoViewModelTests`).

Licenciamento comercial: RF28 → RN24–RN27 → CT23 (`LicencaOfflineTests` e
`ServicoLicencaTests`). Detalhes em [Licenciamento offline](20-licenciamento-offline.md).

## Instalador EXE (2026-09-29)

A distribuição principal agora usa `artifacts\VarthexComanda-1.0.0-Setup-win-x64.exe`.
Dê dois cliques e siga o assistente em português. Inclui o .NET, cria atalhos e
permite desinstalar por Aplicativos Instalados do Windows, sem administrador.
Banco, fotos e backups ficam separados e são preservados.

Para gerar: `.\scripts\gerar-instalador.ps1`. Requer Inno Setup 6 instalado,
ou `-Compilador <caminho de ISCC.exe>`. Saída em `artifacts`, com SHA-256.
Atualizações usam o mesmo instalador; não criam backup dos binários anteriores.
O ZIP e os procedimentos com scripts abaixo continuam como alternativa.
Esta decisão substitui referências anteriores à ausência de instalador EXE.
Sem alteração de RF/RN de negócio; rastreabilidade: RNF09, RNF14, RNF17 e CT21.
559 testes Release passaram; CT21 em Windows limpo sem .NET permanece pendente.


| Objetivo | Requisitos | Regras | Casos de uso | Testes |
| --- | --- | --- | --- | --- |
| OBJ01 | RF06-RF13, RF27 | RN01-RN10, RN21-RN23 | UC02, UC03, UC04, UC06, UC10 | CT01-CT07, CT17, CT19, CT20 |
| OBJ02 | RF14-RF17 | RN09-RN15 | UC05 | CT07-CT11 |
| OBJ03 | RF18-RF20 | RN04, RN05, RN10, RN16, RN17 | UC07 | CT04, CT12, CT18 |
| OBJ04 | RF21-RF24, RF27 | RN19, RN22, RN23 | UC08, UC09, UC10 | CT14-CT16, CT20 |
| OBJ05 | RF25-RF27 | DEC02, DEC05, DEC07, DEC08 | Todos | CT13, CT16, CT19-CT22 |

Nota sobre o CT17 (navegar por teclado): o caso percorre localizar produto, adicionar item e iniciar o encerramento. Foi colocado no OBJ01 porque a maior parte do fluxo (abrir comanda, localizar, adicionar, corrigir quantidade) é de atendimento; o passo "iniciar encerramento" (RF14) toca o OBJ02. O requisito que o justifica é o RNF18 (acessibilidade por teclado), que não pertence a nenhum objetivo desta tabela.

## Rastreabilidade executável dos casos de teste

Cada teste automatizado ligado a um caso carrega `[Trait("Caso", "CTnn")]`. A matriz [CT → testes](../homologacao/rastreabilidade-testes.md) é **gerada** pelo teste `RastreabilidadeDosCasosTests` (não editar à mão) e o teste falha se um caso automatizável ficar sem cobertura. O que ainda falta provar por caso (CT13 físico, CT21, teclado físico, maquininha real) está em [resultados-ct.md](../homologacao/resultados-ct.md); o processo geral da Etapa 8, em [homologacao/README.md](../homologacao/README.md).

## Regra de manutenção

Todo requisito novo deve ter:

- objetivo relacionado;
- regra aplicável ou justificativa de ausência;
- caso de uso ou fluxo;
- pelo menos um teste de aceitação;
- etapa do roadmap;
- atualização no histórico de mudanças.

Nenhum requisito pode ser considerado implementado sem evidência de teste.

## Release 1.4.0 — 22/10/2026

O toggle de aparência mantém RNF07/RNF18 e CT17: o teste
`RibbonPreservaConteudoFiltrosPermissoesETemaEntreSessoes` aciona o padrão
de alternância de acessibilidade e confere tema, persistência e estado dos módulos.
`FundoSistemaTests` cobre a precedência da imagem personalizada; `VersaoDoAplicativoTests`
confere a versão 1.4.0. Não há nova RF/RN de negócio nesta extensão.

Os dois instaladores autocontidos relacionam-se a RNF09/RNF14/RNF17 e CT21.
750 testes distintos passaram em Release; a compilação e os checksums não encerram
CT21 em Windows limpo. Origem das capturas e limites em
[Release 1.4.0](../../specs/2026-10-22-release-1.4.0.md).

## Histórico de pedidos — extensão em desenvolvimento de 22/10/2026

RF30 → UC11 → CT25 → `HistoricoPedidosTests` e `HistoricoPedidosXamlTests`.
RN04/RN05/RN10/RN20/RN22/RN23 são preservadas: a consulta não altera registros,
considera Brasília e abre detalhes com os itens históricos. RNF01/RNF07/RNF18
cobrem consulta local, legibilidade e teclado. O histórico de vendas (RF18–RF20/UC07)
continua independente. Não há migração nem novo estado de pedido.

Evidência: 8 testes novos SQLite e 4 Desktop incluídos na regressão Debug de
762 testes aprovados, além da leitura do XLSX por openpyxl. A matriz gerada
CT01–CT22 permanece preservada; CT25 usa esta cadeia explícita e o critério no
documento de aceitação. Instalação, impressão física e grandes bases não foram
homologadas nesta extensão. [Contrato e capturas](../../specs/2026-10-22-historico-pedidos.md).

## Atendimento móvel inicial — 23/10/2026

RF31 → UC12 → CT26 → ServidorLocalTests (HTTP e SQLite reais), ServidorJanelaTests, BackupViewModelTests e ServicoLicencaTests. RN29, RN01/RN03–RN08/RN10/RN19/RN21/RN23 preservadas. RNF01/RNF07/RNF18: desktop offline, temas, acesso e campos locais. [ADR 0006](adr/0006-servidor-local-atendimento-movel.md). [Contrato e limites](../../specs/2026-10-23-principal-servidor-local.md).

Extensão do mesmo RF31/UC12/CT26: ServidorLocalTests valida entrega autenticada de fotos; backend/tests/VarthexComanda.Servidor.Tests/Web/atendimento.test.cjs executa o JavaScript real com DOM mínimo para navegação, personalização e pendências. Não comprova renderização, responsividade ou comportamento de um navegador real. [Contrato do catálogo web](../../specs/2026-10-23-web-comanda-catalogo-fotos.md).

## KDS desktop — 23/10/2026

RF32 → UC13 → CT27 → CozinhaKdsTests (SQLite/migração/transação), CozinhaViewModelTests (filtros/ações/som/falhas) e CozinhaKdsXamlTests (capturas WPF reais). RN30, RN04–RN06/RN19/RN21, RNF01/RNF07/RNF18. [ADR 0007](adr/0007-kds-nativo-estados-preparo.md) e [spec](../../specs/2026-10-23-cozinha-kds-desktop.md). Não equivale a P1/P2/P3 completos ou a KDS web.
