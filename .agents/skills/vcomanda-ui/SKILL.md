---
name: vcomanda-ui
description: Seleciona e integra bibliotecas de controles de UI desktop .NET no VComanda, com foco em compatibilidade WPF, temas e licenciamento. Use ao avaliar ou aplicar bibliotecas visuais no aplicativo desktop.
---

# Bibliotecas de UI desktop do VComanda

Esta skill trata exclusivamente de bibliotecas de controles para o aplicativo desktop .NET.

## Base do projeto

Leia as instruções aplicáveis, o projeto Desktop e seus estilos compartilhados antes de integrar componentes. A base observada é WPF em net10.0-windows, com CommunityToolkit.Mvvm. Confirme a configuração atual no arquivo de projeto. Preserve as regras visuais registradas em DESIGN.md ao integrar uma biblioteca.

## Catálogo e seleção

Consulte [references/bibliotecas.md](references/bibliotecas.md) para as nove opções apresentadas ao usuário, seus usos e fontes oficiais.

- Para o WPF atual: avalie WPF UI, Material Design in XAML e MahApps.Metro conforme os controles e o estilo necessários.
- Para grades, gráficos e editores empresariais: avalie as suítes WPF de DevExpress, Telerik e Syncfusion, verificando a licença disponível.
- Krypton Toolkit pertence ao WinForms; Windows Community Toolkit oferece controles para WinUI/Uno; FluentAvalonia pertence ao Avalonia. Mantenha-os como informação de compatibilidade, sem instalá-los no WPF nem migrar a plataforma por causa do catálogo.
- CommunityToolkit.Mvvm já presente não é uma biblioteca visual de controles.

Não instale todas as opções em conjunto. Selecione pela necessidade concreta e evite sobrepor bibliotecas completas de temas.

## Integração

Confira na documentação oficial a versão compatível, os pacotes corretos, os recursos de tema e as condições de uso e distribuição. Não invente APIs ou nomes de pacotes. Não compre licenças sem autorização.

Integre os controles selecionados aos recursos XAML, estilos, bindings e comandos existentes. Preserve foco, navegação por teclado, temas claro/escuro, comportamento em DPI e redimensionamento. Não altere cálculos, persistência ou regras de negócio como efeito colateral da troca de controles.

## Verificação

Compile o projeto Desktop após alterações de dependências ou XAML. Verifique as telas afetadas nos temas e estados pertinentes e execute testes existentes quando houver mudança de comportamento. Informe a biblioteca e versão adotadas, onde foram usadas, requisitos de licença e verificações realmente executadas. Se o pedido for somente de orientação ou configuração desta skill, não altere dependências nem telas.
