# Comanda · por Varthex

A identidade **Comanda · por Varthex** aparece na abertura, área de trabalho, ajuda e rodapé. [Logos e recursos da marca](brand/comanda-varthex/README.md).

Sistema Windows para organizar mesas, comandas e consumo em pequenos estabelecimentos de alimentação. Produtos, atendimento, fechamento e histórico funcionam localmente, sem depender da internet.

**Release 1.4.2 · 24/10/2026 · versão em testes.** Correção compartilhada do arraste rápido nas nove janelas internas. Mantém servidor automático, logs e fila de eventos, até 500 comandas, catálogo e Cozinha (KDS). Edições normal e demonstração incluem o .NET e exigem ativação offline.

<img src="docs/design/principal-claro.png" alt="Área de trabalho Comanda com fundo cinza, logo centralizado e alternância de modo escuro" width="1000">

## Baixar para Windows

| Edição 1.4.2 | Para quem | Download |
| --- | --- | --- |
| **Normal — sem demonstração** | Cadastrar os produtos reais do estabelecimento; catálogo vazio em uma instalação nova | [![Baixar edição normal para Windows](docs/design/download-windows.svg)](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.2/VarthexComanda-1.4.2-Setup-win-x64.exe) · [SHA-256](downloads/windows/VarthexComanda-1.4.2-Setup-win-x64.exe.sha256) |
| **Demonstração** | Conhecer o sistema com 12 produtos fictícios e fotos; sem comandas abertas em uma base nova | [![Baixar demonstração para Windows](docs/design/download-windows.svg)](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.2/VarthexComanda-Demo-1.4.2-Setup-win-x64.exe) · [SHA-256](downloads/demo/VarthexComanda-Demo-1.4.2-Setup-win-x64.exe.sha256) |

**Instaladores 1.4.2 verificados em 24/10/2026:** gerados com Inno Setup 7.1.0 e verificados localmente pelo Defender, com proteção ativa e sem detecção. Confira o SHA-256; o resultado local não garante a decisão de outros computadores. [Notas e verificação](downloads/RELEASE-1.4.2.md).

