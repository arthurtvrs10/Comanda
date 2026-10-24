# Servidor automático e informações da rede

Solicitação de 24/10/2026: atualizar o servidor conforme a imagem enviada e iniciar o sistema já com o servidor funcionando. Interpretação executada: iniciar ao abrir o aplicativo Comanda, preservando o servidor HTTP local e o acesso pelo navegador existente. Não envolve instalar um serviço do Windows nem trocar SQLite por Firebird.

## Contrato de direção

- THESIS: verificar neste PC se o servidor está funcionando e conectar outro dispositivo sem precisar iniciá-lo a cada abertura.
- OWN-WORLD: WPF, Segoe UI, cantos retos, fundos neutros, recursos dinâmicos claro/escuro e cabeçalho auxiliar existente. Referência de organização: print do usuário; conteúdo e estados vêm do Comanda real.
- STORY: abrir Comanda → servidor inicia em segundo plano → Configurações → Servidor / acesso pelo celular → conferir IP/porta, copiar link/ler QR ou verificar acessos. Parar/iniciar e testar continuam disponíveis.
- FIRST VIEWPORT: duas colunas em janela ampla, informações do servidor à esquerda e configuração do cliente à direita; status escrito junto às cores. Em janela menor, painéis empilhados com rolagem e ações fixas no rodapé.
- FORM: extensão da tela operacional existente; sem concept roll, comp novo ou raster de produto. Referência não autoriza simular Firebird, código de pareamento em nuvem ou histórico inexistente.

## Ciclo de vida e preferências

Início automático ativado por padrão, inclusive quando as novas chaves ainda não existem. Porta padrão 3001; preferência de início e porta persistidas separadamente em `servidor.iniciar_automaticamente` e `servidor.porta`, sem sobrescrever as demais configurações. A preferência de início afeta as próximas aberturas; desligá-la não encerra uma sessão ativa. Porta é validada e salva ao iniciar manualmente.

MainWindow agenda uma tentativa por abertura após Loaded, fora da thread de interface. Modo de restauração não inicia servidor. Início respeita licença, backup manual em andamento e proteção de porta; falha mantém o desktop utilizável e permite corrigir/repetir manualmente. Não trocar de porta silenciosamente. Fechar somente a janela de informações mantém o serviço; fechar o Comanda encerra o host. Cada início continua gerando chave nova, com autenticação e idempotência existentes.

Parada mantém o host marcado ativo/em parada até terminar o encerramento e descarte. Restauração é impedida enquanto ativo, iniciando ou parando. O descarte síncrono inicia a parada no ThreadPool, evitando esperar no mesmo contexto STA do WPF. O teste integrado revelou uma espera no Dispose ao fechar a janela principal; pilhas registradas em outputs/servidor-pilhas.txt e nova regressão cobre esse encerramento. Avisos operacionais de licença são isolados apenas na fixture para não consultar a licença instalada; comportamento de produção permanece.

## Informações verificáveis

Nome do computador e caminho local do banco são somente leitura. Interfaces IPv4 mostram nome, endereço e situação real; indisponibilidade de leitura não deve impedir o painel. Endereços autorizados para acesso continuam limitados às interfaces locais e a loopback; se apenas loopback estiver selecionado, a tela informa que o endereço funciona somente neste PC.

Escuta da porta e resposta HTTP local são diagnósticos locais. O painel não afirma que o firewall foi liberado nem que outro aparelho já consegue conectar. Teste HTTP usa a página inicial, não altera pedidos e não cria acesso autenticado fictício. Instruções do cliente usam navegador, mesma rede, QR/link e reserva de IP; não enviam caminho do banco ao cliente.

Últimos acessos são **desta sessão**, agrupados pelo IP real da conexão, somente após API autenticada bem-sucedida. Cabeçalhos enviados pelo cliente não definem o IP. Snapshot sincronizado, limitado a 256 endereços e limpo a cada novo início; não grava chave, corpo, nome de cliente ou histórico de 30 dias. Não equivale a medir conexões persistentes ativas.

## Verificação

