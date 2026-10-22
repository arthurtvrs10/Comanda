# Tela de abertura

Data: 22/10/2026. Pedido: apresentar uma tela de abertura ao iniciar o sistema, com composição semelhante à referência Consumer (marca central e versão no canto), usando a textura abstrata fornecida pelo usuário. Extensão estreita do aplicativo WPF no modo Operate, com a marca atual Comanda, por Varthex. A referência orienta a composição; a marca Consumer não foi incorporada.

## Contrato e apresentação

A janela `JanelaAbertura` é transitória, centralizada, sem moldura, sem redimensionamento e sem entrada própria na barra de tarefas. O quadro nominal mede 760 × 490 DIP. A escala é o menor valor entre 1, a largura útil menos 32 DIP dividida por 760 e a altura útil menos 32 DIP dividida por 490, com limite inferior técnico de 0,2. O `Viewbox` preserva a proporção de todos os elementos; o mínimo técnico não representa garantia de acomodação em áreas menores que o quadro reduzido. A janela é excluída do cabeçalho global de `AparenciaJanelaModal`.

A textura usa `UniformToFill`, preservando a proporção e cortando as bordas necessárias para preencher o quadro horizontal. Sobre ela há preto com opacidade de 28% e um gradiente inferior de 170 DIP, de transparente a `#C9000000`. O fundo de reserva é `#101A20`. O logo branco existente `Assets/LogoBranca.png` ocupa uma caixa central de 360 × 100 DIP, com escala uniforme. Essa abertura não altera a identidade, os controles ou o fundo da área de trabalho.

O canto inferior esquerdo apresenta a etapa em Segoe UI de 12 DIP, branco, margem esquerda de 32 DIP e inferior de 49 DIP, com reticências para excesso de texto. A barra indeterminada abaixo mede 210 × 3 DIP, margem inferior de 32 DIP, branco sobre `#44FFFFFF`, sem percentual estimado. A versão fica no canto inferior direito em Segoe UI Light de 50 DIP, margens direita de 30 e inferior de 21 DIP. O texto vem de `VersaoDoAplicativo.Atual`; a versão permanece 1.4.0. Os tamanhos descritos são nominais e acompanham a escala do quadro.

A janela e o logo têm nomes acessíveis; a etapa usa anúncio `Polite` e o progresso informa Inicialização em andamento. Não há ação de entrada ou credenciais na abertura. A leitura por tecnologia assistiva não foi ensaiada nesta passagem.

## Inicialização e encerramento

- A abertura aparece após a verificação de instância única, antes da preparação do sistema. O processamento permite uma passagem do Dispatcher para apresentação da janela.
- O texto inicial é Preparando o sistema…; Preparando os dados… acompanha a preparação dos serviços e dados; Preparando o atendimento… aparece antes de obter a janela principal. São etapas do fluxo real, sem simulação de percentual.
- Um relógio monotônico reinicia em `ContentRendered`. Antes de mostrar a principal, aguarda-se apenas o restante do mínimo de 5 segundos, conforme ajuste solicitado pelo usuário. Se o trabalho já consumiu esse tempo, não há espera adicional.
- A janela principal é exibida antes do fechamento normal da abertura. O fechamento programático limpa a referência antes de fechar, preservando a inicialização e a janela principal.
- Licença, avisos de erro e recuperação fecham a abertura antes de apresentar sua interface. Se o fluxo de licença permitir continuar, uma nova instância da abertura pode ser apresentada para a preparação restante.
- Fechar manualmente a abertura cancela a inicialização e solicita o encerramento do aplicativo. Fechar também interrompe a espera pendente e desativa a barra indeterminada.

Licenciamento, recuperação, autenticação, dados e regras de negócio seguem os fluxos existentes. O trabalho não acrescenta autenticação automática, atalhos de acesso ou alteração da versão.

## Origem do ativo

`Assets/AberturaFundo.png` deriva da imagem fornecida pelo usuário em `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-25929a9f-981c-4cdd-bf3c-a33f39320f03.png`, de 1200 × 2601 pixels. A cópia conserva os pixels originais; o enquadramento acontece somente na interface. O hash SHA-256 dos pixels RGBA registrado pela execução principal é `9f84df42bdda9418a7dabaabe0817aecad9c4e474cc1c9c966b47363f78ab904`.

