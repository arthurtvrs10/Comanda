# Produtos em janela interna — 21/10/2026

Referência observada no Consumer aberto: Produtos aparece em uma janela filha centralizada sobre o fundo, abaixo da Ribbon, com título, maximizar/restaurar e fechar.

O Varthex utiliza `JanelaInterna`, um controle WPF dentro da área de trabalho. Não cria outra janela do Windows para a listagem. O fundo e a imagem configurada continuam visíveis ao redor. Em modo normal, a janela tem até 1180 × 600 pixels lógicos e se adapta ao espaço disponível. Maximizar ocupa somente a área interna; restaurar recupera a posição anterior. Arrastar o título mantém a moldura dentro dos limites, inclusive após redimensionar o sistema.

Fechar remove apenas a janela de Produtos da área de trabalho; o formulário de pesquisa e a instância da listagem são preservados para a próxima abertura. Permissões, bloqueio e os fluxos atuais de cadastro e edição continuam em vigor. A janela não bloqueia a Ribbon.

Atalhos da janela filha: Ctrl+F4 fecha; Ctrl+F10 alterna maximizar/restaurar. Dois cliques no título também alternam o tamanho.

Verificação: testes de limites, arraste, restauração de posição, maximização, fechamento e reabertura nos dois temas; preservação da pesquisa na integração com a Ribbon; tamanhos de área de 1440 × 800 e 800 × 440. Capturas em artifacts/janela-interna/previas.
