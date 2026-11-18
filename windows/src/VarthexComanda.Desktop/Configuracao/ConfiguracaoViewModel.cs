using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public partial class ConfiguracaoViewModel : ObservableObject
{
    private readonly ObterConfiguracao _obterConfiguracao;
    private readonly SalvarConfiguracao _salvarConfiguracao;
    private readonly ILogger? _logger;
    private (string Nome, string Quantidade, string? Pasta) _salvo;
    public bool TemAlteracoes => _salvo != (NomeEstabelecimento, QuantidadeComandasTexto, PastaBackupExterna);
    public string EstadoEdicao => TemAlteracoes ? "Alterações não salvas" : "Nenhuma alteração pendente";
    [ObservableProperty] private bool mensagemErro;
    private void AtualizarEdicao() { OnPropertyChanged(nameof(TemAlteracoes)); OnPropertyChanged(nameof(EstadoEdicao)); Mensagem = ""; }
    partial void OnNomeEstabelecimentoChanged(string value) => AtualizarEdicao();
    partial void OnQuantidadeComandasTextoChanged(string value) => AtualizarEdicao();
    partial void OnPastaBackupExternaChanged(string? value) => AtualizarEdicao();
    private void MarcarSalvo() { _salvo = (NomeEstabelecimento, QuantidadeComandasTexto, PastaBackupExterna); OnPropertyChanged(nameof(TemAlteracoes)); OnPropertyChanged(nameof(EstadoEdicao)); }
    [RelayCommand] private void Descartar() { var salvo = _salvo; NomeEstabelecimento = salvo.Nome; QuantidadeComandasTexto = salvo.Quantidade; PastaBackupExterna = salvo.Pasta; MensagemErro = false; Mensagem = "Alterações descartadas."; }

    public ConfiguracaoViewModel(ObterConfiguracao obterConfiguracao, SalvarConfiguracao salvarConfiguracao, ILogger? logger = null)
    {
        _obterConfiguracao = obterConfiguracao;
        _salvarConfiguracao = salvarConfiguracao;
        _logger = logger;

        // Com o banco corrompido (modo de restauração) a leitura lança; a construção não pode
        // falhar, senão a MainWindow nem abre. Segue com os campos vazios.
        try
        {
            Carregar();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "CarregarConfiguracao");
        }
    }

    public string VersaoTexto => $"Versão {VersaoDoAplicativo.Atual}";

    [ObservableProperty]
    private string nomeEstabelecimento = string.Empty;

    [ObservableProperty]
    private string quantidadeComandasTexto = string.Empty;

    [ObservableProperty]
    private string? pastaBackupExterna;

    [ObservableProperty]
    private string mensagem = string.Empty;

    public void Carregar()
    {
        var configuracao = _obterConfiguracao.Executar();
        NomeEstabelecimento = configuracao.NomeEstabelecimento;
        QuantidadeComandasTexto = configuracao.QuantidadeMaximaComandas?.ToString() ?? string.Empty;
        PastaBackupExterna = configuracao.PastaBackupExterna;
        MarcarSalvo();
        Mensagem = "";
    }

    public void DefinirPastaBackupExterna(string? pasta)
    {
        PastaBackupExterna = pasta;
    }

    [RelayCommand]
    private void Salvar()
    {
        MensagemErro = true;
        int? quantidade = null;
        if (!string.IsNullOrWhiteSpace(QuantidadeComandasTexto))
        {
            if (!int.TryParse(QuantidadeComandasTexto, out var quantidadeValor))
            {
                Mensagem = "Informe uma quantidade de comandas válida.";
                return;
            }

            quantidade = quantidadeValor;
        }

        try
        {
            var resultado = _salvarConfiguracao.Executar(new ConfiguracaoEstabelecimento
            {
                NomeEstabelecimento = NomeEstabelecimento,
                QuantidadeMaximaComandas = quantidade,
                PastaBackupExterna = PastaBackupExterna
            });

            Mensagem = resultado.Sucesso
                ? "Configurações salvas com sucesso."
                : string.Join(" ", resultado.Erros);
            MensagemErro = !resultado.Sucesso;
            if (resultado.Sucesso) MarcarSalvo();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "SalvarConfiguracao");
            Mensagem = "Não foi possível salvar as configurações. Tente novamente.";
        }
    }
}
