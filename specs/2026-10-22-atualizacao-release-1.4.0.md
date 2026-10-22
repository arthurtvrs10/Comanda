# Atualização da pré-release 1.4.0

Data: 22/10/2026. O usuário escolheu atualizar a pré-release 1.4.0 existente e incluir as alterações locais de identidade Comanda · por Varthex, paleta e histórico de vendas. A entrega inclui também Histórico de pedidos e a tela de abertura com mínimo de cinco segundos. Não incrementa a versão e não acrescenta migração de banco.

## Escopo e compatibilidade

Preservar WPF/MVVM, operação local, caminhos de dados, assemblies técnicos, AppIds dos instaladores, licença, perfis, prioridades e dados históricos. Relatórios → Vendas reúne os dois históricos. Abertura usa relógio monotônico para garantir o mínimo, corrigindo arredondamento da espera. O primeiro teste global detectou esse arredondamento; a suíte Desktop completa foi repetida após a correção.

Rastreabilidade: RNF03/RNF07/RNF18 e CT17 para abertura, legibilidade e interação; RF30/UC11/CT25 para histórico de pedidos e apresentação consistente das consultas; RNF09/RNF14/RNF17 e CT21 para empacotamento e atualização. Não altera as regras de cobrança externa ou os snapshots dos itens.

## Pacotes finais

Diretório local: `artifacts/release-1.4.0-revisada/windows/`. Publicação autocontida Windows x64 em Release, produto Comanda, ProductVersion 1.4.0 e FileVersion 1.4.0.0. Os instaladores usam o mesmo diretório publicado; o script Inno da demonstração acrescenta o marcador, catálogo e fotos próprios. A pasta publicada normal não contém catálogo, marcador de demonstração, chave privada, JSON OAuth ou token Google.

| Edição | Arquivo | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| Normal | VarthexComanda-1.4.0-Setup-win-x64.exe | 56.384.427 | `71A1565E950B1CE93E87D0F77370A7EFC84D4C1B8832D2F24866B59E7FF425F8` |
| Demonstração | VarthexComanda-Demo-1.4.0-Setup-win-x64.exe | 74.596.078 | `BDC8D0DDFEFE3D9AB6CBE42A807D32B7B24E0EDF5DB3DE690681EB6616911F99` |

Os dois arquivos SHA-256 atuais estão em downloads/windows/ e downloads/demo/. Os executáveis ficam fora do Git, para anexação à release. Os pacotes anteriores continuam preservados localmente em artifacts/release-1.4.0/windows/. A referência remota anterior à atualização era `556a60bf7d16020111402db763c8b3ddf4b12039`, tanto em main quanto em v1.4.0. A publicação externa será verificada após o envio; este registro preparatório não a comprova.

## Validação

770 testes distintos aprovados em Release: 4 Domain, 133 Application, 303 Infrastructure e 330 Desktop. Logs: outputs/release-1.4.0-atualizada-testes.log e outputs/release-1.4.0-atualizada-desktop-final.log. Os TRX estão em backend/artifacts/release-1.4.0-atualizada-resultados/. Os 21 testes focados de captura repetem testes já contados: outputs/release-1.4.0-atualizada-capturas.log.

O validador docs/scripts/validate.sh passou, com SQL ignorado pela ausência da CLI sqlite3; os testes Infrastructure usam SQLite real. Log: outputs/release-1.4.0-atualizada-documentacao.log. Compilação dos instaladores: outputs/release-1.4.0-revisada-instaladores.log e outputs/release-1.4.0-revisada-demo.log. Há dois avisos xUnit2031 preexistentes no projeto de testes, sem falha da compilação final.

O executável autocontido final foi iniciado e permaneceu ativo, com janela identificada como Comanda · Versão 1.4.0. Não houve autenticação automatizada. A captura nativa ao vivo estava ocluída por outro aplicativo e não foi usada como evidência visual; as capturas controladas abaixo sustentam a revisão da interface.

## Capturas do README

Capturas de janelas e componentes WPF reais via RenderTargetBitmap a 96 DPI, com dados fictícios. Origem em .impeccable/review/release-1.4.0-atualizada/. Os PNGs recebem metadado de procedência antes da cópia; origem e destino têm os mesmos bytes. Não houve ImageGen nem composição artificial das telas.

| Destino em docs/design/ | Origem |
| --- | --- |
| principal-claro.png | fundo-padrao-claro.png |
| principal-escuro.png | fundo-padrao-escuro.png |
| abertura.png | abertura-1366.png |
| atendimento-grade.png | grade-comandas-claro-1180.png |
| atendimento-comanda.png | prioridade-False-Normal.png |
| produtos.png | ribbon-produtos-maximizado-claro.png |
| historico.png | historico-vendas-claro-1180.png |
| historico-pedidos.png | historico-False-1180.png |

Escaneamento de procedência: 8 rasters/0 ausências em docs/design e 5 rasters/0 ausências nos Assets do aplicativo. Os logos modificados derivam dos vetores locais em brand/comanda-varthex. A arte da abertura foi fornecida pelo usuário; sua procedência está na especificação da tela.

O conjunto de marca contém 91 rasters/0 ausências de procedência. O registro foi incorporado sem alterar os pixels das imagens. As oito cópias do README preservam os hashes de suas capturas de origem.

## Revisão visual e registro de design

A revisão independente retornou `ship`, sem achados materiais no README, nas 12 capturas inspecionadas e nas fontes amostradas. Além das oito imagens mapeadas acima, foram revistas quatro capturas em `.impeccable/review/release-1.4.0-atualizada/`: `area-trabalho-maximizada-claro.png`, `historico-vendas-escuro-800.png`, `historico-True-800.png` e `ribbon-produtos-claro-800.png`. A quebra de linha das barras dos históricos a 800 DIP e a rolagem de Produtos foram aceitas no escopo dessa revisão.

As fontes de referência são MainWindow.Ribbon, TemaInterface, PaletaComanda, HistoricoView, EstilosHistorico, FundoSistema e JanelaAbertura. Código e ativos estão no commit `4507323`. A identidade Comanda, por Varthex, a paleta operacional consolidada e o Histórico de vendas já estão registrados no início de DESIGN.md; o apêndice desta atualização vincula essa decisão às evidências de entrega, sem redesenhar componentes nem criar tokens. A revisão não comprova publicação externa ou os ambientes e fluxos pendentes abaixo.

## Limites

Instalação em Windows limpo, impressão física, leitor de tela completo, múltiplos monitores/DPI e OAuth/envio/restauração com conta Google real permanecem pendentes. Não foram instalados os pacotes no perfil real nem publicados dados locais do usuário. O produto continua pré-release, com instaladores sem assinatura de código; a 1.2.2 permanece estável. O escopo não inclui corrigir o formato legado de DESIGN.md ou preencher as ausências preexistentes de PRODUCT.md e .impeccable/design.json.
