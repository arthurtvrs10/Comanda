# Comanda, por Varthex — tela principal

## Notinhas térmicas — 22/10/2026

Modelos reais em `Configuracao/CupomTermico.cs`: conta do cliente, pedido da cozinha, reimpressão identificada, fichas de consumação e teste de impressora. Impressão em preto e branco; cabeçalho compacto com nome do estabelecimento, número do CNPJ sem rótulo e endereço compacto, apenas quando preenchidos e habilitados. Não imprimir razão social, telefone ou campos vazios. Logo preto Comanda por Varthex e assinatura Sistema de gestão no rodapé.

Conta agrupa itens conforme a preferência, apresenta produto, quantidade/unidade, preço unitário, subtotal, observações opcionais e total destacado. Cozinha não mostra valores, destaca quantidades e observações e preserva o envio somente dos itens selecionados/novos; reimpressão tem aviso próprio e não avança os contadores. Fichas mantêm uma ficha por unidade ou uma por quantidade pesada. Não alterar registros de impressão antes da confirmação de envio à fila.

Papel de 58 ou 80 mm por impressora, persistido nas configurações; instalações anteriores usam 80 mm. A prévia permite comparar os tamanhos sem alterar a preferência salva. A área útil informada pelo driver limita a largura do documento e das colunas. O driver continua responsável pela paginação física e corte; o aplicativo não envia comandos ESC/POS. Configurações → Ajustes de cupom → Visualizar exemplo do cupom permite selecionar os cinco modelos com dados de exemplo. Não enviar impressão automaticamente ao abrir a prévia.

## Identidade aplicada — 22/10/2026

### Histórico de vendas no padrão de pedidos

Os resumos no rodapé dos dois históricos ficam alinhados à esquerda. Na categoria RELATÓRIOS, os botões Histórico de pedidos e Histórico de vendas compartilham um único grupo Vendas, sem separador entre os dois.

Tabela única com largura completa, linhas de 34 DIP e cabeçalhos, cores e alinhamento compartilhados em `Atendimento/EstilosHistorico.xaml`. Barra de ações compacta com data, número da comanda, Atualizar, Mais colunas, Exportar Excel, Imprimir e Detalhes. A barra quebra linha em janelas estreitas. Auto ajustar colunas alterna larguras proporcionais e fixas; colunas podem ser reordenadas e ocultadas sem ocultar todas.

Detalhes abrem por botão, duplo clique na linha ou Enter, em modal com número real da comanda e itens. Preservar Ctrl+F e a consulta de vendas concluídas por dia. Rodapé distingue registros exibidos dos indicadores do dia (quantidade, total e ticket médio); filtro de número não recalcula os indicadores diários. Exportação e impressão seguem filtro, ordenação e colunas visíveis; o impresso identifica o total exibido separadamente dos indicadores do dia. Nenhuma alteração nos registros de vendas ou regras de pagamento.

### Paleta operacional consolidada — revisão após auditoria

#### Ajuste solicitado: botões neutros, ícone branco e Relatórios

Navegação atualizada: Principal mantém Mesas / comandas. Histórico de pedidos e Histórico de vendas ficam em **Relatórios**. **Financeiro** permanece como categoria independente, sem histórico e sem comandos para funções financeiras ainda não implementadas.

Todas as categorias da barra superior são exibidas em caixa alta: PRINCIPAL, PRODUTOS, MARKETING, FINANCEIRO, RELATÓRIOS, APPS, CONFIGURAÇÕES e AJUDA. A transformação é somente visual; preservar os nomes internos, pesquisa, seleção e navegação.

Na grade, o corpo dos cartões permanece neutro em todos os estados. A identificação fica em uma faixa horizontal no topo, com o nome dentro: Livre em neutro, Normal em verde, Mediana em amarelo, Alta em vermelho e Em fechamento em amarelo (prevalece sobre a prioridade). Preservar os nomes de prioridade já usados na operação. Número, cliente, mesa e total usam o texto padrão do tema. A pesquisa continua destacando somente a borda do cartão. Não preencher todo o cartão com a cor de estado.

Esta decisão substitui o uso de botões laranja descrito abaixo: botões usam branco/#303336, texto #17252C/#F4F6F7, borda Line e hover #E8ECEF/#383C40. Toggles usam cinza próprio. A identidade continua no logo e o contraste das comandas abertas permanece corrigido. Ícone do aplicativo: símbolo laranja em fundo branco. A categoria Financeiro passa a se chamar **Relatórios**, abrigando o Histórico de vendas atual e permitindo organizar futuramente histórico de produtos, ranking de atendimentos e itens mais vendidos, sem criar comandos para relatórios ainda não implementados.

