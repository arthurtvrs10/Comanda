using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Desktop;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Atendimento.Prototipos;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Time;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try { Executar(); }
        catch(Exception ex) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"erro-prototipo.log"),ex.ToString()); }
    }
    private static void Executar()
    {
        var app=new App(false);app.InitializeComponent();TemaInterface.Aplicar(false,false);
        var pasta=Path.Combine(Path.GetTempPath(),"vcomanda-pagamento-prototipo-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(pasta);
        var fabrica=new Fabrica(Path.Combine(pasta,"simulacao.db"));using(var db=fabrica.CreateDbContext())db.Database.Migrate();
        var repo=new EfComandaRepository(fabrica);var categorias=new EfCategoriaRepository(fabrica);var produtos=new EfProdutoRepository(fabrica);
        var agora=DateTime.UtcNow;var categoria=categorias.Salvar(new() { Id=0,Nome="Demonstração",Ativo=true,CriadoEm=agora,AtualizadoEm=agora });
        Produto Produto(string nome,long preco)=>produtos.Salvar(new() { Id=0,CategoriaId=categoria.Id,Nome=nome,PrecoCentavos=preco,Ativo=true,CriadoEm=agora,AtualizadoEm=agora });
        var lanche=Produto("Lanche da casa",2000);var suco=Produto("Suco natural",800);var agua=Produto("Água",400);var centavos=Produto("Conta para testar divisão",10001);
        var window=new Window { Title="Protótipo · Novo fluxo de pagamento · Dados fictícios",Width=1120,Height=800,MinWidth=820,MinHeight=620,WindowStartupLocation=WindowStartupLocation.CenterScreen };
        window.Height=Math.Min(window.Height,SystemParameters.WorkArea.Height-24);
        window.SetResourceReference(Window.BackgroundProperty,"AjudaCanvas");
        var layout=new DockPanel();var barra=new DockPanel { Margin=new Thickness(16,8,16,8) };DockPanel.SetDock(barra,Dock.Top);layout.Children.Add(barra);
        var tema=new CheckBox { Content="Tema escuro",VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(12,0,0,0) };tema.SetResourceReference(Control.ForegroundProperty,"AjudaInk");DockPanel.SetDock(tema,Dock.Right);barra.Children.Add(tema);tema.Checked+=(_,_)=>TemaInterface.Aplicar(true,false);tema.Unchecked+=(_,_)=>TemaInterface.Aplicar(false,false);
        var reiniciar=new Button { Content="Reiniciar simulação",Padding=new Thickness(10,5,10,5),Margin=new Thickness(8,0,0,0) };DockPanel.SetDock(reiniciar,Dock.Right);barra.Children.Add(reiniciar);
        var exemplo=new ComboBox { ItemsSource=new[] { "Conta de R$ 32,00","Conta de R$ 100,01" },SelectedIndex=0,MinWidth=170,Margin=new Thickness(12,0,0,0) };DockPanel.SetDock(exemplo,Dock.Right);barra.Children.Add(exemplo);
        var variacao=new ComboBox { ItemsSource=new[] { "A · Pagamento direto","B · Por etapas" },SelectedIndex=1,MinWidth=155,Margin=new Thickness(12,0,0,0) };DockPanel.SetDock(variacao,Dock.Right);barra.Children.Add(variacao);
        var rotulo=new TextBlock { Text="SIMULAÇÃO · Dados fictícios",VerticalAlignment=VerticalAlignment.Center,FontSize=11 };rotulo.SetResourceReference(TextBlock.ForegroundProperty,"AjudaMuted");barra.Children.Add(rotulo);
        var host=new ContentControl();layout.Children.Add(host);window.Content=layout;
        PagamentoFluxoPrototipoViewModel? modelo=null;PagamentoGuiadoPrototipoViewModel? guiado=null;var numero=101;
        void AbrirPagamento(int id)
        {
            guiado?.Dispose();guiado=null;modelo?.Dispose();var pagamento=new EncerramentoViewModel(new EncerrarComanda(repo,new SystemClock()),repo);pagamento.Carregar(id);
            modelo=new(pagamento);MostrarVariacao();
        }
        void MostrarVariacao()
        {
            if(modelo?.Pagamento.ComandaAtual is not {} comanda)return;
            guiado?.Dispose();guiado=null;
            window.Title="Protótipo de pagamento · Modelo "+(variacao.SelectedIndex==0?"A · Direto":"B · Por etapas")+" · Dados fictícios";
            if(variacao.SelectedIndex==0){var view=new PagamentoFluxoPrototipoView(modelo);view.VoltarSolicitado+=()=>MostrarPedido(comanda.Id);host.Content=view;}
            else { guiado=new(modelo);var view=new PagamentoGuiadoPrototipoView(guiado);view.VoltarSolicitado+=()=>MostrarPedido(comanda.Id);host.Content=view; }
        }
        void MostrarPedido(int id)
        {
            var dados=repo.BuscarComItens(id)!;var painel=new StackPanel { Margin=new Thickness(24) };
            TextBlock Texto(string texto,int tamanho=14)
            { var t=new TextBlock { Text=texto,FontSize=tamanho,Margin=new Thickness(0,0,0,12),TextWrapping=TextWrapping.Wrap };t.SetResourceReference(TextBlock.ForegroundProperty,"AjudaInk");painel.Children.Add(t);return t; }
            Texto("Comanda "+dados.Comanda.Numero+" · Pedido",24);
            foreach(var item in dados.Itens)Texto(item.NomeProduto+" · "+CentavosParaMoedaConverter.Formatar(item.SubtotalCentavos));
            Texto("Total: "+CentavosParaMoedaConverter.Formatar(dados.Comanda.TotalCentavos));
            Texto("Recebido: "+CentavosParaMoedaConverter.Formatar(PagamentosConta.Total(dados.Comanda.PagamentosJson)));
            Texto(dados.Comanda.Status==StatusComanda.Aberta?"Os recebimentos confirmados permanecem salvos ao voltar ao pedido.":"Venda finalizada. Use Reiniciar simulação para testar outra conta.");
            var voltar=new Button { Content="Continuar pagamento",HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(14,8,14,8),IsEnabled=dados.Comanda.Status==StatusComanda.Aberta };
            voltar.Click+=(_,_)=>AbrirPagamento(id);painel.Children.Add(voltar);host.Content=painel;
        }
        void Reiniciar()
        {
            var comanda=repo.AbrirComanda(++numero,DateTime.UtcNow);
            foreach(var produto in exemplo.SelectedIndex==0?new[] {lanche,suco,agua}:new[] {centavos})repo.AdicionarItem(comanda.Id,produto,1,DateTime.UtcNow);
            AbrirPagamento(comanda.Id);
        }
        reiniciar.Click+=(_,_)=>Reiniciar();exemplo.SelectionChanged+=(_,_)=>Reiniciar();variacao.SelectionChanged+=(_,_)=>MostrarVariacao();window.Closed+=(_,_)=>{guiado?.Dispose();modelo?.Dispose();};Reiniciar();app.Run(window);
    }
    private sealed class Fabrica(string arquivo):IDbContextFactory<VarthexComandaDbContext>
    {
        public VarthexComandaDbContext CreateDbContext()=>new(new DbContextOptionsBuilder<VarthexComandaDbContext>().UseSqlite("Data Source="+arquivo+";Foreign Keys=True").Options);
    }
}
