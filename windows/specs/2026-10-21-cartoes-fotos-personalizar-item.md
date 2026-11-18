# Cartões com fotos e Personalizar item — 21/10/2026

Extensão estreita de operação no WPF .NET 10 existente, orientada pelas duas referências Consumer fornecidas pelo usuário. A última orientação estabelece que clicar no cartão abre Personalizar item antes do lançamento. Substitui apenas as medidas dos cartões e o formulário anterior descritos em `2026-10-21-prioridades-localizador-modal-backup.md`; preserva o restante da interface e o formato legado de `DESIGN.md`, sem criar `PRODUCT.md` ou `.impeccable/design.json`.

## Composição e interação

- Localizar produto abre em Fotos. Cartão inteiro em um `Button` de 156 × 190 DIP, margem de 4 DIP, imagem de 100 DIP de altura, código, nome em até duas linhas com tooltip e preço. Foto existente usa preenchimento proporcional; ausência usa o vetor existente. Borda única, hover e foco acompanham o cartão. Não há Adicionar nem reticências dentro dele.
- Clicar no cartão abre Personalizar item, modal pertencente ao localizador. A lista mantém Adicionar e Personalizar; F11 lança uma unidade, Enter abre personalização do selecionado, Ctrl+F foca a busca e Voltar/Esc retorna ao pedido conforme o fluxo existente.
- Personalizar item mede 880 × 620 DIP, mínimo de 700 × 440 DIP. Mantém cabeçalho cinza compartilhado sem marca, Segoe UI de 12 DIP, cantos retos e recursos dinâmicos `Ajuda*` nos dois temas. Topo reúne quantidade, nome e total; corpo divide Observações e Complementos; rodapé oferece Cancelar e Adicionar item verde.
- Quantidade aceita inteiros de 1 a 999 UN, com campo editável e botões menos/mais. Entrada inválida impede a confirmação e apresenta mensagem. O total acompanha a quantidade válida. Observação permite até 300 caracteres. Complementos informa: “Este catálogo ainda não possui cadastro de complementos.” Não existe seleção funcional de complementos nem quantidade fracionada no domínio atual.
- A observação pendente é copiada para a janela. Cancelar ou fechar não lança produto nem altera o texto pendente. Confirmar passa por `LocalizarProdutoViewModel.Lancar` e `AtendimentoViewModel.AdicionarProduto`, preservando regras existentes e seleção pelo ID do item exato; somente o sucesso limpa a observação pendente e fecha a personalização.

## Fontes

Implementação em `backend/src/VarthexComanda.Desktop/Atendimento/`: `LocalizarProdutoView.xaml`, `LocalizarProdutoView.xaml.cs`, `LocalizarProdutoViewModel.cs`, `PersonalizarItemView.xaml`, `PersonalizarItemView.xaml.cs` e `PersonalizarItemViewModel.cs`. O fluxo de confirmação é o de `AtendimentoViewModel.cs`; `backend/src/VarthexComanda.Domain/Produto.cs` não oferece complementos nem unidade fracionada.

Referências visuais fornecidas: `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-e80725b3-403b-4905-8acc-26c53618a47b.png` e `C:/Users/arthu/AppData/Local/Temp/codex-clipboard-5c6b18f3-7caf-42b8-a07c-ac7646bd4b6a.png`. Orientam composição e comportamento, sem incorporar marca ou ativos Consumer.

## Evidência e limites

`backend/tests/VarthexComanda.Desktop.Tests/Xaml/LocalizadorProdutoTests.cs`: cinco testes focados aprovados, registrados em `backend/artifacts/cartao-personalizacao-resultados/cartao-personalizacao.trx`. Cobrem Fotos inicial, busca/categorias, lançamento persistido, abertura pelo cartão sem lançar antes de confirmar, quantidade inválida, observação, cancelamento e renderização nos dois temas.

As 12 capturas válidas desta extensão estão em `.impeccable/review/cartao-personalizacao/`: `localizador-{False,True}-{740,1100}-fotos.png` (quatro), `localizador-{False,True}-740-lista.png` (duas), `localizador-{False,True}-vazio.png` (duas) e `personalizar-{False,True}-{normal,compacto}.png` (quatro). `False` é claro; `True`, escuro. A foto sintética é fixture temporária de teste, sem ativo demonstrativo entregue. Arquivos adicionais na pasta não ampliam esta evidência; a lista larga não foi revalidada aqui.

As capturas foram abertas individualmente em até duas rodadas próprias. A revisão independente final retornou `ship`, sem achados materiais para cartões e personalização. O detector HTML não se aplica ao WPF. A suíte anterior de 307 testes não foi reexecutada integralmente nesta extensão; não há afirmação de revisão global, integração com hardware ou aplicativo atualizado aberto para uso interativo.