A procedência foi embutida em metadado PNG `tEXt` pelo comando Impeccable `embed-prompt`; o escaneamento da execução principal registrou um raster e nenhuma origem ausente. Não houve geração ou edição com ImageGen. O logo é um recurso existente do projeto e as mudanças de identidade realizadas em outra tarefa não integram esta extensão.

## Fontes conferidas

- `backend/src/VarthexComanda.Desktop/JanelaAbertura.xaml`: composição, imagem, sobreposições, marca, texto, progresso, versão e acessibilidade declarada.
- `backend/src/VarthexComanda.Desktop/JanelaAbertura.xaml.cs`: escala, versão dinâmica, mínimo monotônico e cancelamento da espera.
- `backend/src/VarthexComanda.Desktop/App.xaml.cs`: integração de abertura, etapas, licença, erros, recuperação, cancelamento manual e exibição da principal.
- `backend/src/VarthexComanda.Desktop/AparenciaJanelaModal.cs`: exclusão da abertura do cabeçalho compartilhado.
- `backend/src/VarthexComanda.Desktop/VarthexComanda.Desktop.csproj`: inclusão da textura como recurso.
- `backend/tests/VarthexComanda.Desktop.Tests/Xaml/JanelaAberturaTests.cs`: enquadramento, versão, ausência de moldura modal, etapa, tempo mínimo, cancelamento da espera e fechamento programático.

## Evidências e limites

`outputs/abertura-build.log` confirma compilação do aplicativo com zero avisos e zero erros. `outputs/abertura-testes.log` e `backend/artifacts/abertura-resultados/abertura-focados.trx` registram 11/11 testes focados aprovados; o log contém dois avisos preexistentes xUnit2031 em `AtendimentoTecladoXamlTests.cs`. `outputs/abertura-regressao.log` e `backend/artifacts/abertura-resultados/abertura-desktop.trx` registram 330/330 testes Desktop aprovados. Os focados estão incluídos no total Desktop; as contagens não representam 341 testes distintos. A documentação conferiu os logs e o código amostrado, sem reexecutar testes.

As duas capturas atuais são `.impeccable/review/abertura/abertura-1366.png` (760 × 490 pixels) e `abertura-640.png` (571 × 368 pixels). Os nomes indicam a largura da área útil simulada: respectivamente 1366 × 768 e 640 × 400 DIP. A amostra documental abriu ambas: a textura preenche o quadro, a marca permanece centralizada e etapa, barra e versão preservam a composição na escala reduzida. As capturas vieram de `RenderTargetBitmap` de janelas WPF reais, a 96 DPI, com a etapa Preparando o atendimento…; não são fotografias do instante de abertura ao vivo. Os demais arquivos da pasta são evidência de outros componentes e não ampliam o escopo visual desta tela.

A revisão independente retornou `ship`, sem achados materiais, ao conferir referência, imagens, código e testes. A execução principal abriu `backend/artifacts/abertura/bin/VarthexComanda.Desktop/debug/VarthexComanda.exe` e confirmou a chegada à área principal 1.4.0 sem abertura remanescente, sem automatizar autenticação. O instante de apresentação durante os 900 ms não foi capturado ao vivo. A animação indeterminada, a duração percebida, o uso em múltiplos monitores, outras escalas de DPI e todos os caminhos de erro/licença/recuperação não foram observados visualmente. Os testes e a leitura do código não substituem essa observação.

Não houve nova execução das demais suítes, publicação, push ou atualização de instaladores nesta extensão. As demais alterações presentes no projeto pertencem a outras tarefas e foram preservadas. A escrita documental está limitada a esta especificação e ao acréscimo ao final de `DESIGN.md`: nenhum README, documento em `docs/`, `PRODUCT.md` ou `.impeccable/design.json` foi criado ou refeito. O formato legado e as ausências documentais preexistentes permanecem como registro, sem aprovação global da interface.
