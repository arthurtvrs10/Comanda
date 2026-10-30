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
    private final int graphite = Color.rgb(23,35,44);
    private LinearLayout[] pages = new LinearLayout[4];
    private TextView progress, review;
    private ProgressBar completion;
    private ScrollView scroll;
    private Button next, back;
    private int step=0;
    private final int accent=Color.rgb(181,54,32);
    private LinearLayout column(){LinearLayout v=new LinearLayout(this);v.setOrientation(LinearLayout.VERTICAL);return v;}
    private void showStep(int value){
        step=value;
        if(scroll!=null)scroll.post(()->scroll.scrollTo(0,0));
        for(int i=0;i<pages.length;i++)pages[i].setVisibility(i==step?View.VISIBLE:View.GONE);
        progress.setText(step==3?"Emissão concluída":("Etapa "+(step+1)+" de 3   ·   "+new String[]{"Cliente","Validade","Revisão"}[step]));
        completion.setProgress(step==3?100:(step+1)*100/3);
        back.setVisibility(step>0 && step<3?View.VISIBLE:View.GONE);
        next.setText(step==0?"Continuar para validade":step==1?"Revisar emissão":step==2?"Confirmar e gerar":"Emitir outra licença");
        if(step==2)review.setText("Cliente\n"+client.getText()+"\n\nComputador\n"+computer.getText()+"\n\nValidade\n"+summary.getText());
        message.setText("");
        ((android.view.inputmethod.InputMethodManager)getSystemService(INPUT_METHOD_SERVICE)).hideSoftInputFromWindow(client.getWindowToken(),0);
    }
    private boolean validClient(){
        if(client.getText().toString().trim().isEmpty()){client.setError("Informe o cliente");client.requestFocus();return false;}
        try{ActivationQr.machine(computer.getText().toString());return true;}catch(Exception e){computer.setError("Cole o código de 64 caracteres ou escaneie o QR Code");computer.requestFocus();return false;}
    }
    @Override public void onBackPressed(){if(step>0){showStep(step==3?2:step-1);}else super.onBackPressed();}
    private void manageKey(){new AlertDialog.Builder(this).setTitle("Chave do emissor")
        .setItems(new String[]{"Importar arquivo PEM","Colar chave privada","Bloquear emissor","Remover chave"},(d,w)->{
            if(w==0){Intent i=new Intent(Intent.ACTION_OPEN_DOCUMENT);i.setType("*/*");i.addCategory(Intent.CATEGORY_OPENABLE);startActivityForResult(i,IMPORT_KEY);}
            if(w==1)pastePrivateKey();
            if(w==2){resetPending();clearResult();authorized.setChecked(false);showStep(0);message.setText("Emissor bloqueado. A emissão exige desbloquear o celular.");}
            if(w==3)new AlertDialog.Builder(this).setTitle("Remover chave deste aparelho?").setMessage("Será necessário importar o PEM novamente para emitir.").setNegativeButton("Cancelar",null).setPositiveButton("Remover",(a,b)->{try{vault.remove();resetPending();clearResult();authorized.setChecked(false);showStep(0);keyStatus.setText("Importe a chave para começar");}catch(Exception e){message.setText("Não foi possível remover a chave.");}}).show();
        }).setNegativeButton("Voltar",null).show();}


    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        vault = new KeyVault(this);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE);
        getWindow().setStatusBarColor(graphite);
        getWindow().setNavigationBarColor(graphite);
        LinearLayout root=column(); root.setBackgroundColor(Color.WHITE);setContentView(root);
        LinearLayout header=new LinearLayout(this);header.setPadding(dp(20),dp(8),dp(20),0);header.setGravity(Gravity.CENTER_VERTICAL);root.addView(header);
        TextView title=new TextView(this);title.setText("Varthex Licenças");title.setTextSize(22);title.setTextColor(graphite);title.setTypeface(null,1);header.addView(title,new LinearLayout.LayoutParams(0,-2,1));
        Button key=button(header,"Chave",v->manageKey());key.setLayoutParams(new LinearLayout.LayoutParams(dp(88),dp(52)));
        keyStatus=text(root,vault.exists()?"Chave protegida neste aparelho":"Importe sua chave pelo botão Chave para começar",14);keyStatus.setPadding(dp(20),dp(8),dp(20),dp(8));keyStatus.setTextColor(Color.rgb(36,103,74));
        progress=text(root,"",15);progress.setPadding(dp(20),dp(12),dp(20),dp(12));progress.setBackgroundColor(Color.rgb(255,240,235));
        completion=new ProgressBar(this,null,android.R.attr.progressBarStyleHorizontal);completion.setMax(100);completion.setProgressTintList(android.content.res.ColorStateList.valueOf(accent));root.addView(completion,new LinearLayout.LayoutParams(-1,dp(4)));
        scroll=new ScrollView(this);scroll.setFillViewport(true);root.addView(scroll,new LinearLayout.LayoutParams(-1,0,1));
        LinearLayout content=column();content.setPadding(dp(22),dp(12),dp(22),dp(24));scroll.addView(content);
        for(int i=0;i<pages.length;i++){pages[i]=column();content.addView(pages[i],new LinearLayout.LayoutParams(-1,-2));}
        LinearLayout body=pages[0];
        text(body,"Cliente e computador",24).setTypeface(null,1);
        text(body,"Identifique quem vai receber a licença.",15);
        required(body,"Cliente ou estabelecimento");
        client = input(body,"Ex.: Lanchonete Exemplo",false);
        client.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(200)});
        required(body,"Código do computador");
        computer = input(body,"Cole o código do Windows ou conteúdo do QR",true);
        computer.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(256)});
        computer.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        button(body,"Escanear QR Code",v -> startActivityForResult(new Intent(this,ScanActivity.class),SCAN));
        button(body,"Colar código",v->{android.content.ClipboardManager cb=(android.content.ClipboardManager)getSystemService(CLIPBOARD_SERVICE);if(cb.hasPrimaryClip() && cb.getPrimaryClip().getItemCount()>0)computer.setText(cb.getPrimaryClip().getItemAt(0).coerceToText(this));});
        body=pages[1];
        text(body,"Escolha a validade",24).setTypeface(null,1);
        required(body,"Validade");
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
        body=pages[2];
        text(body,"Confira antes de emitir",24).setTypeface(null,1);
        review=text(body,"",16);review.setTextIsSelectable(true);
        authorized=new CheckBox(this); authorized.setText("Pagamento confirmado ou emissão autorizada"); authorized.setTextColor(graphite); body.addView(authorized);
        body=pages[3];
        text(body,"Licença gerada",24).setTypeface(null,1);
        text(body,"Copie ou compartilhe a licença com o cliente.",16);
        message=text(root,"",14); message.setPadding(dp(22),dp(4),dp(22),dp(4)); message.setAccessibilityLiveRegion(View.ACCESSIBILITY_LIVE_REGION_POLITE);
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
        LinearLayout footer=column();footer.setPadding(dp(20),dp(6),dp(20),dp(12));root.addView(footer);
        back=button(footer,"Voltar",v->showStep(step-1));
        next=button(footer,"Continuar",v->{
            if(step==0){if(validClient())showStep(1);}
            else if(step==1){try{expiration();showStep(2);}catch(Exception e){message.setText(e.getMessage());}}
            else if(step==2)generateLicense();
            else {clearResult();authorized.setChecked(false);client.setText("");computer.setText("");showStep(0);}
        });
        next.setBackgroundTintList(android.content.res.ColorStateList.valueOf(accent));next.setTextColor(Color.WHITE);
        clearResult(); refreshDates();showStep(0);
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
        if(pendingAction!=0)return;
        clearResult();
        if(!authorized.isChecked()){message.setText("Confirme a autorização da emissão."); return;}
        if(!vault.exists()){message.setText("Importe a chave privada para salvar neste aparelho primeiro.");return;}
        try {
            final String name=client.getText().toString().trim();
            final String machine=ActivationQr.machine(computer.getText().toString());
            if(name.isEmpty() || name.length()>200)throw new IllegalArgumentException("Informe o cliente (até 200 caracteres).");
            if(!machine.matches("[0-9A-F]{64}"))throw new IllegalArgumentException("Confira o código do computador: são 64 caracteres.");
            final LocalDate begin=today(), end=expiration();
            pendingClient=name; pendingComputer=machine; pendingStart=begin; pendingEnd=end;
            authenticate(2);
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
                    showStep(3);message.setText("Licença gerada para "+pendingClient+". Copie e cole no VComanda.");authorized.setChecked(false);
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
    private TextView required(LinearLayout body,String label){
        TextView view=text(body,label+" *",14);
        android.text.SpannableString value=new android.text.SpannableString(label+" *");
        value.setSpan(new android.text.style.ForegroundColorSpan(Color.rgb(178,53,22)),label.length()+1,value.length(),android.text.Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
        view.setText(value);view.setContentDescription(label+", obrigatório");return view;
    }
    private int dp(int v){return Math.round(v*getResources().getDisplayMetrics().density);}
    private TextView text(LinearLayout body,String value,int size){TextView t=new TextView(this);t.setText(value);t.setTextSize(size);t.setTextColor(graphite);t.setPadding(0,dp(10),0,dp(6));body.addView(t);return t;}
    private EditText input(LinearLayout body,String hint,boolean multi){EditText e=new EditText(this);e.setHint(hint);e.setTextSize(16);e.setTextColor(graphite);e.setHintTextColor(Color.rgb(82,97,109));e.setPadding(dp(12),dp(12),dp(12),dp(12));e.setSingleLine(!multi);e.setMinHeight(dp(52));e.setSaveEnabled(false);e.setImportantForAutofill(View.IMPORTANT_FOR_AUTOFILL_NO);body.addView(e,new LinearLayout.LayoutParams(-1,-2));return e;}
    private Button button(LinearLayout body,String title,View.OnClickListener action){Button b=new Button(this);b.setText(title);b.setAllCaps(false);b.setMinHeight(dp(52));b.setTextColor(graphite);b.setTextSize(15);b.setOnClickListener(action);body.addView(b,new LinearLayout.LayoutParams(-1,-2));return b;}
}
