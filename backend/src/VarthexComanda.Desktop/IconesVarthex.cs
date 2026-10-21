using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VarthexComanda.Desktop;

internal enum IconeVarthex { Mesas, Cozinha, Painel, Produtos, NovoProduto, Historico, Configuracoes, Numeracao, Impressoras, Colaboradores, Senha, Backup, Assinatura, Ajuda, Diagnostico }

/// <summary>Ilustrações vetoriais originais para comandos, desenhadas em 32 unidades.</summary>
internal static class IconesVarthex
{
    public static FrameworkElement Criar(IconeVarthex tipo, double tamanho = 30)
    {
        var tela=new Canvas { Width=32,Height=32,IsHitTestVisible=false };
        Brush Cor(string hex)=>(Brush)new BrushConverter().ConvertFromString(hex)!;
        void Forma(string d,string cor)=>tela.Children.Add(new Path { Data=Geometry.Parse(d),Fill=Cor(cor) });
        void Linha(string d,string cor,double largura=1.5)=>tela.Children.Add(new Path { Data=Geometry.Parse(d),Stroke=Cor(cor),StrokeThickness=largura,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,StrokeLineJoin=PenLineJoin.Round });
        void Caixa(double x,double y,double w,double h,string cor,double raio=0)
        {
            var r=new Rectangle { Width=w,Height=h,Fill=Cor(cor),RadiusX=raio,RadiusY=raio };Canvas.SetLeft(r,x);Canvas.SetTop(r,y);tela.Children.Add(r);
        }
        void Disco(double x,double y,double diametro,string cor)
        {
            var e=new Ellipse { Width=diametro,Height=diametro,Fill=Cor(cor) };Canvas.SetLeft(e,x);Canvas.SetTop(e,y);tela.Children.Add(e);
        }
        void CaixaProduto()
        {
            Forma("M3,10 L15,4 L28,10 L16,16 Z","#F8CE62");Forma("M3,10 L16,16 V30 L3,24 Z","#E5A431");Forma("M16,16 L28,10 V24 L16,30 Z","#C77E25");
            Forma("M10,6.5 L23,12.5 V17 L20,18.5 V14 L7,8 Z","#FFF0B8");Linha("M6,21 L11,23","#FFF0B8");
        }
        switch(tipo)
        {
            case IconeVarthex.Ajuda:
                Disco(7,2,18,"#6A4F3A");Disco(10,5,12,"#F3C49F");
                Forma("M3,31 V25 C3,16 29,16 29,25 V31 Z","#159DCD");Forma("M12,19 L16,24 L20,19 L23,31 H9 Z","#F7FAFC");
                Linha("M7,12 V18 H10 M25,12 V18 L21,21 H17","#4E6573",2.5);Disco(15,20,4,"#4E6573");break;
            case IconeVarthex.Diagnostico:
                Disco(1,1,23,"#349BC0");Disco(3,3,19,"#E5F5FA");Linha("M5,13 H9 L11,8 L14,17 L17,11 L19,13","#2686B0",1.3);
                Forma("M19,22 L23,18 L32,27 L28,31 Z","#EB9A38");Linha("M23,22 L28,27","#FFD379",1.3);
                Forma("M26,1 L28,4 L31,4 L30,7 L32,10 L28,11 L27,14 L25,11 L22,10 L24,7 L23,4 L26,4 Z","#F1AB39");Disco(25,5,4,"#FBE6AB");break;
            case IconeVarthex.Mesas:
                Caixa(9,13,21,5,"#F7BF28",1);Caixa(11,18,3,10,"#52636B");Caixa(25,18,3,10,"#52636B");
                Caixa(2,3,3,21,"#40545F",1);Caixa(2,18,12,3,"#40545F",1);Caixa(11,21,3,8,"#40545F");Caixa(2,21,3,8,"#40545F");
                Caixa(17,9,5,4,"#F1F6F7",1);Linha("M17,9 H22","#8BA5AF");break;
            case IconeVarthex.Cozinha:
                Caixa(7,17,19,10,"#E0E8EB",2);Caixa(7,23,19,4,"#A7BAC2",1);
                Forma("M8,18 C0,18 1,7 9,8 C10,0 23,0 24,8 C32,7 33,18 25,18 Z","#FAFCFD");
                Linha("M8,18 H25 M9,8 C10,11 11,12 11,14 M24,8 C23,11 22,12 22,14","#87A6B3",1.2);
                Caixa(11,22,11,2,"#38A78A",1);break;
            case IconeVarthex.Painel:
                Caixa(1,5,30,21,"#3B444E",2);Caixa(3,7,26,15,"#721F26",1);Caixa(5,9,22,11,"#351D27",1);
                Linha("M7,11 H11 V18 H7 V11 M15,11 H19 V18 H15 V11 M23,11 H25 V18","#FF625C",1.5);
                Caixa(8,26,3,3,"#71808D");Caixa(23,26,3,3,"#71808D");break;
            case IconeVarthex.Produtos:
                Caixa(4,25,24,4,"#D88930",2);Caixa(3,21,26,3,"#6AA63F",1);
                Forma("M4,20 L9,17 L14,20 L20,17 L28,20 V22 H4 Z","#F7C52E");Caixa(4,17,24,3,"#8F4930",1);
                Forma("M3,15 C3,3 29,3 29,15 Z","#F4AE46");Linha("M10,10 L11,10 M16,8 L17,8 M22,11 L23,11","#FFF0BA",1.3);break;
            case IconeVarthex.NovoProduto:
                CaixaProduto();Disco(17,16,15,"#F3F7F5");Disco(19,18,11,"#2FAD73");Linha("M24.5,20.5 V26.5 M21.5,23.5 H27.5","#FFFFFF",2);break;
            case IconeVarthex.Historico:
                Disco(1,1,30,"#304D86");Disco(3.5,3.5,25,"#C8D7E6");Disco(5,5,22,"#FFFFFF");
                Linha("M16,8 V17 H23","#445873",2);Disco(14.5,15.5,3,"#445873");Linha("M16,5 V7 M16,25 V27 M5,16 H7 M25,16 H27","#8B9EB5",1);break;
            case IconeVarthex.Configuracoes:
                Forma("M13,1 H19 L20,6 L23,8 L28,7 L31,13 L27,17 L27,20 L30,24 L25,29 L21,27 L18,30 H12 L10,26 L7,25 L3,27 L0,21 L4,17 L4,13 L1,9 L6,4 L10,6 Z","#A3ADB7");
                Disco(7,7,19,"#DCE2E7");Disco(10,10,13,"#586E80");Disco(13,13,7,"#F2F5F7");break;
            case IconeVarthex.Numeracao:
                Caixa(2,5,27,23,"#DCEAF4",2);Caixa(2,5,27,5,"#2B8FBC",2);
                Linha("M6,15 L8,13 V23 M13,15 C13,11 19,12 18,16 L13,23 H19 M23,13 H27 L24,17 C29,17 29,23 23,23","#167CA9",2);break;
            case IconeVarthex.Impressoras:
                Caixa(8,1,17,12,"#F3F6F8",1);Linha("M11,4 H21 M11,7 H21","#B0C2CC",1);Caixa(2,10,29,15,"#8DA5B1",2);Caixa(2,10,29,6,"#B9CCD4",2);
                Caixa(7,20,19,3,"#455D69",1);Caixa(9,21,15,10,"#F9FBFC");Linha("M12,25 H21 M12,28 H19","#8BA4B1",1);Disco(25,12,3,"#36B77C");break;
            case IconeVarthex.Colaboradores:
                Disco(20,4,9,"#6D9DBD");Forma("M17,28 V21 C17,14 31,14 31,21 V28 Z","#527FA5");
                Disco(3,5,8,"#F0C368");Forma("M0,28 V22 C0,16 13,16 13,22 V28 Z","#E2AD3E");
                Disco(10,2,12,"#F3C09C");Forma("M10,8 C8,0 23,0 22,8 L18,5 L13,6 Z","#805840");
                Forma("M6,30 V22 C6,15 26,15 26,22 V30 Z","#38AA66");Forma("M12,17 L16,22 L20,17 Z","#EDF8EF");break;
            case IconeVarthex.Senha:
                Linha("M9,15 V9 C9,-1 24,-1 24,9 V15","#98AAB8",3);Caixa(4,13,25,18,"#F5BE23",2);Caixa(4,13,6,18,"#FFDC5B",2);
                Disco(14,18,6,"#795A23");Forma("M16,21 H18 L19,27 H15 Z","#795A23");break;
            case IconeVarthex.Backup:
                Caixa(5,3,23,27,"#7E96A4",2);Caixa(7,5,19,19,"#B6CAD4",1);Caixa(8,25,17,3,"#405967",1);Disco(22,26,1.5,"#62DB9A");
                Forma("M8,12 H13 V7 H17 V12 H22 L15,19 Z","#2884BC");Linha("M20,18 V23 M17.5,20.5 L20,18 L22.5,20.5","#F3F8FA",1.6);break;
            case IconeVarthex.Assinatura:
                Disco(1,1,19,"#D99B1D");Disco(3,3,15,"#FFDA58");Disco(6,6,6,"#AF7C1F");Disco(7,6,4,"#F0F3E9");
                Forma("M16,13 L31,28 L28,31 L24,27 L21,29 L18,26 L20,24 L12,17 Z","#E7AD24");Linha("M17,17 L28,28","#FFE482",1.5);break;
        }
        return new Viewbox { Width=tamanho,Height=tamanho,Child=tela,Stretch=Stretch.Uniform };
    }
}
