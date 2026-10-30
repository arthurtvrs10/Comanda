# Preferências do projeto

Use sempre a skill `impeccable` nas tarefas de interface e experiência de uso do VComanda.

## Publicação e prevenção de regressões

- A fonte oficial de publicação é a branch `codex/release-atual`, contendo todos os ajustes da última release aprovada. Integre nela somente commits prontos para publicação.
- Gere as edições com `scripts/Preparar-Publicacao.ps1`. A única pasta gerada oficial é `artifacts/release-atual`; nunca edite essa cópia como projeto de desenvolvimento.
- Não gere uma nova versão a partir de tags antigas, pastas de versões anteriores ou scripts temporários de `artifacts` e `backend/artifacts`. Não selecione manualmente arquivos de cópias antigas para remontar a release.
- Antes de publicar, confirme que a fonte descende da última release, que os testes críticos passam e que `publicacao.json` identifica o commit e os binários distribuídos. A tag deve apontar para esse commit.
- Preserve os dados, configurações, backups e credenciais do usuário. Limpeza de builds exige conferir caminhos e seguir o inventário; não apague pastas com dados, links ou processos em uso.
- Chamadas das mesas, KDS recente, complementos/estoque, recuperação por e-mail e transferência de licença por código curto permanecem fora da publicação até autorização explícita para cada módulo.