Esta seção prevalece sobre as cores de controles nas revisões anteriores. Fonte de verdade: `PaletaComanda.cs`, com papéis explícitos claro/escuro. Canvas #F4F5F6/#242628, Surface #FFFFFF/#303336, Ink #17252C/#F4F6F7, textos secundários #535D65/#BEC5CB, bordas #859099/#78838C. Manter o shell azul profundo #17252C e a imagem de fundo padrão já aprovada.

Ações primárias usam #FF5934 com texto #17252C nos dois temas; hover #FF795B. `BotaoPrimario` concentra o estilo de entrada, gravação de produto e confirmação de pagamento; densidades específicas permanecem locais. Seleção usa #FFE3D9/#53352D e texto #17252C/#FFF4EF. Comandas abertas usam laranja da marca e texto azul profundo (5,03:1). Verde é reservado para estados positivos; amarelo e vermelho preservam prioridade e urgência. Recursos Ajuda* de superfície são aliases da paleta comum; novas ações usam ActionPrimary/ActionPrimaryInk. A compatibilidade com recursos legados evita reintroduzir superfícies verdes.

Esta revisão substitui as referências anteriores à marca Varthex Comanda e à palavra VARTHEX como fundo. O produto se chama **Comanda**, com assinatura **por Varthex**. A matriz está em `brand/comanda-varthex/fontes/vetores/`; as entregas ficam em `brand/comanda-varthex/exportacoes/`.

Modo Operate: preservar Ribbon, posições dos comandos, perfis e cores semânticas de prioridade e erro. A marca usa laranja #FF5934, azul profundo #17252C e marfim #FFF9F1. Barra e rodapé em azul profundo; logo vetorial no fundo padrão, entrada e rodapé. Ajuda usa logo preto no tema claro e branco no escuro. Ações da ajuda usam azul profundo no claro e laranja com texto azul profundo no escuro. Manter fundos personalizados e a preferência de tema. Não acrescentar cabeçalho de marca acima da Ribbon.

O background padrão acompanha o toggle de tema: no escuro, fundo #17252C e marca central laranja/branca; no claro, fundo cinza original e marca laranja/azul. A troca não altera preferências salvas de imagem ou cor personalizada. Arquivos do fundo escuro em `brand/comanda-varthex/exportacoes/background/escuro/`.

Recursos vetoriais WPF ficam em Assets/MarcaComanda.xaml. O ícone multirresolução usa o símbolo laranja sobre azul profundo. Sincronizar os recursos com `brand/comanda-varthex/scripts/aplicar-desktop.py` quando as matrizes forem alteradas. Nomes técnicos de assemblies, caminhos de dados e contratos de licença permanecem compatíveis com instalações existentes.

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

### Menu da comanda — 21/10/2026

A edição da comanda segue a organização desktop Consumer fornecida pelo usuário: número e situação no topo de 54 DIP, lateral de 210 DIP com cliente, Mais opções, autoria, responsável, observação e bloqueio, e lista de itens à direita. Cabeçalho da lista de 38 DIP e rodapé de 58 DIP com Imprimir e PAGAMENTO. Produtos abre sob demanda sobre o painel de itens; Esc retorna ao pedido, Ctrl+F foca a busca, Enter adiciona o primeiro resultado e F4 abre o pagamento. A janela nativa inicia em 900 × 740 DIP, respeita a área útil, tem mínimo de 800 × 580 DIP e usa as regras compartilhadas de redimensionamento/maximização e o cabeçalho cinza sem marca de `AparenciaJanelaModal`.

Esta superfície estende os recursos neutros dinâmicos `AjudaCanvas`, `AjudaSurface`, `AjudaInk`, `AjudaMuted`, `AjudaLine`, `AjudaSoft` e `AjudaHover`, com ação verde `AjudaAction`/`AjudaActionInk`, Segoe UI, cantos retos e ícones `Path` de 20 DIP alinhados ao centro dos controles. Os menus compactos têm divisores, submenus, foco de teclado e estados desabilitados. A referência orienta composição e hierarquia; marca e ativos Consumer não são incorporados.

