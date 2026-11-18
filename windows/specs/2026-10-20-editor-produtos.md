# Editor de produtos — referência de 20/10/2026

Novo produto e clique em um produto em Produtos e categorias abrem o mesmo modal redimensionável (820×700, mínimo760×520). Foto de 200 px à esquerda; à direita nome, categoria com criação inline, unidade, custo, venda, descrição e código personalizado. Código interno e Ativo no topo. Voltar/Salvar no rodapé; edição permite desativar. Controles de 28 px e fonte12 nos temas claro/escuro.

Custo em centavos não negativos, descrição opcional até2000 caracteres, código opcional até40 caracteres alfanuméricos ASCII/ponto/hífen/barra/sublinhado. Cadastro e edição gravam todos os campos no mesmo registro. Código único sem distinção de maiúsculas/minúsculas; pesquisa inclui código. Migração aditiva AddDetalhesProduto, mantendo dados anteriores e backup preventivo do fluxo existente. Alterações de produtos não mudam nome/preço histórico dos itens já lançados.

O formulário mostra os recursos atuais: venda por unidade. Estoque, ficha técnica, perguntas, complementos, serviço, venda fracionada e canais externos não foram simulados. Os vínculos de cozinha existentes continuam em Configurações do sistema → Locais/cozinhas.

Teste de aplicação cobre criar/editar/metadados opcionais/códigos duplicados e inválidos; teste de infraestrutura cobre gravação/reabertura/busca e índice único; testes de viewmodel preservam dados ao cancelar e mostram dados na edição. Validação visual de novo/edição nos dois temas. Correção de ContentTemplateSelector/SelectionBoxItemStringFormat no ComboBox mantém o nome de categorias selecionadas, conforme o [template oficial WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/combobox-styles-and-templates).

Ribbon: grupo Sistema renomeado para Configurações e Alterar senha agrupado nele. Backup continua no grupo Dados.

Validação conjunta com o painel: 674 testes aprovados (130 aplicação, 272 infraestrutura, 268 desktop, 4 domínio), incluindo a atualização do teste de teclado para o tamanho compacto solicitado.
