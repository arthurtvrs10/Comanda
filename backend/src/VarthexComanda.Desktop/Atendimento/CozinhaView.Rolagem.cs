using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView
{
    private DateTime _ultimaRolagem;
    private bool _alcaEmUso;
    private ScrollViewer? _rolagemAlca;
    private Thumb? _alcaAtual;
    private double _mouseInicialAlca,_offsetInicialAlca,_escalaAlca,_offsetPendente;
    private void Coluna_Loaded(object sender,RoutedEventArgs e)
    {
        if(sender is not ScrollViewer rolagem)return;
        rolagem.ApplyTemplate();
        if(rolagem.Template.FindName("PART_VerticalScrollBar",rolagem) is not ScrollBar barra)return;
        barra.ApplyTemplate();
        if(barra.Template.FindName("PART_Track",barra) is not Track {Thumb: { } alca} track)return;
        alca.DragStarted-=Alca_Iniciar;alca.DragStarted+=Alca_Iniciar;
        alca.DragDelta-=Alca_Mover;alca.DragDelta+=Alca_Mover;
        alca.DragCompleted-=Alca_Terminar;alca.DragCompleted+=Alca_Terminar;
        alca.Tag=(rolagem,track);
    }
    private void Alca_Iniciar(object sender,DragStartedEventArgs e)
    {
        if(sender is not Thumb {Tag:ValueTuple<ScrollViewer,Track> dados} alca)return;
        var (rolagem,track)=dados;
        _alcaAtual=alca;_rolagemAlca=rolagem;_alcaEmUso=true;
        _mouseInicialAlca=Mouse.GetPosition(rolagem).Y;_offsetInicialAlca=rolagem.VerticalOffset;
        _escalaAlca=rolagem.ScrollableHeight/Math.Max(1,track.ActualHeight-alca.ActualHeight);
        _offsetPendente=_offsetInicialAlca;
        CompositionTarget.Rendering+=Alca_Renderizar;
        e.Handled=true;
    }
    private void Alca_Mover(object sender,DragDeltaEventArgs e)
    {
        if(!_alcaEmUso||sender!=_alcaAtual||_rolagemAlca is null)return;
        _offsetPendente=Math.Clamp(_offsetInicialAlca+(Mouse.GetPosition(_rolagemAlca).Y-_mouseInicialAlca)*_escalaAlca,0,_rolagemAlca.ScrollableHeight);
        e.Handled=true;
    }
    private void Alca_Renderizar(object? sender,EventArgs e)=>_rolagemAlca?.ScrollToVerticalOffset(_offsetPendente);
    private void Alca_Terminar(object sender,DragCompletedEventArgs e)
    {
        if(sender!=_alcaAtual)return;
        Alca_Renderizar(null,EventArgs.Empty);PararRolagemAlca();e.Handled=true;
    }
    private void PararRolagemAlca()
    {
        CompositionTarget.Rendering-=Alca_Renderizar;_alcaEmUso=false;_rolagemAlca=null;_alcaAtual=null;_ultimaRolagem=DateTime.UtcNow;
    }
    private bool RolagemEmUso=>_alcaEmUso||Mouse.LeftButton==MouseButtonState.Pressed&&IsMouseOver||DateTime.UtcNow-_ultimaRolagem<TimeSpan.FromMilliseconds(600);
    private void Coluna_MouseWheel(object sender,MouseWheelEventArgs e)
    {
        if(sender is not ScrollViewer rolagem)return;
        _ultimaRolagem=DateTime.UtcNow;
        // Uma única atualização física, sem a repetição por linha do tema WPF.
        rolagem.ScrollToVerticalOffset(rolagem.VerticalOffset-e.Delta);
        e.Handled=true;
    }
}
