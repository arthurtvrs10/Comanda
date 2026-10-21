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

### Comandos específicos de Configurações
Ao lado da central do sistema ficam Numeração de pedidos, Impressoras e Colaboradores, seguidos de Alterar senha e Backup. Modais compactos com Segoe UI 12, controles de 28 px e temas dinâmicos. Senha em janela de 350 px com três campos; listas administrativas em janelas redimensionáveis. Referência: specs/2026-10-20-comandos-configuracao.md.

### Painel de senhas e editor de produto
Apps contém um chamador compacto com teclado e prévia e uma tela pública independente: senha atual grande e cinco anteriores na coluna direita. Cores da tela pública são configuráveis, independentes do tema administrativo. Produto novo/edição usam foto de200 px à esquerda, formulário à direita e ações compactas de28 px. Grupo Configurações reúne também Alterar senha. Detalhes em specs/2026-10-20-painel-senhas.md e specs/2026-10-20-editor-produtos.md.

### Alinhamento compartilhado — revisão de 21/10/2026
Tabelas usam o apresentador com altura completa e conteúdo centralizado verticalmente. Texto e indicadores compartilham o centro da linha, inclusive em alturas diferentes. Preços e códigos numéricos ficam à direita; nomes e categorias à esquerda. Listas e cabeçalhos administrativos herdam a regra comum; campos multilinha preservam alinhamento superior. Evidência e limites em specs/2026-10-21-revisao-interface.md.

### Produtos em janela interna — 21/10/2026
A listagem Produtos abre como janela filha dentro da área de trabalho, sobre o fundo configurado, com título e botões maximizar/restaurar e fechar. A Ribbon permanece disponível. Janela normal de até1180×600, limitada ao espaço interno e movível pelo título. Maximizar não cobre categorias ou rodapé. Detalhes e testes em specs/2026-10-21-janela-interna-produtos.md.

### Ajuda e numeração — 21/10/2026
Categoria Ajuda à direita de Configurações com um único comando visível, Ajuda, que abre as opções de suporte WhatsApp, RustDesk, feedback e tutorial guiado. Feedback, Show training e Diagnóstico de serviços permanecem disponíveis pela pesquisa global, com as permissões existentes; Contate o suporte foi removido. A numeração configura o primeiro número real da grade de comandas, preservando atendimentos abertos fora da faixa. Especificações em specs/2026-10-21-categoria-ajuda.md e specs/2026-10-21-numeracao-comandas.md.

### Opções de ajuda — superfície aprovada em 21/10/2026
O modal de ajuda usa a marca original em versão preta/branca conforme o tema, superfícies e textos cinza neutros, ações verdes e cinco ícones vetoriais coloridos. Seções de título e botão, cantos retos, corpo rolável e rodapé fixo com “Fechar e ocultar botão”; hover cinza e foco de teclado explícito. Ocultar vale para o atalho flutuante na sessão atual; Ajuda na Ribbon continua acessível. Central de ajuda fica “Em preparação” até existir URL. Esta paleta `Ajuda*` é específica da superfície. Composição, recursos e evidências em specs/2026-10-21-categoria-ajuda.md.

Ajuda abre o modal compacto de 360 × 640 junto ao ícone de ajuda, acima dele com intervalo de 4 e bordas direitas alinhadas. Só reduz para caber no espaço disponível, respeitando a área útil do monitor; pode abrir abaixo quando esse lado tiver mais espaço. Não ocupa a altura inteira do sistema e não pode ser arrastado ou redimensionado separadamente.

A ajuda é uma janela não bloqueante: permite usar as outras funções enquanto permanece aberta. Cliques repetidos ativam a mesma instância; fechar permite reabrir.

O atalho flutuante alterna entre o ícone de suporte (abrir) e um X vermelho (fechar), com tooltip e nome acessível acompanhando o estado. Clicar no X fecha a ajuda e restaura o ícone original.

### Ordenação da tabela de produtos — 21/10/2026
Cabeçalhos ordenáveis exibem dois chevrons neutros; a coluna ativa mostra um único chevron crescente/decrescente. Tooltip e texto acessível acompanham a direção. Medida (UN constante) e o preenchimento vazio não mostram o indicador. Preservados o template nativo e os controles de redimensionamento. Detalhes em specs/2026-10-21-ordenacao-produtos.md.

