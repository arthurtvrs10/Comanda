# VComanda — tela principal

A tela inicial segue o esboço fornecido pelo usuário: Garçom ocupa o bloco principal à esquerda, Cozinha e Gestor dividem a coluna central e os atalhos administrativos ficam à direita. É uma tela de operação para estabelecimentos, com uso frequente por toque e teclado.

O verde profundo `#245640` identifica atendimento; areia `#EAE2D1` identifica consulta da cozinha; verde acinzentado `#E1E8E4` identifica gestão. Fundo `#F3F5F2`, texto escuro e tipografia Segoe UI mantêm a interface legível. Blocos e controles usam cantos retos, sem arredondamento; controles têm pelo menos 46 px de altura. Ícones são traços vetoriais simples, sem imagens decorativas.

Preservar a hierarquia do esboço e a marca existente. Exibir F1, F2 e F3 abaixo dos nomes Garçom, Cozinha e Gestor, respectivamente. Não exibir título ou subtítulo introdutório acima dos módulos. Não exibir atendentes fictícios, sessões inventadas ou funções ainda indisponíveis. F1 e F2 pedem PIN; F3 e atalhos administrativos pedem a senha do gestor. Ao terminar um atendimento, revogar o acesso e mostrar o PIN sobre a tela inicial. Voltar ao início não autentica o usuário.

A cozinha é exclusivamente uma consulta de comandas abertas, sem estados de preparo. A tela deve acomodar 1280×780 e 800×600, com rolagem quando necessária. Referências: Fluent 2 (espaçamento e interação por toque) e Square Restaurants (navegação direta por tarefa).


## Navegação interna — opção 04 aprovada

Cabeçalho branco único, sem logo: abas com ícone, nome e atalhos à esquerda; perfil e ações de sessão à direita. Aba atual em verde claro com sublinhado verde escuro, atualizado por clique e teclado. Gestor mantém os destinos administrativos; Garçom vê Atendimento e Histórico; Cozinha mostra Comandas abertas, somente consulta. Em janelas estreitas, as abas quebram linha sem ocultar destinos. Cantos retos e alvos de toque de pelo menos 46 px. Licença abre uma janela, por isso não substitui a indicação da página atual.


## Produtos — catálogo aprovado

Linhas compactas com foto de 56 px, nome, categoria, preço e situação. Busca e filtro acima da lista. Novo produto e clique em um item abrem um modal centralizado, com foto, nome, categoria, preço e nova categoria; edição permite ativar e desativar. Cancelar ou Esc descarta o formulário e alterações pendentes da foto. Cantos retos e verde do sistema.
