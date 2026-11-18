namespace VarthexComanda.Desktop;

public sealed record PermissaoColaborador(string Id, string Nome);

public static class PermissoesColaborador
{
    public static readonly IReadOnlyList<PermissaoColaborador> Todas = Array.AsReadOnly(new[]
    {
        new PermissaoColaborador("atendimento", "Atendimento > Mesas, comandas e balcão"),
        new("editar-pedido", "Atendimento > Abrir pedidos, incluir itens e alterar dados"),
        new("remover-item", "Atendimento > Diminuir quantidade e remover itens"),
        new("cancelar-pedido", "Atendimento > Cancelar pedidos"),
        new("transferir", "Atendimento > Juntar comandas, transferir e copiar itens"),
        new("pagamento", "Atendimento > Receber pagamentos e finalizar vendas"),
        new("produtos", "Catálogo > Cadastrar e alterar produtos, complementos e categorias"),
        new("historico-pedidos", "Histórico > Consultar pedidos"),
        new("historico-vendas", "Histórico > Consultar vendas e comprovantes"),
        new("cozinha", "Cozinha > Consultar e atualizar preparos"),
        new("painel", "Atendimento > Painel e chamada de senhas"),
        new("configuracoes", "Configurações > Sistema, impressão, numeração e segurança"),
        new("colaboradores", "Configurações > Gerenciar colaboradores e permissões"),
        new("backup", "Configurações > Backups e restauração"),
        new("servidor", "Configurações > Servidor e acesso pelo celular"),
        new("logs", "Configurações > Logs e diagnóstico"),
        new("assinatura", "Configurações > Assinatura e licença")
    });

    public static string[] Padrao(string perfil) => perfil switch
    {
        "gestor" => Todas.Select(p => p.Id).ToArray(),
        "cozinha" => ["cozinha"],
        _ => ["atendimento", "editar-pedido", "remover-item", "cancelar-pedido", "transferir", "pagamento", "historico-pedidos", "historico-vendas", "cozinha", "painel"]
    };

    public static bool Permite(Colaborador c, string permissao) => c.Ativo && c.CriarUsuario && c.AcessoLiberado &&
        Todas.Any(p => p.Id == permissao) && (c.Perfil == "gestor" || (c.Permissoes ?? Padrao(c.Perfil)).Contains(permissao));
}