Testes de configuração cobrem padrões, persistência, valores inválidos e preservação das outras preferências. HTTP real cobre autenticação, acessos e reinício. Integração WPF cobre Loaded real com início habilitado/desabilitado, licença inválida, restauração, porta ocupada, tentativa única, teste local e fechamento. Capturas do painel ativo/parado/falha, claro/escuro, largo e pequeno com rolagem em `.impeccable/review/servidor-automatico`.

Capturas usam SQLite descartável e servidor real de ensaio em loopback com chave revogada ao encerrar. As capturas de layout usam interfaces de exemplo declaradas no fixture; as demais consultam as interfaces do PC de ensaio. Sem detector HTML/CSS para WPF, sem imagegen ou novo raster de produto. Firewall, Wi-Fi e leitura do QR por aparelho físico não são homologados por esses testes. RF31/CT26/RN29 e RNF02/RNF07/RNF18.

## Revisão visual e documentação

Extensão compatível com o mundo Comanda vigente, conferida em `JanelaServidor.xaml`, `JanelaServidor.xaml.cs`, `ServidorViewModel.cs` e `MainWindow.Servidor.cs`, com `DESIGN.md` e `PaletaComanda.cs` como referências incumbentes. Preserva superfícies neutras claro/escuro, botões neutros, cantos retos e cores semânticas acompanhadas de estado escrito. Segoe UI: títulos de 22 DIP, estado de 15, seções de 14 e corpo de 12; hierarquia local, sem criar escala global. Grupos têm borda simples e preenchimento de 12 DIP; informações à esquerda e conexão/QR/acessos à direita, empilhados abaixo de 960 DIP úteis, com rolagem e ações no rodapé. Início automático padrão e preferência persistida pertencem ao comportamento operacional da tela.

Evidência amostrada pela revisão independente: 12 PNGs em `.impeccable/review/servidor-automatico`, cobrindo ativo/parado/falha nos dois temas e informações em 1160/800, inclusive fim da rolagem; `verificacao.jpg` fica fora desse conjunto. Precisão sobre os dados de ensaio: as capturas `servidor-info-*` usam rede sintética declarada e servidor real em loopback; as demais consultam interfaces do PC de ensaio. A chave do QR foi revogada após o teste. Parecer `ship`, sem achado material nesse escopo; persistence, fidelity, ceiling, material_fixes e keep foram preenchidos. Não equivale à homologação de aparelho físico, firewall/Wi-Fi, outras escalas de DPI ou tecnologia assistiva completa. Detector HTML/CSS não se aplica ao WPF; sem novo mundo, comp ou raster de produto.

Testes afetados aprovados: oito Desktop e 13 HTTP. A contagem final está na seção seguinte. Esta etapa apenas documenta a compatibilidade e as evidências recebidas, sem novos testes, capturas ou revisão.

`DESIGN.md`, `PRODUCT.md` e `.impeccable/design.json` não foram alterados. Formato legado e afirmações históricas conflitantes em `DESIGN.md` (incluindo ações laranja anteriores aos botões neutros atuais), além das ausências preexistentes de `PRODUCT.md` e do sidecar, permanecem relatados sem reparação lateral ou canonização. A fonte aplicada da paleta continua `PaletaComanda.cs`; as medidas locais deste painel não se tornam regras globais.

## Resultado final

**847 testes aprovados** na execução completa da solução: domínio 4, aplicação 139, infraestrutura 310, servidor 13 e desktop 381. Evidências em outputs/servidor-automatico-regressao.log e backend/artifacts/servidor-automatico-resultados. Dois avisos xUnit2031 preexistentes permanecem. O diagnóstico de espera no encerramento foi feito em processo descartável de testes; dumps temporários não integram o produto. Regressão final inclui o fechamento real com servidor ativo e isolamento dos avisos operacionais somente na fixture.

Publicação local autocontida Release win-x64 concluída em artifacts/servidor-automatico/publish, com log outputs/servidor-automatico-publicacao.log. Sem migração nova, alteração de dados operacionais, serviço do Windows, mudança automática de firewall ou nova publicação dos instaladores da release 1.4.0.
