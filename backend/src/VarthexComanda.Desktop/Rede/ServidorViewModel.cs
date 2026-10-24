using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Rede;

public partial class ServidorViewModel : ObservableObject
{
    private readonly ServidorLocal _servidor;
    private readonly ConfiguracaoServidorService? _preferencias;
    private readonly Func<IReadOnlyList<RedeServidor>> _lerRedes;
    private ConfiguracaoServidor _salva=new();
    private bool _tentouAutomatico,_configuracaoValida=true,_falhaInicio,_encerrado;
    private string _operacao="";
    private DateTime _proximaRede;
    public string NomeHost { get; }=Environment.MachineName;
    public string CaminhoBanco { get; }
    [ObservableProperty] private string porta="3001";
    [ObservableProperty] private bool iniciarAutomaticamente=true;
    [ObservableProperty] private bool ocupada;
    [ObservableProperty] private string estado="Servidor parado";
    [ObservableProperty] private string mensagem="";
    [ObservableProperty] private string? enderecoSelecionado;
    [ObservableProperty] private IReadOnlyList<string> enderecos=Array.Empty<string>();
    [ObservableProperty] private IReadOnlyList<RedeServidor> redes=Array.Empty<RedeServidor>();
    [ObservableProperty] private IReadOnlyList<AcessoServidor> acessos=Array.Empty<AcessoServidor>();
    public bool Ativo=>_servidor.Ativo;
    public bool PodeMudarPorta=>!Ocupada&&!Ativo&&!_servidor.Iniciando&&!_servidor.Parando;
    public bool PodeCopiar=>!Ocupada&&Ativo&&!_servidor.Parando&&LinkAcesso is not null;
    public string TextoAlternar=>Ativo?"Parar servidor":"Iniciar servidor";
    public string TipoEstado=>_servidor.Iniciando||_servidor.Parando||_operacao=="iniciar"?"aguarde":Ativo?"ativo":_falhaInicio?"erro":"parado";
    public bool PodeAlterarPreferencias=>!Ocupada;
    public string PortaEscuta=>Ativo?$"TCP {_servidor.Porta} · Escutando neste computador":"Sem porta em escuta";
    public string TempoAtivo=>_servidor.IniciadoEm is DateTime inicio?$"Iniciado às {Application.Atendimento.FusoBrasilia.ParaLocal(inicio):HH:mm:ss}":"O servidor acompanha o aplicativo Comanda.";
    public string ResumoAcessos=>$"{Acessos.Count} dispositivo{(Acessos.Count==1?"":"s")} nesta sessão";
    public bool SemAcessos=>Acessos.Count==0;
    public string OrientacaoEndereco=>EnderecoSelecionado?.Contains("127.0.0.1")==true
        ?"Este endereço funciona apenas neste PC. Para outro aparelho, selecione o IP da rede."
        :"No outro PC ou celular, conecte à mesma rede e leia o QR Code ou abra o link de acesso.";
    public string? LinkAcesso
    {
        get
        {
            try{return Ativo&&EnderecoSelecionado is not null?_servidor.LinkAcesso(EnderecoSelecionado):null;}
            catch(InvalidOperationException){return null;}
        }
    }

    public ServidorViewModel(ServidorLocal servidor,string caminhoBanco="",ConfiguracaoServidorService? preferencias=null,Func<IReadOnlyList<RedeServidor>>? lerRedes=null)
    {
        _servidor=servidor;_preferencias=preferencias;_lerRedes=lerRedes??InformacoesServidor.LerRedes;CaminhoBanco=caminhoBanco;
        try{_salva=preferencias?.Carregar()??new();porta=_salva.Porta.ToString();iniciarAutomaticamente=_salva.IniciarAutomaticamente;}
        catch{_configuracaoValida=false;iniciarAutomaticamente=false;_falhaInicio=true;mensagem="Configuração do servidor inválida. Confira a porta e use Iniciar servidor para salvar novamente.";}
        Atualizar(true);
    }
    partial void OnEnderecoSelecionadoChanged(string? value)
    {OnPropertyChanged(nameof(LinkAcesso));OnPropertyChanged(nameof(PodeCopiar));OnPropertyChanged(nameof(OrientacaoEndereco));}
    partial void OnOcupadaChanged(bool value)
    {OnPropertyChanged(nameof(PodeMudarPorta));OnPropertyChanged(nameof(PodeCopiar));OnPropertyChanged(nameof(PodeAlterarPreferencias));AlternarCommand.NotifyCanExecuteChanged();TestarCommand.NotifyCanExecuteChanged();AtualizarInformacoesCommand.NotifyCanExecuteChanged();}
    public void Atualizar(bool atualizarRedes=false)
    {
        if(_encerrado)return;
        Estado=_servidor.Parando?"Parando o servidor…":_servidor.Iniciando||_operacao=="iniciar"?"Iniciando o servidor…":Ativo?"Servidor em funcionamento":_falhaInicio?"Servidor não iniciado":"Servidor parado";
        if(Ativo&&!Ocupada)Porta=_servidor.Porta.ToString();
        var atuais=_servidor.Enderecos;
        if(!Enderecos.SequenceEqual(atuais))Enderecos=atuais;
        if(EnderecoSelecionado is null||!atuais.Contains(EnderecoSelecionado))EnderecoSelecionado=atuais.FirstOrDefault();
        var acessosAtuais=_servidor.Acessos;if(!Acessos.SequenceEqual(acessosAtuais))Acessos=acessosAtuais;
        if(atualizarRedes||DateTime.UtcNow>=_proximaRede)
        {
            try{var atuaisRedes=_lerRedes();if(!Redes.SequenceEqual(atuaisRedes))Redes=atuaisRedes;}
            catch{Redes=[new("Não foi possível consultar as interfaces","Rede","—","Indisponível")];}
            _proximaRede=DateTime.UtcNow.AddSeconds(10);
        }
        foreach(var propriedade in new[]{nameof(Ativo),nameof(TipoEstado),nameof(PodeMudarPorta),nameof(PodeCopiar),nameof(TextoAlternar),nameof(PortaEscuta),nameof(TempoAtivo),nameof(ResumoAcessos),nameof(SemAcessos),nameof(LinkAcesso)})OnPropertyChanged(propriedade);
        AlternarCommand.NotifyCanExecuteChanged();TestarCommand.NotifyCanExecuteChanged();
    }
    private bool PodeAlternar()=>!Ocupada&&!_encerrado&&!_servidor.Iniciando&&!_servidor.Parando;
    private bool PodeTestar()=>PodeAlternar()&&Ativo;
    private bool PodeAtualizar()=>!Ocupada&&!_encerrado;
    [RelayCommand(CanExecute=nameof(PodeAtualizar))] private void AtualizarInformacoes()=>Atualizar(true);

