# Janelas de operação e hover — 21/10/2026

Modo: Operate. Extensão do sistema WPF Windows existente, inspirado na organização e interação do Consumer. Preservar marca Varthex, Segoe UI, cantos retos, temas e permissões. O escopo é a área de trabalho com janelas simultâneas, limites de janelas filhas e feedback de hover. Esta revisão substitui as regras anteriores de margem de maximização e teto de tamanho.

## Área de trabalho com janelas simultâneas

`AreaTrabalhoInterna` hospeda várias `JanelaInterna` simultaneamente dentro da área abaixo da Ribbon e acima do rodapé, sobre o fundo configurado. Abrir uma função acrescenta ou ativa sua janela; não substitui as demais por um novo conteúdo único. `MainWindow` mantém um contêiner por tela para Mesas / comandas, Produtos, Histórico de vendas, Cozinha e Backup e restauração no modo de recuperação. Reabrir uma tela já aberta traz a mesma janela para a frente, sem duplicá-la. As permissões e atualizações de dados seguem os fluxos existentes.

A moldura usa `Canvas` e `Line`. O título ativo usa `RibbonHover`, e os títulos em segundo plano usam `RibbonBar`, mantendo texto e ícones brancos. A barra tem altura de 30 DIP e ações de 42 × 30 DIP, com tooltip e nome acessível. Clique ou foco de teclado ativa a janela e atualiza a ordem de sobreposição; o status acessível indica janela ativa ou em segundo plano. O espaço fora das molduras permite alcançar outras janelas visíveis.

A janela normal começa em 1180 × 600 DIP, reduzida ao espaço disponível e com deslocamento inicial entre aberturas. Arrastar o título move a moldura; quatro bordas e quatro cantos permitem redimensionar, com alças transparentes de 6 DIP e cursores de direção. O mínimo físico é 640 × 360 DIP, reduzido quando a própria área de trabalho for menor. Movimento, redimensionamento e mudança de tamanho do sistema mantêm a moldura dentro da área disponível.

Maximizar preenche toda a largura e altura da área de trabalho, sem margens nem tetos de tamanho. As alças ficam ocultas durante a maximização. Restaurar recupera a posição e as dimensões normais, limitadas ao espaço então disponível. Duplo clique no título ou Ctrl+F10 alterna maximizar/restaurar; Ctrl+F4 fecha a janela atual. Ctrl+F6 e Ctrl+Shift+F6 percorrem as janelas nos dois sentidos. O botão de maximização atualiza ícone, tooltip e nome acessível ao restaurar.

Fechar remove somente a janela solicitada; as demais permanecem abertas. Se a janela ativa fechar, a de maior ordem de sobreposição assume a atividade e o foco. Se nenhuma restar, o foco retorna à categoria atual da Ribbon. Reabrir reaproveita a mesma moldura, tela e modelo de dados, preservando estado da sessão. Bloquear ou sair/trocar de perfil remove todas as janelas da área de trabalho pelos fluxos de sessão existentes.

O conteúdo é medido em espaço finito para manter listas virtualizadas. Uma área de visualização com rolagem automática preserva um mínimo lógico de 760 × 540 DIP durante a edição de uma comanda e 720 × 360 DIP nas demais telas. Sem comanda em edição, o mínimo lógico do atendimento é 0 × 0: a grade acompanha a moldura e usa sua própria rolagem. Quando a moldura fica menor que o conteúdo útil do formulário de edição, surgem barras externas de rolagem em vez de cortar campos e ações.

## Grade de comandas

`GradeComandasPanel` divide a largura disponível em colunas iguais, com largura preferida de 144 DIP e mínima de 128 DIP por coluna. Os cartões deixam de ter largura fixa de 136 DIP; ocupam a coluna com margem de 4 DIP, altura de 136 DIP e linha de 144 DIP. Assim, uma linha completa termina com a margem de 4 DIP junto à barra, sem o grande espaço vazio antes reservado à direita.

`RolagemComandas` mantém a barra vertical visível e a horizontal desabilitada, permitindo alcançar os cartões inferiores dentro da grade. `MensagemGrade` fica recolhida quando vazia, sem reservar espaço inferior; mensagens reais continuam visíveis e quebram linha. O mínimo de conteúdo é atualizado por `JanelaInterna.DefinirMinimoConteudo` conforme `ComandaAtual`, preservando a rolagem do formulário durante a edição.

## Janelas nativas filhas

