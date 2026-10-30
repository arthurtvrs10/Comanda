import com.varthex.licencas.LicenseIssuer;
import com.varthex.licencas.LicensePeriod;
import com.varthex.licencas.ActivationQr;
import com.google.zxing.*;
import com.google.zxing.common.HybridBinarizer;
import javax.imageio.ImageIO;
import java.nio.file.*;
import java.nio.charset.StandardCharsets;
import java.security.*;
import java.time.LocalDate;
import java.util.Base64;

public class IssuerTest {
    public static void main(String[] args) throws Exception {
        if(args[0].equals("--verify-windows")){verifyWindows(Paths.get(args[1]));return;}
        Path out=Paths.get(args[0]); Files.createDirectories(out);
        KeyPairGenerator generator=KeyPairGenerator.getInstance("RSA"); generator.initialize(2048);
        KeyPair pair=generator.generateKeyPair();
        String privatePem=pem("PRIVATE KEY",pair.getPrivate().getEncoded());
        String publicPem=pem("PUBLIC KEY",pair.getPublic().getEncoded());
        PrivateKey key=LicenseIssuer.importKey(privatePem,publicPem);
        LicenseIssuer.importKey(" \r\n"+privatePem.replace("\n","\r\n")+"\r\n ",publicPem);
        expectFailure(()->LicenseIssuer.importKey(privatePem.replace("-----END PRIVATE KEY-----",""),publicPem));
        expectFailure(()->LicenseIssuer.importKey(publicPem,publicPem));
        expectFailure(()->LicenseIssuer.importKey("",publicPem));
        expectFailure(()->LicenseIssuer.importKey(null,publicPem));
        String pc=new String(new char[64]).replace('\0','A');
        if(!ActivationQr.machine(pc.toLowerCase()).equals(pc))throw new AssertionError("Código manual normalizado");
        String qr="varthex://ativacao?v=1&maquina="+pc;
        if(!ActivationQr.machine(" "+qr+" ").equals(pc))throw new AssertionError("QR colado");
        expectFailure(()->ActivationQr.machine(qr+"&extra=1"));
        expectFailure(()->ActivationQr.machine(qr.toLowerCase()));
        if(!ActivationQr.parse(qr).equals(pc))throw new AssertionError("QR incompatível");
        expectFailure(()->ActivationQr.parse(null));
        expectFailure(()->ActivationQr.parse(qr+"&extra=true"));
        expectFailure(()->ActivationQr.parse(qr.replace("v=1","v=2")));
        expectFailure(()->ActivationQr.parse(qr.replace("varthex:","https:")));
        expectFailure(()->ActivationQr.parse(qr.substring(0,qr.length()-1)));
        LocalDate today=LocalDate.of(2028,2,28);
        LocalDate renewal=LocalDate.of(2028,3,10);
        if(!LicensePeriod.end(4,today,null,2,null).equals(LocalDate.of(2028,3,1)))throw new AssertionError("Ano bissexto");
        if(!LicensePeriod.end(4,today,renewal,15,null).equals(LocalDate.of(2028,3,25)))throw new AssertionError("Renovação");
        if(!LicensePeriod.end(4,today,today.minusDays(2),1,null).equals(today.plusDays(1)))throw new AssertionError("Licença vencida");
        if(!LicensePeriod.end(5,today,null,0,today).equals(today.plusDays(1)))throw new AssertionError("Dia selecionado inclusivo");
        if(LicensePeriod.end(3,today,null,0,null)!=null)throw new AssertionError("Vitalícia");
        expectFailure(()->LicensePeriod.end(4,today,null,0,null));
        expectFailure(()->LicensePeriod.end(4,today,null,-1,null));
        expectFailure(()->LicensePeriod.end(4,today,null,36501,null));
        expectFailure(()->LicensePeriod.end(5,today,null,0,today.minusDays(1)));
        expectFailure(()->LicenseIssuer.issueUntil(key,pc,"Cliente",today,today));
        expectFailure(()->LicenseIssuer.issueUntil(key,pc,"Cliente",today,today.minusDays(1)));
        LocalDate[] customEnds={today.plusDays(7),LicensePeriod.end(5,today,null,0,LocalDate.of(2028,3,15)),LicensePeriod.end(4,today,renewal,15,null)};
        for(int i=0;i<customEnds.length;i++)Files.write(out.resolve("custom-"+i+".txt"),LicenseIssuer.issueUntil(key,pc,"Personalizada",today,customEnds[i]).getBytes(StandardCharsets.UTF_8));
        if(args.length>1 && !args[1].isEmpty()){
            java.awt.image.BufferedImage image=ImageIO.read(Paths.get(args[1]).toFile());
            int w=image.getWidth(),h=image.getHeight();
            RGBLuminanceSource source=new RGBLuminanceSource(w,h,image.getRGB(0,0,w,h,null,0,w));
            String decoded=new MultiFormatReader().decode(new BinaryBitmap(new HybridBinarizer(source))).getText();
            if(!ActivationQr.parse(decoded).equals(pc))throw new AssertionError("Imagem Windows incompatível");
            System.out.println("QR Code PNG gerado no Windows decodificado pelo ZXing do Android.");
        }
        String[] dates={"2028-01-31","2027-01-31","2026-11-30","2026-09-30"};
        for(int months=0;months<=3;months++){
            String token=LicenseIssuer.issue(key,pc,"João \"Teste\" \\ Café\n🍔",months,LocalDate.parse(dates[months]));
            Files.write(out.resolve("license-"+months+".txt"),token.getBytes(StandardCharsets.UTF_8));
        }
        expectFailure(()->LicenseIssuer.issue(key,"ABC","Teste",1,LocalDate.now()));
        expectFailure(()->LicenseIssuer.issue(key,pc," ",1,LocalDate.now()));
        expectFailure(()->LicenseIssuer.issue(key,pc,"Teste",4,LocalDate.now()));
        expectFailure(()->LicenseIssuer.issue(null,pc,"Teste",1,LocalDate.now()));
        expectFailure(()->LicenseIssuer.importKey(privatePem,pem("PUBLIC KEY",generator.generateKeyPair().getPublic().getEncoded())));
        Files.write(out.resolve("private-synthetic.pem"),privatePem.getBytes(StandardCharsets.UTF_8));
        Files.write(out.resolve("public.pem"),publicPem.getBytes(StandardCharsets.UTF_8));
        System.out.println("Java: planos antigos e personalizados, renovação, QR, datas e entradas inválidas verificados; apenas chaves sintéticas.");
    }
    static void verifyWindows(Path folder)throws Exception{
        byte[] encoded=Base64.getDecoder().decode(new String(Files.readAllBytes(folder.resolve("public.pem")),StandardCharsets.UTF_8).replace("-----BEGIN PUBLIC KEY-----","").replace("-----END PUBLIC KEY-----","").replaceAll("\\s",""));
        PublicKey key=KeyFactory.getInstance("RSA").generatePublic(new java.security.spec.X509EncodedKeySpec(encoded));
        LocalDate today=LocalDate.of(2028,2,28);
        for(int i=0;i<=5;i++){
            String[] parts=new String(Files.readAllBytes(folder.resolve("windows-"+i+".txt")),StandardCharsets.UTF_8).split("\\.");
            byte[] payload=Base64.getDecoder().decode(parts[0]);Signature sig=Signature.getInstance("RSASSA-PSS");sig.setParameter(new java.security.spec.PSSParameterSpec("SHA-256","MGF1",java.security.spec.MGF1ParameterSpec.SHA256,32,1));sig.initVerify(key);sig.update(payload);
            if(!sig.verify(Base64.getDecoder().decode(parts[1])))throw new AssertionError("Assinatura Windows incompatível");
            String json=new String(payload,StandardCharsets.UTF_8);int plan=i==0?3:i<=3?i-1:i;
            LocalDate end=LicensePeriod.end(plan,today,i==4?LocalDate.of(2028,3,10):null,15,LocalDate.of(2028,3,15));
            if(!json.contains(today+"T00:00:00-03:00") || (end!=null&&!json.contains(end+"T00:00:00-03:00")) || (end==null&&!json.contains("\"Vencimento\":null")))throw new AssertionError("Datas Windows incompatíveis");
        }
        System.out.println("Java: 6 licenças Windows verificadas, com períodos equivalentes e assinaturas compatíveis.");
    }
    interface Action {void run() throws Exception;}
    static void expectFailure(Action f)throws Exception{try{f.run();}catch(IllegalArgumentException|GeneralSecurityException e){return;}throw new AssertionError("Entrada inválida aceita");}
    static String pem(String kind,byte[] data){return "-----BEGIN "+kind+"-----\n"+Base64.getEncoder().encodeToString(data)+"\n-----END "+kind+"-----";}
}
