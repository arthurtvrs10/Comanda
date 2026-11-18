using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using VarthexComanda.Application.Licenciamento;

try
{
    if(args.Contains("--ajuda")||args.Contains("--help"))
    {
        Console.WriteLine("Varthex Licenças 1.1.0 — emissor Windows offline\nUso: VarthexComanda.Licencas.exe [caminho-da-chave.pem]\nValidades: 1/2/3 meses, vitalícia, dias ou último dia de uso.\nAceita código do computador ou conteúdo do QR Code. Renovação antecipada disponível.\nAs licenças são compatíveis com as edições Normal, Lanchonete e Restaurante do VComanda.\nA chave privada não acompanha o programa. Cada emissão exige confirmação.\nGeração de novo par de chaves: inicializar privada.pem publica.pem (não substitui arquivos).\nUma nova chave pública exige atualizar também o VComanda e o aplicativo Android.");return 0;
    }
    if(args.Length==3&&args[0]=="inicializar")
    {
        if(Path.GetFullPath(args[1]).Equals(Path.GetFullPath(args[2]),StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Use arquivos diferentes para as chaves privada e pública.");
        if(File.Exists(args[1])||File.Exists(args[2]))throw new InvalidOperationException("Chaves já existem; não serão substituídas.");
        using var rsa=RSA.Create(3072);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[2]))!);
        File.WriteAllText(args[1],rsa.ExportPkcs8PrivateKeyPem());File.WriteAllText(args[2],rsa.ExportSubjectPublicKeyInfoPem());
        Console.WriteLine("Par de chaves criado. Guarde a privada fora do computador do cliente. Para utilizar o novo par, atualize a pública também no VComanda e no Android.");return 0;
    }
    if(args.Length>1)throw new ArgumentException("Uso: VarthexComanda.Licencas.exe [caminho-da-chave.pem]. Consulte --ajuda.");
    Console.WriteLine("Varthex Licenças 1.1.0 — uso exclusivo do responsável\nHorário de Brasília (UTC−3). Nenhuma emissão é realizada sem confirmação.\n");
    string Perguntar(string texto){Console.Write(texto+": ");return Console.ReadLine()?.Trim()??"";}
    DateOnly LerData(string texto)=>DateOnly.TryParseExact(Perguntar(texto),"dd/MM/yyyy",CultureInfo.GetCultureInfo("pt-BR"),DateTimeStyles.None,out var dia)?dia:throw new ArgumentException("Informe uma data válida no formato dd/MM/aaaa.");
    var privada=args.Length==1?args[0]:Perguntar("Caminho da chave privada .pem");
    var computador=EmissaoLicenca.LerComputador(Perguntar("Código do computador ou conteúdo do QR Code"));
    var cliente=Perguntar("Nome do cliente/estabelecimento");if(string.IsNullOrWhiteSpace(cliente)||cliente.Length>200)throw new ArgumentException("Informe o cliente com até 200 caracteres.");
    Console.WriteLine("\n1 — 1 mês\n2 — 2 meses\n3 — 3 meses\n0 — Vitalícia\n4 — Quantidade de dias\n5 — Último dia de uso\n");
    if(!int.TryParse(Perguntar("Escolha a validade"),out var opcao)||!Enum.IsDefined((PlanoLicenca)opcao))throw new ArgumentException("Validade inválida.");
    var plano=(PlanoLicenca)opcao;var dias=0;DateOnly? ultimoDia=null,renovacao=null;
    if(plano==PlanoLicenca.Dias&&!int.TryParse(Perguntar("Quantidade de dias (1 a 36.500)"),out dias))throw new ArgumentException("Quantidade de dias inválida.");
    if(plano==PlanoLicenca.Data)ultimoDia=LerData("Último dia de uso (dd/MM/aaaa, inclusive)");
    if(plano is not (PlanoLicenca.Vitalicia or PlanoLicenca.Data))
    {
        var renovar=Perguntar("Renovação antecipada? (S/N)");
        if(renovar.Equals("S",StringComparison.OrdinalIgnoreCase))renovacao=LerData("Vencimento atual, à meia-noite (dd/MM/aaaa)");
        else if(!renovar.Equals("N",StringComparison.OrdinalIgnoreCase)&&renovar.Length!=0)throw new ArgumentException("Responda S ou N para renovação antecipada.");
    }
    var hoje=DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(EmissaoLicenca.FusoBrasilia).DateTime);
    var periodo=EmissaoLicenca.Calcular(plano,hoje,dias,ultimoDia,renovacao);
    Console.WriteLine($"\nConfira a emissão\nCliente: {cliente}\nComputador: {computador}\nInício: {periodo.Inicio:dd/MM/yyyy HH:mm zzz}");
    Console.WriteLine(periodo.Vencimento is { } fim?$"Uso até: {fim.AddDays(-1):dd/MM/yyyy}\nVencimento exclusivo: {fim:dd/MM/yyyy HH:mm zzz}":"Validade: vitalícia");
    if(!Perguntar("Pagamento confirmado ou emissão autorizada? Digite SIM para emitir").Equals("SIM",StringComparison.OrdinalIgnoreCase)){Console.WriteLine("Emissão cancelada. Nenhuma licença foi gerada.");return 0;}
    if(DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(EmissaoLicenca.FusoBrasilia).DateTime)!=hoje)throw new InvalidOperationException("A data mudou. Confira novamente a validade antes de emitir.");
    var pem=File.ReadAllText(privada.Trim('"'));
    using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Varthex.licenca-publica.pem")!)
    using(var leitor=new StreamReader(stream))EmissaoLicenca.ConferirChave(pem,leitor.ReadToEnd());
    var dados=new DadosLicenca(1,Guid.NewGuid().ToString("N"),computador,cliente,periodo.Inicio,periodo.Vencimento);
    var chave=LicencaOffline.Emitir(dados,pem);var pasta=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"VarthexLicencas","emitidas");Directory.CreateDirectory(pasta);
    var arquivo=Path.Combine(pasta,"licenca-"+dados.Id+".txt");File.WriteAllText(arquivo,chave);
    Console.WriteLine($"\nLicença salva em: {arquivo}\n\n{chave}");
    if(!Console.IsInputRedirected){Console.WriteLine("Pressione Enter para fechar.");Console.ReadLine();}return 0;
}
catch(Exception ex)
{
    Console.Error.WriteLine("Não foi possível emitir: "+(ex is CryptographicException?"Confira a chave privada do emissor original.":ex.Message));
    if(!Console.IsInputRedirected){Console.WriteLine("Pressione Enter para fechar.");Console.ReadLine();}return 1;
}
