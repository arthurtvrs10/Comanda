# Ícones centralizados — 21/10/2026

Modo: Operate. Ajuste de alinhamento e área segura dos ícones existentes, preservando identidade, cores e orientação dos vetores. Integra-se ao sistema WPF Windows atual e às janelas já documentadas.

## Quadro e desenho

`IconesVarthex` compõe os 21 desenhos coloridos em `DrawingGroup` congelado e os apresenta por `DrawingImage`. O contorno pintado, incluindo a espessura dos traços, recebe escala uniforme para caber em 28 × 28 unidades e fica centralizado em (16, 16), dentro de quadro transparente de 32 × 32. A margem segura é de 2 unidades; não há rotação para corrigir alinhamento. As diagonais intencionais, como lápis e chave, permanecem. O contorno do ícone de cozinha mantém a forma visível em superfícies claras.

As imagens são decorativas, centralizadas horizontal e verticalmente e não recebem cliques. `OnCreateAutomationPeer` retorna `null`, preservando a identificação acessível no botão e evitando um elemento decorativo adicional na navegação assistiva.

### Engrenagem de Configurações

A correção específica da engrenagem substitui seu contorno anterior por oito dentes simétricos, repetidos a cada 45 graus, com raios interno de 10,5 e externo de 14 unidades e centro em (16, 16). Usa preenchimento cinza azulado `#8194A3`, contorno `#536A7D` de 0,8 unidade e anéis concêntricos com `#E8EDF1`. A normalização considera o contorno pintado e mantém a área segura de 28 × 28. Na Ribbon, este ícone usa 32 DIP, enquanto os demais mantêm 30 DIP. Nenhum outro ícone foi alterado nesta correção.

## Aplicação nos controles

Os comandos da Ribbon usam duas linhas fixas de 32 DIP: ícone e rótulo. Botões mantêm altura de 78 DIP, preenchimento de 10 × 5 DIP e conteúdo centralizado. O eixo do ícone independe da quantidade de linhas do rótulo, que permanece dentro do botão.

Os caminhos locais foram centralizados nas ações de atendimento, edição/novo produto, colaboradores, impressoras, chamador de senhas e chave de assinatura. As ações de atendimento usam extremidades e junções arredondadas nos traços. Os ícones das seções de ajuda usam 22 × 22 DIP com alinhamento horizontal e vertical centralizado. As demais regras de forma, tema e interação dos controles permanecem.

## Evidência

A suíte desktop completa passou com 289 testes aprovados após este ajuste. `IconesAlinhamentoTests` renderiza os 21 desenhos em 16, 20, 30 e 32 DIP, nas escalas de Windows de 100%, 125%, 150% e 200%. Confere contorno visível centralizado com tolerância de 1 pixel e margem de pelo menos 1 pixel até o recorte. O teste da Ribbon percorre todas as categorias e confere eixo comum, centralização horizontal e encaixe de ícones e rótulos.

Capturas em `artifacts/icones-centralizados/`: `icones-catalogo.png`, `ribbon-configuracoes-claro.png`, `ribbon-configuracoes-escuro.png`, `produtos-lista.png` e `suporte-compacto-False.png`.

Após a correção da engrenagem, a suíte desktop passou novamente com 289 testes aprovados. Capturas atualizadas em `artifacts/engrenagem-corrigida/`: `icones-catalogo.png`, `ribbon-configuracoes-claro.png` e `ribbon-configuracoes-escuro.png`.

Fontes principais: `IconesVarthex.cs`, `MainWindow.Ribbon.cs`, `Atendimento/AtendimentoView.xaml` e as telas locais de produtos, colaboradores, impressoras, painel de senhas, assinatura e ajuda, sob `backend/src/VarthexComanda.Desktop/`. Documentação anterior e formato legado de `DESIGN.md` preservados; nenhum sidecar criado.
