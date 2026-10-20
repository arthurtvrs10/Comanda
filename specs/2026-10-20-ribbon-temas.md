# Ribbon e aparência — 20/10/2026

## Inventário e conexão

Aplicação Windows WPF/MVVM, com instâncias reutilizadas de AtendimentoView, ProdutosView, HistoricoView, BackupView e ConfiguracaoView. CozinhaView consulta comandas abertas. Não há roteador web. A navegação mantém os comandos roteados Ctrl+1 a Ctrl+5 e as barreiras de autenticação existentes: PIN para garçom/cozinha, senha para gestor e modo de recuperação restrito a backup.

| Categoria | Recursos conectados |
| --- | --- |
| Principal | Mesas/comandas para atendimento; consulta de comandas abertas no perfil cozinha |
| Produtos | Produtos e categorias no catálogo existente; novo produto no diálogo existente |
| Marketing | Sem módulos existentes; mensagem contextual, sem comandos fictícios |
| Financeiro | Histórico de vendas existente, com filtros e detalhes |
| Apps | Sem módulos existentes; mensagem contextual, sem comandos fictícios |
| Configurações | Estabelecimento, preferências de atendimento, senha/PIN, backup/restauração, licença/versão |

Caixa independente, PDV, delivery, clientes, complementos, estoque, financeiro de contas, impressão, integrações e dispositivos não foram inventados. Forma de pagamento continua no fechamento existente; não há nova integração com adquirentes.

## Navegação

Categorias apenas selecionam a Ribbon, sem trocar o conteúdo. Os comandos abrem a tela correspondente. A faixa tem grupos, separadores, ícones vetoriais e rótulos; categoria ativa tem fundo e sublinhado. Abaixo de 1050 px, os grupos passam a um menu hierárquico. As categorias continuam acessíveis por teclado via Tab e Enter.

Instâncias de telas e modelos são reutilizadas. Trocar a tela não descarta configurações em edição nem filtros do catálogo, histórico ou atendimento. Trocar uma seção interna das configurações também preserva seus campos. Fechar, bloquear ou sair de um perfil mantém confirmação de descarte quando há alterações pendentes.

Pesquisa: Ctrl+K; seta para baixo entra nos resultados; setas selecionam; Enter abre; Esc limpa/retorna. Funcionalidades usam busca sem distinção de acentos. Produtos ativos são consultados no catálogo inteiro, sem depender do filtro atual; a pesquisa respeita as permissões. O gestor abre o catálogo filtrado; o garçom abre o atendimento com o produto pesquisado. Não existe entidade independente de cliente ou pedido numerado para pesquisar.

## Temas

Recursos dinâmicos para fundos, textos, bordas, seleção e estados em telas, tabelas, campos, menus, avisos e diálogos WPF. O seletor de categoria recebeu template próprio para evitar fundo claro do tema nativo no modo escuro. Cores e marca verde preservadas. Preferência salva em `aparencia.txt` na raiz de dados da aplicação e restaurada ao abrir. Se não for possível gravar, o tema funciona na sessão e há aviso.

Caixas de diálogo nativas do Windows, como seleção de arquivo/pasta e a barra de título, seguem as configurações do sistema operacional.

## Validação

Suíte: 107 testes de aplicação, 251 de interface, 4 de domínio e 265 de infraestrutura (627 no total). Abrange cadastro, comanda, preço histórico, fechamento, forma de pagamento existente, backup, migração e recuperação.

Novo teste integrado: RibbonPreservaConteudoFiltrosPermissoesETemaEntreSessoes. Verifica ordem das categorias, seleção sem navegação, comandos conectados, filtros, rascunhos, pesquisa restrita por perfil, tema persistido e modo compacto. Prévias reais WPF em 1280×780 e 800×600, claro/escuro, ficam em `artifacts/ribbon/previas` com dados fictícios.

Sem novas migrações nesta reorganização. Alterações anteriores de mesas e observações foram preservadas. Não foi publicado instalador nem alterada a base operacional para validar a interface. Pendente: homologação com operador e dispositivos reais, menus nativos conforme o tema do Windows e instalação da nova compilação no ambiente final.

