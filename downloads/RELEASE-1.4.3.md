# Comanda · por Varthex 1.4.3 — Colaboradores, permissões e acesso

Pré-release Windows x64 de **25/10/2026**, nas edições normal e demonstração. Atualiza a 1.4.2; a versão estável anterior 1.2.2 permanece disponível.

## Mudanças

- Cadastro de colaboradores com e-mail, percentual de comissão cadastral e múltiplas funções (Supervisor, Atendente, Entregador e Cozinha).
- Login opcional, liberação de acesso, administrador, senha mestra e permissões por área e operação. Permissões podem ser copiadas de outro colaborador.
- Autorizações pontuais por pessoa habilitada, sem trocar o usuário do atendimento.
- Indicadores verdes e vermelhos com texto, colunas ajustadas à janela e paginação na lista de colaboradores.
- Correção da solicitação de senha após concluir/fechar um atendimento com a segurança desativada. Sessões individuais continuam respeitando suas permissões.
- Guia para iniciar o servidor e liberar o aplicativo no Firewall do Windows após cancelar o aviso de rede. Acesso em Ajuda e na janela do servidor; nenhuma regra de firewall é alterada automaticamente.
- Site com mascote, apresentação do atendimento pelo celular e ajustes visuais.

## Escopo da publicação

A publicação foi preparada e compilada a partir de uma cópia isolada da 1.4.2 com apenas as alterações acima. Produtos, complementos, estoque de complementos e suas regras de personalização, inclusive no navegador, continuam em desenvolvimento e não acompanham o commit, a tag nem os instaladores. Não há migração de banco nesta versão. O percentual de comissão é cadastral, sem cálculo de comissões nesta release.

## Instalação e limites

Feche o aplicativo e faça backup antes de atualizar. Instalação por usuário no Windows x64, sem administrador, com runtime .NET incluso e ativação offline obrigatória. Atualizações preservam dados e licença. Edição normal com catálogo vazio em uma base nova; demonstração com catálogo fictício e base separada. Nenhum banco local, credencial ou chave privada acompanha os instaladores.

Os instaladores não possuem assinatura de código e não alteram o firewall. Instalação em Windows limpo, impressão física, rede móvel física e OAuth com conta Google real continuam pendentes de homologação.

## Validação e arquivos

**Validação:** 881 testes .NET em Release aprovados (405 Desktop, 312 Infrastructure, 146 Application, 14 Servidor e 4 Domain) e 7 testes JavaScript do atendimento móvel. Site aprovado em typecheck, lint e build de produção com Webpack na cópia isolada.

Instaladores gerados com Inno Setup 7.1.0 e verificados localmente pelo Microsoft Defender, definições 1.461.201.0, sem detecção. A verificação local não garante o resultado de outros computadores.

| Edição | SHA-256 |
| --- | --- |
| [Normal](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.3/VarthexComanda-1.4.3-Setup-win-x64.exe) | `1E55D502EDBC67EF079C20AEBEECD18DF35671D98124354C1BFA55520D289B1D` |
| [Demonstração](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.3/VarthexComanda-Demo-1.4.3-Setup-win-x64.exe) | `F7807E0EBC67BC3D14A52AF9F183A60FC182B7382A4E57EA460046943BAA62DA` |

Os instaladores e seus arquivos SHA-256 acompanham os [anexos da release 1.4.3](https://github.com/arthurtvrs10/Comanda/releases/tag/v1.4.3).
