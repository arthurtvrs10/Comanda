# Revisão de interface — 21/10/2026

Revisão com Impeccable, em modo Operate, preservando a identidade e os fluxos do Varthex. Os critérios de interface nativa foram adaptados ao desktop WPF; não se trata de uma certificação de acessibilidade.

## Resultado

| Dimensão | Avaliação | Evidência e limite |
| --- | --- | --- |
| Acessibilidade | 3/4 | Foco de célula preservado, status acompanhado de texto, teclado coberto pelos testes. Leitor de tela completo não foi exercitado. |
| Desempenho | 3/4 | Virtualização nativa do DataGrid preservada; não foi realizado ensaio de carga com catálogo grande. |
| Temas | 3/4 | Conferência de claro/escuro e correção de cores de mensagens. Janelas nativas de arquivo e impressora seguem o Windows. |
| Consistência de plataforma | 3/4 | Estilos comuns para texto/célula, alinhamento de listas e campos compactos. Há telas operacionais de toque e diálogos compactos com densidades diferentes por função. |
| Adaptação | 3/4 | Capturas de telas amplas e estreitas (800/1280), rolagem disponível e teste de fontes 12/16. Diferentes escalas físicas de DPI não foram exercitadas. |
| Total | 15/20 | Bom, com limites de verificação registrados acima. |

## Problemas encontrados e corrigidos

- **P1 — compilação anterior continuava aberta.** O processo visível usava `produtos-tabela`, anterior às correções de alinhamento. Foi encerrado normalmente pela interface, sem descartar formulários. O novo processo foi conferido pelo caminho do executável `revisao-interface`.
- **P2 — alinhamento dependia de estilos locais.** A célula passou a ocupar a linha inteira; texto e indicadores possuem centralização explícita. Estilos `TextoTabela`, `NumeroTabela` e `CelulaTabela` no App.xaml fornecem uma regra comum. Os preços e códigos numéricos usam alinhamento horizontal à direita.
- **P2 — listas administrativas e cabeçalhos não compartilhavam alinhamento.** ListViewItem, ListBoxItem e GridViewColumnHeader agora têm centralização vertical. A lista de origens herda o estilo comum; histórico e backup preservam a herança quando acrescentam altura própria.
- **P2 — mensagens com cores fixas.** Histórico, backup e encerramento passaram a usar os recursos de tema para mensagens e estados vazios.
- **P3 — densidade compacta sem regra explícita de alinhamento de conteúdo.** Configurações e DialogoCompacto agora centralizam o conteúdo de seus controles; campos multilinha com alinhamento superior explícito continuam apropriados à edição de parágrafos.

## Cobertura

Revisados código, estilos compartilhados e capturas de Produtos, novo/edição, comandas, cozinha, pagamento, configurações (sete seções), login, alteração de senha, assinatura e painel de senhas. Listas de histórico e backup também foram examinadas no código e pelos testes de carga de XAML. Impressoras, numeração e colaboradores mantêm seus testes próprios. Não houve impressão física, chamada de áudio nem operação financeira durante esta revisão.

- 271 testes de Desktop aprovados.
- Novo teste geométrico com janela WPF real: todas as células de produtos comparadas com o centro da linha, tolerância de 1,1 pixel lógico; temas claro/escuro; linhas de 32/44; fontes de 12/16; seleção, produto ativo/inativo e nome longo. Indicadores de status medidos com a mesma regra.
- Capturas de conferência em `artifacts/revisao-interface/previas/`.

A revisão não altera cadastros, dados de venda ou regras de acesso.
