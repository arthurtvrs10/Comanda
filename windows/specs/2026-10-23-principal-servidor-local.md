# Principal e primeiro servidor local

Data: 23/10/2026. O usuário pediu mover Produtos, históricos e Painel de senhas para Principal, criar COZINHA (KDS), remover Produtos/Marketing/Financeiro/Relatórios/Apps e iniciar servidor semelhante ao Food Flow POS instalado no computador. Ao esclarecer a prioridade, escolheu garçom lançando pedidos pelo celular.

## Entrega e limites do escopo

Quatro categorias: Principal, Cozinha (KDS), Configurações e Ajuda. Principal reúne Mesas / comandas, Produtos, Histórico de pedidos, Histórico de vendas e Painel de senhas. Guardas de permissão, pesquisa global, janelas internas e modo compacto existentes permanecem; Produtos continua usando a guarda administrativa independentemente da categoria. Cozinha (KDS) contém a consulta local existente, sem estados novos ou KDS remoto nesta etapa.

Servidor opt-in em Configurações → Servidor / acesso pelo celular, porta padrão 3001 e porta/endereço selecionáveis. QR Code e cópia de link permitem acesso pelo navegador na mesma rede. Fechar a janela de configuração mantém o servidor ligado; parar ou fechar o aplicativo encerra o acesso. Nenhum firewall, túnel ou inicialização automática é configurado.

Food Flow foi lido somente como referência: README, server/index.js e componentes de acesso/KDS em C:/Users/arthu/foodflow-pos. O banco desse projeto não foi lido ou alterado. A implementação preserva .NET/WPF/SQLite e usa ASP.NET Core/Kestrel no mesmo processo; a decisão foi registrada na ADR 0006 a partir da autorização explícita do responsável.

## Contrato do acesso remoto

GET / e arquivos estáticos expõem somente a interface. /api/catalogo e /api/comandas exigem Bearer em cabeçalho; não aceitar chave em query. GET /api/comandas/{id} mostra comanda aberta e itens sem expor pagamentos. POST /api/comandas abre número permitido; POST /api/comandas/{id}/itens inclui produto, quantidade, observação e nome informado pelo atendente. Sem endpoints de administração, cobrança, fechamento, cancelamento, remoção ou base de dados.

Chave aleatória de 32 bytes, por sessão, no fragmento do link. Sem logs HTTP que gravem chave ou atendimento, sem cache de respostas, origens externas recusadas e corpo limitado a 16 KiB. Aceitar somente origem de conexão IPv4 privada/local. HTTP não cifra o tráfego: usar rede confiável. O QR autoriza esse conjunto de operações e não autentica individualmente um colaborador; o nome informado é identificação operacional.

Reutilizar AbrirComanda/AdicionarItem e repositórios com contexto SQLite por operação. Validação UN/KG/L, snapshots, estados e transações existentes preservados. Escritas remotas serializadas; Idempotency-Key com mesmo corpo retorna a confirmação anterior durante a sessão. Corpo diferente na mesma identificação é conflito. Guardar até 20.000 confirmações, interrompendo novas escritas ao atingir o limite sem expulsar antigas. Não prometer idempotência após reinício: o novo link troca a chave e exige conferir a comanda antes de novo lançamento.

O cliente mantém uma operação pendente em sessionStorage enquanto a resposta é inconclusiva e permite reenviar com a mesma identificação. Uma confirmação de escrita não é desfeita por falha posterior ao atualizar a tela. Nomes/observações/produtos são inseridos como texto no DOM. Fonte e detector estático foram conferidos, mas o JavaScript não foi ensaiado visualmente em navegador neste ambiente.

Licença válida é verificada; o arquivo protegido tem acesso serializado entre chamadas remotas e timer local. Restaurar dados/fotos exige servidor parado/não iniciando; início recusa backup ou recuperação em andamento. Eventos de commit atualizam grade e comanda selecionada no Dispatcher, preservando rascunhos e seleção. Testes não usam credenciais ou dados operacionais reais.

## Interface e evidências