Mais opções reúne troca de comanda, conta resumida, transferência/cópia com seleção e confirmação, novas fichas, rascunho no WhatsApp, recálculo pelos preços lançados, dados do pedido e exclusão confirmada. Imprimir reúne conta, conta e bloqueio, novos itens de cozinha e reimpressão. Bloqueio protege a alteração dos itens e permite pagar e imprimir. Autoria, responsável, observação, bloqueio e contadores separados de cozinha/fichas são persistidos; uma prévia fechada sem envio aceito não consome pendências. A seleção após adicionar usa o ID exato do item alterado, e Voltar/pagamento salvam a observação antes de sair, preservando o texto em caso de falha.

Comportamento, adaptações e limites em specs/2026-10-21-menu-comanda.md. Evidência: 730 testes aprovados e 14 capturas controladas nos dois temas. A revisão independente retornou `ship` para os dois achados corrigidos nesta passagem; não é aprovação global, nem comprovação de moldura nativa, impressão física ou envio real no WhatsApp. O formato legado e as demais seções deste arquivo permanecem preservados.

### Prioridades, localizador de produtos e backup modal — 21/10/2026

Esta extensão substitui o catálogo sobreposto e a página de progresso embutida descritos nas seções anteriores. O número da comanda e a situação Em aberto usam a prioridade: Normal verde (`PrioridadeNormal`, #2D8050), Mediana amarela (`PrioridadeMediana`, #F2CB59) e Alta vermelha (`PrioridadeAlta`, #B6322C). O número usa variantes próprias para contraste em cada tema; a situação mantém texto explícito e abre as três opções ao clicar. O código do pedido aparece separado e em negrito, como #102. Os rótulos Normal/Mediana/Alta representam os valores persistidos Normal/Alta/Urgente, sem migração. Bloquear pedido e Exibição completa passam a alternâncias compactas com trilho de 38 × 20 DIP e pino de 14 DIP, foco visível, hover e estado desabilitado; o bloqueio conserva sua persistência e regras.

Produtos abre Localizar produto em janela filha modal de 900 × 740 DIP, mínimo de 740 × 520 DIP, com cabeçalho cinza compartilhado sem marca, recursos neutros `Ajuda*`, Segoe UI de 12 DIP e cantos retos. Busca, categorias reais na lateral de 172 DIP, Todas, Atualizar e alternância Lista/Fotos organizam o catálogo. A lista usa linhas e cabeçalho de 42 DIP, conteúdo centralizado verticalmente e nome elástico com reticências e tooltip. Fotos usa quadros internos de 156 × 238 DIP, imagem existente do catálogo ou placeholder vetorial. F11 adiciona uma unidade, Enter personaliza quantidade e observação, Ctrl+F focaliza a busca e Esc/Voltar retorna ao pedido ativo. A referência Consumer orienta a composição, sem incorporar marca ou ativos externos.

Fazer backup agora e o atalho Criar backup abrem Progresso do backup como modal separado, de 660 × 510 DIP com mínimo de 460 × 340 DIP, sobre a configuração preservada. A barra é indeterminada durante a execução; no término fica completa em sucesso/aviso e vazia em falha, acompanhada do resultado textual e das etapas reais. Não exibir percentual estimado. Voltar e fechamento permanecem bloqueados durante o trabalho; após o término, fechar retorna à configuração. O progresso de cópia em pasta sincronizada não comprova upload à nuvem.

Comportamento, adaptações, fontes e limites em specs/2026-10-21-prioridades-localizador-modal-backup.md. Evidência desta extensão: 307/307 testes Desktop, 28 capturas controladas conferidas individualmente e revisão independente `ship` sem achados materiais no escopo. As demais suítes não foram reexecutadas nesta passagem; as seções e o formato legado permanecem preservados, sem aprovação global da interface.

### Cartões com fotos e Personalizar item — 21/10/2026

Esta extensão substitui somente os cartões de 156 × 238 DIP e o formulário de personalização anteriores. Localizar produto abre em Fotos por padrão; cada cartão é um único botão de 156 × 190 DIP, com imagem de 100 DIP de altura, código, nome em até duas linhas com tooltip e preço. Uma borda, hover e foco de teclado delimitam a ação inteira. Clicar abre Personalizar item; o cartão não contém Adicionar nem reticências. A lista preserva suas ações rápidas, e a ação Personalizar abre a mesma janela.

Personalizar item usa 880 × 620 DIP, mínimo de 700 × 440 DIP, cabeçalho cinza compartilhado sem marca, Segoe UI de 12 DIP e recursos neutros `Ajuda*`. Quantidade inteira de 1 a 999 UN com botões menos/mais e campo editável, total, observação de até 300 caracteres, Cancelar e Adicionar item verde. Complementos informa a ausência de cadastro no catálogo atual. Cancelar ou fechar não lança produto nem altera a observação pendente; confirmar usa o lançamento existente, seleciona o item exato e limpa a observação somente após sucesso. Detalhes, cinco testes aprovados, 12 capturas válidas nos dois temas e limites em specs/2026-10-21-cartoes-fotos-personalizar-item.md. A suíte anterior de 307 testes não foi reexecutada nesta extensão; o restante do documento permanece como registro.

### Localizador — lista e cartão conforme o modelo — 21/10/2026

Refinamento restrito ao localizador WPF, seguindo as duas referências Consumer fornecidas e preservando Operate, Segoe UI de 12 DIP e neutros `Ajuda*`. A lista mantém cabeçalho e linhas de 42 DIP; seleção azul `#1969CE` com texto branco, preço em negrito e colunas Categoria, Cód., Nome do Produto, Preço de Venda, Adicionar (F11) e Personalizar (Enter). Ações usam áreas de 40 × 38 DIP, círculos vetoriais de 18 DIP e símbolos centralizados, em `#487FAF` ou `#CAE5FF` na linha selecionada. Preços do localizador usam duas casas em pt-BR sem `R$`.

Esta extensão substitui o cartão anterior de 156 × 190 DIP por 124 × 154 DIP: imagem de 66 DIP, código à esquerda em faixa de 20 DIP, nome centralizado em negrito de até duas linhas com tooltip e preço centralizado na faixa inferior de 26 DIP. Fotos continua sendo o modo inicial; o cartão inteiro abre Personalizar item antes do lançamento, sem mudar esse formulário. Fotos reais do cadastro, placeholder neutro e ações da lista permanecem. Fontes, cinco testes aprovados, oito capturas revistas nos dois temas e limites em specs/2026-10-21-localizador-modelo-consumer.md. A lista larga e a suíte global anterior não foram revalidadas nesta passagem; o registro não representa aprovação global. Ao concluir, a versão atualizada foi aberta na tela principal, aguardando desbloqueio pelo usuário.

### Comanda em fechamento — 22/10/2026

O estado interno `Bloqueada` passa a aparecer como Em fechamento no cabeçalho, na alternância lateral e no cartão da grade. Cabeçalho e cartão usam amarelo `PrioridadeMediana` (#F2CB59) com texto #302500; o número usa `PrioridadeNumeroMediana` para contraste em cada tema. O fechamento prevalece visualmente sobre a prioridade, inclusive Alta, sem alterar o valor persistido. O menu oferece Imprimir conta e iniciar fechamento. Mantidos Operate, Segoe UI, cantos retos, temas e composição existentes.

A alternância salva a observação pela guarda existente e aguarda a escrita do fechamento fora da linha da interface, mostrando Salvando fechamento… ou Reabrindo comanda… e impedindo ações concorrentes, pagamento e saída até concluir. Sucesso atualiza somente os dados da comanda, seu cartão observável e sua entrada na lista aberta, preservando itens e seleção; falha mantém o estado anterior e restaura a alternância. O fechamento concluído continua impedindo alterações nos itens e permitindo pagar e imprimir. Detalhes, 11/11 testes focados, dez capturas válidas nos dois temas e revisão independente `ship` restrita em specs/2026-10-22-fechamento-comanda.md. Não há nova execução da suíte global nem medição de latência em hardware; documentação e formato legado preservados.

### Convite Google Drive e medidas — 22/10/2026

O convite de conexão Google estende o modo Operate: modal central de 450 × 560 DIP, mínimo declarado de 410 × 510, sem redimensionamento, cabeçalho cinza compartilhado de 30 DIP sem marca, Segoe UI de 12 DIP e título interno de 23 DIP seminegrito. Vetor Drive centralizado de 64 DIP, neutros dinâmicos `Ajuda*`, cantos retos, corpo rolável e ação principal verde. Configurar depois permanece no rodapé e libera após cinco segundos monotônicos; X e Alt+F4 respeitam a mesma guarda. Falta de credenciais e falhas de autorização têm texto explícito. Esta superfície não introduz novo ativo raster nem uma nova identidade visual.

Cadastro e edição colocam Unidade (UN), Quilo (KG) e Litro (L) na mesma linha junto à categoria, e o preço informa a medida escolhida. Personalizar item preserva o modal de 880 × 620 DIP, mínimo de 700 × 440, os neutros e a ação verde; quantidade apresenta inteiros em UN e três casas decimais em KG/L. Esta extensão substitui a descrição anterior de quantidade sempre inteira e o fluxo Google por pasta sincronizada. OneDrive continua como pasta sincronizada; o cartão Google passa a representar conexão direta autorizada.

Detalhes, dez capturas atuais nos dois temas e limites em specs/2026-10-22-drive-oauth-medidas-venda.md. Revisão independente `ship` restrita à extensão visual, capturas e código amostrado, sem OAuth/upload com conta real. Formato legado de DESIGN.md preservado; ausência prévia de PRODUCT.md e `.impeccable/design.json` não foi reparada nesta extensão.

### Remoção da tela inicial — 22/10/2026

Esta extensão conclui a entrada direta já descrita neste documento: remove do projeto a tela antiga de cartões Garçom, Cozinha, Gestor e acessos rápidos, além do host residual `MenuInicial`. A tela já não era instanciada pelo código atual; excluir `InicioView.xaml` e seu code-behind evita empacotar o recurso órfão. Iniciar e retornar ao menu usam a área principal com Ribbon em Principal e canvas neutro, preservando Consumer, WPF, Operate, Segoe UI, temas e janelas internas. Não há novo componente, paleta ou ativo.

O retorno do bloqueio passa a dizer Voltar ao menu. Voltar mantém o acesso bloqueado; Mesas / comandas e Cozinha continuam exigindo PIN e os destinos administrativos exigem acesso de gestor. F1/F2/F3 e a consulta da cozinha permanecem. Comportamento, cinco capturas e resultado focado de 2/2 testes em specs/2026-10-22-remocao-tela-inicial.md. Revisão independente `ship` restrita à remoção e preservação desses fluxos. A suíte Desktop completa de 317 testes não foi reexecutada nesta extensão; instalador, versão instalada, demais módulos e desvios documentais preexistentes ficam fora da validação.

### Fundo padrão VARTHEX — 22/10/2026

A área de trabalho vazia passa a usar cinza neutro `#F2F2F2` com VARTHEX em `#858585`, centralizado pelos limites efetivamente pintados das letras. O fundo mantém essa aparência nos temas claro e escuro; a preferência da área de trabalho é independente dos controles. A imagem nativa WPF é um `DrawingImage` transparente de 1600 × 900 unidades, com geometria Segoe UI SemiBold de 100 unidades, exibida com escala uniforme e sem corte. Janelas internas permanecem acima. Esta extensão preserva o sistema Consumer, modo Operate, marca e composição existentes; o nome no fundo não redefine o logotipo.

Novas preferências usam a cor neutra. O antigo `#F3F5F2` sem imagem recebe o novo padrão; ao abrir a seção Imagem de fundo, a preferência do formulário é normalizada para salvar esse valor. Imagens personalizadas têm precedência, conservam as posições existentes e mantêm a cor anterior; outras cores personalizadas são preservadas. Restaurar padrão limpa a imagem, repõe a cor neutra e seleciona Centralizar. Detalhes, cinco capturas atuais, 3/3 testes Desktop focados, 3/3 de preferências da aplicação e uma execução adicional de captura aprovada em specs/2026-10-22-fundo-padrao-varthex.md. Revisão independente `ship` restrita à extensão; suíte global, instalador e versão instalada não foram revalidados. Formato legado e ausências documentais preexistentes permanecem preservados.

### Alternância de tema e documentação da release 1.4.0 — 22/10/2026

Modo escuro reutiliza `AlternanciaCompacta` no cabeçalho principal: trilho de 38 × 20 DIP, pino de 14 DIP, área mínima de 32 DIP de altura, Segoe UI de 12 DIP e rótulo branco. Permanecem foco visível, hover, estado desabilitado e recursos dinâmicos existentes; o nome acessível é Modo escuro e a tooltip informa a persistência neste computador. Os eventos de marcar e desmarcar aplicam o tema; a inicialização respeita a preferência salva e a guarda de igualdade evita regravá-la. Não há novo componente, paleta ou sistema visual. O canvas padrão continua cinza neutro nos dois temas.

O README, em modo Read, passa a mostrar seis capturas nativas atuais: principal clara/escura, grade de comandas, comanda, produtos e histórico, com dados sintéticos e origem registrada. A release prepara edições normal e demonstração separadas. Evidências, procedência das imagens, 750 testes distintos aprovados em Release, instaladores e limites em specs/2026-10-22-release-1.4.0.md. A revisão independente retornou `ship` restrito à alternância, README e capturas, sem achados materiais; Windows limpo e OAuth com conta Google real permanecem pendentes. A publicação externa não é comprovada por este registro. Formato legado, demais seções e ausências documentais preexistentes permanecem preservados.

### Histórico de pedidos — 22/10/2026

Principal / Vendas recebe Histórico de pedidos como consulta própria, preservando o Histórico de vendas financeiro. A superfície estende o modo Operate e a composição Consumer fornecida pelo usuário, sem incorporar marca ou ativos externos: Segoe UI de 12 DIP, `Language="pt-BR"`, neutros dinâmicos `Ajuda*`, cantos retos e ícones vetoriais de 22 DIP na barra de ações. A barra quebra linha, o resumo do filtro usa reticências e tooltip, e a tabela somente leitura tem cabeçalho mínimo de 36 DIP, linhas de 34 DIP, seleção azul `#176BD1` com texto branco e rolagem nos dois eixos. Quantidade e total ficam sob a tabela; refletem os registros exibidos, incluindo abertos e cancelados quando selecionados, sem representar receita de vendas.

Pesquisar abre modal de 500 × 635 DIP, mínimo de 440 × 480, com situação, data de abertura/fechamento, período e horários, seis atalhos de período, cliente, colaborador, códigos e inclusão de cancelados. Mais colunas abre a árvore de 350 × 590 DIP, mínimo de 320 × 400, separada em Dados do cliente, Totais do pedido, Pagamento e Outras informações; Código permanece obrigatório. Agrupar por oferece Criado por, Origem, Tipo e Pagamento. Ambos reutilizam `DialogoCompacto` e o cabeçalho compartilhado sem marca, corpo rolável, Voltar e ação de confirmação. Auto ajustar colunas reutiliza `AlternanciaCompacta`; visibilidade, agrupamento e ajuste são preferências JSON locais, independentes do banco.

Abrir apresenta os dados e itens registrados em consulta; Abrir comanda só aparece para pedido aberto e retorna ao atendimento existente. Exportar Excel e Imprimir usam as colunas visíveis e a ordem exibida, ficam desabilitados sem resultados e conservam o filtro atual. Detalhes, fontes, 762 testes distintos aprovados em Debug, oito capturas controladas revisadas nos dois temas e limites em specs/2026-10-22-historico-pedidos.md. A revisão independente retornou `ship` sem achados materiais no histórico e nos dois modais; não houve impressão física nem nova publicação da pré-release 1.4.0. As capturas publicadas no README e os instaladores anteriores permanecem como registro daquela release. Formato legado e ausências documentais preexistentes preservados.

### Tela de abertura — 22/10/2026

Ao iniciar, Comanda apresenta uma janela transitória sem moldura de 760 × 490 DIP, centralizada e reduzida proporcionalmente para caber na área útil com margem total de 32 DIP. A textura abstrata fornecida pelo usuário preenche o quadro com corte proporcional, sob preto a 28% e gradiente inferior. O logo branco existente Comanda, por Varthex ocupa o centro em caixa de 360 × 100 DIP. Etapa real de 12 DIP e barra indeterminada de 210 × 3 DIP ficam no canto inferior esquerdo; a versão dinâmica usa Segoe UI Light de 50 DIP no canto inferior direito. A referência Consumer orienta somente a composição. A abertura estende o modo Operate e não redefine a identidade, a paleta dos controles ou o fundo da área de trabalho.

A apresentação aguarda somente o restante de um mínimo de 5 segundos contado desde ContentRendered; trabalho mais demorado não recebe outra espera. A janela fecha antes de licença, erro ou recuperação e, no caminho normal, logo após exibir a principal. Fechar manualmente cancela a inicialização. Não há novo fluxo de autenticação ou alteração das regras de negócio. A textura foi incorporada sem mudar os pixels, com procedência embutida; nenhum ativo foi gerado por ImageGen.

Detalhes, procedência, duas capturas WPF e limites em specs/2026-10-22-tela-abertura.md. Evidências: compilação do aplicativo com zero avisos e erros, 11/11 testes focados e 330/330 Desktop aprovados, além de abertura real até a tela principal 1.4.0. Os testes focados estão contidos na suíte Desktop; o log de testes conserva dois avisos preexistentes do analisador xUnit. Revisão independente retornou `ship` sem achados materiais no escopo. As capturas são renderizações WPF controladas; movimento, duração percebida e todos os caminhos de falha não foram observados visualmente. Versão, instaladores e publicação permanecem fora desta extensão. Formato legado, conteúdo anterior e ausências documentais preexistentes preservados.

### Atualização da pré-release 1.4.0 — 22/10/2026

Esta entrega consolida a identidade Comanda, por Varthex, a paleta operacional e o Histórico de vendas já registrados no início deste documento, junto ao Histórico de pedidos e à abertura de cinco segundos. Preservar o modo Operate, WPF, Segoe UI, cantos retos, temas, perfis e preferências existentes. Relatórios / Vendas reúne os dois históricos; suas barras quebram linha a 800 DIP e Produtos conserva rolagem em largura reduzida. A atualização não introduz um novo sistema visual nem redefine os tokens existentes. Fontes: MainWindow.Ribbon, TemaInterface, PaletaComanda, HistoricoView, EstilosHistorico, FundoSistema e JanelaAbertura.

O README em modo Read passa de seis a oito capturas WPF atuais: principal clara e escura, abertura, grade de comandas, comanda, produtos e os dois históricos. As imagens usam dados fictícios, têm procedência incorporada sem alteração de pixels e preservam os bytes das capturas de origem. Quatro capturas complementares cobrem a área de trabalho maximizada, os dois históricos a 800 DIP e Produtos em largura reduzida. A revisão independente retornou `ship`, sem achados materiais no README, nas 12 capturas inspecionadas e nas fontes amostradas.

Detalhes, mapeamento de imagens, logs, 770 testes distintos aprovados em Release e hashes dos instaladores normal e demonstração em specs/2026-10-22-atualizacao-release-1.4.0.md. Os 21 testes de captura repetem testes já incluídos nessa contagem; o teste inicial da espera de abertura falhou por arredondamento e os 330 Desktop passaram após a correção monotônica. A versão permanece 1.4.0 por escolha do usuário, com pacotes anteriores preservados localmente. Publicação externa ainda pendente neste registro. Windows limpo, impressão física, leitor de tela completo, múltiplos monitores/DPI e Google com conta real continuam fora da validação. Preservados o formato legado e os desvios preexistentes de PRODUCT.md e .impeccable/design.json.

## Configurações do sistema — redesenho

Navegação lateral de 222 DIP com ícones e seleção neutra, conteúdo em superfície própria, títulos de 25 DIP e descrições curtas. Campos de 36 DIP organizados por identificação, endereço e assinatura. Rodapé persistente com estado de edição separado das mensagens de erro e ação Salvar alterações. Visitar seções não altera preferências.

Opções booleanas usam toggles com texto Ativado/Desativado e foco de teclado. Segurança mostra o efeito da alternância em tempo real, informa as duas proteções obrigatórias (início e Bloquear) e oferece as ações existentes para alterar senha do gestor e PIN. Mudanças só persistem ao salvar. Seleções exclusivas e vínculos de produtos continuam usando rádio e seleção múltipla, respectivamente. Preservar temas claro/escuro, rolagem e largura mínima de 800 DIP.


## Consolidação de acesso e segurança — 22/10/2026

Esta decisão substitui os registros anteriores de PIN compartilhado e códigos de recuperação: base nova inicia na área de trabalho vazia sem senha; proteção opcional por toggle, cadastro imediato de senha e recuperação distintas e salvamento explícito. Todos os usuários acessam as áreas nesta entrega. Login sobre o sistema visível e desabilitado, blur de 5 DIP apenas na área de fundo; painel Surface, texto Ink e botão neutro compartilhado. Campos e placeholders centralizados; toggle Mostrar senha. Recuperação offline por senha com hash/salt; sem código ou Resend. Ajuda ❓ solicitada pelo usuário explica a recuperação por tooltip e foco de teclado. Segurança mantém apenas o toggle, resumo de estado e ações úteis após salvar. Diálogos usam densidade compartilhada, rolagem e limite pela área útil da tela.
