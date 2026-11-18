# Fundo padrão VARTHEX

Data: 22/10/2026. Pedido: colocar uma imagem de fundo padrão com cor neutra e o nome VARTHEX ao centro. Extensão da área de trabalho WPF existente, preservando o sistema visual inspirado no Consumer, modo Operate, Ribbon, Segoe UI, temas e janelas internas.

## Contrato e apresentação

O canvas padrão usa `#F2F2F2` e o nome VARTHEX usa `#858585`. O nome ocupa o centro da área de trabalho disponível abaixo da Ribbon e acima do rodapé, calculado pelos limites pintados da geometria das letras, sem deslocamento causado pelas métricas de linha da fonte. Janelas internas e controles continuam sobre esse fundo e podem cobri-lo.

`FundoSistema.ImagemPadrao` é uma imagem vetorial nativa `DrawingImage`, congelada, com quadro transparente de 1600 × 900 unidades. A inscrição é geometria de Segoe UI SemiBold, tamanho 100, criada com `FormattedText.BuildGeometry`. Uma translação coloca o centro dos limites da geometria em (800, 450). O retângulo transparente conserva o quadro; a cor de fundo pertence ao canvas separado. `Stretch.Uniform` e alinhamento central nos dois eixos preservam a proporção e impedem corte, mesmo em áreas estreitas. O tamanho nominal da geometria não é um tamanho fixo em tela.

O canvas padrão continua cinza-claro em ambos os temas. A alternância de tema altera os controles existentes, sem reinterpretar a preferência de fundo. Esta inscrição no canvas não é um redesenho do logotipo ou uma regra tipográfica para outras superfícies. Não foi criado PNG nem usado ImageGen: texto simples e escalável na interface foi implementado com geometria WPF.

## Preferências e comportamento final

- Novas preferências usam `PreferenciasSistema.CorFundoPadrao`, definido como `#F2F2F2`, sem imagem personalizada e com posição Centralizar.
- O valor legado `#F3F5F2`, sem imagem, é apresentado como o novo padrão por `FundoSistema.Cor`. Abrir Imagem de fundo normaliza a cópia da preferência no formulário; o fluxo existente de salvar persiste essa cópia. Carregar a área principal não regrava o registro por si só.
- Uma imagem personalizada válida tem precedência sobre a inscrição padrão. Com imagem, o valor legado de cor é mantido. Outras cores personalizadas também são preservadas; sem imagem, a inscrição padrão aparece sobre a cor escolhida.
- As posições personalizadas permanecem: Centralizar usa `Stretch.None`, Preencher usa `Stretch.Fill`, Zoom usa `Stretch.UniformToFill`, e Repetir usa `TileMode.Tile` com viewport absoluto nas dimensões da imagem. O padrão vetorial sempre usa escala uniforme, independentemente da posição persistida.
- Restaurar padrão limpa a imagem personalizada, restaura `CorFundoPadrao` e seleciona Centralizar. A prévia da seção usa o mesmo pincel da área de trabalho.
- Se leitura, cor ou imagem falhar ao aplicar o fundo principal, o tratamento existente retorna à nova cor e ao pincel padrão. O seletor continua aceitando PNG/JPG/JPEG/BMP de até 3 MB e até 8000 pixels por dimensão.

## Fontes conferidas

- `backend/src/VarthexComanda.Desktop/Configuracao/FundoSistema.cs`: geometria, centralização, compatibilidade de cor e precedência/posicionamento da imagem.
- `backend/src/VarthexComanda.Desktop/MainWindow.ConfiguracoesSistema.cs`: leitura, aplicação e fallback.
- `backend/src/VarthexComanda.Desktop/MainWindow.xaml`: camadas `FundoTrabalho`, `ImagemTrabalho` e `ConteudoPrincipal`, nessa ordem.
- `backend/src/VarthexComanda.Desktop/Configuracao/JanelaConfiguracoesSistema.cs`, método `Fundo`: normalização, paleta, prévia, seleção de imagem e restauração.
- `backend/src/VarthexComanda.Application/Configuracao/PreferenciasSistema.cs`: valor inicial e persistência das preferências.
- `backend/tests/VarthexComanda.Desktop.Tests/Xaml/FundoSistemaTests.cs`: precedência, repetição, cor legada com imagem, retorno ao padrão e preservação de outra cor.

## Evidências e limites

`backend/artifacts/fundo-varthex-resultados/fundo-varthex.trx` registra 3/3 testes Desktop aprovados: `ImagemPersonalizadaTemPrecedenciaEPodeRetornarAoPadrao`, `PinBloqueiaMenusEDesbloqueiaPorToqueESaidaBloqueiaNovamente` e `RibbonPreservaConteudoFiltrosPermissoesETemaEntreSessoes`. `fundo-capturas.trx`, na mesma pasta, registra outra execução aprovada de 1/1 do teste de fundo para gerar as capturas. A execução principal também confirmou 3/3 testes `PreferenciasSistemaTests` aprovados, cobrindo o fluxo de preferências. São seis testes distintos; a captura repete um deles. A documentação conferiu os dois TRX Desktop e o código amostrado; não executou novamente aplicativo, testes ou launcher.

Cinco capturas atuais na pasta `.impeccable/review/fundo-varthex/`: `fundo-padrao-claro.png`, `fundo-padrao-escuro.png` (ambas 1280 × 720), `principal-sem-selecao-800.png`, `principal-sem-selecao-1200.png` e `area-trabalho-organizada-escuro.png`. A amostra documental abriu `fundo-padrao-escuro.png` e `principal-sem-selecao-800.png`: o nome está centralizado no canvas neutro, com adaptação proporcional em largura menor. A captura organizada integra a evidência de janelas acima do fundo, cobrindo o centro. As demais capturas da pasta são herdadas e não ampliam a validação desta extensão.

A revisão independente final retornou `ship`, sem correção material, baseada nessas cinco capturas e no código amostrado. A suíte global não foi reexecutada nesta extensão. O código está no commit `00ba62d`. A execução principal abriu diretamente `backend/artifacts/fundo-varthex/bin/VarthexComanda.Desktop/debug/VarthexComanda.exe` e confirmou na janela real, sem autenticar, o canvas `#F2F2F2` com VARTHEX centralizado. Essa abertura refere-se à compilação de desenvolvimento; o instalador e a versão instalada não foram atualizados ou validados.

As decisões anteriores são herdadas, sem aprovação global da interface. O formato legado de `DESIGN.md`, a ausência prévia de `PRODUCT.md` e `.impeccable/design.json` e demais desvios documentais não foram reparados nem promovidos a regras novas. A escrita documental se limita a esta especificação e ao acréscimo correspondente em `DESIGN.md`.
