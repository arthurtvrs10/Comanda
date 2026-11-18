using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class LimitesJanelaModalTests
{
    [Fact]
    public void JanelaFilhaDoSistemaMaximizaSomenteNaAreaAbaixoDaRibbon()
    {
        var pasta=Path.Combine(Path.GetTempPath(),"canvas-modal-"+Guid.NewGuid());
        try
        {
            ThreadingHelper.EmSta(()=>
            {
                CarregamentoDeXamlTests.GarantirApp();
                var dono=CarregamentoDeXamlTests.CriarMainWindow(new VarthexComanda.Infrastructure.Storage.AppPaths(pasta));
                dono.Left=40;dono.Top=30;dono.Width=1100;dono.Height=740;dono.Opacity=0;dono.ShowInTaskbar=false;
                var filha=new Window { Width=600,Height=480,ResizeMode=ResizeMode.CanResize,Opacity=0,ShowInTaskbar=false };
                try
                {
                    dono.Show();dono.UpdateLayout();filha.Owner=dono;filha.Show();
                    filha.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                    filha.WindowState=WindowState.Maximized;
                    var trabalho=(FrameworkElement)dono.FindName("ConteudoPrincipal");
                    var inicio=trabalho.PointToScreen(new Point());var fim=trabalho.PointToScreen(new Point(trabalho.ActualWidth,trabalho.ActualHeight));
                    var area=Area(filha);
                    Assert.InRange(Math.Abs(area.Esquerda-inicio.X),0,2);Assert.InRange(Math.Abs(area.Topo-inicio.Y),0,2);
                    Assert.InRange(Math.Abs(area.Direita-fim.X),0,2);Assert.InRange(Math.Abs(area.Base-fim.Y),0,2);
                    Assert.True(area.Topo>Area(dono).Topo+100);
                }
                finally { filha.Close();dono.Close(); }
            });
        }
        finally { if(Directory.Exists(pasta))Directory.Delete(pasta,true); }
    }

    [Fact]
    public void MaximizarJanelaFilhaOcupaAreaDoProprietarioERestauraTamanhoAnterior()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var dono=new Window { Left=70,Top=50,Width=1100,Height=740,Opacity=0,ShowInTaskbar=false };
            var filha=new Window { Left=160,Top=100,Width=600,Height=480,ResizeMode=ResizeMode.CanResize,Opacity=0,ShowInTaskbar=false };
            try
            {
                dono.Show();filha.Owner=dono;filha.Show();
                filha.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                var original=Area(filha);
                filha.WindowState=WindowState.Maximized;
                var areaDono=Area(dono);var areaFilha=Area(filha);
                Assert.InRange(Math.Abs(areaFilha.Esquerda-areaDono.Esquerda),0,2);
                Assert.InRange(Math.Abs(areaFilha.Topo-areaDono.Topo),0,2);
                Assert.InRange(Math.Abs(areaFilha.Direita-areaDono.Direita),0,2);
                Assert.InRange(Math.Abs(areaFilha.Base-areaDono.Base),0,2);
                Assert.InRange(Math.Abs((areaFilha.Direita-areaFilha.Esquerda)-(areaDono.Direita-areaDono.Esquerda)),0,2);
                filha.WindowState=WindowState.Normal;
                Assert.Equal(original,Area(filha));
            }
            finally { filha.Close();dono.Close(); }
        });
    }

    private static Retangulo Area(Window janela)
    {
        Assert.True(GetWindowRect(new WindowInteropHelper(janela).Handle,out var area));return area;
    }
    [StructLayout(LayoutKind.Sequential)] private struct Retangulo { public int Esquerda,Topo,Direita,Base; }
    [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(IntPtr hwnd,out Retangulo area);
}