    public async Task IniciarAutomaticamenteAsync()
    {
        if(_tentouAutomatico||_encerrado)return;_tentouAutomatico=true;
        if(!_configuracaoValida||!IniciarAutomaticamente||Ativo)return;
        await IniciarAsync(automatico:true);
    }
    [RelayCommand(CanExecute=nameof(PodeAlternar))]
    private async Task AlternarAsync()
    {
        if(!Ativo){await IniciarAsync(automatico:false);return;}
        Ocupada=true;Mensagem="Parando o servidor…";
        try{await Task.Run(()=>_servidor.PararAsync());Mensagem="Servidor parado. Os links anteriores não permitem novos lançamentos.";}
        catch{Mensagem="Não foi possível concluir a parada. Confira o estado antes de restaurar um backup.";}
        finally{Ocupada=false;Atualizar();}
    }
    private async Task IniciarAsync(bool automatico)
    {
        if(!int.TryParse(Porta,out var valor)||valor is <1024 or >65535){Mensagem="Informe uma porta entre 1024 e 65535.";return;}
        Ocupada=true;_operacao="iniciar";_falhaInicio=false;Mensagem="";Atualizar();
        try
        {
            if(!automatico){_salva=new(IniciarAutomaticamente,valor);_preferencias?.Salvar(_salva,DateTime.UtcNow);_configuracaoValida=true;}
            await Task.Run(()=>_servidor.IniciarAsync(valor));
            Mensagem=automatico?"Servidor iniciado automaticamente. Fechar esta janela mantém o acesso disponível.":"Servidor iniciado. Leia o QR Code ou copie o link para conectar outro aparelho.";
        }
        catch
        {
            _falhaInicio=true;
            Mensagem="Não foi possível iniciar o servidor. Confira as configurações, a licença e se a porta está livre. O Comanda continua disponível neste PC.";
        }
        finally{_operacao="";Ocupada=false;Atualizar();}
    }
    [RelayCommand] private void SalvarInicializacao()
    {
        try{var nova=_salva with {IniciarAutomaticamente=IniciarAutomaticamente};_preferencias?.Salvar(nova,DateTime.UtcNow);_salva=nova;Mensagem=IniciarAutomaticamente?"Início automático ativado para as próximas aberturas do Comanda.":"Início automático desativado. O servidor atual só para ao usar Parar servidor ou fechar o Comanda.";}
        catch{IniciarAutomaticamente=_salva.IniciarAutomaticamente;Mensagem="Não foi possível salvar a preferência. Tente novamente.";}
    }
    [RelayCommand(CanExecute=nameof(PodeTestar))]
    private async Task TestarAsync()
    {
        Ocupada=true;Mensagem="Testando o servidor neste computador…";
        try
        {
            using var cliente=new HttpClient{Timeout=TimeSpan.FromSeconds(3)};
            using var resposta=await cliente.GetAsync($"http://127.0.0.1:{_servidor.Porta}/");resposta.EnsureSuccessStatusCode();
            Mensagem="O servidor respondeu neste PC. A conexão pela rede também depende do Wi-Fi e do Firewall do Windows.";
        }
        catch{Mensagem="O servidor não respondeu ao teste local. Atualize as informações e confira a porta.";}
        finally{Ocupada=false;Atualizar();}
    }
    public void Encerrar(){_encerrado=true;_servidor.Dispose();}
}
