# Consolidação de configurações e acesso — pré-release 1.4.0

Escopo autorizado pelo usuário em 22/10/2026: limpar a interface com Impeccable, validar, commitar e atualizar README e release. Preservados WPF/.NET10, SQLite, operação offline, assemblies, caminhos de dados e AppIds dos instaladores. Sem migração de banco nesta recompilação.

## Revisão e correções

Segurança mantém toggle, resumo de estado e ações aplicáveis; as ações de senha aparecem apenas com proteção ativa e alterações salvas. Login segue Surface/Ink/BotaoPrimario, com placeholders centralizados, toggle de senha e blur somente no sistema desabilitado. Modais de credenciais seguem densidade e tema compartilhados, limite pela área útil e rolagem. A ajuda ❓ foi explicitamente solicitada pelo usuário e funciona também ao receber foco de teclado.

Padrão sem senha e área inicial vazia. Proteção opcional exige cadastro imediato de senha de acesso e senha de recuperação diferentes ao ligar o toggle. Primeiro cadastro não pede senha atual. Todas as áreas ficam liberadas após login, preservando o operador individual nos registros. PIN compartilhado não é uma opção de entrada na nova tela. O fluxo de códigos de recuperação foi removido; credenciais de 64 bytes anteriores continuam reconhecidas, e o formato intermediário de 96 bytes pode entrar com a senha anterior e configurar a recuperação nova. Formato atual: 128 bytes, com sal e hash de 32 bytes para cada senha. Gravação das alterações usa arquivo temporário e substituição; nenhuma senha em texto no arquivo.

Rastreabilidade: extensão de RNF07/RNF18 e CT17, documentada em requisitos e aceitação, conforme pedido do responsável; RNF09/RNF14/RNF17 e CT21 para pacotes. Regras de dinheiro e histórico não foram alteradas.

## Evidências

783 testes distintos: 4 Domain, 134 Application, 303 Infrastructure e 342 Desktop. Resultados em backend/artifacts/revisao-resultados/{domain,application,infrastructure}-final.trx e desktop-final.trx. A suíte Desktop completa passou em Release; repetições focadas de captura não aumentam o total. A primeira passagem revelou quatro testes do comportamento anterior; foram reescritos para cadastro, login, bloqueio, acesso livre e alterações de senha reais, sem ignorar testes.

Validador docs/scripts/validate.sh aprovado (outputs/revisao-documentacao.log), com SQLite CLI ausente; Infrastructure testa SQLite real. Os dois avisos xUnit2031 preexistentes permanecem em testes de teclado. Revisão visual nos temas claro/escuro, 800 e 1180 DIP, com capturas em outputs/revisao-visual/. Imagens segurança/bloqueio do README são capturas WPF reais com metadado de origem, sem montagem ou IA.

Pacotes autocontidos Release win-x64 em artifacts/release-1.4.0-seguranca/. Logs outputs/release-seguranca-instalador.log e outputs/release-seguranca-demo.log. Sem credenciais locais, JSON OAuth ou tokens no pacote.

| Arquivo | Bytes | SHA-256 |
| --- | ---: | --- |
| VarthexComanda-1.4.0-Setup-win-x64.exe | 56392970 | `05C775437D68F7B1A645A08E87C0514EE7DC3F85505F619724F563E8E3036689` |
| VarthexComanda-Demo-1.4.0-Setup-win-x64.exe | 74604034 | `74F792FCDCD858479E1B457FD01C7A0ECA10075D4949E1412AF72D619A1F9C10` |

## Limites

Não foi feita instalação no perfil real nem em Windows limpo. Impressão física, autorização/envio no Google real, leitor de tela completo e vários monitores/DPI ainda não homologados. Sem assinatura de código; manter pré-release. Senhas locais não substituem controle de acesso do Windows. O código não implementa recuperação por e-mail ou Resend.