### Janelas de operação e hover — 21/10/2026
A área de trabalho abaixo da Ribbon mantém várias janelas internas simultâneas de Mesas / comandas, Produtos, Histórico de vendas, Cozinha e Backup no modo de recuperação. Abrir novamente uma tela traz sua janela existente para a frente. Fechar remove somente aquela janela e preserva as demais e o estado dos modelos na sessão; bloqueio e saída/troca de perfil retiram todas as janelas da área. Título permite mover; quatro bordas e quatro cantos permitem redimensionar. Tamanho normal inicial de 1180 × 600 DIP, limitado à área disponível, com mínimo físico de 640 × 360 DIP quando houver espaço. Conteúdo usa uma área finita com rolagem quando necessário. Título ativo usa `RibbonHover`; títulos em segundo plano usam `RibbonBar`.

Maximizar ocupa toda a largura e altura da área de trabalho, sem margens ou tetos, preservando Ribbon e rodapé. Restaurar recupera a geometria normal; redimensionar o sistema mantém as janelas dentro da área disponível. Ctrl+F6 e Ctrl+Shift+F6 alternam a janela ativa; Ctrl+F4 fecha e Ctrl+F10 maximiza/restaura. Janelas nativas filhas redimensionáveis da janela principal maximizam no retângulo da área de trabalho convertido para a tela e limitado à área útil do monitor; outros proprietários usam seu retângulo completo. A janela principal, a tela pública de senhas e a ajuda fixa ancorada ficam fora dessa regra.

O recurso dinâmico `HoverOverlay` acrescenta uma camada neutra sobre botões e seletores: preto translúcido no claro (`#16000000`) e branco translúcido no escuro (`#26FFFFFF`). Os botões locais do atendimento usam o mesmo recurso. Controles desabilitados não mostram a camada; foco e pressionamento mantêm seus estados próprios. Barras sempre escuras de categorias, rodapé e título da janela interna usam a camada branca nos dois temas. Esta extensão preserva o sistema Windows inspirado no Consumer, a marca, a tipografia e as demais cores existentes. Comportamento, escopo e evidências em specs/2026-10-21-janelas-hover.md.

### Grade de comandas — 21/10/2026
Cartões distribuem a largura útil em colunas iguais, com referência de 144 DIP e mínimo de 128 DIP por coluna, sem reservar uma coluna vazia à direita. Linhas de 144 DIP, cartões de 136 DIP de altura e margens de 4 DIP. A grade tem barra vertical visível e rolagem horizontal desabilitada; mensagem vazia não reserva espaço no rodapé. Sem comanda em edição, a grade acompanha a moldura e usa sua própria rolagem. A edição mantém mínimo lógico de 760 × 540 DIP e rolagem externa para preservar o formulário. Detalhes e evidência dos 287 testes desktop em specs/2026-10-21-janelas-hover.md.

### Ícones centralizados — 21/10/2026
Os 21 vetores coloridos compartilham quadro transparente de 32 × 32 unidades e contorno pintado centralizado em até 28 × 28, incluindo a espessura dos traços e margem segura de 2 unidades. Escala uniforme preserva proporções e orientação do desenho. A Ribbon usa linhas fixas de 32 DIP para ícone e rótulo, mantendo os ícones no mesmo eixo mesmo quando o texto quebra linha. Ícones locais e os de ajuda recebem alinhamento horizontal e vertical centralizado; os da ajuda usam 22 × 22 DIP. Marca e cores permanecem; lápis e chave conservam suas diagonais intencionais. Evidência dos 289 testes desktop e capturas em specs/2026-10-21-icones-centralizados.md.

A engrenagem de Configurações usa oito dentes simétricos, anéis concêntricos em cinza azulado e apresentação de 32 DIP na Ribbon, mantendo o quadro seguro compartilhado; esta correção específica substitui o desenho anterior sem alterar os demais ícones.

### Backup e pastas sincronizadas — 21/10/2026

A superfície de Backup segue o modo Operate e a organização da referência Consumer fornecida: configuração à esquerda, restauração à direita e separador fino entre as áreas. Google Drive e Dropbox têm cartões independentes com ícones vetoriais centralizados, pasta, situação e ações Configurar/Desconectar pasta. Os quatro novos vetores (Google Drive, Dropbox, Pasta e Fotos) seguem o mesmo quadro seguro dos demais ícones. A pasta local fica acima dos cartões. Usa Segoe UI de 12 DIP no corpo e títulos de 18 DIP, cantos retos, bordas finas, os recursos neutros dinâmicos `AjudaCanvas`, `AjudaSurface`, `AjudaInk`, `AjudaMuted` e `AjudaLine`, e ações verdes `AjudaAction`/`AjudaActionInk`. Esta extensão é específica da tela de Backup.

