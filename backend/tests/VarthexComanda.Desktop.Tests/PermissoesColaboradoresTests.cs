using System.Text.Json.Nodes;

namespace VarthexComanda.Desktop.Tests;

public sealed class PermissoesColaboradoresTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "vcomanda-permissoes-" + Guid.NewGuid().ToString("N"));
    private string Arquivo => Path.Combine(_pasta, "colaboradores.json");
    public void Dispose() { if (Directory.Exists(_pasta)) Directory.Delete(_pasta, true); }

    [Fact]
    public void CadastroSemLoginPersisteDadosENaoPermiteEntrar()
    {
        var s = new ServicoColaboradores(Arquivo);
        s.Salvar(new() { Nome = "Pessoa Teste", CriarUsuario = false, Email = "teste@example.com", ComissaoPercentual = 2.5m, Funcoes = ["Atendente", "Entregador"] }, null);
        s.Salvar(new() { Nome = "Outra Pessoa", CriarUsuario = false }, null);
        var pessoa = new ServicoColaboradores(Arquivo).Listar().Single(c=>c.Nome=="Pessoa Teste");
        Assert.Equal(2.5m, pessoa.ComissaoPercentual); Assert.Equal(2, pessoa.Funcoes.Length);
        Assert.False(s.Autenticar("", "", out _));
        Assert.False(PermissoesColaborador.Permite(pessoa, "atendimento"));
        Assert.Throws<ArgumentException>(()=>s.Salvar(pessoa with { CriarUsuario = true, Login = "teste" }, null));
        s.Salvar(pessoa with { CriarUsuario = true, Login = "teste" }, "senha-segura");
        Assert.True(s.Autenticar("teste", "senha-segura", out _));
    }

    [Theory]
    [InlineData(-1, "teste@example.com")]
    [InlineData(101, "teste@example.com")]
    [InlineData(5, "invalido")]
    public void ValidacoesNaoPersistemCadastroInvalido(int percentual, string email)
    {
        var s = new ServicoColaboradores(Arquivo);
        Assert.Throws<ArgumentException>(()=>s.Salvar(new() { Nome = "Teste", CriarUsuario = false, ComissaoPercentual = percentual, Email = email }, null));
        Assert.Empty(s.Listar());
    }

    [Fact]
    public void ArquivoLegadoPreservaAcessoMasListaVaziaNegaTudo()
    {
        var s = new ServicoColaboradores(Arquivo);
        var pessoa = new Colaborador { Nome = "Teste", Login = "teste" };
        s.Salvar(pessoa, "senha-segura");
        var json = JsonNode.Parse(File.ReadAllText(Arquivo))!;
        foreach (var campo in new[] { "Permissoes", "CriarUsuario", "AcessoLiberado", "SenhaMestra", "Funcoes", "Email", "ComissaoPercentual" })
            json[0]!["Dados"]!.AsObject().Remove(campo);
        File.WriteAllText(Arquivo, json.ToJsonString());
        Assert.True(s.Autenticar("teste", "senha-segura", out _));
        Assert.True(s.Pode("pagamento")); Assert.False(s.Pode("colaboradores"));
        new ServicoColaboradores(Arquivo).Salvar(pessoa with { Permissoes = [] }, null);
        Assert.False(s.Pode("pagamento")); Assert.False(s.Pode("atendimento"));
        Assert.True(s.SessaoIndividualIniciada);
        new ServicoColaboradores(Arquivo).Salvar(pessoa with { AcessoLiberado = false }, null);
        Assert.False(s.Pode("atendimento")); Assert.Null(s.SessaoAtual);
        Assert.True(s.SessaoIndividualIniciada); // Não volta a ser um acesso anônimo irrestrito.
    }

    [Fact]
    public void SenhaMestraAutorizaSomentePermissaoPermitidaEPreservaOperador()
    {
        var s = new ServicoColaboradores(Arquivo);
        var operador = new Colaborador { Nome = "Caixa Teste", Login = "caixa", Permissoes = ["atendimento"] };
        var responsavel = new Colaborador { Nome = "Responsável Teste", Login = "responsavel", SenhaMestra = true, Permissoes = ["remover-item"] };
        s.Salvar(operador, "senha-caixa"); s.Salvar(responsavel, "senha-responsavel");
        Assert.True(s.Autenticar("caixa", "senha-caixa", out _));
        Assert.False(s.AutorizarComSenhaMestra("responsavel", "senha-responsavel", "pagamento"));
        Assert.False(s.AutorizarComSenhaMestra("responsavel", "errada", "remover-item"));
        Assert.True(s.AutorizarComSenhaMestra("responsavel", "senha-responsavel", "remover-item"));
        Assert.Equal(operador.Id, s.SessaoAtual!.Id); Assert.False(s.Pode("remover-item"));
        var registro = File.ReadAllText(Arquivo + ".autorizacoes.jsonl");
        Assert.Contains(responsavel.Id.ToString(), registro); Assert.Contains(operador.Id.ToString(), registro);
        Assert.DoesNotContain("senha-responsavel", registro);
        new ServicoColaboradores(Arquivo).Salvar(responsavel with { AcessoLiberado = false }, null);
        Assert.False(s.AutorizarComSenhaMestra("responsavel", "senha-responsavel", "remover-item"));
    }

    [Fact]
    public void PropriaContaNaoPodeElevarPrivilegiosENemAlterarArraysDaSessao()
    {
        var s = new ServicoColaboradores(Arquivo);
        var pessoa = new Colaborador { Nome = "Teste", Login = "teste", Permissoes = ["atendimento"] };
        s.Salvar(pessoa, "senha-segura"); s.Autenticar("teste", "senha-segura", out _);
        s.SessaoAtual!.Permissoes![0] = "colaboradores";
        Assert.False(s.Pode("colaboradores"));
        Assert.Throws<InvalidOperationException>(()=>s.Salvar(pessoa with { Perfil = "gestor" }, null));
    }
}
