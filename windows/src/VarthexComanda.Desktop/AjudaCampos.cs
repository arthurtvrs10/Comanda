using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace VarthexComanda.Desktop;

/// <summary>Rótulo acessível compartilhado. Mantém texto, vínculos e destino do Label.</summary>
public sealed class RotuloAjuda : TextBlock
{
    public static readonly DependencyProperty TextoProperty=DependencyProperty.Register(nameof(Texto),typeof(string),typeof(RotuloAjuda),new PropertyMetadata("",(d,_)=>((RotuloAjuda)d).Atualizar()));
    public string Texto {get=>(string)GetValue(TextoProperty);set=>SetValue(TextoProperty,value);}
    public RotuloAjuda(){TextWrapping=TextWrapping.Wrap;VerticalAlignment=VerticalAlignment.Center;Foreground=TemaInterface.PincelDoPapel("Ink");}
    private void Atualizar()
    {
        Inlines.Clear();var titulo=Texto??"";
        Inlines.Add(new Run(titulo.Replace(" *","")));
        if(titulo.Contains('*'))Inlines.Add(new Run(" *"){Foreground=Brushes.Firebrick});
        var ajuda=AjudaCampos.Explicacao(titulo);if(ajuda is not null)Inlines.Add(new InlineUIContainer(AjudaCampos.Botao(titulo,ajuda)){BaselineAlignment=BaselineAlignment.Center});
    }
}
public static class AjudaCampos
{
    public static string? Explicacao(string titulo)
    {
        var t=titulo.Replace("*","").Replace(":","").Trim().ToLowerInvariant();
        if(t=="pasta do onedrive")return "Pasta sincronizada pelo OneDrive neste Windows. O VComanda grava cópias em Varthex\\Backups. Confira a conta escolhida e o estado da sincronização no aplicativo OneDrive.";
        if(t.StartsWith("cliente"))return "Nome opcional para identificar o cliente da comanda. Quando preenchido, aparece também no KDS.";
        if(t=="tipo de pedido")return "Escolha como o pedido será atendido: Local (Mesa), Salão, Balcão, Para levar ou Delivery. O KDS mostra essa informação em uma etiqueta colorida.";
        if(t=="imagem do estabelecimento")return "Foto ou logo que identifica o estabelecimento. A mesma imagem é usada no ticket da comanda. Prefira uma imagem nítida e com bom contraste para a impressão térmica.";
        if(t=="senha de atendimento")return "Número usado para chamar o cliente quando o pedido estiver pronto.";
        if(t.Contains("copiar permissões"))return "Escolha um colaborador para usar as permissões dele como ponto de partida. Confira antes de salvar.";
        if(t.Contains("pesquisar permissões"))return "Digite o nome de uma área ou operação para localizar uma permissão.";
        if(t.Contains("pesquisar nos registros"))return "Digite parte do conteúdo para localizar eventos nos registros do sistema.";
        if(t.Contains("endereço do servidor"))return "Endereço de acesso do servidor na rede local. Os aparelhos precisam estar conectados à mesma rede.";
        if(t.Contains("caminho do banco"))return "Local do arquivo de dados utilizado por este computador. Não mova nem exclua o arquivo durante o uso.";
        if(t.Contains("caminho")&&t.Contains("impressora"))return "Caminho da impressora compartilhada na rede, no formato \\\\computador\\impressora.";
        if(t=="começa com")return "Primeiro número da sequência de comandas. Confira a faixa antes de salvar.";
        if(t.Contains("cpf")||t.Contains("cnpj")||t=="documento")return "CPF identifica uma pessoa; CNPJ identifica uma empresa. Digite os números no campo correspondente. A pontuação é aplicada automaticamente nos cadastros.";
        if(t.Contains("razão social"))return "Nome oficial da empresa no cadastro do CNPJ. Pode ser diferente do nome usado pelo estabelecimento.";
        if(t.Contains("inscrição estadual"))return "Registro da empresa na Secretaria da Fazenda do estado. Informe o número ou ISENTO quando aplicável. Consulte seu contador em caso de dúvida.";
        if(t.Contains("telefone")||t.Contains("fone"))return "Selecione o país e informe o telefone com DDD. No Brasil, o celular aparece como (61) 9 0000-0900.";
        if(t=="cep")return "Código postal do endereço, com oito números. Exemplo: 70000-000.";
        if(t=="uf"||t=="estado (uf)")return "Sigla do estado do endereço, como DF, SP ou RJ.";
        if(t=="endereço")return "Nome da rua, avenida ou outro logradouro. Preencha o número no campo próprio.";
        if(t=="número")return "Número do imóvel. Se não houver, informe S/N.";
        if(t=="complemento")return "Detalhe do endereço, como loja, sala, bloco ou andar. Pode ficar vazio.";
        if(t=="bairro")return "Bairro onde fica o endereço cadastrado.";
        if(t=="cidade")return "Município onde fica o endereço cadastrado.";
        if(t.Contains("e-mail"))return "Endereço de e-mail para contato, no formato nome@dominio.com.";
        if(t.Contains("comissão"))return "Percentual de comissão por atendimento atribuído ao colaborador. Zero significa sem comissão.";
        if(t.StartsWith("nome"))return "Nome usado para identificar este cadastro no sistema. No estabelecimento, use o nome conhecido pelos clientes.";
        if(t.StartsWith("login"))return "Identificação usada pelo colaborador para entrar no sistema. Deve ser exclusiva.";
        if(t.Contains("confirmar")&&t.Contains("senha"))return "Digite novamente a mesma senha para confirmar.";
        if(t=="senha")return "Credencial de acesso ao sistema. Não compartilhe sua senha com outras pessoas.";
        if(t.Contains("custo"))return "Quanto custa adquirir ou produzir uma unidade do produto. Informe o valor em reais.";
        if(t.Contains("preço")||t.Contains("valor de venda"))return "Valor cobrado do cliente, em reais, conforme a unidade de venda do produto.";
        if(t=="categoria")return "Grupo usado para organizar e localizar produtos no catálogo.";
        if(t.Contains("descrição"))return "Informações que ajudam a reconhecer este cadastro e seus detalhes.";
        if(t.Contains("código personalizado"))return "Código opcional definido por você para localizar o produto.";
        if(t.Contains("código do alimento"))return "Número gerado automaticamente em sequência ao salvar o alimento.";
        if(t.Contains("quantidade disponível"))return "Saldo de unidades disponíveis para seleção. A disponibilidade é conferida antes de adicionar o item.";
        if(t.Contains("mínimo obrigatório"))return "Quantidade mínima exigida na seleção. Zero torna a escolha opcional.";
        if(t.Contains("máximo permitido"))return "Limite de unidades que podem ser selecionadas, respeitando o estoque disponível.";
        if(t.Contains("observaç"))return "Instruções adicionais relacionadas ao pedido ou ao item. Confira o conteúdo antes de confirmar.";
        if(t.Contains("recebido")&&t.Contains("dinheiro"))return "Valor em dinheiro entregue pelo cliente. O sistema usa esse valor para calcular o troco.";
        if(t.Contains("valor")&&t.Contains("pagamento"))return "Valor a registrar nesta forma de pagamento. Pode ser apenas parte do saldo da conta.";
        if(t.Contains("pessoas")&&t.Contains("dividir"))return "Quantidade de pessoas entre as quais será dividido o saldo restante da conta.";
        if(t.Contains("papel"))return "Largura do papel da impressora térmica: 58 ou 80 milímetros.";
        if(t.Contains("impressora"))return "Selecione a impressora configurada neste computador para esta saída de impressão.";
        if(t.Contains("cabeçalho"))return "Mensagem que aparece na parte superior do comprovante.";
        if(t.Contains("rodapé"))return "Mensagem que aparece na parte inferior do comprovante.";
        if(t.Contains("comanda"))return "Número que identifica a conta do atendimento no sistema.";
        if(t=="data")return "Selecione a data para consultar os registros do período.";
        return null;
    }
    public static Button Botao(string titulo,string ajuda)
    {
        var azul=new SolidColorBrush(Color.FromRgb(39,129,222));var desenho=new DrawingGroup();
        desenho.Children.Add(new GeometryDrawing(null,new Pen(azul,1.5),new EllipseGeometry(new Point(9,9),7,7)));
        desenho.Children.Add(new GeometryDrawing(null,new Pen(azul,1.5){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round},Geometry.Parse("M6.7,6.5 C6.7,3.8 12,3.8 12,6.5 C12,8.6 9,8.2 9,10.8 M9,13 L9,13.2")));
        var b=new Button {Content=new Image {Source=new DrawingImage(desenho),Width=17,Height=17},MinWidth=22,MinHeight=22,Width=22,Height=22,Padding=new Thickness(1),Margin=new Thickness(4,0,0,0),Background=Brushes.Transparent,BorderThickness=new Thickness(0),Cursor=Cursors.Hand,ToolTip="Entenda: "+titulo};
        System.Windows.Automation.AutomationProperties.SetName(b,"Ajuda: "+titulo);
        var texto=new TextBlock {Text=ajuda,TextWrapping=TextWrapping.Wrap,MaxWidth=300,Margin=new Thickness(12)};texto.SetResourceReference(TextBlock.ForegroundProperty,"Ink");
        var quadro=new Border {Child=texto,BorderThickness=new Thickness(1)};quadro.SetResourceReference(Border.BackgroundProperty,"Surface");quadro.SetResourceReference(Border.BorderBrushProperty,"Line");
        var popup=new Popup {Child=quadro,PlacementTarget=b,Placement=PlacementMode.Bottom,StaysOpen=false,AllowsTransparency=true};
        b.Click+=(_,_)=>popup.IsOpen=!popup.IsOpen;b.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){popup.IsOpen=false;e.Handled=true;}};b.Unloaded+=(_,_)=>popup.IsOpen=false;return b;
    }
    internal static void Padronizar(Label label)
    {
        if(label.Content is not string||label.ContentTemplate is not null)return;
        if(label.Parent is Panel painel&&painel.Children.OfType<Button>().Any(b=>System.Windows.Automation.AutomationProperties.GetName(b).StartsWith("Ajuda:")))return;
        var fabrica=new FrameworkElementFactory(typeof(RotuloAjuda));fabrica.SetBinding(RotuloAjuda.TextoProperty,new System.Windows.Data.Binding("."));label.ContentTemplate=new DataTemplate {VisualTree=fabrica};
    }
}

