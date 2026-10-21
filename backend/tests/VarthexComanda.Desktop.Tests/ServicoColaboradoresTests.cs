namespace VarthexComanda.Desktop.Tests;

public sealed class ServicoColaboradoresTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(),"varthex-colaboradores-"+Guid.NewGuid().ToString("N"));
    private string Arquivo => Path.Combine(_pasta,"colaboradores.json");
    public void Dispose() { if(Directory.Exists(_pasta))Directory.Delete(_pasta,true); }

    [Fact]
    public void ContasPersistemComHashEPerfilRealSemSubstituirCredencialLegada()
    {
        var servico=new ServicoColaboradores(Arquivo);
        var pessoa=new Colaborador { Nome="Ana Teste",Login="ana",Perfil="gestor",Funcao="Gerência" };
        servico.Salvar(pessoa,"senha-segura");
        Assert.DoesNotContain("senha-segura",File.ReadAllText(Arquivo));
        var novaInstancia=new ServicoColaboradores(Arquivo);
        Assert.True(novaInstancia.Autenticar(" ANA ","senha-segura",out _));
        Assert.Equal(pessoa.Id,novaInstancia.SessaoAtual?.Id);
        var master=new AcessoGestor(Path.Combine(_pasta,"gestor.credencial"));
        master.Cadastrar("senha-responsavel");
        Assert.True(master.EntrarColaborador(novaInstancia));
        Assert.True(master.AutenticadoPorColaborador);
        Assert.Throws<InvalidOperationException>(()=>master.AlterarSenha("senha-responsavel","alteracao-indevida"));
        Assert.True(master.ConferirSenha("senha-responsavel"));
        master.Sair();Assert.False(master.Autenticado);
        Assert.False(master.AutenticadoPorColaborador);
    }

    [Fact]
    public void LoginDuplicadoOuSenhaFracaNaoAlteraContaExistente()
    {
        var servico=new ServicoColaboradores(Arquivo);
        var pessoa=new Colaborador { Nome="Ana Teste",Login="ana" };
        servico.Salvar(pessoa,"senha-segura");
        Assert.Throws<ArgumentException>(()=>servico.Salvar(new Colaborador { Nome="Outra",Login="ANA" },"outra-senha"));
        Assert.Throws<ArgumentException>(()=>servico.Salvar(pessoa with { Nome="Nome alterado" },"123"));
        Assert.Single(servico.Listar());Assert.Equal("Ana Teste",servico.Listar()[0].Nome);
        Assert.True(servico.Autenticar("ana","senha-segura",out _));
    }

    [Fact]
    public void SenhaIndividualExigePropriaSessaoESenhaAtualEInativoNaoEntra()
    {
        var servico=new ServicoColaboradores(Arquivo);
        var pessoa=new Colaborador { Nome="João Teste",Login="joao",Perfil="garcom" };
        servico.Salvar(pessoa,"senha-segura");
        Assert.Throws<InvalidOperationException>(()=>servico.AlterarSenha(pessoa.Id,"senha-segura","nova-senha"));
        Assert.True(servico.Autenticar("joao","senha-segura",out _));
        Assert.Throws<InvalidOperationException>(()=>servico.AlterarSenha(Guid.NewGuid(),"senha-segura","nova-senha"));
        Assert.Throws<InvalidOperationException>(()=>servico.AlterarSenha(pessoa.Id,"errada","nova-senha"));
        servico.AlterarSenha(pessoa.Id,"senha-segura","nova-senha");
        Assert.False(servico.Autenticar("joao","senha-segura",out _));
        Assert.True(servico.Autenticar("joao","nova-senha",out _));
        var equipe=new AcessoAtendimento(Path.Combine(_pasta,"equipe.credencial"));
        Assert.True(equipe.EntrarColaborador(servico));
        Assert.False(new AcessoGestor(Path.Combine(_pasta,"gestor.credencial")).EntrarColaborador(servico));
        // Uma alteração de outra instância também é verificada ao conceder novo acesso.
        new ServicoColaboradores(Arquivo).Salvar(pessoa with { Ativo=false },null);
        Assert.False(equipe.EntrarColaborador(servico));
        Assert.Null(servico.SessaoAtual);
        Assert.False(servico.Autenticar("joao","nova-senha",out _));
    }
}
