package com.varthex.licencas;

import android.app.*;
import android.os.Bundle;
import android.content.*;
import android.graphics.Color;
import android.text.InputType;
import android.view.*;
import android.widget.*;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.security.PrivateKey;
import java.time.*;
import java.time.format.DateTimeFormatter;

public class MainActivity extends Activity {
    private KeyVault vault;
    private PrivateKey pendingImport;
    private int pendingAction;
    private String pendingClient, pendingComputer;
    private LocalDate pendingStart, pendingEnd;
    private EditText days;
    private Button endDate;
    private CheckBox renewal;
    private LocalDate selectedEnd = LocalDate.now(ZoneOffset.ofHours(-3)).plusDays(30);
    private static final int AUTH = 21, SCAN = 22;
    private EditText client, computer, output;
    private TextView keyStatus, summary, message;
    private Spinner plan;
    private Button date, generate, copy, share;
    private CheckBox authorized;
    private LocalDate start = LocalDate.now(ZoneOffset.ofHours(-3));
    private String generated = "";
    private static final int IMPORT_KEY = 20;
    private static final DateTimeFormatter DISPLAY = DateTimeFormatter.ofPattern("dd/MM/yyyy");
    private final int graphite = Color.rgb(35,37,42);

    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        vault = new KeyVault(this);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE);
        getWindow().setStatusBarColor(graphite);
        getWindow().setNavigationBarColor(graphite);
        ScrollView scroll = new ScrollView(this);
        LinearLayout body = new LinearLayout(this);
        body.setOrientation(LinearLayout.VERTICAL);
        body.setPadding(dp(22), dp(22), dp(22), dp(28));
        body.setBackgroundColor(Color.rgb(243,245,242));
        scroll.addView(body);
        setContentView(scroll);
        text(body,"Varthex • Licenças",26);
        text(body,"Gere uma chave de ativação sem internet.",15);
        keyStatus = text(body,vault.exists()?"Chave salva • emissão protegida pelo bloqueio do aparelho":"Importe a chave uma vez para salvar neste aparelho",14);
        button(body,"Importar chave privada (.pem)",v -> {
            Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
            intent.setType("*/*"); intent.addCategory(Intent.CATEGORY_OPENABLE);
            startActivityForResult(intent, IMPORT_KEY);
        });
        button(body,"Colar chave privada",v -> pastePrivateKey());
        button(body,"Bloquear emissor",v -> { resetPending(); clearResult(); keyStatus.setText("Emissor bloqueado. A próxima emissão exige autenticação."); });
        button(body,"Remover chave deste aparelho",v -> new AlertDialog.Builder(this).setTitle("Remover chave salva?")
            .setMessage("Você precisará importar o arquivo PEM novamente para emitir licenças.")
            .setNegativeButton("Cancelar",null).setPositiveButton("Remover",(d,w)->{
                resetPending(); clearResult();
                try { vault.remove(); keyStatus.setText("Chave removida deste aparelho"); }
                catch(Exception e){message.setText("Não foi possível remover a chave. Tente novamente.");}
            }).show());
        text(body,"Cliente ou estabelecimento",14);
        client = input(body,"Ex.: Lanchonete Exemplo",false);
        client.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(200)});
        text(body,"Código do computador",14);
        computer = input(body,"Cole os 64 caracteres exibidos no Windows",true);
        computer.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        button(body,"Escanear QR Code",v -> startActivityForResult(new Intent(this,ScanActivity.class),SCAN));
        text(body,"Validade",14);
        plan = new Spinner(this);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(this,android.R.layout.simple_spinner_item,new String[]{"1 mês","2 meses","3 meses","Vitalícia","Personalizado: quantidade de dias","Personalizado: data de vencimento"});
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item); plan.setAdapter(adapter);
        body.addView(plan,new LinearLayout.LayoutParams(-1,dp(52)));
        renewal=new CheckBox(this); renewal.setText("Renovação antecipada: acrescentar ao vencimento atual"); body.addView(renewal);
        renewal.setOnCheckedChangeListener((b,checked)->refreshDates());
        days=input(body,"Quantidade de dias (1 a 36.500)",false); days.setInputType(InputType.TYPE_CLASS_NUMBER); days.setText("30");
        days.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(5)});
        endDate=button(body,"",v -> new DatePickerDialog(this,(picker,y,m,d)->{selectedEnd=LocalDate.of(y,m+1,d);refreshDates();},selectedEnd.getYear(),selectedEnd.getMonthValue()-1,selectedEnd.getDayOfMonth()).show());
        date=button(body,"",v -> new DatePickerDialog(this,(picker,y,m,d)-> {start=LocalDate.of(y,m+1,d); refreshDates();},start.getYear(),start.getMonthValue()-1,start.getDayOfMonth()).show());
        summary=text(body,"",14);
        plan.setOnItemSelectedListener(new android.widget.AdapterView.OnItemSelectedListener(){
            public void onItemSelected(android.widget.AdapterView<?> p,View v,int pos,long id){ refreshDates(); }
            public void onNothingSelected(android.widget.AdapterView<?> p){}
        });
        text(body,"Horário de Brasília (UTC−3). Na renovação, informe a data de vencimento indicada na licença atual.",13);
        authorized=new CheckBox(this); authorized.setText("Pagamento confirmado ou emissão autorizada"); authorized.setTextColor(graphite); body.addView(authorized);
        generate=button(body,"Gerar chave de ativação",v -> generateLicense());
        message=text(body,"",14); message.setAccessibilityLiveRegion(View.ACCESSIBILITY_LIVE_REGION_POLITE);
        output=input(body,"A chave gerada aparecerá aqui",true); output.setKeyListener(null); output.setTextIsSelectable(true); output.setMinLines(3); output.setMaxLines(6); output.setSaveEnabled(false);
        copy=button(body,"Copiar chave",v -> {
            ((android.content.ClipboardManager)getSystemService(CLIPBOARD_SERVICE)).setPrimaryClip(ClipData.newPlainText("Licença Varthex",generated));
            message.setText("Chave copiada. Cole na tela de ativação do cliente.");
        });
        share=button(body,"Compartilhar chave",v -> {
            Intent intent=new Intent(Intent.ACTION_SEND); intent.setType("text/plain"); intent.putExtra(Intent.EXTRA_TEXT,generated);
            startActivity(Intent.createChooser(intent,"Enviar chave de ativação"));
        });
        text(body,"A chave é salva criptografada neste aparelho. Cada emissão exige a credencial de bloqueio do celular. Sem internet e sem incluir a chave privada nas licenças.",13);
        text(body,"Desenvolvido pela Varthex",12);
        clearResult(); refreshDates();
        android.text.TextWatcher changes=new android.text.TextWatcher(){
            public void beforeTextChanged(CharSequence s,int a,int c,int f){}
            public void onTextChanged(CharSequence s,int a,int b,int c){ clearResult(); authorized.setChecked(false); }
            public void afterTextChanged(android.text.Editable e){}
        };
        client.addTextChangedListener(changes); computer.addTextChangedListener(changes);
        days.addTextChangedListener(new android.text.TextWatcher(){
            public void beforeTextChanged(CharSequence s,int a,int c,int f){}
            public void onTextChanged(CharSequence s,int a,int b,int c){refreshDates();}
            public void afterTextChanged(android.text.Editable e){}
        });
    }

    private void pastePrivateKey(){
        LinearLayout content=new LinearLayout(this);
        content.setOrientation(LinearLayout.VERTICAL);
        content.setPadding(dp(20),dp(8),dp(20),dp(8));
        text(content,"Cole todo o conteúdo do PEM, incluindo BEGIN PRIVATE KEY e END PRIVATE KEY.",14);
        EditText pem=input(content,"Toque e segure para colar a chave",true);
        pem.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD | InputType.TYPE_TEXT_FLAG_MULTI_LINE | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        pem.setMinLines(3); pem.setMaxLines(6);
        pem.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(32768)});
        TextView error=text(content,"",14);
        error.setAccessibilityLiveRegion(View.ACCESSIBILITY_LIVE_REGION_POLITE);
        AlertDialog dialog=new AlertDialog.Builder(this).setTitle("Colar chave privada")
                .setView(content).setNegativeButton("Cancelar",null).setPositiveButton("Conferir chave",null).create();
        dialog.setOnDismissListener(d -> pem.setText(""));
        dialog.getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE);
        dialog.setOnShowListener(d -> dialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(v -> {
            resetPending(); clearResult();
            try(InputStream expected=getAssets().open("licenca-publica.pem")){
                PrivateKey imported=LicenseIssuer.importKey(pem.getText().toString(),readLimited(expected));
                dialog.dismiss();
                saveImported(imported);
            } catch(Exception e){
                keyStatus.setText("Chave não importada");
                error.setText("Não foi possível conferir a chave. Cole o PEM completo do emissor original.");
            }
        }));
        dialog.show();
    }
    private LocalDate today(){return LocalDate.now(ZoneOffset.ofHours(-3));}
    private LocalDate expiration(){
        int count=0;
        if(plan.getSelectedItemPosition()==4){
            try { count=Integer.parseInt(days.getText().toString()); }
            catch(NumberFormatException e){throw new IllegalArgumentException("Informe a quantidade de dias.");}
        }
        return LicensePeriod.end(plan.getSelectedItemPosition(),today(),renewal.isChecked()?start:null,count,selectedEnd);
    }
    private void refreshDates(){
        if(date==null || summary==null)return;
        int pos=plan.getSelectedItemPosition();
        boolean renewable=pos!=3 && pos!=5;
        renewal.setVisibility(renewable?View.VISIBLE:View.GONE);
        date.setVisibility(renewable && renewal.isChecked()?View.VISIBLE:View.GONE);
        date.setText("Vencimento atual: "+start.format(DISPLAY));
        days.setVisibility(pos==4?View.VISIBLE:View.GONE);
        endDate.setVisibility(pos==5?View.VISIBLE:View.GONE);
        endDate.setText("Último dia de uso: "+selectedEnd.format(DISPLAY));
        clearResult();
        if(authorized!=null)authorized.setChecked(false);
        try {
            LocalDate end=expiration();
            summary.setText(end==null?"Sem vencimento (vitalícia)":"Uso até "+end.minusDays(1).format(DISPLAY)+". Vence em "+end.format(DISPLAY)+", às 00:00, horário de Brasília.");
        } catch(Exception e){summary.setText(e.getMessage());}
    }
    private void clearResult(){
        generated="";
        if(output!=null) output.setText("");
        if(copy!=null) copy.setEnabled(false);
        if(share!=null) share.setEnabled(false);
        if(message!=null) message.setText("");
    }
    private void generateLicense(){
        clearResult();
        if(!authorized.isChecked()){message.setText("Confirme a autorização da emissão."); return;}
        if(!vault.exists()){message.setText("Importe a chave privada para salvar neste aparelho primeiro.");return;}
        try {
            final String name=client.getText().toString().trim();
            final String machine=computer.getText().toString().trim().toUpperCase(java.util.Locale.ROOT);
            if(name.isEmpty() || name.length()>200)throw new IllegalArgumentException("Informe o cliente (até 200 caracteres).");
            if(!machine.matches("[0-9A-F]{64}"))throw new IllegalArgumentException("Confira o código do computador: são 64 caracteres.");
            final LocalDate begin=today(), end=expiration();
            new AlertDialog.Builder(this).setTitle("Conferir emissão")
                .setMessage("Cliente: "+name+"\n\nComputador: "+machine+"\n\n"+summary.getText())
                .setNegativeButton("Voltar",null).setPositiveButton("Confirmar e desbloquear",(d,w)->{
                    pendingClient=name; pendingComputer=machine; pendingStart=begin; pendingEnd=end;
                    authenticate(2);
                }).show();
        }catch(Exception e){message.setText(e.getMessage());}
    }
    private void saveImported(PrivateKey imported){pendingImport=imported;authenticate(1);}
    private void authenticate(int action){
        KeyguardManager manager=(KeyguardManager)getSystemService(KEYGUARD_SERVICE);
        if(!manager.isDeviceSecure()){
            resetPending(); message.setText("Configure um PIN, senha ou padrão de bloqueio nas configurações do celular para proteger a chave.");return;
        }
        Intent intent=manager.createConfirmDeviceCredentialIntent("Varthex Licenças",action==1?"Confirme para salvar a chave protegida":"Confirme para emitir esta licença");
        if(intent==null){resetPending();message.setText("Não foi possível abrir o desbloqueio do aparelho.");return;}
        pendingAction=action;
        try{startActivityForResult(intent,AUTH);}catch(Exception e){resetPending();message.setText("Não foi possível abrir o desbloqueio do aparelho.");}
    }
    private void resetPending(){pendingImport=null;pendingAction=0;pendingClient=null;pendingComputer=null;pendingStart=null;pendingEnd=null;}
    @Override protected void onActivityResult(int request,int result,Intent data){
        super.onActivityResult(request,result,data);
        if(request==SCAN){
            if(result==RESULT_OK && data!=null){
                String machine=data.getStringExtra("machine");
                if(machine!=null && machine.matches("[0-9A-F]{64}")){computer.setText(machine);message.setText("Computador identificado. Confira o cliente e a validade.");}
            }
            return;
        }
        if(request==AUTH){
            try {
                if(result!=RESULT_OK){message.setText("Desbloqueio cancelado. Nenhuma licença foi emitida.");return;}
                if(pendingAction==1 && pendingImport!=null){
                    vault.save(pendingImport);keyStatus.setText("Chave salva e protegida neste aparelho");
                }else if(pendingAction==2 && pendingStart!=null){
                    if(!today().equals(pendingStart)){message.setText("A data mudou. Confira novamente o período antes de emitir.");return;}
                    PrivateKey key=vault.unlock();
                    generated=LicenseIssuer.issueUntil(key,pendingComputer,pendingClient,pendingStart,pendingEnd);
                    output.setText(generated);copy.setEnabled(true);share.setEnabled(true);
                    message.setText("Licença gerada para "+pendingClient+". Copie e cole no VComanda.");authorized.setChecked(false);
                }else message.setText("Operação interrompida. Confira os dados e tente novamente.");
            }catch(Exception e){clearResult();message.setText("Não foi possível acessar a chave protegida. Tente desbloquear novamente; se o bloqueio do aparelho mudou, remova a chave e importe o PEM novamente.");}
            finally{resetPending();}
            return;
        }
        if(request!=IMPORT_KEY || result!=RESULT_OK || data==null || data.getData()==null)return;
        resetPending();clearResult();
        try(InputStream input=getContentResolver().openInputStream(data.getData()); InputStream expected=getAssets().open("licenca-publica.pem")){
            saveImported(LicenseIssuer.importKey(readLimited(input),readLimited(expected)));
        }catch(Exception e){message.setText("Não foi possível importar. Selecione o PEM completo do emissor original.");}
    }
    private String readLimited(InputStream stream) throws IOException {
        if(stream==null) throw new IOException("Arquivo não disponível.");
        ByteArrayOutputStream bytes=new ByteArrayOutputStream(); byte[] buffer=new byte[2048]; int n;
        while((n=stream.read(buffer))!=-1){if(bytes.size()+n>32768) throw new IOException("Arquivo maior que o esperado para uma chave PEM.");bytes.write(buffer,0,n);}
        return new String(bytes.toByteArray(),StandardCharsets.UTF_8);
    }
    @Override protected void onStop(){super.onStop();if(pendingAction==0)resetPending();}
    @Override protected void onDestroy(){resetPending();super.onDestroy();}
    private int dp(int v){return Math.round(v*getResources().getDisplayMetrics().density);}
    private TextView text(LinearLayout body,String value,int size){TextView t=new TextView(this);t.setText(value);t.setTextSize(size);t.setTextColor(graphite);t.setPadding(0,dp(10),0,dp(6));body.addView(t);return t;}
    private EditText input(LinearLayout body,String hint,boolean multi){EditText e=new EditText(this);e.setHint(hint);e.setTextSize(16);e.setTextColor(graphite);e.setHintTextColor(Color.DKGRAY);e.setSingleLine(!multi);e.setMinHeight(dp(52));e.setSaveEnabled(false);e.setImportantForAutofill(View.IMPORTANT_FOR_AUTOFILL_NO);body.addView(e,new LinearLayout.LayoutParams(-1,-2));return e;}
    private Button button(LinearLayout body,String title,View.OnClickListener action){Button b=new Button(this);b.setText(title);b.setAllCaps(false);b.setMinHeight(dp(50));b.setOnClickListener(action);body.addView(b,new LinearLayout.LayoutParams(-1,-2));return b;}
}
