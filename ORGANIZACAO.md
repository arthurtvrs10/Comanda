# Organização das pastas

```text
VComanda/
  windows/             aplicativo Windows e seus materiais
  site/                site e seus materiais
  mobile/              Android, downloads e protótipos Android
  services/            serviços externos
  compartilhado/
    brand/             identidade visual
    apresentacao/      apresentação
    planejamento-web/  documentação da futura versão web do sistema
    referencias/       referências compartilhadas
    outros-projetos/   materiais pessoais e de outros projetos preservados
  artifacts/           publicação oficial, dependências e materiais preservados
```

`VComanda.code-workspace` abre Windows e Site como duas áreas no VS Code. O Git e as configurações ocultas dos editores continuam na raiz comum.

O antigo README do produto foi preservado em `windows/LEIAME-PRODUTO.md`, e seu documento de design está em `windows/DESIGN.md`. As antigas pastas `brand`, `doc_springweb`, `docs`, `downloads`, `prototypes` e `specs` deixaram a raiz após a organização dos seus conteúdos.

## Movimentações concluídas

Após o fechamento dos editores em 18/11/2026, as oito pastas antes bloqueadas foram movidas para seus destinos em `windows` e `site`. As pastas antigas `backend` e `outputs` deixaram a raiz; `outputs` foi removida somente depois de ficar vazia.

`artifacts/release-atual` permanece no caminho oficial exigido pelo projeto. Nenhuma release foi gerada ou alterada.

Os registros de movimentação estão em `windows/organizacao-pastas.json` e `compartilhado/organizacao-pastas.json`.

## Limite desta organização

Esta etapa move pastas e atualiza seus índices, sem alterar código ou executar builds e testes. Os caminhos internos afetados pelas movimentações ainda precisam ser revistos antes de compilar ou publicar. As verificações realizadas antes da reorganização não validam os novos caminhos.