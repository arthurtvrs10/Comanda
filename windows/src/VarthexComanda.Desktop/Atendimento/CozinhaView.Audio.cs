using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView
{
    private SoundPlayer? _campainha;
    private Stream? _audioStream;
    private SoundPlayer? _toquePronto;
    private Stream? _streamPronto;
    private IConfiguracaoRepository? _preferenciasAudio;
    private bool _avisoVolumeMostrado,_modalVolume;
    private const string ChaveAvisoVolume="cozinha.audio.nao-avisar-volume.v1";
    private void InicializarAudio(IConfiguracaoRepository? preferencias)
    {
        _preferenciasAudio=preferencias;
        try{_audioStream=typeof(CozinhaView).Assembly.GetManifestResourceStream("VComanda.KdsCampainhaDupla.wav");if(_audioStream is not null){_campainha=new SoundPlayer(_audioStream);_campainha.Load();}}
        catch{_campainha?.Dispose();_campainha=null;_audioStream?.Dispose();_audioStream=null;}
        ViewModel.NotificarNovoItem=()=>_campainha?.Play();
        try{_streamPronto=typeof(CozinhaView).Assembly.GetManifestResourceStream("VComanda.KdsPedidoPronto.wav");if(_streamPronto is not null){_toquePronto=new SoundPlayer(_streamPronto);_toquePronto.Load();}}
        catch{_toquePronto?.Dispose();_toquePronto=null;_streamPronto?.Dispose();_streamPronto=null;}
        ViewModel.NotificarPedidoPronto=()=>_toquePronto?.Play();
    }
    private void VerificarVolume()
    {
        if(!IsVisible||!ViewModel.SomAtivo||_modalVolume||_arrastando||RolagemEmUso)return;
        try
        {
            if(_preferenciasAudio?.ObterValor(ChaveAvisoVolume)=="1")return;
            var volume=VolumeWindows.Consultar();if(volume is null)return;
            if(!volume.Value.Mudo&&volume.Value.Percentual>=80){_avisoVolumeMostrado=false;return;}
            if(_avisoVolumeMostrado)return;
            _avisoVolumeMostrado=true;_modalVolume=true;
            try
            {
                var janela=new Window {Title="Volume do computador",Owner=Window.GetWindow(this),Width=480,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner};
                janela.SetResourceReference(BackgroundProperty,"Surface");janela.SetResourceReference(ForegroundProperty,"Ink");
                var painel=new StackPanel {Margin=new Thickness(24)};painel.Children.Add(new TextBlock {Text="Confira o volume dos alertas",FontSize=18,FontWeight=FontWeights.SemiBold});
                painel.Children.Add(new TextBlock {Text=(volume.Value.Mudo?"O computador está no mudo.":$"O volume do computador está em {volume.Value.Percentual}%.")+" Recomendamos 80% ou mais para ouvir os novos pedidos na cozinha. Ajuste o volume pelo Windows.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,20)});
                var acoes=new StackPanel {Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
                var naoRepetir=new Button {Content="Não repetir mais",MinHeight=38,Padding=new Thickness(12,8,12,8),Margin=new Thickness(0,0,8,0)};
                naoRepetir.Click+=(_,_)=>{try{_preferenciasAudio?.Definir(ChaveAvisoVolume,"1",DateTime.UtcNow);janela.Close();}catch(Exception ex){JanelaAviso.Mostrar("Volume",ex.Message,TipoAviso.Erro);}};
                var obrigado=new Button {Content="Obrigado",MinHeight=38,Padding=new Thickness(16,8,16,8),IsCancel=true};acoes.Children.Add(naoRepetir);acoes.Children.Add(obrigado);painel.Children.Add(acoes);janela.Content=painel;janela.ShowDialog();
            }
            finally{_modalVolume=false;}
        }
        catch{ /* A indisponibilidade do áudio não bloqueia o atendimento. */ }
    }
}
