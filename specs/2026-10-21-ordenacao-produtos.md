# Indicadores de ordenação em Produtos — 21/10/2026

Os cabeçalhos ordenáveis mostram dois pequenos chevrons neutros. Após o clique, a coluna ativa mostra um único chevron para cima (crescente) ou para baixo (decrescente). O tooltip e o texto acessível informam a ordem atual e a ação do próximo clique. Os indicadores permanecem centralizados na altura do cabeçalho nos temas claro e escuro.

O template nativo do cabeçalho permanece, incluindo os controles de redimensionamento. A personalização limita-se ao conteúdo do cabeçalho. O preenchimento vazio à direita não apresenta indicador. Medida não ordena porque todos os produtos desta versão usam UN.

Cód. sistema, Nome, Preço custo, Preço venda, Status venda e Cód. personalizado usam a ordenação nativa pelos valores reais. Valores monetários continuam numéricos. Categoria ordena pelo nome exibido usando comparação pt-BR, com código do produto como desempate estável. A ordenação atua apenas na visão da coleção; não grava alterações no cadastro.

Validação: teste com cliques reais do cabeçalho WPF confere IDs 2/10/30, alternância crescente/decrescente, nomes de categorias, preços, nomes, status, indicadores e coleção original intacta. O teste existente de alinhamento de células também passou. Capturas em artifacts/produtos-ordenacao/ nos estados inicial, crescente, decrescente e tema escuro.
