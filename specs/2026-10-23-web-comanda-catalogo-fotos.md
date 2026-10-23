# Comanda e catálogo web com fotos

Data: 23/10/2026. Refinamento autorizado pelo usuário: no celular, abrir uma comanda deve seguir a ideia do desktop, exibindo seus itens e total; + Produtos abre outra tela com fotos dos produtos. RF31 → UC12 → CT26, RN29 e as regras de quantidade, snapshots e fechamento existentes. Extensão da ADR 0006; sem nova stack, migração ou permissão remota.

## Direção e contrato

Operate, extensão do mundo Comanda. Preservar marca SVG existente, Segoe UI/system-ui, cantos retos, superfícies neutras e cores de ação já aprovadas; tema escuro acompanha o dispositivo. Referência visual incumbente: LocalizarProdutoView.xaml e .impeccable/review/catalogo-modelo/localizador-False-740-fotos.png, conferida antes da edição; não é captura da nova web. Não há comp aprovado nem mudança de identidade.

Três telas: lista de comandas → conferência da comanda → localizador de produtos. A conferência mostra número, mesa/cliente quando registrados, situação, todos os itens/quantidades/observações/subtotais e total persistido. O estado vazio orienta + Produtos. O localizador tem busca por nome/código, categorias e fotos por padrão; categorias laterais em telas amplas e faixa rolável até 600 px. Grade de duas colunas no celular, com foto, código, nome e preço/unidade; cartão inteiro abre personalização. Não incluir ações remotas de pagamento, fechamento, exclusão ou edição do cadastro.

Personalização usa dialog nativo com foco protegido, produto/preço, quantidade UN/KG/L, controles de incremento, observação e prévia do total. Confirmação no servidor fecha o diálogo e retorna à conferência. Sincronização a cada quatro segundos atualiza os itens sem trocar o localizador, filtros ou rascunho em edição. Fechamento recebido do desktop interrompe a inclusão, fecha personalização e retorna à conferência com estado amarelo. Voltar interno e histórico do navegador preservam a comanda. Falhas ambíguas mantêm a operação para reenvio com a mesma identificação. Sucesso seguido de falha na consulta não é apresentado como falha de gravação.

Alvos de toque de pelo menos 46 px, SVGs geométricos consistentes, foco visível, sem conteúdo inserido como HTML, títulos focáveis ao navegar, estados de vazio/erro/indisponível. Fotos têm área com proporção estável, carregamento sob demanda e substituto neutro quando ausentes ou ilegíveis. Não usar fotos inventadas no catálogo operacional.

## Entrega das imagens

O catálogo informa somente temFoto. GET /api/produtos/{id}/foto exige Bearer da sessão, licença e origem local como as demais APIs. O servidor resolve a imagem a partir do produto ativo e categoria ativa, usando a mesma pasta de fotos do desktop; não aceita caminho enviado pelo cliente. Recusa nomes com separadores/streams, formatos fora de JPG/PNG/BMP, links de arquivo, arquivo ausente, cabeçalho incompatível ou maior que 10 MiB. Não expõe nome local, diretório ou imagens por rota estática.

No cliente, fetch autenticado recebe o binário e cria URL blob local, permitida pela CSP. A chave não é colocada na URL da imagem. Cache somente em memória da página; atualizar catálogo e sair liberam os blobs. BFCache recria imagens ao voltar. Não há cache HTTP, nova imagem de marca, ImageGen ou imagem copiada para o repositório.

## Qualidade, evidências e limites

Barra de qualidade: fluxo operacional completo e consistente com o desktop, sem regressão na confirmação, sem exposição de arquivos e sem alegar homologação visual sem renderização real. O contexto Impeccable já havia sido carregado nesta sessão; DESIGN.md e orientações de refinamento/Operate/craft-floor foram lidas, preservando drift preexistente sem reparação lateral.

12 testes HTTP/SQLite aprovados: os cinco anteriores e sete casos de foto, incluindo autenticação, bytes, metadados sem caminho, ausência, nomes impróprios, formato, tamanho e inativação. Log outputs/web-comanda-http.log, TRX backend/artifacts/web-comanda-resultados/web-comanda-http.trx.

Sete testes Node executam o app.js real com DOM mínimo em memória: abertura/conferência, catálogo e fetch de foto, personalização KG, sincronização/rascunho/fechamento, reenvio idempotente, sucesso seguido de falha na consulta, filtro/foto inválida/histórico e nova comanda/atendente. Log outputs/web-comanda-fluxo.log. Não equivalem a testes em navegador.

Três testes WPF afetados (ServidorJanelaTests) aprovados; log outputs/web-comanda-desktop.log. node --check passou. Uma execução do detector estático da web retornou [] em outputs/web-comanda-detect.json; não comprova renderização. Publicação Release win-x64 autocontida concluída em artifacts/web-comanda/publish, log outputs/web-comanda-publicacao.log. Nenhum instalador ou release externa atualizado; aplicativo aberto anteriormente não foi encerrado para trocar seus binários.

O navegador autorizado retornou Local browser use is disabled in Work settings. Nenhum navegador alternativo, captura simulada ou contorno foi usado. Não há capturas da web para esta extensão: composição móvel/desktop, teclados, foco real, leitura assistiva e fotos renderizadas precisam de navegador habilitado/celular real. Rede Wi-Fi e firewall permanecem pendentes conforme o contrato inicial. As capturas WPF anteriores não atestam esta web. Revisão final de fonte/contrato deve declarar esse limite.

## Parecer final e registro do sistema

A revisão independente final retornou disposition `ship`, sem achados materiais, exclusivamente para fonte, contrato e testes acima. Os 12 HTTP/SQLite, sete Node com DOM mínimo e três WPF afetados não constituem homologação visual ou interação em navegador. O detector teve uma execução com resultado `[]`; a publicação autocontida é local, sem novo instalador ou release externa e sem encerrar o aplicativo anterior. Permanecem os limites de navegador, fotos renderizadas, composição, teclado/foco/navegação reais, leitura assistiva e Wi-Fi/firewall.

O registro acrescentado ao DESIGN.md descreve somente características verificáveis nas fontes desta extensão Operate. A referência `localizador-False-740-fotos.png` continua identificada como WPF incumbente, sem representar esta web. Seções e tokens anteriores, formato legado e ausências preexistentes de PRODUCT.md e .impeccable/design.json foram preservados; nenhum novo raster ou sidecar foi criado.