[Notas da release 1.4.2](https://github.com/arthurtvrs10/Comanda/releases/tag/v1.4.2) · [Instalação normal](downloads/windows/README.md) · [Instalação da demonstração](downloads/demo/README.md)

Instalação por usuário, sem administrador, em Windows x64. Feche o aplicativo antes de atualizar. Reinstalar ou atualizar preserva os dados e a licença existentes. Os instaladores não possuem assinatura de código.

A release é uma **pré-release**: instalação em Windows limpo e autorização/envio com uma conta Google real continuam pendentes. A [versão estável anterior 1.2.2](https://github.com/arthurtvrs10/Comanda/releases/tag/v1.2.2) permanece disponível.

## Principal, Balcão, Cozinha e atendimento pelo celular

Principal reúne Mesas / comandas, Produtos, os dois históricos e Painel de senhas. COZINHA (KDS) é uma categoria própria; Configurações e Ajuda permanecem. As antigas categorias Produtos, Marketing, Financeiro, Relatórios e Apps foram removidas da navegação.

O primeiro servidor local permite ao garçom abrir comandas e lançar produtos pelo navegador na mesma rede. O servidor inicia ao abrir o desktop. Em **Configurações → Servidor / acesso pelo celular**, selecione o IP da rede e leia o QR Code no celular. Informe seu nome antes de lançar itens; quantidade e observação seguem as regras de UN/KG/L. Cobrança e fechamento ficam no computador. Pare o servidor antes de restaurar backups.

**Servidor automático na 1.4.1:** o servidor inicia automaticamente ao abrir o Comanda. O painel **Informações do Servidor**, acessível pelo mesmo comando em Configurações, reúne nome do computador, IPs, porta, QR Code e acessos autenticados desta sessão. Permite desativar o início automático, testar o serviço neste PC e iniciar/parar manualmente. Porta ocupada ou falha de inicialização não impedem o uso do desktop; restauração mantém o servidor parado. Essa atualização está incluída nos dois instaladores da 1.4.1. [Fluxo e validação](specs/2026-10-24-servidor-automatico-informacoes.md).

**Atendimento na 1.4.1:** Configurações → **Logs e fila de eventos** acompanha o servidor e o desktop com busca, filtros e detalhes. Quantidade de comandas configurável até 500 posições; pedidos anteriores preservados. No catálogo de fotos, UN adiciona uma unidade no clique, KG/L abrem personalização e … permite personalizar qualquer medida. O KDS destaca itens prontos com texto verde e riscado. [Contrato e validação](specs/2026-10-24-registros-kds-limite-catalogo.md). Os botões + e … ficam dentro do cartão, abaixo do preço.


No celular, abrir uma comanda mostra seus itens, observações e total. **+ Produtos** abre uma tela separada com busca, categorias e cartões com as mesmas fotos cadastradas no desktop. Toque no cartão para ajustar quantidade/observações e confirmar; o sistema retorna à comanda atualizada. Produtos sem imagem mostram um substituto neutro. [Fluxo e validação](specs/2026-10-23-web-comanda-catalogo-fotos.md).

**Cozinha (KDS) no desktop:** cartões por comanda/mesa, observações, estados Pendente → Em preparo → Pronto → Entregue, ações por item e pelo conjunto exibido, filtros de local/situação e aviso sonoro opcional. Atualiza a cada três segundos; atenção após 10 minutos e atraso após 20. Entrega e cobrança são independentes. [Fluxo, migração e validação](specs/2026-10-23-cozinha-kds-desktop.md).

A release 1.4.1 integra esses recursos aos dois instaladores. HTTP local verificado; homologação em celular físico e outras redes permanece pendente. [Contrato e limites](specs/2026-10-23-principal-servidor-local.md) · [Decisão arquitetural](docs/docs/adr/0006-servidor-local-atendimento-movel.md).

## Celular e firewall

O servidor inicia automaticamente no desktop, na porta padrão **3001**. Confira em **Configurações → Servidor / acesso pelo celular**; se o início automático estiver desativado, clique em **Iniciar servidor**. Selecione o IP da rede e leia o QR Code atualizado no celular conectado ao mesmo Wi-Fi. O link contém a chave da sessão; abrir apenas o endereço não autoriza operações. Fechar o aplicativo ou parar o servidor encerra o acesso; reiniciar exige um novo link.

O instalador não altera o firewall. Se houver bloqueio, permita o executável instalado no perfil da rede, somente entrada TCP na porta escolhida e sub-rede local. Regras de bloqueio prevalecem sobre permissões. Não desligue o firewall nem encaminhe portas no roteador. A permissão de um executável em artifacts não cobre necessariamente o caminho instalado. A demonstração oferece o mesmo servidor com base separada.

## Landing page

A apresentação está em `site/`, em Next.js e TypeScript, separada do servidor de atendimento. Execute `npm ci` nessa pasta e `npm run dev -- --port 3010`. Para produção local: `npm run build` e `npx next start --port 3010`. A porta 3010 evita conflito com o acesso móvel em 3001.

## Novidades da 1.4.2

- **Arraste das nove janelas internas:** Mesas / comandas, Balcão, Produtos, históricos de pedidos e vendas, Cozinha, Configurações, Backup e Logs compartilham a mesma correção. O movimento usa a posição do mouse em uma referência fixa e aplica apenas o destino mais recente por quadro, sem acumular posições intermediárias.
- **Cronômetros durante o gesto:** a atualização visual dos tempos das comandas pausa durante o arraste e retoma no próximo segundo, calculando o tempo real.
- **Validação:** 869 testes aprovados em Release, incluindo 17 testes de janelas e movimento. Rajadas de 2.002 movimentos nas nove telas preservam conteúdo, posição final e medição. A fluidez com mouse físico ainda precisa ser confirmada no uso real.

[Notas, arquivos e limites da 1.4.2](downloads/RELEASE-1.4.2.md).

### Recursos da 1.4.1 mantidos

- **Servidor automático:** preferência e porta salvas, informações de rede, QR Code, teste local e acessos autenticados da sessão. Falhas de inicialização não bloqueiam o desktop.
- **Logs e fila de eventos:** em Configurações, consulte registros com detalhes, busca e filtros. Acompanhe abertura de comandas, inclusão de itens e preparo no desktop e no servidor, com estados reais de espera, processamento e conclusão. A fila não reenvia pedidos.
- **Até 500 comandas:** configure de 1 a 500 posições em Numeração de pedidos. A faixa respeita o número inicial; pedidos existentes e históricos são preservados.
- **Catálogo por medida:** clique no cartão ou em + para adicionar uma unidade de UN. KG/L abrem a personalização para informar a medida. … sempre personaliza. As ações ficam dentro do cartão, abaixo do preço.
- **KDS mais claro:** itens prontos ficam com texto verde e riscado; observações permanecem legíveis.

[Notas, verificação e limites da 1.4.1](downloads/RELEASE-1.4.1.md).

## Recursos mantidos

- **Pagamento por etapas:** selecione dinheiro, Pix, débito ou crédito, informe o valor na janela de lançamento e confirme o recebimento. Registros parciais e mistos ficam salvos; resumo com valor pago, recebido, troco em dinheiro, saldo e progresso. Finalize ou finalize e imprima após quitar a conta.
- **Conferir e rachar a conta:** toggle à esquerda revela os itens e a divisão igual por pessoas. A seleção de itens calcula o pagamento e identifica as linhas já pagas; cada seleção inclui toda a quantidade da linha.
- **Drive após o login:** convite uma vez por abertura quando não conectado. Adiar mantém a conexão manual em Backup → Google Drive; bloquear/desbloquear não repete o convite.
- **Renovação por WhatsApp:** aviso nos últimos sete dias da licença, com destaque vermelho e botão verde. A renovação é combinada com o suporte, e a nova chave é inserida na ativação.
- **Demonstração pronta para começar:** catálogo de exemplo sem pedidos, comandas ou vendas pré-abertas. Os dados reais ficam separados.

- **Configurações redesenhadas:** navegação com ícones, toggles com estado visível, campos agrupados e aviso de alterações reais. A quantidade de comandas fica em Numeração de pedidos.
- **Acesso opcional:** instalações novas abrem na área inicial, sem senha e sem abrir comandas automaticamente. Todos os usuários têm acesso às áreas nesta versão.
- **Proteção por senha:** ao ligar o toggle em Segurança, crie e confirme a senha de acesso e uma senha de recuperação diferente (mínimo de 4 caracteres). Salve as alterações para aplicar. Login inicial: `gestor`.
- **Bloqueio e recuperação:** com proteção ativa, o login aparece ao iniciar e bloquear. Campos centralizados, placeholders, toggle Mostrar senha e sistema desfocado ao fundo. Esqueci minha senha permite definir uma nova senha de acesso usando a senha de recuperação, sem internet.
- **Impressos térmicos:** conta, cozinha, reimpressão, fichas e teste de impressora com papel de 58 ou 80 mm, conforme a impressora configurada.

- **Área de trabalho com janelas internas:** abra, mova, redimensione, maximize e organize os módulos abaixo da Ribbon. A antiga tela de seleção de perfis foi removida.
- **Abertura com a marca:** arte abstrata, logo central, versão e indicação de carregamento. Permanece por pelo menos 5 segundos antes de apresentar a tela principal.
- **Modo escuro por toggle:** ative ou desative no cabeçalho; a preferência fica salva neste computador. Fundo padrão com logo central: cinza no claro e azul profundo no escuro, preservando imagens e cores personalizadas. Controles com superfícies neutras nos dois temas.
- **Mesas e comandas:** observações, responsável, prioridades por cor, transferência/cópia de itens e indicação amarela de **Em fechamento**. Numeração inicial configurável.
- **Catálogo e lançamento:** produtos com fotos ou em lista, filtro por categoria, ordenação visível e personalização de quantidade e observações. Venda por **UN, KG ou L**, com até três casas decimais para KG/L.
- **Backup com progresso:** janela própria, barra e mensagens da execução, restauração de dados e fotos, Google Drive por OAuth preparado e OneDrive por pasta sincronizada.
- **Ajuda:** painel compacto ao lado do atalho, WhatsApp, RustDesk, sugestões e tutorial guiado. A central de ajuda está em preparação.
- **Relatórios:** Histórico de pedidos e Histórico de vendas reunidos em Principal. Pesquisa, colunas, ordenação, exportação Excel e prévia de impressão; detalhes preservam os registros originais.

## Recursos

| Área | O que oferece |
| --- | --- |
| Produtos | Categorias, preço de custo e venda, fotos, unidades de venda e ativação/desativação |
| Atendimento | Mesas opcionais, comandas numeradas, cliente opcional, itens, quantidades, observações e total |
| Fechamento | Recebimentos manuais parciais/mistos, divisão por pessoas ou itens, saldo, troco em dinheiro e finalização com impressão |
| Relatórios | Históricos de pedidos e vendas, filtros, colunas, detalhes, Excel, impressão e resumo diário |
| Continuidade | Backup automático/manual local, checksum, restauração, recuperação de comandas e logs |
| Acesso | Senha opcional, login de gestor/colaboradores, senha de recuperação offline e licença vinculada ao computador |

A cobrança é realizada fora do sistema; não há integração com maquininha, emissão fiscal ou estoque completo. O atendimento pelo celular depende do servidor e da mesma rede. A Cozinha (KDS) é local, sem KDS web. Pedidos independentes e evolução do atendimento continuam no [plano](docs/docs/21-evolucao-atendimento.md).

## Instalar e começar

1. Escolha a edição normal ou demonstração e execute o instalador.
2. Ative o aplicativo com a licença fornecida pelo responsável.
3. A instalação nova abre sem senha. Na edição normal, cadastre categorias e produtos; a demonstração já traz o catálogo fictício. Se a proteção estiver ativada, entre com seu login e senha.
4. Abra **Principal → Mesas / comandas**, selecione o número e lance os produtos.
5. Confira o total e realize a cobrança externamente. Em Pagamento, selecione a forma, registre cada recebimento e finalize quando o saldo estiver quitado.

A edição normal usa `%LOCALAPPDATA%\VarthexComanda` e a demonstração usa `%LOCALAPPDATA%\VarthexComandaDemo`. Os atalhos e instalações são separados; a demonstração abre pelo atalho **Comanda Demonstração**. O catálogo fictício é carregado somente na primeira base vazia e não substitui alterações posteriores.

Para ativar a senha: **Configurações → Configurações do sistema → Segurança → Ativar proteção por senha**. O toggle abre o cadastro; cancelar mantém a proteção desativada. Guarde a senha de recuperação separadamente. Quem já tem credenciais pode configurar ou alterar a recuperação em **Segurança → Senha de recuperação**, confirmando a senha de acesso atual. Não há senha mestra universal, recuperação por e-mail ou chave Resend nesta versão.

A proteção é local e não substitui o controle de acesso do Windows. Atualizações preservam a preferência existente; desative o toggle e salve caso queira acesso livre. Enquanto houver alterações pendentes, salve antes de gerenciar as senhas.

Em **Configurações → Backup e restauração**, escolha a pasta local e faça uma cópia. O Google Drive exige cadastrar/importar um cliente OAuth de aplicativo para computador; nenhuma credencial acompanha o instalador. O convite aparece uma vez por abertura, depois do login, e pode ser adiado após cinco segundos; para conectar mais tarde, abra Backup → Google Drive. O OneDrive usa a pasta sincronizada pelo aplicativo instalado no Windows. O backup local continua disponível sem conexão.

[Operação e recuperação](docs/docs/12-operacao-implantacao.md) · [Licenciamento](docs/docs/20-licenciamento-offline.md) · [Configurar Google Drive](docs/docs/22-configuracao-google-drive.md)

## Pagamento e renovação

<img src="docs/design/pagamento.png" alt="Pagamento com formas à esquerda, resumo dos recebimentos e saldo restante" width="1000">

<img src="docs/design/aviso-renovacao.png" alt="Aviso de vencimento em vermelho e botão verde de renovação pelo WhatsApp" width="540">

Recebimentos são registros manuais da cobrança externa, sem integração bancária. Remover um lançamento não faz estorno. Enquanto houver recebimentos ativos, alterações dos itens e cancelamento da comanda ficam bloqueados. Não há fiado, taxas/descontos ou dados de maquininha nesta entrega.

A atualização adiciona a migração de recebimentos à comanda e à venda. Mantenha o backup antes de atualizar; o sistema preserva uma cópia antes de migrar.

## Configurações e acesso

<img src="docs/design/seguranca.png" alt="Configurações de segurança com toggle de proteção por senha" width="1000">

<img src="docs/design/bloqueio.png" alt="Login e senha com toggle Mostrar senha e sistema desfocado ao fundo" width="1000">

## Telas da release

Capturas da interface WPF 1.4.2, geradas novamente em 24/10/2026 com dados fictícios de teste. Não contêm registros de clientes. [Galeria e origem das novas capturas](docs/design/release-1.4.2/README.md).

**Janelas organizadas na área de trabalho**

<img src="docs/design/release-1.4.2/janelas-claro.png" alt="Mesas e produtos em janelas internas independentes na área de trabalho, com dados fictícios" width="1000">

**Balcão**

<img src="docs/design/release-1.4.2/balcao.png" alt="Balcão da versão 1.4.2 com pedidos fictícios e ações de atendimento" width="1000">

**Tela de abertura**

<img src="docs/design/abertura.png" alt="Abertura Comanda por Varthex com arte abstrata, logo branco, carregamento e versão 1.4.2" width="760">

**Modo escuro e toggle ativado**

<img src="docs/design/principal-escuro.png" alt="Tema escuro com alternância ativada, fundo azul profundo e marca Comanda centralizada" width="1000">

**Grade de comandas**

<img src="docs/design/atendimento-grade.png" alt="Grade atual de comandas livres e ocupadas, com barra de rolagem" width="1000">

**Consumo e fechamento**

<img src="docs/design/atendimento-comanda.png" alt="Comanda com prioridade, responsável, observações, itens, subtotal e pagamento" width="1000">

**Produtos em janela interna**

<img src="docs/design/produtos.png" alt="Janela interna de produtos com busca, ordenação e ações de cadastro" width="1000">

**Histórico de vendas**

<img src="docs/design/historico.png" alt="Histórico com filtro de data, vendas e detalhes dos itens" width="1000">

**Histórico de pedidos**

<img src="docs/design/historico-pedidos.png" alt="Histórico de pedidos com pesquisa por período, colunas, situação, totais e exportação" width="1000">

**Catálogo com fotos e ações abaixo do preço**

<img src="docs/design/catalogo-fotos.png" alt="Produtos com fotos, preço e botões de adicionar e personalizar dentro do cartão" width="1000">

**Logs e fila de eventos**

<img src="docs/design/logs-eventos.png" alt="Fila de eventos do servidor e desktop com origem, operação, estado e filtros" width="1000">

**Cozinha (KDS)**

<img src="docs/design/cozinha-kds.png" alt="Cozinha com observações destacadas e itens prontos em verde e riscados" width="1000">

## Android — gerador de licenças

O aplicativo Android é exclusivo do responsável pela emissão de licenças; não é um aplicativo de atendimento. A chave privada do emissor não acompanha os pacotes e não deve ser compartilhada com clientes.

[Downloads e instruções do emissor](mobile/licencas/README.md) · [Arquivos Android](downloads/android/README.md)

## Desenvolvimento

Stack atual: **C# e .NET 10 LTS, WPF/MVVM, SQLite e Entity Framework Core**, com camadas Domain, Application, Infrastructure e Desktop.

No Windows, com SDK .NET 10:

```powershell
dotnet build backend/VarthexComanda.slnx
dotnet test backend/VarthexComanda.slnx --configuration Release
dotnet run --project backend/src/VarthexComanda.Desktop
```

Instalador normal: `scripts/gerar-instalador.ps1`. Demonstração: `scripts/gerar-demo.ps1`. Preferem Inno Setup 7 e verificam os instaladores com Defender antes de gerar os hashes. ZIP alternativo: `scripts/publicar.ps1`.

Distribua somente os pacotes preparados. A pasta `artifacts` pode conter material privado do emissor e não deve ser publicada inteira. JSON OAuth, tokens Google e chaves privadas ficam fora do Git e dos instaladores.

## Documentação e validação

Esta atualização 1.4.2 reúne a correção compartilhada do arraste, novas capturas e instaladores normal e demonstração. Os arquivos históricos das versões anteriores permanecem disponíveis.

Validação registrada nas [notas de distribuição 1.4.2](specs/2026-10-24-release-1.4.2.md). Instalação em Windows limpo, impressão física, fluidez com mouse físico e OAuth com conta Google real permanecem pendentes.

- [Índice completo](docs/README.md)
- [Notas e evidências da 1.4.0](specs/2026-10-22-release-1.4.0.md)
- [Atualização da 1.4.0, capturas e novos pacotes](specs/2026-10-22-atualizacao-release-1.4.0.md)
- [Abertura do sistema](specs/2026-10-22-tela-abertura.md)
- [Histórico de alterações](docs/CHANGELOG.md)
- [Drive, medidas e migração](specs/2026-10-22-drive-oauth-medidas-venda.md)
- [Plano de evolução](docs/docs/21-evolucao-atendimento.md)
- [Homologação](docs/homologacao/README.md)

Testes automatizados e capturas locais não substituem homologação no estabelecimento, instalação em Windows limpo ou ensaio de autorização e backup com conta Google real.

## Licença do código

Nenhuma licença de uso do código foi definida. A licença comercial de ativação do aplicativo é um assunto separado.
