using VarthexComanda.Application.Licenciamento;
var folder = args[0];
var publicKey = File.ReadAllText(Path.Combine(folder,"public.pem"));
string[] dates = ["2028-01-31", "2027-01-31", "2026-11-30", "2026-09-30"];
for (var months=0; months<=3; months++) {
    var token=File.ReadAllText(Path.Combine(folder,$"license-{months}.txt"));
    var start=DateTimeOffset.Parse(dates[months]+"T00:00:00-03:00");
    var data=LicencaOffline.Validar(token,publicKey,new string('A',64),start);
    if(data.Cliente!="João \"Teste\" \\ Café\n🍔" || data.Inicio!=start || data.Vencimento!=(months==0 ? (DateTimeOffset?)null : start.AddMonths(months))) throw new Exception("Payload incompatível");
    if(months>0) {
        try {LicencaOffline.Validar(token,publicKey,new string('A',64),start.AddMonths(months)); throw new Exception("Licença vencida aceita");}
        catch(InvalidOperationException) {}
    }
}
Console.WriteLine(".NET: 4 licenças Java aceitas, datas e nomes preservados, 3 vencimentos rejeitados.");
var customStart=DateTimeOffset.Parse("2028-02-28T00:00:00-03:00");
string[] ends=["2028-03-06", "2028-03-16", "2028-03-25"];
for(var i=0;i<ends.Length;i++) {
    var token=File.ReadAllText(Path.Combine(folder,$"custom-{i}.txt"));
    var end=DateTimeOffset.Parse(ends[i]+"T00:00:00-03:00");
    var data=LicencaOffline.Validar(token,publicKey,new string('A',64),customStart);
    if(data.Inicio!=customStart || data.Vencimento!=end)throw new Exception("Período personalizado incompatível");
    LicencaOffline.Validar(token,publicKey,new string('A',64),end.AddTicks(-1));
    try {LicencaOffline.Validar(token,publicKey,new string('A',64),end);throw new Exception("Vencida aceita");}catch(InvalidOperationException){}
    try {LicencaOffline.Validar(token,publicKey,new string('B',64),customStart);throw new Exception("Outra máquina aceita");}catch(InvalidOperationException){}
}
Console.WriteLine(".NET: 3 licenças personalizadas aceitas, renovação já utilizável, limites de vencimento e outra máquina verificados.");

var syntheticPrivate=File.ReadAllText(Path.Combine(folder,"private-synthetic.pem"));
EmissaoLicenca.ConferirChave(syntheticPrivate,publicKey);
for(var plano=0;plano<=5;plano++){
    var p=EmissaoLicenca.Calcular((PlanoLicenca)plano,new(2028,2,28),15,new(2028,3,15),plano==4?new DateOnly(2028,3,10):null);
    var dados=new DadosLicenca(1,"windows-sintetica-"+plano,new string('A',64),"Cliente de teste Windows",p.Inicio,p.Vencimento);
    var token=LicencaOffline.Emitir(dados,syntheticPrivate);LicencaOffline.Validar(token,publicKey,dados.Computador,p.Inicio);
    File.WriteAllText(Path.Combine(folder,"windows-"+plano+".txt"),token);
}
Console.WriteLine(".NET: 6 planos do emissor Windows emitidos e aceitos com chave sintética.");
