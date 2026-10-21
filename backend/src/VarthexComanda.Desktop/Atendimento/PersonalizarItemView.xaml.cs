using System.Windows;

namespace VarthexComanda.Desktop.Atendimento;

public partial class PersonalizarItemView : Window
{
    public PersonalizarItemViewModel ViewModel { get; }
    public PersonalizarItemView(LocalizarProdutoViewModel catalogo, ProdutoLocalizado produto)
    {
        InitializeComponent();
        ViewModel = new(catalogo, produto);
        DataContext = ViewModel;
        Loaded += (_, _) => { CampoQuantidade.Focus(); CampoQuantidade.SelectAll(); };
    }
    private void Adicionar_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Adicionar()) DialogResult = true;
    }
}
