# Cabeçalhos dos modais — 21/10/2026

Modo: Operate. Extensão restrita de apresentação das janelas auxiliares WPF. O usuário pediu o cabeçalho da referência fornecida: título à esquerda, barra cinza-escura e X à direita, sem ícone ou nome Varthex no cabeçalho. A referência é `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-010db6c5-75e0-4089-b23e-4632934fb2d2.png`.

## Aparência compartilhada

O cabeçalho tem 30 DIP de altura, fundo `#484644` e título branco em Segoe UI de 12 DIP, seminegrito. A margem horizontal do título é de 10 DIP; o texto acompanha a propriedade `Title` da janela e trunca com reticências quando falta espaço. A moldura externa tem traço cinza `#A9A9A9` de 1 DIP e cantos retos. Essa barra mantém a mesma apresentação nos temas claro e escuro; o conteúdo conserva seu tema.

O botão Fechar fica na extremidade direita e mede 42 × 30 DIP. O X é um vetor branco; hover vermelho `#C42B1C`, pressionamento `#9F2318` e foco visível pelo recurso compartilhado `FocoVisivel`. O botão Maximizar/restaurar usa a mesma área, à esquerda de Fechar, com hover neutro `#666462` e pressionamento `#777572`. Seu símbolo, tooltip e nome acessível acompanham o estado maximizado/restaurado.

Não há ícone de aplicativo na barra. O título genérico `Varthex Comanda` é substituído por `Aviso`; `Ativação — Varthex Comanda` passa a `Ativação`. Os títulos específicos das tarefas permanecem. A marca presente dentro do conteúdo da ajuda não é removida por essa mudança.

## Aplicação e comportamento

`AparenciaJanelaModal` instala um `Window.Template` compartilhado e protege contra aplicação repetida na mesma janela. O `WindowChrome` usa área de título de 30 DIP, borda de redimensionamento de 5 DIP, espessura de vidro zero, cantos de raio zero e botões próprios. O título continua funcionando como área nativa de movimentação; os botões aceitam interação nessa área.

As janelas configuradas por `DialogoCompacto` recebem a aparência antes da primeira exibição. As demais janelas auxiliares recebem o padrão pelo tratamento global do evento `Loaded` em `App.xaml.cs`, somente quando a origem é a própria janela. A janela principal real (`MainWindow`) e a tela pública de senhas (`JanelaExibicaoSenhas`) são excluídas.

O botão Maximizar/restaurar só aparece para `CanResize` e `CanResizeWithGrip`. Seu comando alterna os estados normal e maximizado, preservando as regras de limites da janela proprietária existentes. Janelas sem redimensionamento exibem apenas Fechar. O X chama `Window.Close`, mantendo o evento `Closing` e suas possibilidades de cancelar o fechamento, como confirmação de alterações ou operação em andamento.

O apresentador mantém o conteúdo, seus templates e formatação. O contexto de dados e os manipuladores de eventos da janela permanecem; essa extensão não refaz os formulários. A aparência da janela principal e a apresentação pública de senhas mantêm seus comportamentos próprios.

## Evidência e limites

A suíte desktop passou com 293 testes: os 291 existentes e dois novos em `AparenciaJanelaModalTests`. A verificação do cabeçalho cobre altura, ausência de imagem na barra, preservação do conteúdo, atualização dinâmica do título, visibilidade do botão de maximizar e respeito ao cancelamento de fechamento. Os testes existentes de limites de maximização de janelas nativas filhas também passaram.

Quatro capturas do template completo da janela, incluindo a barra de título, foram abertas e verificadas nos temas claro e escuro. Em `.impeccable/review/modais-padrao/`:

- `modal-senha-claro.png` e `modal-senha-escuro.png`, 350 × 311 pixels;
- `modal-redimensionavel-claro.png` e `modal-redimensionavel-escuro.png`, 900 × 570 pixels.

As capturas representam uma janela compacta de senha e uma janela redimensionável de diagnóstico. A evidência é nativa de Windows, sem detector de layout web. A revisão independente retornou `ship` para o padrão dos cabeçalhos, sem correções materiais; o veredito é restrito a essa alteração e não representa aprovação global da interface. Movimentação, redimensionamento e Alt+F4 foram avaliados pela implementação e pelos testes existentes, não demonstrados por capturas estáticas.

Fontes: `backend/src/VarthexComanda.Desktop/AparenciaJanelaModal.cs`, `backend/src/VarthexComanda.Desktop/App.xaml.cs`, `backend/src/VarthexComanda.Desktop/Configuracao/DialogoCompacto.cs` e `backend/tests/VarthexComanda.Desktop.Tests/Xaml/AparenciaJanelaModalTests.cs`. O formato legado e as demais seções de `DESIGN.md` foram preservados. Esta passagem não cria `PRODUCT.md`, não gera `.impeccable/design.json` e não corrige documentação fora do escopo de cabeçalhos.
