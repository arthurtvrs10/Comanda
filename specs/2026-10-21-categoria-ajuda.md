# Categoria Ajuda — 21/10/2026

Categoria após Configurações, com um único comando visível: Ajuda, no grupo Ajuda e suporte. A mesma regra vale na faixa expandida e no menu compacto. Contate o suporte foi removido dos destinos. Feedback, Show training e Diagnóstico de serviços permanecem na pesquisa global, sem aparecer na Ribbon; as permissões existentes continuam valendo, inclusive acesso de gestor para diagnóstico.

## Opções de ajuda — composição aprovada

Ajuda abre o modal de suporte, também acessível pelo atalho flutuante no canto inferior direito. O atalho tem 54 × 54 e nome acessível “Abrir opções de ajuda”; o clique respeita o bloqueio e a recuperação do sistema. Feedback e tutorial continuam acessíveis pelas seções do próprio modal.

O modal apresenta a marca Varthex Comanda, o título “Opções de ajuda” e cinco seções, cada uma com ícone, título e um botão:

| Seção | Ação | Ícone vetorial |
| --- | --- | --- |
| WhatsApp | Abrir WhatsApp | Balão e telefone verdes |
| Central de ajuda | Em preparação, desabilitado | Livro azul com interrogação laranja |
| Suporte remoto | Baixar RustDesk | Monitor com setas azuis e laranja |
| Ideias e sugestões | Enviar feedback | Balão azul |
| Treinamento | Iniciar tutorial guiado | Livro azul com reprodução laranja |

A assinatura “© 2026 VARTHEX” encerra o conteúdo. O rodapé fixo contém “Fechar e ocultar botão”, à esquerda, com cantos retos, fundo transparente e hover cinza. Essa ação fecha o modal e oculta o atalho flutuante durante a instância atual do aplicativo; não grava preferência. A categoria Ajuda da Ribbon continua permitindo abrir o modal pelo comando Ajuda.

### Marca e temas

Usar os arquivos existentes da marca, sem redesenhá-los. `brand/comanda-organizada/out/assets/01-logo/black/logo-horizontal-black-2x.png` foi copiado para `backend/src/VarthexComanda.Desktop/Assets/LogoAjudaClara.png`; a versão `white/logo-horizontal-white-2x.png` para `Assets/LogoAjudaEscura.png`. Os pares têm hashes SHA-256 idênticos e são recursos incorporados. `AjudaLogo` seleciona dinamicamente a versão apropriada; a imagem ocupa 190 × 44, com proporção preservada e nome acessível “Varthex Comanda”.

Os recursos `Ajuda*` definem uma superfície local neutra, preservando o verde das ações e as cores dos ícones. A paleta desta janela não substitui os temas das demais telas:

| Recurso | Claro | Escuro |
| --- | --- | --- |
| AjudaCanvas | #F1F1F1 | #252525 |
| AjudaSurface | #FAFAFA | #303030 |
| AjudaInk | #292929 | #F0F0F0 |
| AjudaMuted | #616161 | #BDBDBD |
| AjudaLine | #D1D1D1 | #505050 |
| AjudaSoft | #E7E7E7 | #383838 |
| AjudaHover | #D4D4D4 | #4B4B4B |
| AjudaAction | #245640 | #98D6B4 |
| AjudaActionHover | #1B4433 | #B0E3C7 |
| AjudaActionInk | #FFFFFF | #173725 |

Segoe UI 12 no corpo e ações, 13 semibold nas seções e 18 semibold no título. Seções com borda de 1, preenchimento 10 × 6 e intervalo de 6. Botões verdes com altura mínima de 30; ação indisponível usa os neutros `AjudaSoft`, `AjudaLine` e `AjudaMuted`. O estilo local de botão referencia explicitamente `FocoVisivel`, inclusive na ação do rodapé, para preservar o contorno de foco por teclado.

### Tamanho e estados

Modal de tamanho padrão 360 × 640 unidades, colado ao ícone de ajuda: bordas direitas alinhadas e intervalo de 4 acima do botão. Recebe o próprio botão como âncora, incluindo abertura pela Ribbon. Se não houver espaço suficiente acima e houver mais espaço abaixo, abre abaixo; reduz quando necessário para caber na parte visível da janela e na área útil do monitor. Não cresce para preencher a altura do sistema. A posição acompanha movimento, tamanho e estado da janela principal, além do layout do ícone; considera a escala de DPI do Windows. Não pode ser arrastado ou redimensionado separadamente. O corpo tem rolagem vertical automática e não tem rolagem horizontal. O rodapé permanece disponível na janela compacta. Os eventos do proprietário e da âncora são removidos ao fechar. Teste com janelas reais confere alinhamento ao ícone, intervalo, limites do tamanho padrão e redução em janela menor. Capturas: artifacts/ajuda-icone/suporte-modal-icone-False.png e suporte-modal-icone-True.png.

Falhas ao abrir os canais apresentam uma mensagem no próprio modal, com anúncio acessível cortês. A falha do WhatsApp inclui o telefone do suporte; não fecha a janela. Feedback e treinamento abrem somente após clique e recebem o modal como proprietário.

