using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed partial class JanelaConfiguracoesSistema
{
    private TextBox? _campoNomeEstabelecimento;
    private TextBlock? _erroNomeEstabelecimento;
    internal sealed record PaisTelefone(string Iso,string Nome,string Ddi)
    {
        public override string ToString()=> $"{Ddi} {Nome}";
    }
    internal static IReadOnlyList<PaisTelefone> PaisesTelefone()
    {
        using var stream=typeof(JanelaConfiguracoesSistema).Assembly.GetManifestResourceStream("VarthexComanda.Desktop.Assets.PaisesTelefone.json")!;
        return JsonSerializer.Deserialize<PaisTelefone[]>(stream)!;
    }
    private StackPanel BlocoGeral(string nome,string ajuda,FrameworkElement controle)
    {
        var bloco=new StackPanel {Margin=new Thickness(0,0,12,12)};
        var titulo=new StackPanel {Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,3)};
        object rotulo=nome;
        if(nome=="Nome do estabelecimento")
        {
            var texto=new TextBlock();texto.Inlines.Add(new System.Windows.Documents.Run(nome));texto.Inlines.Add(new System.Windows.Documents.Run(" *"){Foreground=Brushes.Firebrick});rotulo=texto;
            System.Windows.Automation.AutomationProperties.SetIsRequiredForForm(controle,true);
        }
        titulo.Children.Add(new Label {Content=rotulo,Target=controle,Padding=new Thickness(0),VerticalAlignment=VerticalAlignment.Center});
        var botao=AjudaCampos.Botao(nome,ajuda);
        titulo.Children.Add(botao);bloco.Children.Add(titulo);bloco.Children.Add(controle);_camposGeral!.Children.Add(bloco);return bloco;
    }
    private TextBox DadoGeral(string nome,string valor,Action<string> alterar,string ajuda,Func<string,string>? mascara=null,int limite=200)
    {
        var input=new TextBox {Text=mascara is null?valor:mascara(valor),MinHeight=36,FontSize=13,MaxLength=limite};
        System.Windows.Automation.AutomationProperties.SetName(input,nome);
        var bloco=BlocoGeral(nome,ajuda,input);VincularEntrada(input,alterar,mascara);
        if(nome=="Nome do estabelecimento")
        {
            _campoNomeEstabelecimento=input;input.MaxLength=120;
            _erroNomeEstabelecimento=new TextBlock {Foreground=Brushes.Firebrick,TextWrapping=TextWrapping.Wrap,Visibility=Visibility.Collapsed,Margin=new Thickness(0,4,0,0)};
            System.Windows.Automation.AutomationProperties.SetLiveSetting(_erroNomeEstabelecimento,System.Windows.Automation.AutomationLiveSetting.Assertive);bloco.Children.Add(_erroNomeEstabelecimento);
            input.TextChanged+=(_,_)=>{if(!string.IsNullOrWhiteSpace(input.Text)&&input.Text.Trim().Length<=120){if(_mensagem.Text==_erroNomeEstabelecimento.Text)_mensagem.Text="";_erroNomeEstabelecimento.Visibility=Visibility.Collapsed;input.ClearValue(Control.BorderBrushProperty);}};
        }
        return input;
    }
    private bool ValidarNomeEstabelecimento()
    {
        if(!string.IsNullOrWhiteSpace(_nome)&&_nome.Trim().Length<=120)return true;
        if(_secao!="Geral")Mostrar("Geral");
        var erro=string.IsNullOrWhiteSpace(_nome)?"Preencha o nome do estabelecimento para salvar as alterações.":"Use até 120 caracteres no nome do estabelecimento.";
        _mensagem.Text=erro;
        if(_erroNomeEstabelecimento is not null){_erroNomeEstabelecimento.Text=erro;_erroNomeEstabelecimento.Visibility=Visibility.Visible;}
        if(_campoNomeEstabelecimento is not null){_campoNomeEstabelecimento.BorderBrush=Brushes.Firebrick;_campoNomeEstabelecimento.BringIntoView();_campoNomeEstabelecimento.Focus();}
        return false;
    }
    internal static void VincularEntrada(TextBox input,Action<string> alterar,Func<string,string>? mascara)
    {
        var ajustando=false;
        input.TextChanged+=(_,_)=>
        {
            if(ajustando)return;
            if(mascara is not null)
            {
                var antes=Mascaras.Digitos(input.Text[..input.CaretIndex]).Length;var texto=mascara(input.Text);
                if(input.Text!=texto){ajustando=true;input.Text=texto;input.CaretIndex=Mascaras.PosicaoDoCursor(texto,antes);ajustando=false;}
            }
            alterar(input.Text);
        };
        if(mascara is null)return;
        input.PreviewKeyDown+=(_,e)=>
        {
            if(input.SelectionLength>0||e.Key is not (Key.Back or Key.Delete))return;
            var indice=e.Key==Key.Back?input.CaretIndex-1:input.CaretIndex;
            if(indice<0||indice>=input.Text.Length||char.IsAsciiDigit(input.Text[indice]))return;
            var passo=e.Key==Key.Back?-1:1;
            while(indice>=0&&indice<input.Text.Length&&!char.IsAsciiDigit(input.Text[indice]))indice+=passo;
            if(indice<0||indice>=input.Text.Length)return;
            var quantidade=Mascaras.Digitos(input.Text[..indice]).Length;
            input.Text=input.Text.Remove(indice,1);input.CaretIndex=Mascaras.PosicaoDoCursor(input.Text,quantidade);e.Handled=true;
        };
    }
    private void DocumentoGeral()
    {
        var tipo=new ComboBox {ItemsSource=new[]{"CNPJ","CPF"},SelectedItem=_prefs.TipoDocumento is "CPF" or "CNPJ"?_prefs.TipoDocumento:Mascaras.Digitos(_prefs.Documento).Length is >0 and <=11?"CPF":"CNPJ",Width=90,MinHeight=36,Margin=new Thickness(0,0,8,0)};
        var input=new TextBox {Text=Mascaras.Documento(_prefs.Documento,(string)tipo.SelectedItem),MinHeight=36,FontSize=13,MaxLength=18};
        var linha=new DockPanel();DockPanel.SetDock(tipo,Dock.Left);linha.Children.Add(tipo);linha.Children.Add(input);
        BlocoGeral("CPF / CNPJ","Escolha CPF para o documento de uma pessoa ou CNPJ para o documento de uma empresa. Digite apenas os números; a pontuação é automática.",linha);
        System.Windows.Automation.AutomationProperties.SetName(tipo,"Tipo de documento");System.Windows.Automation.AutomationProperties.SetName(input,"CPF / CNPJ");
        VincularEntrada(input,v=>{_prefs.Documento=v;_prefs.TipoDocumento=(string)tipo.SelectedItem;},v=>Mascaras.Documento(v,(string)tipo.SelectedItem));
        tipo.SelectionChanged+=(_,_)=>
        {
            var novo=(string)tipo.SelectedItem;
            if(novo=="CPF"&&Mascaras.Digitos(input.Text).Length>11)
            {
                tipo.SelectedItem="CNPJ";_mensagem.Text="Apague o CNPJ antes de selecionar CPF, para não perder números.";return;
            }
            _prefs.TipoDocumento=novo;input.Text=Mascaras.Documento(input.Text,novo);AtualizarEstado();
        };
    }
    internal static string FormatarTelefoneNacional(string valor,string iso)
    {
        var n=Mascaras.Digitos(valor);
        if(iso!="BR")return n[..Math.Min(n.Length,15)];
        n=n[..Math.Min(n.Length,11)];
        if(n.Length<=2)return n.Length==0?"":"("+n;
        var local=n[2..];var inicio=local.Length>8?local[..1]+" "+local.Substring(1,Math.Min(4,local.Length-1)):local[..Math.Min(4,local.Length)];
        var corte=local.Length>8?5:4;
        return "("+n[..2]+") "+inicio+(local.Length>corte?"-"+local[corte..]:"");
    }
    private void TelefoneGeral()
    {
        var campo=new CampoTelefonePadrao(_prefs.Telefone,_prefs.PaisTelefone);
        BlocoGeral("Telefone principal",AjudaCampos.Explicacao("Telefone principal")!,campo);
        campo.Alterado+=(_,_)=>{_prefs.Telefone=campo.Telefone;_prefs.PaisTelefone=campo.CodigoPais;AtualizarEstado();};
    }
    internal static string FormatarCep(string valor)
    {
        var n=Mascaras.Digitos(valor);n=n[..Math.Min(8,n.Length)];return n.Length>5?n[..5]+"-"+n[5..]:n;
    }
    private void NormalizarDadosEstabelecimento()
    {
        var documento=Mascaras.Digitos(_prefs.Documento);
        if(documento.Length is 11 or 14)
        {
            _prefs.TipoDocumento=documento.Length==11?"CPF":"CNPJ";
            _prefs.Documento=Mascaras.Documento(documento,_prefs.TipoDocumento);
        }
        if(Mascaras.Digitos(_prefs.Cep).Length==8)_prefs.Cep=FormatarCep(_prefs.Cep);
        _prefs.Uf=_prefs.Uf.Trim().ToUpperInvariant();
        var pais=PaisesTelefone().FirstOrDefault(p=>p.Iso==_prefs.PaisTelefone);
        if(pais is null||string.IsNullOrWhiteSpace(_prefs.Telefone))return;
        var nacional=_prefs.Telefone.Trim();
        if(nacional.StartsWith(pais.Ddi,StringComparison.Ordinal))nacional=nacional[pais.Ddi.Length..];
        var numeros=Mascaras.Digitos(nacional);
        if(pais.Iso=="BR"&&numeros.Length is 12 or 13&&numeros.StartsWith("55",StringComparison.Ordinal))numeros=numeros[2..];
        if(pais.Iso=="BR"&&numeros.Length is not (10 or 11))return;
        _prefs.Telefone=pais.Ddi+" "+FormatarTelefoneNacional(numeros,pais.Iso);
    }
}
