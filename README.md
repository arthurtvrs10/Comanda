# Varthex Comanda

Sistema Windows para organizar mesas, comandas e consumo em pequenos estabelecimentos de alimentação. Produtos, atendimento, fechamento e histórico funcionam localmente, sem depender da internet.

**Nova release: 1.4.0 · 22/10/2026 · versão em testes.** Disponível nas edições normal e demonstração. Ambas incluem o .NET e exigem ativação offline.

<img src="docs/design/principal-claro.png" alt="Área de trabalho Varthex com fundo neutro, nome centralizado e toggle de modo escuro no cabeçalho" width="1000">

## Baixar para Windows

| Edição 1.4.0 | Para quem | Download |
| --- | --- | --- |
| **Normal — sem demonstração** | Cadastrar os produtos reais do estabelecimento; catálogo vazio em uma instalação nova | [Baixar instalador Windows x64](https://github.com/arthurtvrs10/Varthex-Orders/releases/download/v1.4.0/VarthexComanda-1.4.0-Setup-win-x64.exe) · [SHA-256](downloads/windows/VarthexComanda-1.4.0-Setup-win-x64.exe.sha256) |
| **Demonstração** | Conhecer o sistema com 12 produtos fictícios, fotos e preços de teste | [Baixar instalador Windows x64](https://github.com/arthurtvrs10/Varthex-Orders/releases/download/v1.4.0/VarthexComanda-Demo-1.4.0-Setup-win-x64.exe) · [SHA-256](downloads/demo/VarthexComanda-Demo-1.4.0-Setup-win-x64.exe.sha256) |

[Notas da release 1.4.0](https://github.com/arthurtvrs10/Varthex-Orders/releases/tag/v1.4.0) · [Instalação normal](downloads/windows/README.md) · [Instalação da demonstração](downloads/demo/README.md)

Instalação por usuário, sem administrador, em Windows x64. Feche o aplicativo antes de atualizar. Reinstalar ou atualizar preserva os dados e a licença existentes. Os instaladores não possuem assinatura de código.

A release é uma **pré-release**: instalação em Windows limpo e autorização/envio com uma conta Google real continuam pendentes. A [versão estável anterior 1.2.2](https://github.com/arthurtvrs10/Varthex-Orders/releases/tag/v1.2.2) permanece disponível.

## Novidades da 1.4.0

- **Área de trabalho com janelas internas:** abra, mova, redimensione, maximize e organize os módulos abaixo da Ribbon. A antiga tela de seleção de perfis foi removida.
- **Modo escuro por toggle:** ative ou desative no cabeçalho; a preferência fica salva neste computador. Fundo padrão cinza neutro com VARTHEX ao centro, preservando imagens personalizadas.
- **Mesas e comandas:** observações, responsável, prioridades por cor, transferência/cópia de itens e indicação amarela de **Em fechamento**. Numeração inicial configurável.
- **Catálogo e lançamento:** produtos com fotos ou em lista, filtro por categoria, ordenação visível e personalização de quantidade e observações. Venda por **UN, KG ou L**, com até três casas decimais para KG/L.
- **Backup com progresso:** janela própria, barra e mensagens da execução, restauração de dados e fotos, Google Drive por OAuth preparado e OneDrive por pasta sincronizada.
- **Ajuda:** painel compacto ao lado do atalho, WhatsApp, RustDesk, sugestões e tutorial guiado. A central de ajuda está em preparação.

## Recursos

| Área | O que oferece |
| --- | --- |
| Produtos | Categorias, preço de custo e venda, fotos, unidades de venda e ativação/desativação |
| Atendimento | Mesas opcionais, comandas numeradas, cliente opcional, itens, quantidades, observações e total |
| Fechamento | Conferência e confirmação manual da cobrança externa, com registro da forma informada |
| Histórico | Consulta de vendas, itens e resumo diário |
| Continuidade | Backup automático/manual local, checksum, restauração, recuperação de comandas e logs |
| Acesso | PIN de atendimento, acesso de gestor, colaboradores e ativação offline vinculada ao computador |

A cobrança é realizada fora do sistema; não há integração com maquininha, emissão fiscal, estoque completo ou atendimento em rede. A cozinha disponível é uma consulta local de comandas abertas, sem estados de preparo. Pedidos independentes, preparo e atendimento móvel continuam no [plano de evolução](docs/docs/21-evolucao-atendimento.md).

## Instalar e começar

1. Escolha a edição normal ou demonstração e execute o instalador.
2. Ative o aplicativo com a licença fornecida pelo responsável.
3. Entre no perfil adequado. Na edição normal, cadastre categorias e produtos; a demonstração já traz o catálogo fictício.
4. Abra **Principal → Mesas / comandas**, selecione o número e lance os produtos.
5. Confira o total, realize a cobrança externamente e confirme o fechamento no sistema.

A edição normal usa `%LOCALAPPDATA%\VarthexComanda` e a demonstração usa `%LOCALAPPDATA%\VarthexComandaDemo`. Os atalhos e instalações são separados; a demonstração abre pelo atalho **Comanda Demonstração**. O catálogo fictício é carregado somente na primeira base vazia e não substitui alterações posteriores.

Em **Configurações → Backup e restauração**, escolha a pasta local e faça uma cópia. O Google Drive exige cadastrar/importar um cliente OAuth de aplicativo para computador; nenhuma credencial acompanha o instalador. O convite pode ser adiado após cinco segundos. O OneDrive usa a pasta sincronizada pelo aplicativo instalado no Windows. O backup local continua disponível sem conexão.

[Operação e recuperação](docs/docs/12-operacao-implantacao.md) · [Licenciamento](docs/docs/20-licenciamento-offline.md) · [Configurar Google Drive](docs/docs/22-configuracao-google-drive.md)

## Telas da release

Capturas da interface WPF da 1.4.0, geradas com dados fictícios de teste. Não contêm registros de clientes. O fundo da área de trabalho permanece neutro nos dois temas.

**Modo escuro e toggle ativado**

<img src="docs/design/principal-escuro.png" alt="Tema escuro da Ribbon com toggle ativado e fundo VARTHEX neutro" width="1000">

**Grade de comandas**

<img src="docs/design/atendimento-grade.png" alt="Grade atual de comandas livres e ocupadas, com barra de rolagem" width="1000">

**Consumo e fechamento**

<img src="docs/design/atendimento-comanda.png" alt="Comanda com prioridade, responsável, observações, itens, subtotal e pagamento" width="1000">

**Produtos em janela interna**

<img src="docs/design/produtos.png" alt="Janela interna de produtos com busca, ordenação e ações de cadastro" width="1000">

**Histórico de vendas**

<img src="docs/design/historico.png" alt="Histórico com filtro de data, vendas e detalhes dos itens" width="1000">

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

Instalador normal: `scripts/gerar-instalador.ps1`. Demonstração: `scripts/gerar-demo.ps1`. Exigem Inno Setup 6. ZIP alternativo: `scripts/publicar.ps1`.

Distribua somente os pacotes preparados. A pasta `artifacts` pode conter material privado do emissor e não deve ser publicada inteira. JSON OAuth, tokens Google e chaves privadas ficam fora do Git e dos instaladores.

## Documentação e validação

- [Índice completo](docs/README.md)
- [Notas e evidências da 1.4.0](specs/2026-10-22-release-1.4.0.md)
- [Histórico de alterações](docs/CHANGELOG.md)
- [Drive, medidas e migração](specs/2026-10-22-drive-oauth-medidas-venda.md)
- [Plano de evolução](docs/docs/21-evolucao-atendimento.md)
- [Homologação](docs/homologacao/README.md)

Testes automatizados e capturas locais não substituem homologação no estabelecimento, instalação em Windows limpo ou ensaio de autorização e backup com conta Google real.

## Licença do código

Nenhuma licença de uso do código foi definida. A licença comercial de ativação do aplicativo é um assunto separado.