## Canais definidos pelo usuário

WhatsApp (61)98553-2004: https://wa.me/5561985532004 . O sistema abre uma conversa com texto preparado; o operador revisa e confirma o envio no WhatsApp.

Central de ajuda: endereço ainda não disponível. A ação indica Em preparação.

RustDesk: botão abre https://rustdesk.com/ no navegador, sem instalar ou executar software automaticamente. Endereço verificado na página oficial e na documentação de clientes https://rustdesk.com/docs/en/client/ em21/10/2026.

Feedback: tipo (ideia, sugestão ou problema), mensagem de10–2000 caracteres e ações de copiar ou preparar envio pelo WhatsApp. Nenhuma mensagem é enviada automaticamente.

## Tutorial guiado

A janela principal de ajuda é não bloqueante: usa `Show()` e mantém as demais funções acessíveis. Existe uma instância por janela principal; novos cliques apenas a ativam. Após fechar, é possível reabrir. Feedback e tutorial continuam com seus próprios fluxos. Teste de integração confere a Ribbon utilizável com ajuda aberta, reutilização da instância e reabertura após fechar.

Com a ajuda aberta, o botão flutuante mostra um X vermelho (`#D64545`) e o texto acessível/tooltip “Fechar ajuda”. Clicar nele fecha a janela. Ao fechar por qualquer caminho, o ícone original e o texto “Abrir opções de ajuda” retornam. Os comandos da Ribbon continuam reutilizando a janela aberta. Capturas dos estados em artifacts/ajuda-toggle/ajuda-botao-x.png e ajuda-botao-original.png.

Sete etapas dentro do sistema, com Anterior, Próximo, Concluir e Fechar. A categoria da Ribbon acompanha cada etapa; ao fechar, a categoria anterior é restaurada. O tutorial não cadastra produtos, não abre comandas e não altera dados.

O mesmo fluxo é usado pelo comando Show training e pela seção Treinamento do modal. A sequência de categorias é Principal, Principal, Produtos, Apps, Apps, Configurações e Ajuda; a integração passa o callback de seleção da Ribbon e restaura a categoria anterior em `finally`.

## Diagnóstico funcional

Verifica SQLite em modo somente leitura (conexão, quick_check e tabelas principais), leitura/gravação das pastas de dados, backups, fotos e logs com arquivo temporário removido ao concluir, licença local e acesso às filas de impressão do Windows. Operação local não exige serviço online. Resultados possíveis: Funcionando, Atenção, Falha e Não utilizado.

Execução em segundo plano, com atualização de resultados, data/hora e cópia do relatório. A falha em uma verificação não impede as demais. A enumeração de filas não confirma uma impressão física; a orientação remete a Testar impressora. Não há testes de envio real de mensagens ou de assistência remota.

Testes: links somente após clique, validação e composição do feedback, etapas do tutorial, estados de falha, banco intacto após diagnóstico e remoção do arquivo temporário.

## Evidência da revisão visual

A revisão aprovou a entrega após explicitar o foco dos botões locais. A suíte desktop passou com 278 testes antes desse ajuste de uma linha; os três testes direcionados de Ajuda passaram depois, incluindo a referência a `FocoVisivel` nos botões de canal e do rodapé. Não se atribui ao teste direcionado uma nova execução da suíte completa.

Capturas em `artifacts/ajuda-aplicada/`: `suporte-False.png` e `suporte-True.png` (claro/escuro, conteúdo 470 × 810); `suporte-compacto-False.png` e `suporte-compacto-True.png` (claro/escuro, conteúdo 380 × 450). A verificação cobre marca carregada nos dois temas, fundos e textos neutros, ação indisponível, callbacks de feedback/tutorial, erro de abertura do WhatsApp e ocultação do atalho.

Fontes da implementação: `Ajuda/JanelaSuporte.xaml`, `Ajuda/JanelaSuporte.xaml.cs`, `Ajuda/JanelasAjuda.cs`, `MainWindow.Ajuda.cs`, `MainWindow.xaml`, `MainWindow.Ribbon.cs`, `IconesVarthex.cs` e `TemaInterface.cs`, sob `backend/src/VarthexComanda.Desktop/`. A identidade global e `.impeccable/design.json` permanecem preservados.

### Consolidação da categoria — 21/10/2026

`DestinoRibbon.ExibirNaRibbon` separa a presença visual da descoberta pela pesquisa. A faixa expandida e seu menu compacto filtram por esse sinalizador; a pesquisa continua consultando todos os destinos. A revisão mantém apenas Ajuda visível e conserva Feedback, Show training e Diagnóstico de serviços na busca. Não há destino Contate o suporte. O conteúdo e os cinco canais da janela de suporte permanecem os mesmos.

A suíte desktop completa passou com 282 testes após a implementação desta revisão, incluindo as janelas internas, limites de maximização e hover descritos em `specs/2026-10-21-janelas-hover.md`. Esta evidência complementa os registros anteriores; a atualização documental não executou uma nova rodada de testes.