WPF preserva Operate, marca Comanda, Segoe UI, neutros e cantos retos. A janela do servidor tem 660 × 600 DIP, mínimo 520 × 460, corpo rolável, porta, endereço, área branca de QR 180 DIP e ações explícitas. QR é gerado em memória com QRCoder já existente; não há novo raster de produto. A web usa CSS responsivo e tema do dispositivo, logotipo SVG existente, identificação do atendente, comandas, busca/categoria, personalização de quantidade/observação e confirmação de pendências.

Capturas controladas WPF em .impeccable/review/servidor-principal/: menu-principal-{claro,escuro}-{1280,800}.png, menu-kds-{claro,escuro}-1280.png e servidor-{ativo,parado}-{claro,escuro}.png. São 10 arquivos inspecionados; outros arquivos da pasta não ampliam essa evidência. O servidor das capturas está limitado a loopback e usa dados fictícios. A primeira preparação de tema não sincronizava o toggle/fundo; a fixture foi corrigida e as capturas regeneradas.

Regressão Debug final aprovada: 4 Domain + 134 Application + 303 Infrastructure + 356 Desktop + 5 Servidor = 802 testes distintos. Log outputs/servidor-regressao-final.log. Uma primeira execução encontrou referência à categoria antiga num teste de Ajuda; o teste foi atualizado. A correção final dos endereços em modo loopback passou novamente nos 5 testes HTTP e 3 testes de janela/atualização, sem ampliar o total distinto. Logs outputs/servidor-testes-final.log e outputs/servidor-capturas-confirmacao.log. A suíte de guardas licença/backup/interface passou 17/17. Há dois avisos xUnit2031 preexistentes no projeto Desktop.Tests.

Build Debug passou com zero avisos/erros do aplicativo. Publicação Release autocontida win-x64 passou, em artifacts/servidor-inicial/publish, incluindo Microsoft.AspNetCore.App 10.0.12 e Kestrel; não gera novo instalador nem publica a release. Log outputs/servidor-publicacao.log. node --check passou no app.js; detector Impeccable estático do diretório Web retornou lista vazia em outputs/servidor-detect.json. Detector HTML não se aplica ao WPF. Nenhum ImageGen foi usado.

## Validação pendente

O navegador integrado retornou Local browser use is disabled in Work settings. A prévia de dados fictícios ficou restrita a loopback e foi encerrada; não houve teste visual/browser end-to-end nem teste com celular físico. Wi-Fi/firewall, acesso simultâneo entre desktop e vários aparelhos, leitura por tecnologia assistiva, navegador antigo, perda/reinício real e carga prolongada permanecem para homologação. Não declarar equivalência completa ao Food Flow ou P3 concluída.

RF31/UC12/CT26/RN29, RN01/RN03–RN08/RN10/RN19/RN21/RN23 e RNF01/RNF07/RNF18. Formato legado de DESIGN.md e ausências preexistentes de PRODUCT.md/design.json preservados.

Referência técnica consultada: [Kestrel no ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0).

## Encerramento da revisão de interface

A revisão independente retornou `ship`, sem achados materiais no escopo inspecionado: as 10 capturas nativas descritas acima e as fontes/contratos correspondentes. A disposição cobre a reorganização de Principal, a categoria Cozinha (KDS) e os estados parado/ativo da janela do servidor. Não amplia a evidência para outros arquivos existentes na pasta de capturas nem atesta visual ou interação da interface web móvel.

As imagens são renderizações do corpo real WPF por RenderTargetBitmap em fixture isolada, com dados fictícios e servidor em loopback; o link mostrado foi revogado ao encerrar a sessão. A preparação sincroniza a alternância de tema antes de renderizar. A inspeção da fonte web, a verificação de sintaxe JavaScript e o detector estático não substituem ensaio em navegador. O bloqueio Local browser use is disabled in Work settings foi respeitado, sem contorno; permanecem os limites de homologação já registrados.

O registro em DESIGN.md é uma extensão do sistema aprovado e preserva todas as seções anteriores. O resultado desta etapa é implementação local revisada dentro desses limites; não representa liberação operacional em rede, instalador publicado, equivalência completa ao Food Flow ou encerramento da P3.
