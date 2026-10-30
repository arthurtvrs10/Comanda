# Comanda 1.4.10 — fonte única de publicação

Pré-release Windows x64 de 30/10/2026. Mantém as funcionalidades e a configuração da 1.4.9: cliente, tipo de pedido, observações e operador automático, além de QR/link da rede real, OneDrive, impressão e catálogos. Não adiciona migrações.

## Correção do processo

A publicação passa a usar somente a branch codex/release-atual, descendente da última release, e a pasta gerada artifacts/release-atual. Não são mais montadas releases a partir de pastas manuais de versões anteriores.

- Bloqueia fonte sem manifesto, commit desatualizado, perda de controles aprovados, alteração de arquivos após a preparação e divergência de metadados.
- Bloqueia reutilização de publicação de outro commit/versão e binários alterados.
- publicacao.json acompanha o aplicativo com a versão, commit e hashes. Não é assinatura de código.
- Limpeza de builds antigos com inventário e verificação de caminhos, links, processos, dados e credenciais. Fontes antigas elegíveis são preservadas em arquivos ZIP antes de excluir as pastas de trabalho.

## Uso

Para gerar instaladores: `pwsh -File scripts/Preparar-Publicacao.ps1`. Integre primeiro os commits aprovados em codex/release-atual. Não edite o conteúdo de artifacts/release-atual/fonte. Para verificar uma instalação, consulte publicacao.json ao lado do executável. Builds antigos e instaladores já baixados antes desta correção não passam a ter estas proteções retroativamente.

Faça backup e feche o programa antes de atualizar. Mesmos identificadores de instalação e pastas de dados das versões anteriores; não inclui base do desenvolvedor, credenciais ou configuração de cliente. Não é necessário reinstalar ou apagar a base.

## Validação e escopo

Nove verificações das proteções aprovadas em fixtures locais: fonte íntegra, perda de campo, alteração/adição de arquivo, commit antigo, ausência de manifesto, pacote íntegro, pacote de outro commit e binário alterado. A validação final dos instaladores e testes direcionados será registrada nas notas publicadas.

A base funcional mantém a validação da 1.4.9 (809 testes .NET); isso não representa nova execução integral nesta correção de processo. Homologação de hardware e Windows limpo permanece pendente. Instaladores sem assinatura de código; publicação em pré-release.

Continuam fora: chamadas das mesas, KDS recente, complementos/estoque e suas migrações, recuperação por e-mail e transferência de licença por código curto. O emissor Android não mudou.

## Downloads

- [VarthexComanda-1.4.10-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.10/VarthexComanda-1.4.10-Setup-win-x64.exe) · [SHA-256](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.10/VarthexComanda-1.4.10-Setup-win-x64.exe.sha256)
- [VarthexComanda-Demo-1.4.10-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.10/VarthexComanda-Demo-1.4.10-Setup-win-x64.exe) · [SHA-256](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.10/VarthexComanda-Demo-1.4.10-Setup-win-x64.exe.sha256)
- [VarthexComanda-Demo-Restaurante-1.4.10-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.10/VarthexComanda-Demo-Restaurante-1.4.10-Setup-win-x64.exe) · [SHA-256](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.10/VarthexComanda-Demo-Restaurante-1.4.10-Setup-win-x64.exe.sha256)
