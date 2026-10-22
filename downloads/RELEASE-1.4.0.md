# Varthex Comanda 1.4.0

Pré-release de 22/10/2026 para Windows x64. A versão estável anterior continua sendo a 1.2.2.

## Duas edições

- **Normal, sem demonstração:** `VarthexComanda-1.4.0-Setup-win-x64.exe`. Catálogo vazio em uma instalação nova, para cadastrar os produtos reais.
- **Demonstração:** `VarthexComanda-Demo-1.4.0-Setup-win-x64.exe`. Inclui 12 produtos fictícios com fotos e preços de teste. Instalação, atalho e dados separados da edição normal.

Ambas incluem o runtime .NET e exigem ativação offline. Os arquivos `.sha256` permitem conferir a integridade de cada instalador.

## Novidades

- Ribbon e área de trabalho com janelas internas que podem ser abertas juntas, movidas, redimensionadas e maximizadas abaixo dos menus.
- Entrada direta na área de trabalho, removendo a antiga tela de cartões de perfis. PIN e permissões continuam protegendo os módulos.
- Toggle **Modo escuro** no cabeçalho, com preferência persistida. Fundo padrão cinza neutro com **VARTHEX** ao centro, preservando imagens personalizadas.
- Mesas e comandas com responsável, observações, prioridade por cor, transferência/cópia de itens e situação **Em fechamento** em amarelo. Numeração inicial configurável.
- Produtos com fotos por padrão ou visualização em lista, categorias, ordenação indicada nos cabeçalhos e personalização de itens.
- Venda por **UN, KG ou L**. KG/L aceitam até três casas decimais; preço e medida históricos são preservados.
- Backup com janela de progresso, barra e mensagens; restauração de dados e fotos; OneDrive por pasta sincronizada.
- Integração Google Drive preparada por OAuth no navegador, sem credenciais incluídas, com cópia local criada antes do envio opcional. Convite ao gestor pode ser adiado após cinco segundos.
- Ajuda compacta ao lado do atalho, com WhatsApp, RustDesk, sugestões e tutorial guiado.
- README atualizado com capturas atuais dos temas claro/escuro e dos módulos.

## Atualização e dados

Feche o aplicativo antes de executar o instalador. Atualizar ou reinstalar preserva os dados e a licença. Mantenha um backup antes da atualização: a versão inclui migração das quantidades para representar UN/KG/L; retornar a uma versão antiga exige restaurar o backup anterior.

Dados normais: `%LOCALAPPDATA%\VarthexComanda`. Demonstração: `%LOCALAPPDATA%\VarthexComandaDemo`. Abra a demonstração pelo atalho **Comanda Demonstração**. A carga fictícia ocorre uma única vez em base vazia.

## Validação e limites

750 testes distintos aprovados em Release: 4 Domain, 133 Application, 295 Infrastructure e 318 Desktop. A validação documental passou; a fase SQL do script documental foi ignorada por ausência da ferramenta sqlite3, e os testes Infrastructure usam SQLite real. A captura do histórico foi repetida e passou 1/1, sem ampliar o número de testes distintos.

Esta versão continua em testes. Instalação em Windows limpo e homologação no equipamento do estabelecimento permanecem pendentes. O instalador não possui assinatura de código.

O Google Drive exige cadastrar e importar um cliente OAuth do tipo aplicativo para computador. Não há conta real autorizada/validada nesta entrega; autorização, envio e recuperação com conta real continuam pendentes. A central de ajuda está em preparação. Não há atendimento em rede, emissão fiscal ou integração com maquininha; a cobrança continua externa.
