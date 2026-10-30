# Comanda 1.4.9 — ajustes da comanda recuperados

Pré-release Windows x64 de 30/10/2026. Runtime incluso e ativação offline obrigatória.

## Alterações

- Removidos o seletor Responsável e o botão Vincular cliente. A conta conectada continua sendo registrada na autoria do atendimento, sem alterar a autoria histórica.
- Cliente opcional diretamente na comanda; tipo obrigatório com Local (Mesa), Salão, Balcão, Para levar e Delivery.
- Observações multilinha com exemplo “Ponto da carne, alergias...”, seguidas de Em fechamento.
- Salvamento conjunto de cliente, tipo e observações ao mudar o tipo ou sair do formulário. Dados inválidos ou comanda bloqueada não são gravados parcialmente.
- Preservada a correção de QR/link do servidor da 1.4.8, além de OneDrive, impressão e catálogos.

## Banco e escopo

Uma nova migração adiciona apenas a coluna tipo_pedido. Comandas antigas com mesa passam a Local (Mesa); as demais, a Balcão, preservando os outros dados. Faça backup e feche o programa antes de atualizar. Após aplicar a migração, prefira restaurar o backup se precisar voltar a uma versão anterior; não faça downgrade sobre a mesma base sem validar compatibilidade.

Fonte isolada da 1.4.8. Não inclui complementos/estoque e suas quatro migrações, chamadas/QR Codes das mesas, alterações recentes do KDS, recuperação por e-mail ou transferência de licença por código curto. A identificação visual de tipos no KDS recente continua fora desta publicação. O emissor Android não mudou.

## Validação

809 testes .NET distintos aprovados nesta fonte isolada: 464 Desktop, 322 Infrastructure e 23 Servidor. Incluem operador automático, retirada dos controles antigos, ordem dos novos campos, salvamento ao mudar o tipo/sair, validação sem alteração parcial, bloqueio da comanda, preservação de tipos explícitos ao vincular mesa e migração de dados antigos. O modelo de banco foi conferido sem alterações pendentes nem migrações de complementos. Capturas WPF reais dos dois temas verificadas.

Os três instaladores finais foram verificados pelo Microsoft Defender (definições 1.463.36.0), sem detecção nesta análise.

Capturas reais da comanda nos temas claro e escuro, com dados de teste. Instaladores sem assinatura de código. Homologação em Windows limpo, celular e impressoras permanece pendente; esta é uma pré-release.

## Downloads

- [VarthexComanda-1.4.9-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.9/VarthexComanda-1.4.9-Setup-win-x64.exe) — SHA-256 `3DBE7FF405BB639531E1F59548FA8DF8D0C925D06F9FA7BABA59FF02C31C28FE`
- [VarthexComanda-Demo-1.4.9-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.9/VarthexComanda-Demo-1.4.9-Setup-win-x64.exe) — SHA-256 `E92C9A105606EEEAEE27907F64ECEC8A9C7BD27EE8DDE5E512C67353AC9167DE`
- [VarthexComanda-Demo-Restaurante-1.4.9-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.9/VarthexComanda-Demo-Restaurante-1.4.9-Setup-win-x64.exe) — SHA-256 `240EB59E8BB299EC7BFBBAFC0DBA125E3CC0A05346EFAC7E7B3053AC74CD23B3`