`LimitesJanelaModal` aplica os limites a janelas WPF com proprietário e `ResizeMode.CanResize` ou `CanResizeWithGrip`. O registro global ocorre no carregamento da própria janela, uma vez por instância. O gancho `WM_GETMINMAXINFO` define o tamanho e o posicionamento de maximização e o limite superior de redimensionamento.

Quando o proprietário é `MainWindow`, a área considerada é o retângulo da área de trabalho `ConteudoPrincipal`, convertido para coordenadas de tela e intersectado com a área útil do monitor. A janela maximizada preenche esse retângulo, preservando Ribbon e rodapé, sem margens nem tetos de tamanho. Para outros proprietários, usa-se o retângulo completo do proprietário, também limitado à área útil do monitor. A conversão de coordenadas respeita a escala do Windows. Em áreas menores, o limite mínimo de rastreio é reduzido para não superar o máximo disponível. Restaurar mantém o tamanho normal do Windows. O gancho é removido ao fechar.

A janela principal e a tela pública independente de senhas não têm proprietário e ficam fora dessa regra. Janelas fixas também ficam fora, incluindo a ajuda compacta ancorada, que preserva seu posicionamento junto ao botão. Os limites não alteram campos, ações, permissões ou modalidade das janelas existentes.

## Hover compartilhado

O recurso dinâmico `HoverOverlay` representa feedback neutro sobre o fundo atual, com valores WPF em ordem ARGB:

| Contexto | Valor | Tratamento |
| --- | --- | --- |
| Tema claro | `#16000000` | Preto translúcido sobre a superfície |
| Tema escuro | `#26FFFFFF` | Branco translúcido sobre a superfície |
| Barras sempre escuras | `#26FFFFFF` nos dois temas | Mantém feedback visível sobre o fundo escuro |

O template global de Button mostra a camada ao passar o ponteiro e preserva o fundo, texto e conteúdo originais. Mantém a borda de hover existente, o estado pressionado e o foco explícito por teclado. O ComboBox mostra a mesma camada no botão que abre a lista. O template local `BotaoPainelComanda` usa o recurso, mantém sua borda de hover de 2 DIP e seu estado pressionado. A camada não recebe cliques e fica oculta em controles desabilitados.

Recursos locais usam a camada branca na barra superior de categorias e ações, no rodapé e no título da janela interna, pois essas superfícies são escuras nos dois temas. As abas da Ribbon continuam usando seu template e recursos próprios. Templates específicos, como os botões da ajuda, preservam seus estados locais; a mudança não substitui suas paletas.

## Evidência e limites

A suíte desktop completa passou com 287 testes aprovados após a correção da grade. `GradeComandasLayoutTests` verifica 20 cartões, margem direita de 4 DIP na linha completa e rolagem até o fim nas larguras 462, 800, 1180 e 1920 DIP, nos temas claro e escuro. Em moldura de 640 × 360 DIP, confere rolagem interna disponível sem rolagem externa da grade; também cobre a rolagem preservada na edição e a exibição de mensagens reais. Capturas em `artifacts/grade-comandas/`: `grade-comandas-claro-462.png`, `grade-comandas-escuro-800.png` e `grade-comandas-janela-pequena.png`.

A cobertura da área de trabalho inclui coexistência, ativação, fechamento independente, acesso às janelas por clique e redimensionamento pelas oito alças. `JanelaInternaTests` cobre movimento, contenção, maximização integral, restauração e fechamento nos temas claro/escuro e em áreas de 1920 × 1080, 1440 × 800 e 800 × 440. A integração de `CarregamentoDeXamlTests` verifica reuso da janela de atendimento e preservação da observação do novo item e da busca de Produtos. `LimitesJanelaModalTests` usa janelas nativas reais com proprietário de 1100 × 740 para verificar restauração e limites; também confere que uma filha de `MainWindow` maximiza exatamente na área abaixo da Ribbon e acima do rodapé.

Fontes: `AreaTrabalhoInterna.cs`, `JanelaInterna.cs`, `LimitesJanelaModal.cs`, `App.xaml`, `App.xaml.cs`, `TemaInterface.cs`, `Atendimento/GradeComandasPanel.cs`, `Atendimento/AtendimentoView.xaml`, `MainWindow.xaml`, `MainWindow.xaml.cs` e `MainWindow.Ribbon.cs`, sob `backend/src/VarthexComanda.Desktop/`.

Esta revisão não substitui a identidade visual nem regulariza a documentação histórica. `PRODUCT.md` e `.impeccable/design.json` não existem na raiz atual; não foram criados nesta extensão. `DESIGN.md` conserva seu formato anterior e registra somente as regras aprovadas desta entrega.
