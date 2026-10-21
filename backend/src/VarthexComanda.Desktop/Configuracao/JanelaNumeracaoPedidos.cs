using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaNumeracaoPedidos : Window
{
    private readonly NumeracaoPedidosService _service;
    private readonly ConfiguracaoNumeracaoPedidos _configuracao;
    private readonly TextBox _inicio;
    private readonly CheckBox _ativa;
    private readonly CheckBox _senha;
    private readonly TextBlock _mensagem;
    private readonly TextBlock _ultimo;
    private readonly StackPanel _opcoes;
    private bool _reiniciar;
    private bool _salvou;
    public bool TemAlteracoes => !_salvou && (_reiniciar || _ativa.IsChecked != _configuracao.Ativa
        || _senha.IsChecked != _configuracao.UsarComoSenhaAtendimento || _inicio.Text != _configuracao.Inicio.ToString(CultureInfo.InvariantCulture));

    public JanelaNumeracaoPedidos(NumeracaoPedidosService service)
    {
        _service = service;
        _configuracao = service.Carregar();
        Title = "Numeração de pedidos";
        Width = 560;
        Height = 440;
        MinWidth = 470;
        MinHeight = 410;
        ResizeMode = ResizeMode.CanResize;
        DialogoCompacto.Aplicar(this);

        var raiz = new DockPanel { Margin = new Thickness(20, 16, 20, 12) };
        raiz.SetResourceReference(Panel.BackgroundProperty, "Canvas");
        var superficie = new Border { Child = raiz };
        superficie.SetResourceReference(Border.BackgroundProperty, "Canvas");
        Content = superficie;
        var rodape = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 8, 0, 0) };
        var voltar = new Button { Content = "Voltar", IsCancel = true, MinWidth = 94, Margin = new Thickness(0, 0, 10, 0) };
        var salvar = new Button { Content = "Salvar", MinWidth = 100 };
        salvar.Click += (_, _) => Salvar();
        rodape.Children.Add(voltar);
        rodape.Children.Add(salvar);
        DockPanel.SetDock(rodape, Dock.Bottom);
        raiz.Children.Add(rodape);
        var conteudo = new StackPanel();
        raiz.Children.Add(new ScrollViewer { Content = conteudo, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });

        _ativa = new CheckBox { Content = "Utilizar numeração personalizada para pedidos", IsChecked = _configuracao.Ativa, Margin = new Thickness(0, 0, 0, 12) };
        conteudo.Children.Add(_ativa);
        _opcoes = new StackPanel();
        conteudo.Children.Add(_opcoes);
        _inicio = new TextBox { Text = _configuracao.Inicio.ToString(CultureInfo.InvariantCulture), MaxLength = 9, TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 3, 0, 10) };
        System.Windows.Automation.AutomationProperties.SetName(_inicio, "Começa com");
        _opcoes.Children.Add(new Label { Content = "Começa com", Target = _inicio, Padding = new Thickness(0) });
        _opcoes.Children.Add(_inicio);
        _senha = new CheckBox { Content = new TextBlock { Text = "Usar o código como senha de atendimento no final do comprovante da conta", TextWrapping = TextWrapping.Wrap }, IsChecked = _configuracao.UsarComoSenhaAtendimento, Margin = new Thickness(0, 0, 0, 10) };
        _opcoes.Children.Add(_senha);
        _opcoes.Children.Add(new TextBlock { Text = "Módulo aplicável", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 3) });
        _opcoes.Children.Add(new CheckBox { Content = "Mesas / comandas", IsChecked = true, IsEnabled = false, Margin = new Thickness(0, 0, 0, 10) });
        conteudo.Children.Add(new TextBlock { Text = "Número atual (último criado)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 3) });
        var linhaUltimo = new StackPanel { Orientation = Orientation.Horizontal };
        _ultimo = new TextBlock { Text = _configuracao.UltimoCriado > 0 ? _configuracao.UltimoCriado.ToString(CultureInfo.InvariantCulture) : "Nenhum pedido numerado", VerticalAlignment = VerticalAlignment.Center, MinWidth = 175 };
        linhaUltimo.Children.Add(_ultimo);
        var reiniciar = new Button { Content = "Limpar / reiniciar", Margin = new Thickness(0, 0, 0, 4) };
        reiniciar.Click += (_, _) =>
        {
            if (!new Atendimento.JanelaConfirmador().Confirmar("Reiniciar numeração?", "A sequência será reiniciada ao salvar. O histórico será preservado e os códigos já utilizados não serão repetidos. Deseja continuar?")) return;
            _reiniciar = true;
            _ultimo.Text = "Reinício pendente · salve para aplicar";
        };
        linhaUltimo.Children.Add(reiniciar);
        conteudo.Children.Add(linhaUltimo);
        var explicacao = new TextBlock { Text = "Vale para novos pedidos. Os números das comandas e dos pedidos existentes são preservados.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
        explicacao.SetResourceReference(TextBlock.ForegroundProperty, "Text59665E");
        conteudo.Children.Add(explicacao);
        _mensagem = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
        _mensagem.SetResourceReference(TextBlock.ForegroundProperty, "TextA12822");
        conteudo.Children.Add(_mensagem);
        _ativa.Click += (_, _) => _opcoes.IsEnabled = _ativa.IsChecked == true;
        _opcoes.IsEnabled = _ativa.IsChecked == true;
        Closing += (_, e) =>
        {
            if (TemAlteracoes && !new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?", "Há alterações de numeração não salvas. Deseja descartá-las e fechar?")) e.Cancel = true;
        };
    }

    private void Salvar()
    {
        if (!long.TryParse(_inicio.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) || numero is < 1 or > NumeracaoPedidosService.LimiteNumero)
        {
            _mensagem.Text = "Informe um número inicial de 1 a 999.999.999.";
            _inicio.Focus();
            _inicio.SelectAll();
            return;
        }
        try
        {
            _configuracao.Inicio = numero;
            _configuracao.Ativa = _ativa.IsChecked == true;
            _configuracao.UsarComoSenhaAtendimento = _senha.IsChecked == true;
            if (_reiniciar) _service.Reiniciar(_configuracao, DateTime.UtcNow);
            else _service.Salvar(_configuracao, DateTime.UtcNow);
            _salvou = true;
            if (IsVisible) DialogResult = true;
        }
        catch (Exception ex)
        {
            _mensagem.Text = ex is ArgumentException or InvalidOperationException ? ex.Message : "Não foi possível salvar a numeração. Tente novamente.";
        }
    }
}
