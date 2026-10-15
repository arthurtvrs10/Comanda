using System.Windows;

namespace VarthexComanda.Desktop.Atendimento;

public partial class ConfirmacaoView : Window
{
    public ConfirmacaoView(string titulo, string mensagem)
    {
        InitializeComponent();
        Title = titulo;
        TextoMensagem.Text = mensagem;
        if (titulo == "Cancelar comanda")
        {
            BotaoNao.Content = "_Manter aberta";
            BotaoSim.Content = "_Cancelar comanda";
            BotaoNao.Width = 185;
            BotaoSim.Width = 205;
        }
    }

    private void Sim_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Nao_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
