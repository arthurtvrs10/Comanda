# Movimento das janelas internas

Data: 23/10/2026. O usuário relatou travamento ao mover Mesas/comandas e outras janelas no workspace. Correção do contêiner compartilhado JanelaInterna, preservando identidade, conteúdo e regras de negócio. RNF02/RNF18, preservação de CT17.

## Diagnóstico e correção

Cada DragDelta alterava Margin e chamava AjustarLimites, voltando a medir a área. A fixture WPF com duas janelas e 240 movimentos registrou 240 medições adicionais do workspace, sem redimensionar o conteúdo. O primeiro teste falhou com 242 medições acumuladas em vez das duas iniciais. Isto reproduz o recálculo excessivo; não é captura de travamento da sessão operacional.

A moldura agora reutiliza TranslateTransform para sua posição. O movimento limita coordenadas e altera somente a transformação visual, sem recalcular tamanho ou margem. Maximizar zera a posição visual e restaurar recupera a normal. Redimensionamento mantém dimensões e limites. Valores não finitos são recusados. Captura do mouse e identidade visual permanecem.

O [código oficial do Thumb WPF](https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/Primitives/Thumb.cs) foi consultado para confirmar que DragDelta usa a posição relativa atual do controle. O deslocamento visual é atualizado imediatamente, sem aguardar outro layout.

Testes conferem TranslatePoint, isto é, posição efetiva, em vez do mecanismo interno Margin. Novo teste verifica 50 movimentos sem UpdateLayout intermediário, alvo do clique, dimensões e restauração. Outro mede a área/conteúdo em 240 movimentos, mantendo texto e janela ativa. Testes existentes cobrem limites, oito alças, janelas simultâneas, maximização, fechamento e rolagem da grade.

## Isolamento dos testes

A regressão completa expôs inicialização operacional agendada pelo WPF ao construir o App da fixture, desligando a aplicação de testes. O construtor público continua iniciando normalmente; um construtor interno permite carregar o mesmo App.xaml sem iniciar banco, licença ou sessão operacional nos testes. Não há novo argumento ou preferência acessível ao usuário. Uma tentativa de subclasse em outro assembly era incompatível com BAML e foi removida.

## Evidências

Antes: outputs/workspace-antes.log e workspace-antes.trx — teste falhou conforme esperado, 240 medições adicionais da área, zero do conteúdo, 7,37 ms. Depois: outputs/workspace-depois.log e workspace-depois.trx — sete testes afetados passaram, zero medições adicionais da área/conteúdo, 5,97 ms. Na regressão final o mesmo ensaio registrou 3,36 ms. O dado relevante é eliminar as medições por movimento; tempos únicos da fixture não são benchmark do sistema real.

Regressão desktop final: 364 testes aprovados em outputs/workspace-desktop-final.log; TRX em backend/artifacts/workspace-resultados/workspace-desktop-final.trx, incluindo dois novos testes de movimento. As primeiras regressões falharam em cascata pela inicialização da fixture; a final usa o App real isolado. Dois avisos xUnit2031 preexistentes em AtendimentoTecladoXamlTests. Demais projetos não foram alterados e não se declara nova execução global nesta tarefa.

Versão local autocontida Release win-x64 concluída em artifacts/workspace-corrigido/publish; log outputs/workspace-publicacao.log. Sem nova migração ou alteração de dados, instalador ou release externa. A instância operacional não foi encerrada automaticamente. Testes usam WPF real e eventos de arraste controlados, sem mouse físico: fluidez na sessão do usuário, outras escalas e uso prolongado precisam ser conferidos em operação. Sem mudança visual, novo raster ou detector HTML/CSS para WPF.
