using System.Windows;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private bool ImprimirPedido(SaidaPedido saida)
    {
        var vm = _atendimentoView.ViewModel;
        if (_bloqueado || vm.ComandaAtual is not { } comanda) return false;
        var owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? this;
        var servico = _repositorioSistema is null ? null : new ConfiguracoesImpressaoService(_repositorioSistema);
        var impressora = servico?.ObterCaixaConfigurado();
        if (saida is SaidaPedido.Conta or SaidaPedido.ContaBloquear)
        {
            var numeracao = _repositorioSistema is null ? null : new NumeracaoComandasService(_repositorioSistema).Carregar();
            var texto = ComprovanteConta.MontarTexto(LerPreferencias(), _configuracaoView.ViewModel.NomeEstabelecimento, comanda.Numero, vm.Itens, comanda.CodigoPedido, false, numeracao?.UsarComoSenhaAtendimento == true ? comanda.Numero : null);
            var enviada = ComprovanteConta.Visualizar(owner, texto, impressora);
            if (enviada && saida == SaidaPedido.ContaBloquear && !vm.DefinirBloqueio(true))
                throw new InvalidOperationException("A conta foi enviada, mas não foi possível bloquear o pedido. Bloqueie-o antes de continuar.");
            return enviada;
        }
        var cozinha = saida is SaidaPedido.CozinhaNovos or SaidaPedido.CozinhaTodos;
        var reimprimir = saida == SaidaPedido.CozinhaTodos;
        var quantidades = vm.Itens.ToDictionary(i => i.Id, i => reimprimir ? i.Quantidade : Math.Max(0, i.Quantidade - (cozinha ? i.QuantidadeCozinha : i.QuantidadeFicha)))
            .Where(p => p.Value > 0).ToDictionary(p => p.Key, p => p.Value);
        if (quantidades.Count == 0) return false;
        if (cozinha && servico is not null)
        {
            var preferencias = servico.Carregar();
            var saidas = preferencias.Cozinhas.Values.Where(p => p.Habilitada).ToArray();
            // O catálogo atual não atribui os produtos a cozinhas; uma única saída evita duplicar pedidos.
            if (saidas.Length > 1)
            {
                var janela = new Window { Title="Selecionar cozinha",Owner=owner,Width=420,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false };
                var painel = new System.Windows.Controls.StackPanel { Margin=new Thickness(20) };janela.Content=painel;
                painel.Children.Add(new System.Windows.Controls.TextBlock { Text="Qual cozinha deve receber estes itens?",Margin=new Thickness(0,0,0,12) });
                var locais = preferencias.Cozinhas.Where(p=>p.Value.Habilitada).ToArray();
                var lista = new System.Windows.Controls.ComboBox { ItemsSource=locais,DisplayMemberPath="Key",SelectedIndex=0 };
                System.Windows.Automation.AutomationProperties.SetName(lista,"Cozinha de destino");painel.Children.Add(lista);
                var continuar = new System.Windows.Controls.Button { Content="Continuar",IsDefault=true,Margin=new Thickness(0,12,0,8) };
                continuar.Click+=(_,_)=>janela.DialogResult=true;painel.Children.Add(continuar);
                painel.Children.Add(new System.Windows.Controls.Button { Content="Cancelar",IsCancel=true });
                if(janela.ShowDialog()!=true)return false;
                var selecionada=((KeyValuePair<string,PreferenciaImpressora>)lista.SelectedItem).Value;
                impressora=selecionada.Modo==ModoImpressora.MesmaDoCaixa?preferencias.Caixa:selecionada;
            }
            if (saidas.Length == 1) impressora = saidas[0].Modo == ModoImpressora.MesmaDoCaixa ? preferencias.Caixa : saidas[0];
        }
        var titulo = cozinha ? (reimprimir ? "Reimpressão na cozinha" : "Pedido para cozinha") : "Fichas de consumação";
        var linhas = new List<string> { _configuracaoView.ViewModel.NomeEstabelecimento, titulo.ToUpperInvariant(), $"Comanda {comanda.Numero:00} · {vm.IdentificacaoPedido}", $"{DateTime.Now:dd/MM/yyyy HH:mm}", comanda.NumeroMesa is int mesa ? $"Mesa {mesa}" : "Balcão" };
        if (!string.IsNullOrWhiteSpace(comanda.Observacao)) linhas.Add("Pedido: " + comanda.Observacao);
        foreach (var item in vm.Itens.Where(i => quantidades.ContainsKey(i.Id)))
        {
            var qtd = quantidades[item.Id];
            if (cozinha)
            {
                linhas.Add($"\n{qtd} × {item.NomeProduto}");
                if (!string.IsNullOrWhiteSpace(item.Observacao)) linhas.Add("Observação: " + item.Observacao);
            }
            else
                for (var n = 1; n <= qtd; n++)
                {
                    linhas.Add(new string('-', 38));
                    linhas.Add($"1 × {item.NomeProduto}\nComanda {comanda.Numero:00} · {vm.IdentificacaoPedido}\nFicha {item.QuantidadeFicha + n}/{item.Quantidade}");
                    if (!string.IsNullOrWhiteSpace(item.Observacao)) linhas.Add(item.Observacao);
                }
        }
        var impressa = ComprovanteConta.Visualizar(owner, string.Join("\n", linhas), impressora, titulo);
        if (impressa && !reimprimir)
        {
            try { vm.RegistrarImpressao(quantidades, cozinha); }
            catch { throw new InvalidOperationException("Impressão enviada, mas não foi possível registrar os itens impressos. Confira a saída antes de imprimir novamente."); }
        }
        return impressa;
    }
}
