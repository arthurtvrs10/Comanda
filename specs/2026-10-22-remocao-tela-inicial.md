# Remoção da tela inicial

Data: 22/10/2026. Extensão restrita ao pedido de remover a tela antiga de cartões Garçom, Cozinha, Gestor e acessos rápidos. Referência do usuário: `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-b3a23d5b-b54d-4c1d-b8c5-0a76fe2c8322.png`.

## Contrato e apresentação

A entrada e o retorno levam diretamente ao menu e ao canvas da área principal. O fluxo é iniciar → menu → comando → autenticação necessária → módulo; voltar → menu. A primeira área visível conserva a Ribbon em Principal e o canvas neutro, sem os cartões antigos. A remoção preserva o sistema WPF inspirado no Consumer, modo Operate, Segoe UI, temas e janelas internas. Não cria identidade visual, componente ou ativo.

## Comportamento final

`InicioView.xaml` e `InicioView.xaml.cs` foram excluídos. O host residual `MenuInicial` saiu de `MainWindow.xaml`, e os vínculos correspondentes saíram do code-behind. A tela antiga já não era instanciada pelo código atual; a exclusão evita manter e empacotar seu recurso órfão no BAML.

`MostrarInicio` passa a `MostrarAreaPrincipal` nas entradas, retornos e ações de sessão. Esse caminho bloqueia o atendimento, oculta a camada de PIN, habilita o menu e seleciona Principal. O botão da camada de bloqueio passa de Voltar ao início para Voltar ao menu. Esse retorno permite escolher comandos sem autenticar a sessão.

Mesas / comandas e Cozinha continuam solicitando PIN; destinos administrativos continuam exigindo acesso de gestor. PIN incorreto mantém o bloqueio, PIN correto libera a área solicitada e saída/bloqueio revoga o acesso novamente. Permanecem F1 para Garçom, F2 para Cozinha e F3 para Gestor, nas condições existentes de teclado e sessão. A cozinha continua sendo consulta. A alteração não remove autenticação nem modifica as regras dos módulos.

## Fontes conferidas

- `backend/src/VarthexComanda.Desktop/MainWindow.xaml`: canvas, janelas internas e ausência do host `MenuInicial`.
- `backend/src/VarthexComanda.Desktop/MainWindow.xaml.cs`: `MostrarAreaPrincipal`, retorno, escolha de área e atalhos F1/F2/F3.
- `backend/src/VarthexComanda.Desktop/BloqueioAtendimentoView.xaml`: rótulo Voltar ao menu.
- `backend/tests/VarthexComanda.Desktop.Tests/Xaml/CarregamentoDeXamlTests.cs`: verificação da ausência do host e do tipo `InicioView`, bloqueio e retorno.

## Evidências e limites

Compilação de desenvolvimento em `backend/artifacts/remover-inicio/bin/VarthexComanda.Desktop/debug/VarthexComanda.exe`. O instalador e a versão instalada não foram alterados nem validados por esta extensão.

Resultado focado: 2/2 testes aprovados em `backend/artifacts/remover-inicio-resultados/entrada-menu.trx`:

- `PinBloqueiaMenusEDesbloqueiaPorToqueESaidaBloqueiaNovamente`: PIN incorreto/correto, novo bloqueio, retorno ao menu, cozinha e ausência de `MenuInicial` e do tipo `InicioView` no assembly.
- `RibbonPreservaConteudoFiltrosPermissoesETemaEntreSessoes`: preservação do conteúdo, filtros, permissões e tema entre sessões.

A suíte Desktop completa de 317 testes não foi reexecutada nesta extensão. A conferência de diferenças não encontrou problemas de whitespace. Esta documentação não executou novamente os testes, o aplicativo ou o launcher.

Cinco capturas atuais em `.impeccable/review/remover-inicio/`: `principal-sem-selecao-1200.png`, `principal-sem-selecao-800.png`, `bloqueio-pin-800.png`, `abas-garcom-1200.png` e `cozinha-consulta.png`. A amostra documental abriu a área principal de 800 e o bloqueio de PIN de 800: a primeira mostra o menu compacto sobre o canvas sem cartões; a segunda mostra a autenticação com Voltar ao menu. As demais capturas integram a evidência da revisão independente.

A revisão independente final retornou `ship`, sem achados materiais no escopo de remoção e preservação. Isso não representa aprovação global da interface, validação da versão instalada ou dos demais módulos. A documentação fica em commit separado do código. O formato legado de `DESIGN.md`, a ausência prévia de `PRODUCT.md` e `.impeccable/design.json` e os demais desvios de artefatos permanecem preservados; não foram promovidos a novas regras do sistema nem reparados nesta extensão.
