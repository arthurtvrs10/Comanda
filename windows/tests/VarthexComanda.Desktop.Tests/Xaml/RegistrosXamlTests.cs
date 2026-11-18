using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Abstractions;
using VarthexComanda.Desktop.Registros;
using VarthexComanda.Infrastructure.Logging;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class RegistrosXamlTests
{
    [Fact]
    public async Task FiltrosLeituraDeArquivoEmUsoERecuperacaoDeFalha()
    {
        var pasta=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(pasta);
        try
        {
            var arquivo=Path.Combine(pasta,"varthex-comanda-20261024.log");
            File.WriteAllText(arquivo,"2026-10-24 12:00:00.000 [INF] Operação concluída\n2026-10-24 12:00:01.000 [ERR] Falha ao consultar\n");
            var registro=new RegistroEventos();using var espera=registro.Iniciar(OrigemEvento.Servidor,AcaoEvento.ReceberItem,true);
            using(var pronto=registro.Iniciar(OrigemEvento.Atendimento,AcaoEvento.AdicionarItem))pronto.Finalizar(EstadoEvento.Concluido);
            var vm=new RegistrosViewModel(registro,new(pasta));await vm.AtualizarCommand.ExecuteAsync(null);
            Assert.Equal(2,vm.Logs.Count);Assert.Equal(2,vm.Eventos.Count);
            vm.Nivel="ERR";Assert.Single(vm.Logs);vm.Busca="concluída";Assert.Empty(vm.Logs);vm.Nivel="Todos os níveis";Assert.Single(vm.Logs);
            vm.Busca="";vm.Estado="Em curso";Assert.Single(vm.Eventos);vm.Origem="Cozinha";Assert.Empty(vm.Eventos);
            using(var exclusivo=File.Open(arquivo,FileMode.Open,FileAccess.ReadWrite,FileShare.None))
            { await vm.AtualizarCommand.ExecuteAsync(null);Assert.NotEmpty(vm.Mensagem);Assert.Equal(2,vm.Logs.Count); }
            await vm.AtualizarCommand.ExecuteAsync(null);Assert.Empty(vm.Mensagem);
            vm.Origem="Servidor";Assert.Single(vm.Eventos);
            espera.Finalizar(EstadoEvento.Cancelado);await vm.AtualizarCommand.ExecuteAsync(null);Assert.Empty(vm.Eventos);
        }
        finally{Directory.Delete(pasta,true);}
    }

    [Fact]
    public async Task TelaRealNosDoisTemasComFilaLogsEVazioSemCortes()
    {
        var pasta=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(pasta);
        try
        {
            var logger=LoggingConfigurator.CreateLogger(pasta);using(logger as IDisposable)
            {logger.Information("Comanda iniciada");logger.Information("Evento 2: Atendimento / Adicionar item / Concluído");logger.Warning("Consulta indisponível. Tente novamente.");}
            var registro=new RegistroEventos();using var espera=registro.Iniciar(OrigemEvento.Servidor,AcaoEvento.ReceberPedido,true);
            using var processando=registro.Iniciar(OrigemEvento.Atendimento,AcaoEvento.AdicionarItem);
            using(var pronto=registro.Iniciar(OrigemEvento.Cozinha,AcaoEvento.AtualizarPreparo))pronto.Finalizar(EstadoEvento.Concluido);
            using(var recusado=registro.Iniciar(OrigemEvento.Atendimento,AcaoEvento.AbrirComanda))recusado.Finalizar(EstadoEvento.Recusado);
            using(var falha=registro.Iniciar(OrigemEvento.Servidor,AcaoEvento.ReceberItem)){}
            var vm=new RegistrosViewModel(registro,new(pasta));await vm.AtualizarCommand.ExecuteAsync(null);
            ThreadingHelper.EmSta(()=>
            {
                CarregamentoDeXamlTests.GarantirApp();
                try
                {
                    foreach(var escuro in new[]{false,true})
                    foreach(var tamanho in new[]{new Size(1040,680),new Size(760,560)})
                    {
                        TemaInterface.Aplicar(escuro,false);
                        var view=new RegistrosView(vm);var janela=new Window{Title="Logs e fila de eventos",Content=view,Width=tamanho.Width,Height=tamanho.Height};janela.Show();
                        try
                        {
                            janela.UpdateLayout();var abas=AlinhamentoInterfaceTests.Filhos(view).OfType<TabControl>().Single();
                            foreach(var aba in new[]{0,1})
                            {
                                abas.SelectedIndex=aba;janela.UpdateLayout();
                                janela.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ContextIdle);
                                var tabela=(DataGrid)view.FindName(aba==0?"TabelaLogs":"TabelaEventos");Assert.True(tabela.ActualHeight>100);
                                Assert.Equal(aba==0?3:5,tabela.Items.Count);
                                Capturar(janela,$"registros-{(escuro?"escuro":"claro")}-{tamanho.Width}-{aba}");
                            }
                            vm.Busca="não existe";janela.UpdateLayout();Assert.Empty(vm.Eventos);
                            Capturar(janela,$"registros-vazio-{escuro}-{tamanho.Width}");vm.Busca="";
                        }
                        finally{janela.Close();}
                    }
                }
                finally{TemaInterface.Aplicar(false,false);}
            });
        }
        finally{Directory.Delete(pasta,true);}
    }
    private static void Capturar(FrameworkElement view,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrEmpty(pasta))return;Directory.CreateDirectory(pasta);
        var bitmap=new RenderTargetBitmap((int)view.ActualWidth,(int)view.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
