using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Catalogo;

public sealed record ComplementoPermitidoLinha(string Grupo,string Nome,string Preco);

public partial class ProdutosViewModel : ObservableObject
{
    private List<GrupoComplementos> _gruposComplementosProduto = [];
    public List<GrupoComplementos> GruposComplementosProduto { get=>_gruposComplementosProduto; set { _gruposComplementosProduto=value; OnPropertyChanged(); OnPropertyChanged(nameof(ResumoComplementos)); OnPropertyChanged(nameof(ComplementosVisiveis)); } }
    public string CodigoAlimento=>ProdutoSelecionado?.Id.ToString(CultureInfo.InvariantCulture)??"Automático ao salvar";
    public IReadOnlyList<ComplementoPermitidoLinha> ComplementosVisiveis=>GruposComplementosProduto.SelectMany(g=>g.Opcoes.Select(o=>new ComplementoPermitidoLinha(g.Nome,o.Nome,o.PrecoCentavos==0?"Grátis":VarthexComanda.Desktop.Atendimento.CentavosParaMoedaConverter.Formatar(o.PrecoCentavos)))).ToArray();
    public string ResumoComplementos => GruposComplementosProduto.Count == 0 ? "Nenhum grupo configurado" : $"{GruposComplementosProduto.Count} grupo(s) · {GruposComplementosProduto.Sum(g=>g.Opcoes.Count)} opção(ões)";
    public bool CatalogoComplementos { get; init; }
    [ObservableProperty] private bool controlarEstoqueComplemento=true;
    [ObservableProperty] private string quantidadeEstoqueComplemento="0";
    [ObservableProperty] private string minimoComplemento="0";
    [ObservableProperty] private string maximoComplemento="";
    public string TituloLista => $"{(CatalogoComplementos?"Complementos":"Produtos")} ({Produtos.Count})";
    public ProdutosViewModel CriarCatalogoComplementos() => new(_listarCategoriasAtivas,_cadastrarCategoria,_pesquisarProdutos,_cadastrarProduto,_alterarProduto,_desativarProduto,_definirFotoProduto,_removerFotoProduto,_logger) { CatalogoComplementos=true };
    public void PrepararComplementos() { _pesquisarProdutos.PrepararComplementos();Pesquisar(); }
    public IReadOnlyList<Produto> ProdutosParaVinculo() => _pesquisarProdutos.Executar(null,null);
    public IReadOnlyList<Produto> ComplementosParaVinculo() => _pesquisarProdutos.Executar(null,null,true).Where(p=>p.Ativo).ToArray();
    public string? SalvarVinculos(Produto produto,List<GrupoComplementos> grupos)
    {
        if(Autorizar?.Invoke()==false)return "Você não tem permissão para alterar os complementos.";
        var atual=_pesquisarProdutos.Executar(null,null).SingleOrDefault(p=>p.Id==produto.Id);
        if(atual is null)return "Produto não encontrado. Atualize a lista.";
        var resultado=_alterarProduto.Executar(atual.Id,atual.Nome,atual.CategoriaId??0,atual.PrecoCentavos,atual.Ativo,new(atual.CustoCentavos,atual.Descricao,atual.CodigoPersonalizado,atual.UnidadeVenda,grupos));
        return resultado.Sucesso?null:string.Join(" ",resultado.Erros);
    }
    public Func<bool>? Autorizar { get; set; }
    private static readonly CultureInfo CulturaMoeda = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ListarCategoriasAtivas _listarCategoriasAtivas;
    private readonly CadastrarCategoria _cadastrarCategoria;
    private readonly PesquisarProdutos _pesquisarProdutos;
    private readonly CadastrarProduto _cadastrarProduto;
    private readonly AlterarProduto _alterarProduto;
    private readonly DesativarProduto _desativarProduto;
    private readonly DefinirFotoProduto _definirFotoProduto;
    private readonly RemoverFotoProduto _removerFotoProduto;
    private readonly ILogger? _logger;
    public bool AdiarFotoAteSalvar { get; set; }
    private bool _removerFotoPendente;