### Cabeçalho compacto — ajuste pela referência
Removida a linha superior com logo, perfil e botões avulsos. Categorias, pesquisa, modo escuro e menu “Bloquear ou trocar usuário” compartilham a barra superior. Menu principal, alternância de perfil e bloqueio continuam disponíveis nesse menu, com as permissões existentes. Comandos na faixa abaixo, sem caixas individuais e com grupos separados. Validação: 251 testes de interface aprovados.

### Continuidade de cor da categoria selecionada
A aba ativa e toda a faixa de comandos usam o mesmo recurso RibbonSurface, mais claro que a barra de categorias. Removidos espaçamento e sublinhado da aba para formar uma área contínua. Tema escuro: barra #23262B e faixa #323236; tema claro: faixa #E1E8E4. Navegação e permissões preservadas; teste integrado da Ribbon aprovado nos dois temas.

### Entrada direta no sistema
Removida a tela de escolha Garçom/Cozinha/Gestor da inicialização. A aplicação abre a área de trabalho com Ribbon na categoria Principal. Mesas/comandas solicita o PIN antes de liberar atendimento. O menu de usuário permite entrar em atendimento, cozinha ou gestor com as credenciais existentes. Voltar do bloqueio retorna à área de trabalho, sem exibir a antiga tela. Recuperação de banco continua restrita ao gestor. Validação: 251 testes de interface aprovados.

### Modal de login no botão de sessão
O botão “Bloquear ou trocar usuário” não abre mais menu. Ele revoga o acesso atual e abre uma janela modal central com perfil e senha/PIN. Credenciais e permissões existentes são reutilizadas. Cancelar deixa o sistema bloqueado. Recuperação de banco aceita somente o gestor. Primeiro cadastro da senha continua pelo fluxo existente. Validação: 252 testes da interface aprovados.

### Comandos visíveis com autenticação ao entrar
Todos os comandos de módulos existentes ficam visíveis na Ribbon e no menu compacto para todos os perfis. A pesquisa de funcionalidades também mostra esses destinos, sem expor dados de produtos a perfis sem acesso. Ao clicar em função administrativa sem autorização, solicita a senha do gestor antes de navegar. Mesas/comandas continua solicitando PIN quando bloqueado. A consulta de cozinha pode ser aberta por qualquer perfil autenticado. Recuperação mantém somente backup utilizável. Validação: 252 testes de interface aprovados.

### Opções da grade de comandas
Menu “Opções” à direita da pesquisa, alinhado à extremidade da tela. “Atualizar” recarrega a grade preservando o número pesquisado. “Juntar pedidos” permite escolher duas comandas abertas e confirmar a transferência integral dos itens para o destino, somando os valores e liberando o número de origem. Itens mantêm nome, preço e observação originais; linhas não são mescladas. Transação única impede gravação parcial. Origem é marcada como cancelada, zerada e recebe anotação da transferência; não é gerada venda nem pagamento. Mesma origem/destino, origem vazia e comandas fechadas são recusadas. Testes: 107 aplicação, 254 interface, 4 domínio e 267 infraestrutura aprovados (632). Rastreabilidade regenerada.

### Comanda em janela modal
Ao abrir uma comanda com a interface visível, o painel de lançamento passa para uma janela modal central sobre a grade. Permite redimensionar, maximizar/restaurar e fechar pelo X. Nome e identificador da comanda aparecem no título. Conteúdo existente de catálogo, observações, itens, quantidades, dados da comanda e pagamento reutilizado. Diálogos de confirmação e pagamento usam a janela ativa como proprietária. Fechar preserva itens já gravados e segue o bloqueio de atendimento existente. Impressão/WhatsApp dos exemplos não foram adicionados pois não existem integrações correspondentes. Validação: 255 testes da interface aprovados, incluindo janela real maximizada/restaurada/fechada e reabertura da comanda com itens preservados.
