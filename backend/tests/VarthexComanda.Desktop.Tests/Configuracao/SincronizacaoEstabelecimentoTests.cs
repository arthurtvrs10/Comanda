using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Configuracao;
public class SincronizacaoEstabelecimentoTests
{
    [Fact] public void ContinuarEstabelecimentoSalvaEmGeralSemConcluirAssistenteOuMudarSeguranca()
    {
        using var b=new BancoDeHomologacao();var r=new EfConfiguracaoRepository(b.Fabrica);var geral=new ConfiguracoesSistemaService(r);geral.Salvar(new(){SolicitarSenha=true,CorFundo="#123456"},"Anterior",20,DateTime.UtcNow);
        var s=new PrimeiraConfiguracaoService(b.Fabrica,r,b.Paths);var e=s.Carregar();e.Nome="Restaurante novo";e.TipoDocumento="CPF";e.Cupom.Documento="123.456.789-09";e.Cupom.RazaoSocial="Empresa exemplo";e.Cupom.Telefone="(61) 9 9999-0000";e.Cupom.Endereco="Rua Um";e.Cupom.Numero="10";e.Cupom.Bairro="Centro";e.Cupom.Cep="70000-000";e.Cupom.Cidade="Brasília";e.Cupom.Uf="DF";
        s.SalvarEstabelecimento(e);s.SalvarRascunho(e);
        var salvo=geral.Carregar();Assert.Equal("Restaurante novo",new ObterConfiguracao(r).Executar().NomeEstabelecimento);Assert.Equal(e.Cupom.Documento,salvo.Documento);Assert.Equal("CPF",salvo.TipoDocumento);Assert.Equal(e.Cupom.Endereco,salvo.Endereco);Assert.Equal(e.Cupom.Telefone,salvo.Telefone);Assert.True(salvo.SolicitarSenha);Assert.Equal("#123456",salvo.CorFundo);Assert.True(s.ConfiguracaoPendente);Assert.Equal(20,new ObterConfiguracao(r).Executar().QuantidadeMaximaComandas);
        var retomado=s.Carregar();Assert.Equal(e.Nome,retomado.Nome);Assert.Equal("CPF",retomado.TipoDocumento);
    }
    [Fact] public void GeralAtualizadoSubstituiSomenteCamposAlteradosEPreservaDemaisRascunhos()
    {
        using var b=new BancoDeHomologacao();var r=new EfConfiguracaoRepository(b.Fabrica);var geral=new ConfiguracoesSistemaService(r);geral.Salvar(new(){Telefone="(61) 3333-4444",Endereco="Rua antiga"},"Primeiro",20,DateTime.UtcNow);
        var s=new PrimeiraConfiguracaoService(b.Fabrica,r,b.Paths);var e=s.Carregar();e.Cupom.Bairro="Bairro no rascunho";e.Produtos.Add(new("Produto em edição","UN",100));e.Etapa=3;s.SalvarRascunho(e);
        var p=geral.Carregar();p.Endereco="Rua atualizada";p.Telefone="";p.TipoDocumento="CPF";geral.Salvar(p,"Nome atualizado",20,DateTime.UtcNow);
        e=s.Carregar();Assert.Equal("Nome atualizado",e.Nome);Assert.Equal("Rua atualizada",e.Cupom.Endereco);Assert.Empty(e.Cupom.Telefone);Assert.Equal("CPF",e.TipoDocumento);Assert.Equal("Bairro no rascunho",e.Cupom.Bairro);Assert.Single(e.Produtos);Assert.Equal(3,e.Etapa);
        s.SalvarRascunho(e);Assert.Equal("Nome atualizado",s.Carregar().Nome);
    }
    [Fact] public void RascunhoLegadoNaoReintroduzDadosAntigosDeGeral()
    {
        using var b=new BancoDeHomologacao();var r=new EfConfiguracaoRepository(b.Fabrica);var s=new PrimeiraConfiguracaoService(b.Fabrica,r,b.Paths);var e=EstadoConfiguracao.Exemplo();e.Etapa=4;s.SalvarRascunho(e);
        new ConfiguracoesSistemaService(r).Salvar(new(){Cidade="Cidade atual",TipoDocumento="CPF"},"Nome atual",20,DateTime.UtcNow);
        var retomado=s.Carregar();Assert.Equal("Nome atual",retomado.Nome);Assert.Equal("Cidade atual",retomado.Cupom.Cidade);Assert.Equal("CPF",retomado.TipoDocumento);Assert.NotEmpty(retomado.Produtos);
    }
    [Fact] public void DadosInvalidosNaoAlteramGeralETipoCpfSemDocumentoEhPreservado()
    {
        using var b=new BancoDeHomologacao();var r=new EfConfiguracaoRepository(b.Fabrica);new ConfiguracoesSistemaService(r).Salvar(new(){TipoDocumento="CPF"},"Original",20,DateTime.UtcNow);var s=new PrimeiraConfiguracaoService(b.Fabrica,r,b.Paths);var e=s.Carregar();Assert.Equal("CPF",e.TipoDocumento);e.Nome="";Assert.Throws<ArgumentException>(()=>s.SalvarEstabelecimento(e));Assert.Equal("Original",new ObterConfiguracao(r).Executar().NomeEstabelecimento);
    }
}
