# Localizador de produtos — modelo Consumer

Data: 21/10/2026. Refinamento visual solicitado como “Faça igual”, com duas imagens de referência: lista com seleção azul e ações circulares; cartão compacto com foto, código, nome e preço. Escopo restrito ao localizador WPF existente, em modo Operate. O formulário Personalizar item permanece como implementado anteriormente.

## Fontes e referências

- Implementação final: `backend/src/VarthexComanda.Desktop/Atendimento/LocalizarProdutoView.xaml` e `LocalizarProdutoViewModel.cs`.
- Fluxo preservado conferido em `LocalizarProdutoView.xaml.cs`: cartão e ação Personalizar abrem `PersonalizarItemView` por `ShowDialog()`; Adicionar mantém o comando existente.
- Referências do usuário: `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-f0bdc603-3d5a-4318-bed8-246f11ee89a2.png` e `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-f6f10ba9-bdb5-4a7d-8683-a1e365b66cef.png`.

## Composição final

A janela preserva Segoe UI de 12 DIP, cantos retos e recursos neutros dinâmicos `Ajuda*`. Busca, categorias, alternância Lista/Fotos, Atualizar e retorno ao pedido mantêm suas funções. Fotos continua sendo o modo inicial. A referência orienta a composição; não foi incorporada marca ou fotografia externa.

Na lista, linhas e cabeçalho medem 42 DIP. As colunas são Categoria, Cód., Nome do Produto, Preço de Venda, Adicionar (F11) e Personalizar (Enter), com os atalhos na segunda linha dos dois últimos cabeçalhos. A seleção usa `#1969CE` e texto branco; preços em negrito. Adicionar e Personalizar têm áreas de 40 × 38 DIP, círculos de 18 DIP e símbolos vetoriais de mais/reticências centralizados. As ações usam `#487FAF` normalmente e `#CAE5FF` na linha selecionada, mantendo hover, pressionamento, foco e estado desabilitado.

O cartão passa de 156 × 190 para 124 × 154 DIP, com margem de 4 DIP e imagem de 66 DIP de altura. O código fica à esquerda em faixa de 20 DIP; o nome, centralizado em negrito, ocupa até duas linhas e oferece tooltip; o preço fica centralizado na faixa inferior de 26 DIP. `PrecoVenda` formata duas casas decimais em pt-BR (`N2`), sem `R$`, tanto na lista quanto nos cartões. As fotos reais continuam vinculadas ao cadastro; sua ausência usa o placeholder vetorial neutro existente. A imagem sintética de verificação não é um ativo entregue ao catálogo.

O cartão inteiro abre Personalizar item antes de lançar o produto. A lista conserva Adicionar/F11 e Personalizar/Enter, além de Ctrl+F na busca e Esc/Voltar. Esta passagem não muda o formulário, a personalização, a persistência ou a regra de lançamento.

## Evidências e limites

- Build desta passagem em `backend/artifacts/catalogo-modelo`.
- `LocalizadorProdutoTests`: 5/5 aprovados; resultado em `backend/artifacts/catalogo-modelo-resultados/catalogo-modelo.trx`. A suíte Desktop anterior de 307 testes não foi reexecutada.
- Oito capturas válidas em `.impeccable/review/catalogo-modelo/`, abertas individualmente em uma rodada própria: `localizador-{False,True}-740-lista.png` (duas), `localizador-{False,True}-{740,1100}-fotos.png` (quatro) e `localizador-{False,True}-vazio.png` (duas). `False` representa claro e `True`, escuro; as dimensões avaliadas foram 740 × 520 e 1100 × 740. A lista larga não foi revalidada.
- A revisão independente abriu as duas referências e as oito capturas e retornou `ship`, sem correção material no escopo restrito. As demais imagens eventualmente presentes na pasta não ampliam esta evidência.

A verificação é de uma superfície WPF; não há detector HTML nem comparação visual automatizada comprovada. Ao concluir, a versão anterior foi fechada pela janela principal e o executável de `backend/artifacts/catalogo-modelo` foi aberto e conferido na tela principal, aguardando desbloqueio pelo usuário. A evidência das novas funções continua sendo a execução focada e as capturas controladas. Esta documentação preserva o legado de `DESIGN.md`, não representa aprovação global da interface e não cria ou repara `PRODUCT.md` ou `.impeccable/design.json`, ausentes antes desta passagem.
