# VComanda — tela principal

A tela inicial segue o esboço fornecido pelo usuário: Garçom ocupa o bloco principal à esquerda, Cozinha e Gestor dividem a coluna central e os atalhos administrativos ficam à direita. É uma tela de operação para estabelecimentos, com uso frequente por toque e teclado.

O verde profundo `#245640` identifica atendimento; areia `#EAE2D1` identifica consulta da cozinha; verde acinzentado `#E1E8E4` identifica gestão. Fundo `#F3F5F2`, texto escuro e tipografia Segoe UI mantêm a interface legível. Blocos e controles usam cantos retos, sem arredondamento; controles têm pelo menos 46 px de altura. Ícones são traços vetoriais simples, sem imagens decorativas.

Preservar a hierarquia do esboço e a marca existente. Exibir F1, F2 e F3 abaixo dos nomes Garçom, Cozinha e Gestor, respectivamente. Não exibir título ou subtítulo introdutório acima dos módulos. Não exibir atendentes fictícios, sessões inventadas ou funções ainda indisponíveis. F1 e F2 pedem PIN; F3 e atalhos administrativos pedem a senha do gestor. Ao terminar um atendimento, revogar o acesso e mostrar o PIN sobre a tela inicial. Voltar ao início não autentica o usuário.

A cozinha é exclusivamente uma consulta de comandas abertas, sem estados de preparo. A tela deve acomodar 1280×780 e 800×600, com rolagem quando necessária. Referências: Fluent 2 (espaçamento e interação por toque) e Square Restaurants (navegação direta por tarefa).


## Navegação interna — opção 04 aprovada

Cabeçalho branco único, sem logo: abas com ícone, nome e atalhos à esquerda; perfil e ações de sessão à direita. Aba atual em verde claro com sublinhado verde escuro, atualizado por clique e teclado. Gestor mantém os destinos administrativos; Garçom vê Atendimento e Histórico; Cozinha mostra Comandas abertas, somente consulta. Em janelas estreitas, as abas quebram linha sem ocultar destinos. Cantos retos e alvos de toque de pelo menos 46 px. Licença abre uma janela, por isso não substitui a indicação da página atual.


## Produtos — catálogo aprovado

Linhas compactas com foto de 56 px, nome, categoria, preço e situação. Busca e filtro acima da lista. Novo produto e clique em um item abrem um modal centralizado, com foto, nome, categoria, preço e nova categoria; edição permite ativar e desativar. Cancelar ou Esc descarta o formulário e alterações pendentes da foto. Cantos retos e verde do sistema.

## Configurações — central do gestor aprovada
Cinco seções: Estabelecimento, Atendimento, Segurança, Backup e Licença e sistema. Navegação lateral em telas amplas e categorias acima em telas menores que 800 px. Fundo #F3F5F2, seleção #E1E8E4, ações principais #245640, cantos retos e controles de pelo menos 46 px. Somente Salvar alterações no formulário, com indicação de pendências; ao sair, confirmação com Continuar editando e Descartar e sair. Credenciais em modal próprio, backups e ativação reaproveitam os fluxos existentes. Não duplicar backup e licença no cabeçalho normal; preservar o acesso de recuperação de banco corrompido.

## Ribbon e temas — revisão de 20/10/2026

Esta seção substitui a navegação interna descrita na opção 04. Preservar marca, verde, Segoe UI, cantos retos, perfis e bloqueio. Categorias na ordem Principal, Produtos, Marketing, Financeiro, Apps, Configurações. Selecionar uma categoria somente troca os comandos da faixa; comandos abrem as telas existentes. Não criar destinos para módulos ausentes. Marketing e Apps mostram estado vazio explicativo.

Modo de uso: Operate. Faixa compacta com grupos nomeados, separadores e ícones vetoriais; categoria ativa tem fundo e sublinhado. Em larguras abaixo de 1050 px, grupos recolhidos em menu. Pesquisa global Ctrl+K, com Enter, Esc e setas. Os modelos de tela permanecem em memória para preservar filtros e edições; saída da sessão protege alterações pendentes. Sem menu lateral permanente de categorias globais; a navegação interna das configurações continua específica daquela tela.

Tema claro e escuro com recursos dinâmicos em toda a interface WPF. Fundos verdes escuros, texto claro de contraste e seleção verde no escuro; verde original no claro. Preferência persistida na raiz de dados. Diálogos nativos seguem o Windows. Referências visuais: anexos Consumer principal, produtos e modo escuro, usados pela lógica de organização e interação, sem reutilizar marca ou ativos do Consumer.

### Cabeçalho compacto
A pedido do usuário, não exibir logo nem linha separada de ações acima da Ribbon. Categorias, pesquisa, tema e menu de sessão na mesma barra; comandos logo abaixo, sem moldura individual. O menu de sessão reúne menu principal, alternância de perfil e bloqueio. Preservar as permissões e a confirmação de alterações pendentes.

A categoria selecionada e toda a faixa de comandos compartilham a mesma cor mais clara (RibbonSurface), formando uma superfície contínua sem sublinhado ou espaço entre a aba e a faixa. A barra de categorias permanece mais escura. No escuro: #23262B na barra e #323236 na faixa; texto #F1F3F2 na aba selecionada.

Paleta da amostra fornecida: barra #0A0A0A e faixa/aba ativa #323130 no tema escuro. Hover neutro #484644; no tema claro faixa #E5E4E3. Categorias com 16 px de preenchimento lateral e 6 px de intervalo. A aba ativa mantém a cor contínua da faixa durante o hover.

### Entrada direta
A tela inicial de blocos Garçom/Cozinha/Gestor deixa de ser exibida. Abrir na área de trabalho com a Ribbon em Principal. Acesso a Mesas/comandas solicita PIN; as áreas de atendimento, cozinha e gestor ficam no menu de usuário. Permissões, bloqueio e recuperação preservados.

Configurações do sistema: modal central redimensionável com navegação lateral de sete seções e ações Salvar/Opções avançadas no rodapé. Usa recursos de tema existentes; Backup e Alterar senha permanecem no Ribbon. Detalhes em specs/2026-10-20-configuracoes-sistema.md.
