# VComanda para Windows

Esta pasta reúne os materiais existentes da versão Windows:

- `src`, `tests`, `tools` e `VarthexComanda.slnx`: código, testes, ferramentas e solução.
- `demo` e `demo-restaurante`: catálogos de demonstração.
- `downloads`: instaladores, pacotes e notas de versão.
- `scripts`: scripts existentes de publicação e homologação.
- `docs` e `specs`: documentação e especificações Windows.
- `artifacts`, `outputs` e `prototypes`: artefatos, registros, capturas e protótipos.

A última etapa foi somente de organização física: sem mudanças no conteúdo do código, sem compilação, testes ou publicação. Os links de compatibilidade criados anteriormente foram removidos. Caminhos relativos dentro de scripts e projetos não foram adaptados nesta etapa; precisam ser revistos antes de compilar ou publicar a estrutura reorganizada. As verificações realizadas antes desta movimentação não validam os novos caminhos.

O arquivo `organizacao-pastas.json` registra origens e destinos. As oito pastas bloqueadas foram transferidas após fechar os editores em 18/11/2026. A publicação oficial em `../artifacts/release-atual` permanece no caminho exigido pelo projeto.

A branch oficial de publicação continua sendo `codex/release-atual`. Nenhuma cópia antiga foi utilizada para remontar uma versão. Site, Android, serviços e materiais compartilhados continuam fora desta pasta.

`VarthexComanda.Desktop.Tests.csproj.original` é uma variante preservada na primeira movimentação; não faz parte da solução ativa.