    public ProdutosViewModel(
        ListarCategoriasAtivas listarCategoriasAtivas,
        CadastrarCategoria cadastrarCategoria,
        PesquisarProdutos pesquisarProdutos,
        CadastrarProduto cadastrarProduto,
        AlterarProduto alterarProduto,
        DesativarProduto desativarProduto,
        DefinirFotoProduto definirFotoProduto,
        RemoverFotoProduto removerFotoProduto,
        ILogger? logger = null)
    {
        _listarCategoriasAtivas = listarCategoriasAtivas;
        _cadastrarCategoria = cadastrarCategoria;
        _pesquisarProdutos = pesquisarProdutos;
        _cadastrarProduto = cadastrarProduto;
        _alterarProduto = alterarProduto;
        _desativarProduto = desativarProduto;
        _definirFotoProduto = definirFotoProduto;
        _removerFotoProduto = removerFotoProduto;
        _logger = logger;

        Categorias = new ObservableCollection<Categoria>();
        Produtos = new ObservableCollection<Produto>();
        Produtos.CollectionChanged += (_,_) => { OnPropertyChanged(nameof(ResumoCatalogo));OnPropertyChanged(nameof(TituloLista)); };

        // Com o banco corrompido (modo de restauração) estas leituras lançam; a construção
        // não pode falhar, senão a MainWindow nem abre. Segue com listas vazias.
        try
        {
            CarregarCategorias();
            Pesquisar();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "CarregarProdutos");
            Categorias.Clear();
            Produtos.Clear();
        }
    }

    public string ResumoCatalogo => CatalogoComplementos ? $"Complementos: {Produtos.Count}     ·     Esgotados: {Produtos.Count(p=>p.ControlarEstoque && p.QuantidadeEstoque==0)}" : $"Total de itens: {Produtos.Count}     ·     Custo médio: {(Produtos.Count == 0 ? 0 : Produtos.Average(p => p.CustoCentavos) / 100d).ToString("C2", CulturaMoeda)}     ·     {(CatalogoComplementos?"Adicional médio":"Venda média")}: {(Produtos.Count == 0 ? 0 : Produtos.Average(p => p.PrecoCentavos) / 100d).ToString("C2", CulturaMoeda)}";
    public ObservableCollection<Categoria> Categorias { get; }
    public ObservableCollection<Produto> Produtos { get; }

    [ObservableProperty]
    private Categoria? categoriaFiltro;

    [ObservableProperty]
    private string textoBusca = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemProdutoSelecionado))]
    [NotifyPropertyChangedFor(nameof(TituloFormulario))]
    private Produto? produtoSelecionado;

    public bool TemProdutoSelecionado => ProdutoSelecionado is not null;

    public string TituloFormulario => ProdutoSelecionado is null ? "NOVO PRODUTO" : "EDITAR PRODUTO";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoverFotoCommand))]
    private string? fotoPendenteCaminho;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoverFotoCommand))]
    private string? fotoArquivoAtual;

    [ObservableProperty]
    private string nomeProduto = string.Empty;

    [ObservableProperty]
    private Categoria? categoriaProduto;

    [ObservableProperty]
    private string precoProdutoReais = string.Empty;

    [ObservableProperty]
    private string custoProdutoReais = string.Empty;
    [ObservableProperty]
    private string descricaoProduto = string.Empty;
    [ObservableProperty]
    private string codigoPersonalizadoProduto = string.Empty;

    [ObservableProperty]
    private bool produtoAtivo = true;
    [ObservableProperty] private UnidadeVenda unidadeProduto;
    public IReadOnlyList<UnidadeVenda> UnidadesVenda { get; } = Enum.GetValues<UnidadeVenda>();
    public bool PermiteMedidaVariavel => !CatalogoComplementos;
    public bool VendaUnidade { get => UnidadeProduto == UnidadeVenda.Unidade; set { if(value) UnidadeProduto=UnidadeVenda.Unidade; } }
    public bool VendaQuilo { get => UnidadeProduto == UnidadeVenda.Quilo; set { if(value) UnidadeProduto=UnidadeVenda.Quilo; } }
    public bool VendaLitro { get => UnidadeProduto == UnidadeVenda.Litro; set { if(value) UnidadeProduto=UnidadeVenda.Litro; } }
    public string RotuloPreco => CatalogoComplementos?"Preço adicional (R$)":"Preço de venda por " + UnidadeProduto.Simbolo() + " (R$)";
    partial void OnUnidadeProdutoChanged(UnidadeVenda value)
    { OnPropertyChanged(nameof(VendaUnidade));OnPropertyChanged(nameof(VendaQuilo));OnPropertyChanged(nameof(VendaLitro));OnPropertyChanged(nameof(RotuloPreco)); }

    [ObservableProperty]
    private string novaCategoriaNome = string.Empty;

    [ObservableProperty]
    private string mensagem = string.Empty;

    partial void OnCategoriaFiltroChanged(Categoria? value) => Pesquisar();

    partial void OnTextoBuscaChanged(string value) => Pesquisar();

    partial void OnProdutoSelecionadoChanged(Produto? value)
    {
        OnPropertyChanged(nameof(CodigoAlimento));
        if (value is null)
        {
            ControlarEstoqueComplemento=CatalogoComplementos;QuantidadeEstoqueComplemento="0";
            MinimoComplemento="0";MaximoComplemento="";
        GruposComplementosProduto = [];
        NomeProduto = string.Empty;
            CategoriaProduto = null;
            PrecoProdutoReais = string.Empty;
            CustoProdutoReais=string.Empty;DescricaoProduto=string.Empty;CodigoPersonalizadoProduto=string.Empty;
            ProdutoAtivo = true;
            UnidadeProduto = UnidadeVenda.Unidade;
            FotoArquivoAtual = null;
            FotoPendenteCaminho = null;
            Mensagem = string.Empty;
            return;
        }

        ControlarEstoqueComplemento=value.ControlarEstoque;QuantidadeEstoqueComplemento=value.QuantidadeEstoque.ToString(CultureInfo.InvariantCulture);
        MinimoComplemento=value.MinimoComplemento.ToString(CultureInfo.InvariantCulture);MaximoComplemento=value.MaximoComplemento?.ToString(CultureInfo.InvariantCulture)??"";
        GruposComplementosProduto = value.GruposComplementos;
        NomeProduto = value.Nome;
        CategoriaProduto = Categorias.FirstOrDefault(c => c.Id == value.CategoriaId);
        PrecoProdutoReais = (value.PrecoCentavos / 100m).ToString("0.00", CulturaMoeda);
        CustoProdutoReais=(value.CustoCentavos/100m).ToString("0.00",CulturaMoeda);
        DescricaoProduto=value.Descricao??string.Empty;CodigoPersonalizadoProduto=value.CodigoPersonalizado??string.Empty;
        ProdutoAtivo = value.Ativo;
        UnidadeProduto = value.UnidadeVenda;
        FotoArquivoAtual = value?.FotoArquivo;
        FotoPendenteCaminho = null;
        Mensagem = string.Empty;
    }

    [RelayCommand]
    private void Pesquisar()
    {
        var resultado = _pesquisarProdutos.Executar(CatalogoComplementos?null:CategoriaFiltro?.Id, TextoBusca,CatalogoComplementos);
        Produtos.Clear();
        foreach (var produto in resultado)
        {
            Produtos.Add(produto);
        }
    }

    [RelayCommand]
    private void LimparFiltro()
    {
        CategoriaFiltro = null;
        TextoBusca = string.Empty;
    }

    [RelayCommand]
    private void Novo()
    {
        MinimoComplemento="0";MaximoComplemento="";
        _removerFotoPendente = false;
        FotoArquivoAtual = null;
        NovaCategoriaNome = string.Empty;
        ProdutoSelecionado = null;
        GruposComplementosProduto = [];
        NomeProduto = string.Empty;
        CategoriaProduto = null;
        PrecoProdutoReais = string.Empty;
        CustoProdutoReais=string.Empty;DescricaoProduto=string.Empty;CodigoPersonalizadoProduto=string.Empty;
        ProdutoAtivo = true;
        UnidadeProduto = UnidadeVenda.Unidade;
        FotoPendenteCaminho = null;
        Mensagem = string.Empty;
    }

    [RelayCommand]
    private void Salvar()
    {
        if (Autorizar?.Invoke() == false) { Mensagem = "Você não tem permissão para alterar o catálogo."; return; }
        if (!CatalogoComplementos && CategoriaProduto is null)
        {
            Mensagem = "Selecione uma categoria.";
            return;
        }
        if (!TentarConverterPreco(PrecoProdutoReais, out var precoCentavos,CatalogoComplementos))
        {
            Mensagem = "Informe um preço válido.";
            return;
        }

        var criando = ProdutoSelecionado is null;
        long custoCentavos=0;
        if(!string.IsNullOrWhiteSpace(CustoProdutoReais)&&!TentarConverterPreco(CustoProdutoReais,out custoCentavos,true))
        {Mensagem="Informe um preço de custo válido, maior ou igual a zero.";return;}
        long estoque=0;
        if(CatalogoComplementos && (!long.TryParse(QuantidadeEstoqueComplemento,NumberStyles.None,CultureInfo.InvariantCulture,out estoque)||estoque<0))
        {Mensagem="Informe uma quantidade inteira de estoque, maior ou igual a zero.";return;}
        int minimo=0;int? maximo=null;
        if(CatalogoComplementos)
        {
            if(!int.TryParse(MinimoComplemento,NumberStyles.None,CultureInfo.InvariantCulture,out minimo)) { Mensagem="Informe um mínimo inteiro, maior ou igual a zero.";return; }
            if(!string.IsNullOrWhiteSpace(MaximoComplemento))
            { if(!int.TryParse(MaximoComplemento,NumberStyles.None,CultureInfo.InvariantCulture,out var limite)){Mensagem="Informe um máximo inteiro, maior ou igual a zero, ou deixe vazio para usar o estoque.";return;}maximo=limite; }
        }
        var detalhes=new DetalhesProduto(custoCentavos,DescricaoProduto,CodigoPersonalizadoProduto,UnidadeProduto,GruposComplementosProduto,
            CatalogoComplementos?ControlarEstoqueComplemento:null,CatalogoComplementos?estoque:null,CatalogoComplementos?ProdutoSelecionado?.VersaoEstoque:null,minimo,maximo);

        try
        {
            var resultado = ProdutoSelecionado is null
                ? _cadastrarProduto.Executar(NomeProduto, CategoriaProduto?.Id??0, precoCentavos,detalhes,ProdutoAtivo,CatalogoComplementos)
                : _alterarProduto.Executar(ProdutoSelecionado.Id, NomeProduto, CategoriaProduto?.Id??0, precoCentavos, ProdutoAtivo,detalhes);

            if (!resultado.Sucesso)
            {
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            if (criando && FotoPendenteCaminho is not null)
            {
                var criado = resultado.Valor!;
                string? falhaFoto = null;
                try
                {
                    var resultadoFoto = _definirFotoProduto.Executar(criado.Id, FotoPendenteCaminho);
                    if (!resultadoFoto.Sucesso)
                    {
                        falhaFoto = "Produto cadastrado, mas a foto não foi adicionada: " + string.Join(" ", resultadoFoto.Erros);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "Falha em {Operacao}", "SalvarFotoDoProduto");
                    falhaFoto = "Produto cadastrado, mas não foi possível salvar a foto. Tente novamente.";
                }

                if (falhaFoto is not null)
                {
                    // O produto continua cadastrado: limpa os filtros para ele aparecer na lista e o
                    // seleciona para o usuario tentar a foto de novo (sem risco de cadastrar duplicado).
                    CategoriaFiltro = null;
                    TextoBusca = string.Empty;
                    Pesquisar();
                    var selecionado = Produtos.FirstOrDefault(p => p.Id == criado.Id);
                    if (selecionado is not null)
                    {
                        ProdutoSelecionado = selecionado;
                        Mensagem = falhaFoto;
                    }
                    else
                    {
                        Novo();
                        Mensagem = falhaFoto + " Localize o produto na lista para adicionar a foto.";
                    }
                    return;
                }
            }

            if (!criando && AdiarFotoAteSalvar && (FotoPendenteCaminho is not null || _removerFotoPendente))
            {
                var id = resultado.Valor!.Id;
                if (FotoPendenteCaminho is not null)
                {
                    var foto = _definirFotoProduto.Executar(id, FotoPendenteCaminho);
                    if (!foto.Sucesso) { Mensagem = "Dados salvos, mas a foto não foi atualizada: " + string.Join(" ", foto.Erros); return; }
                }
                else
                {
                    var foto = _removerFotoProduto.Executar(id);
                    if (!foto.Sucesso) { Mensagem = "Dados salvos, mas a foto não foi removida: " + string.Join(" ", foto.Erros); return; }
                }
            }
            Novo();
            Pesquisar();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "SalvarProduto");
            Mensagem = "Não foi possível salvar o produto. Tente novamente.";
        }
    }

    [RelayCommand]
    private void Desativar()
    {
        if (Autorizar?.Invoke() == false) { Mensagem = "Você não tem permissão para alterar o catálogo."; return; }
        if (ProdutoSelecionado is null)
        {
            Mensagem = "Selecione um produto para desativar.";
            return;
        }

        try
        {
            var resultado = _desativarProduto.Executar(ProdutoSelecionado.Id);
            if (!resultado.Sucesso)
            {
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            Novo();
            Pesquisar();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "DesativarProduto");
            Mensagem = "Não foi possível desativar o produto. Tente novamente.";
        }
    }

    public void DefinirFoto(string caminhoOrigem)
    {
        if (ProdutoSelecionado is null || AdiarFotoAteSalvar)
        {
            _removerFotoPendente = false;
            FotoPendenteCaminho = caminhoOrigem;
            Mensagem = string.Empty;
            return;
        }

        try
        {
            var resultado = _definirFotoProduto.Executar(ProdutoSelecionado.Id, caminhoOrigem);
            if (!resultado.Sucesso)
            {
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            ProdutoSelecionado.FotoArquivo = resultado.Valor!.FotoArquivo;
            FotoArquivoAtual = resultado.Valor.FotoArquivo;
            Mensagem = string.Empty;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "DefinirFoto");
            Mensagem = "Não foi possível salvar a foto. Tente novamente.";
        }
    }

    [RelayCommand(CanExecute = nameof(PodeRemoverFoto))]
    private void RemoverFoto()
    {
        if (AdiarFotoAteSalvar)
        {
            _removerFotoPendente = true;
            FotoPendenteCaminho = null;
            FotoArquivoAtual = null;
            return;
        }
        if (ProdutoSelecionado is null)
        {
            FotoPendenteCaminho = null;
            return;
        }

        try
        {
            var resultado = _removerFotoProduto.Executar(ProdutoSelecionado.Id);
            if (!resultado.Sucesso)
            {
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            ProdutoSelecionado.FotoArquivo = null;
            FotoArquivoAtual = null;
            Mensagem = string.Empty;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "RemoverFoto");
            Mensagem = "Não foi possível remover a foto. Tente novamente.";
        }
    }

    private bool PodeRemoverFoto() => FotoArquivoAtual is not null || FotoPendenteCaminho is not null;

    [RelayCommand]
    private void AdicionarCategoria()
    {
        if (Autorizar?.Invoke() == false) { Mensagem = "Você não tem permissão para alterar o catálogo."; return; }
        try
        {
            var resultado = _cadastrarCategoria.Executar(NovaCategoriaNome);
            if (!resultado.Sucesso)
            {
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            NovaCategoriaNome = string.Empty;
            Mensagem = string.Empty;
            CarregarCategorias();
            CategoriaProduto = Categorias.FirstOrDefault(c => c.Id == resultado.Valor!.Id);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "AdicionarCategoria");
            Mensagem = "Não foi possível adicionar a categoria. Tente novamente.";
        }
    }

    private void CarregarCategorias()
    {
        var categoriaSelecionadaId = CategoriaProduto?.Id;
        Categorias.Clear();
        foreach (var categoria in _listarCategoriasAtivas.Executar())
        {
            Categorias.Add(categoria);
        }
        if (categoriaSelecionadaId is not null)
        {
            CategoriaProduto = Categorias.FirstOrDefault(c => c.Id == categoriaSelecionadaId);
        }
    }

    private static bool TentarConverterPreco(string texto, out long precoCentavos,bool permitirZero=false)
    {
        precoCentavos = 0;
        const NumberStyles estilo = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        if (!decimal.TryParse(texto, estilo, CulturaMoeda, out var valor))
        {
            return false;
        }
        if (valor < 0 || (!permitirZero&&valor==0) || valor>long.MaxValue/100m)
        {
            return false;
        }
        precoCentavos = (long)Math.Round(valor * 100m, MidpointRounding.AwayFromZero);
        return true;
    }
}