O corpo tem rolagem para preservar a composição em 760 × 440 DIP; mensagens e a ação Fazer backup agora ficam no rodapé fixo. Em recuperação, o histórico abre expandido antes da configuração e o rodapé oferece Restaurar dados. Histórico vazio usa uma mensagem compacta, sem reservar a altura da lista. Restaurar dados e Restaurar somente fotos têm descrições distintas; atividade bloqueia interações concorrentes e fechamento durante criação manual ou restauração de fotos.

Configurar Google Drive ou Dropbox significa selecionar uma pasta já sincronizada pelo aplicativo instalado neste computador. Exibir cópia gravada na pasta e avisos de indisponibilidade; não representar autenticação, upload confirmado ou sincronização concluída. As preferências ficam fora do banco para permitir recuperação com banco corrompido. Comportamento, limites, fontes e evidência dos 706 testes e oito capturas em specs/2026-10-21-backup-sincronizado.md. Documentação anterior e formato legado preservados; esta extensão não representa aprovação do restante do sistema.

### Cabeçalhos dos modais — 21/10/2026

As janelas auxiliares WPF compartilham o cabeçalho solicitado na referência do usuário: barra cinza-escura `#484644` de 30 DIP, título branco à esquerda e fechar à direita, sem ícone ou nome da marca no cabeçalho. Segoe UI de 12 DIP seminegrito, margem esquerda de 10 DIP, moldura cinza de 1 DIP e cantos retos. Botões de título medem 42 × 30 DIP, com símbolos vetoriais; fechar usa hover vermelho e foco de teclado visível. Maximizar/restaurar aparece somente nas janelas que já permitem redimensionamento. Títulos acompanham a propriedade da janela e usam reticências quando necessário; o título genérico Varthex Comanda passa a Aviso, e Ativação — Varthex Comanda passa a Ativação.

O padrão altera a moldura, preservando conteúdo, tema do corpo, contexto de dados, confirmações de fechamento e regras existentes de maximização. A janela principal e a tela pública de senhas ficam fora dessa aplicação; a marca presente no conteúdo da ajuda permanece. A evidência inclui 293 testes desktop e quatro capturas completas nos dois temas. Fontes, estados e limites em specs/2026-10-21-cabecalhos-modais.md; a documentação não representa revisão global da interface.

### Backup — progresso e OneDrive — 21/10/2026

Esta extensão substitui as regras anteriores de Dropbox e histórico visível na tela de Backup. Os cartões passam a ser Google Drive e OneDrive, mantendo a composição neutra, Segoe UI de 12 DIP, ações verdes e cabeçalho compartilhado de 30 DIP. OneDrive usa uma nuvem vetorial azul original, centralizada no quadro seguro comum. Ao lado do aviso de restauração aparece um losango de contorno vermelho e fundo branco de 24 DIP. Backups recentes, tabela e restauração por seleção de registro deixam de aparecer; a restauração continua pelo seletor de arquivo.

Fazer backup agora troca o conteúdo da mesma janela para uma página de progresso, com estado no topo, etapas reais com horário de Brasília e caminhos efetivamente gravados, rolagem automática e Voltar no rodapé. Pontos pulsam por opacidade somente quando as animações de área de cliente do Windows estão habilitadas; conclusão usa ícone de sucesso, aviso ou falha. Avisos no registro usam seminegrito e texto neutro para preservar contraste. Não há percentual estimado nem espera artificial. Voltar e fechamento ficam bloqueados durante a execução; ao concluir, Voltar retorna à configuração e uma nova execução limpa o registro anterior.

OneDrive tem preferência própria; a antiga chave Dropbox não é reinterpretada. O usuário seleciona sua pasta já sincronizada, sem promessa de upload concluído. A criação continua local primeiro, com cópias e preservação dos arquivos existentes. Comportamento, limites e evidência de 714 testes e 14 capturas em specs/2026-10-21-progresso-backup-onedrive.md. As seções anteriores permanecem como registro; esta extensão não declara revisão global da interface.
