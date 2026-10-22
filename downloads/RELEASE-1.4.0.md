# Comanda · por Varthex 1.4.0

Pré-release para Windows x64, atualizada em 22/10/2026. A versão estável anterior continua sendo a 1.2.2.

**Esta atualização substitui os instaladores anteriores da 1.4.0.** Se você já baixou essa versão, baixe novamente e confira o novo SHA-256. O código da tag e os pacotes foram atualizados para a mesma revisão.

## Duas edições

- **Normal, sem demonstração:** `VarthexComanda-1.4.0-Setup-win-x64.exe`. Catálogo vazio em uma instalação nova, para cadastrar os produtos reais.
- **Demonstração:** `VarthexComanda-Demo-1.4.0-Setup-win-x64.exe`. Inclui 12 produtos fictícios com fotos e preços de teste. Instalação, atalho e dados separados da edição normal.

Ambas incluem o runtime .NET e exigem ativação offline. Os arquivos `.sha256` permitem conferir a integridade de cada instalador.

| Edição | SHA-256 desta recompilação |
| --- | --- |
| Normal | `71A1565E950B1CE93E87D0F77370A7EFC84D4C1B8832D2F24866B59E7FF425F8` |
| Demonstração | `BDC8D0DDFEFE3D9AB6CBE42A807D32B7B24E0EDF5DB3DE690681EB6616911F99` |

## O que mudou nesta atualização

- Identidade **Comanda · por Varthex**: marca vetorial na entrada, área de trabalho e rodapé, variações na ajuda, novo ícone e títulos atualizados.
- Tela de abertura com arte abstrata, logo central, versão e indicação de carregamento. Exibida por pelo menos **5 segundos** antes da tela principal; o relógio monotônico garante o prazo completo.
- Superfícies neutras nos dois temas. Fundo padrão cinza no claro e azul profundo no escuro, com marca centralizada. Imagens e cores personalizadas são preservadas.
- **Relatórios → Vendas** reúne Histórico de pedidos e Histórico de vendas. Consulta por situação/período, colunas e agrupamento no histórico de pedidos, detalhes, exportação Excel e prévia de impressão.
- Histórico de vendas em tabela integral com filtros de data/comanda, colunas, ordenação, detalhes por botão/duplo clique/Enter, Excel e impressão. Os indicadores do dia são identificados separadamente do total exibido.
- README com oito capturas atuais da interface WPF, usando dados fictícios.

## Recursos mantidos

- Ribbon e área de trabalho com janelas internas que podem ser abertas juntas, movidas, redimensionadas e maximizadas abaixo dos menus.
- Entrada direta na área de trabalho, removendo a antiga tela de cartões de perfis. PIN e permissões continuam protegendo os módulos.
- Toggle **Modo escuro** no cabeçalho, com preferência persistida neste computador.
- Mesas e comandas com responsável, observações, prioridade por cor, transferência/cópia de itens e situação **Em fechamento** em amarelo. Numeração inicial configurável.
- Produtos com fotos por padrão ou visualização em lista, categorias, ordenação indicada nos cabeçalhos e personalização de itens.
- Venda por **UN, KG ou L**. KG/L aceitam até três casas decimais; preço e medida históricos são preservados.
- Backup com janela de progresso, barra e mensagens; restauração de dados e fotos; OneDrive por pasta sincronizada.
- Integração Google Drive preparada por OAuth no navegador, sem credenciais incluídas, com cópia local criada antes do envio opcional. Convite ao gestor pode ser adiado após cinco segundos.
- Ajuda compacta ao lado do atalho, com WhatsApp, RustDesk, sugestões e tutorial guiado.

## Atualização e dados

Feche o aplicativo antes de executar o instalador. Atualizar ou reinstalar preserva os dados e a licença. Mantenha um backup antes da atualização: a versão inclui migração das quantidades para representar UN/KG/L; retornar a uma versão antiga exige restaurar o backup anterior. Esta recompilação não acrescenta migração de banco.

Dados normais: `%LOCALAPPDATA%\VarthexComanda`. Demonstração: `%LOCALAPPDATA%\VarthexComandaDemo`. Abra a demonstração pelo atalho **Comanda Demonstração**. A carga fictícia ocorre uma única vez em base vazia.

## Validação e limites

**770 testes distintos aprovados em Release:** 4 Domain, 133 Application, 303 Infrastructure e 330 Desktop. Após um achado de arredondamento na espera da abertura, a suíte Desktop completa passou com a correção. Os testes de captura são repetições e não aumentam esse total. A validação documental passou; a fase SQL foi ignorada por ausência da ferramenta sqlite3, e os testes Infrastructure usam SQLite real.

Esta versão continua em testes. Instalação em Windows limpo, impressão física e homologação no equipamento do estabelecimento permanecem pendentes. O instalador não possui assinatura de código.

O Google Drive exige cadastrar e importar um cliente OAuth do tipo aplicativo para computador. Não há conta real autorizada/validada nesta entrega; autorização, envio e recuperação com conta real continuam pendentes. A central de ajuda está em preparação. Não há atendimento em rede, emissão fiscal ou integração com maquininha; a cobrança continua externa.
