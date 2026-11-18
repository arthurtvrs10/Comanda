# Comanda · por Varthex 1.4.4 — Pedido e impressão

Pré-release Windows x64 de **28/10/2026**, nas edições normal e demonstração. Atualiza a 1.4.3 e mantém a versão estável anterior 1.2.2 disponível.

## Mudanças

- **Prioridade** acima de Vincular mesa e Vincular cliente na lateral do pedido, com opções Normal, Mediana e Alta e indicação da prioridade atual.
- **Vincular mesa** logo abaixo de Prioridade, usando a edição de mesa existente. Campo vazio mantém o pedido como balcão. As duas opções deixam o submenu Mais opções → Dados do pedido.
- Campo de observação e espaçamentos ajustados para acomodar os botões e manter o controle Em fechamento visível em janelas menores.
- Número da comanda em **faixa preta, com texto branco centralizado**, nos modelos de conta, cozinha, reimpressão e fichas de consumação, em papel de 58 e 80 mm.
- Código do pedido em linha separada; identificação MESA/BALCÃO e data/hora da geração da impressão no horário de Brasília preservadas.

## Escopo e instalação

Publicação preparada a partir do commit 2bf58e7 já enviado, com atualização da versão para 1.4.4. Nenhum arquivo local de complementos, estoque ou personalização em desenvolvimento acompanha os instaladores. Não há migração de banco nesta versão. Mantém colaboradores, permissões, senha mestra e guia do servidor da 1.4.3.

Feche o aplicativo e faça backup antes de atualizar. Instalação por usuário no Windows x64, com runtime .NET incluso e ativação offline obrigatória. Dados e licença são preservados. Edição normal com catálogo vazio em uma instalação nova; demonstração com 12 produtos fictícios e base separada. Nenhum banco local, credencial ou chave privada acompanha os pacotes.

Os instaladores não possuem assinatura de código nem alteram automaticamente o firewall. Impressão em impressora física, instalação em Windows limpo, rede móvel física e OAuth com conta Google real continuam pendentes de homologação.

## Validação e arquivos

**881 testes .NET em Release aprovados:** 405 Desktop, 312 Infrastructure, 146 Application, 14 Servidor e 4 Domain. Mais 7 testes JavaScript do atendimento pelo navegador aprovados. A fonte foi extraída do commit enviado e testada sem o módulo local de complementos. Prévia de impressão validada em 58 e 80 mm; a homologação em impressora física permanece pendente.

Instaladores normal e demonstração gerados com Inno Setup 7.1.0 e verificados localmente pelo Microsoft Defender, definições 1.463.9.0, sem detecção. A verificação local não garante o resultado de outros computadores.

| Edição | SHA-256 |
| --- | --- |
| [Normal](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.4/VarthexComanda-1.4.4-Setup-win-x64.exe) | `F69E49B13E0DEBE745E137792B396869AB1F4D6C40442934DEDB3F61EE559C9A` |
| [Demonstração](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.4/VarthexComanda-Demo-1.4.4-Setup-win-x64.exe) | `2F634477B296B0BEE3034825F306C9F2785CD54F0A036D94B73F3EEE276C6D4D` |

Os instaladores e seus arquivos SHA-256 estão nos [anexos da release 1.4.4](https://github.com/arthurtvrs10/Comanda/releases/tag/v1.4.4).
