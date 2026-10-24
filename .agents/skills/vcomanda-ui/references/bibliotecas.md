# Bibliotecas de UI e compatibilidade

Catálogo para avaliação, não lista de pacotes obrigatórios. Verifique suporte e termos atuais na fonte oficial antes de recomendar uma adoção concreta.

| Biblioteca | Plataforma | Uso a avaliar | Fonte oficial |
| --- | --- | --- | --- |
| WPF UI | WPF | Navegação, controles e aparência Fluent | https://github.com/lepoco/wpfui |
| Material Design in XAML | WPF | Componentes e temas Material | https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit |
| MahApps.Metro | WPF | Janelas, controles e temas Metro | https://github.com/MahApps/MahApps.Metro |
| DevExpress | WPF e WinForms, suítes próprias | Grades e controles empresariais | https://www.devexpress.com/ |
| Telerik UI | WPF, WinForms e WinUI, suítes próprias | Grades, documentos e controles empresariais | https://www.telerik.com/products/wpf/overview.aspx |
| Syncfusion | WPF, WinForms, WinUI e MAUI, suítes próprias | Componentes empresariais e visualização de dados | https://www.syncfusion.com/ |
| Krypton Toolkit | WinForms | Controles e temas WinForms | https://github.com/Krypton-Suite/Standard-Toolkit |
| Windows Community Toolkit | WinUI e Uno, conforme pacote | Controles e utilitários dessas plataformas | https://learn.microsoft.com/en-us/dotnet/communitytoolkit/windows/ |
| FluentAvalonia | Avalonia | Controles e aparência Fluent em Avalonia | https://github.com/amwx/FluentAvalonia |

## Critério para o VComanda

O desktop atual é WPF. Krypton, FluentAvalonia e controles WinUI não são substitutos diretos de controles WPF. CommunityToolkit.Mvvm já usado pelo projeto oferece infraestrutura MVVM; não equivale à biblioteca de controles Windows Community Toolkit.

Priorize estilos e controles WPF existentes. Considere WPF UI, Material ou MahApps se houver necessidade concreta compatível com a direção visual. Não misture sistemas completos de temas para obter um único detalhe.

Avalie DevExpress, Telerik ou Syncfusion se um controle complexo justificar o custo e o acoplamento. Suítes comerciais exigem conferir direitos de desenvolvimento e distribuição; eventuais licenças gratuitas dependem dos termos e da elegibilidade. Não assuma que avaliação gratuita autoriza distribuir em produção.

Antes de adicionar um pacote, registre: lacuna atual, controle escolhido, suporte ao framework alvo, tema claro/escuro, teclado e acessibilidade, impacto no instalador e operação offline, licença e alternativa com recursos existentes. Evite impor uma migração arquitetural por preferência estética.
