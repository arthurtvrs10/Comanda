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
    private int selectedPlan;
    private Button[] planButtons=new Button[6];
    private TextView[] stepLabels=new TextView[3];
    private StepMark[] stepMarks=new StepMark[3];
    private TextView screenTitle, subtitle, footerCount, lastDay;
    private LinearLayout reviewRows;
    private Button date, generate, copy, share;
    private CheckBox authorized;
    private LocalDate start = LocalDate.now(ZoneOffset.ofHours(-3));
    private String generated = "";
    private Button sendToPc;
    private TextView transferCode;
    private boolean sending;
    private long transferExpires;
    private final android.os.Handler transferTimer=new android.os.Handler(android.os.Looper.getMainLooper());
    private final Runnable expireTransfer=new Runnable(){public void run(){if(transferExpires>0){long left=transferExpires-System.currentTimeMillis()/1000;if(left<=0){transferCode.setText("Código expirado. Toque em Enviar ao PC para gerar outro.");transferExpires=0;}}if(!isDestroyed())transferTimer.postDelayed(this,1000);}};
    private static final int IMPORT_KEY = 20;
    private static final DateTimeFormatter DISPLAY = DateTimeFormatter.ofPattern("dd/MM/yyyy");
    private final int graphite = Color.rgb(23,35,44);
    private LinearLayout[] pages = new LinearLayout[4];
    private TextView progress, review;

    private ScrollView scroll;
    private Button next, back;
    private int step=0;
    private final int accent=Color.rgb(181,54,32);
    private LinearLayout column(){LinearLayout v=new LinearLayout(this);v.setOrientation(LinearLayout.VERTICAL);return v;}
    private void showStep(int value){
        step=value;
        if(scroll!=null)scroll.post(()->scroll.scrollTo(0,0));
        for(int i=0;i<pages.length;i++)pages[i].setVisibility(i==step?View.VISIBLE:View.GONE);
        footerCount.setText(step==3?"Emissão concluída":"Etapa "+(step+1)+" de 3");
        screenTitle.setText(new String[]{"Nova licença","Validade","Confira antes de emitir","Licença gerada"}[step]);
        subtitle.setText(new String[]{"Identifique quem vai usar o VComanda.","Escolha por quanto tempo liberar o acesso.","Revise os dados e autorize a emissão.","Copie ou compartilhe com o cliente."}[step]);
        for(int i=0;i<3;i++){stepMarks[i].state=i<step?2:i==step?1:0;stepMarks[i].invalidate();stepLabels[i].setText(new String[]{"Cliente","Validade","Revisão"}[i]);stepLabels[i].setTextColor(i<step?Color.rgb(36,103,74):i==step?accent:Color.rgb(105,119,130));}

        back.setVisibility(step>0 && step<3?View.VISIBLE:View.GONE);
        next.setText(step==0?"Continuar":step==1?"Revisar licença":step==2?"Confirmar e gerar":"Emitir outra licença");
        if(step==2){reviewRows.removeAllViews();reviewRow("Cliente",client.getText().toString().trim());String machine=ActivationQr.machine(computer.getText().toString());reviewRow("Computador",machine.substring(0,6)+"…"+machine.substring(60));reviewRow("Validade",new String[]{"1 mês","2 meses","3 meses","Vitalícia","Dias personalizados","Data personalizada"}[selectedPlan]);LocalDate end=expiration();reviewRow("Último dia de uso",end==null?"Sem vencimento":longDate(end.minusDays(1)));}
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
        getWindow().setStatusBarColor(Color.WHITE);
        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR);
        getWindow().setNavigationBarColor(Color.WHITE);
        LinearLayout root=column(); root.setBackgroundColor(Color.WHITE);setContentView(root);
        LinearLayout header=new LinearLayout(this);header.setPadding(dp(20),dp(8),dp(20),0);header.setGravity(Gravity.CENTER_VERTICAL);root.addView(header);
        TextView title=new TextView(this);title.setText("Varthex Licenças");title.setTextSize(20);title.setTextColor(graphite);title.setTypeface(null,1);header.addView(title,new LinearLayout.LayoutParams(0,-2,1));
        Button key=button(header,"Chave",v->manageKey());key.setLayoutParams(new LinearLayout.LayoutParams(dp(72),dp(52)));link(key);
        divider(root);
        keyStatus=text(root,vault.exists()?"Chave protegida neste aparelho":"Importe sua chave pelo botão Chave para começar",14);keyStatus.setPadding(dp(20),dp(8),dp(20),dp(8));keyStatus.setTextColor(Color.rgb(36,103,74));
        screenTitle=text(root,"",28);screenTitle.setTypeface(null,1);screenTitle.setPadding(dp(22),dp(20),dp(22),dp(4));
        subtitle=text(root,"",16);subtitle.setPadding(dp(22),0,dp(22),dp(14));
        LinearLayout steps=new LinearLayout(this);steps.setPadding(dp(22),dp(8),dp(22),dp(18));root.addView(steps);
        for(int i=0;i<3;i++){LinearLayout item=new LinearLayout(this);item.setGravity(Gravity.CENTER_VERTICAL);steps.addView(item,new LinearLayout.LayoutParams(0,dp(36),1));stepMarks[i]=new StepMark(i+1);item.addView(stepMarks[i],new LinearLayout.LayoutParams(dp(24),dp(24)));TextView t=new TextView(this);t.setTextSize(13);t.setPadding(dp(6),0,0,0);item.addView(t);stepLabels[i]=t;}

        divider(root);
        scroll=new ScrollView(this);scroll.setFillViewport(true);root.addView(scroll,new LinearLayout.LayoutParams(-1,0,1));
        LinearLayout content=column();content.setPadding(dp(22),dp(12),dp(22),dp(24));scroll.addView(content);
        for(int i=0;i<pages.length;i++){pages[i]=column();content.addView(pages[i],new LinearLayout.LayoutParams(-1,-2));}
        LinearLayout body=pages[0];

        required(body,"Cliente ou estabelecimento");
        client = input(body,"Ex.: Lanchonete Exemplo",false);
        client.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(200)});
        required(body,"Computador do cliente");
        computer = input(body,"Cole o código do Windows ou conteúdo do QR",true);
        computer.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(256)});
        computer.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        button(body,"Escanear QR Code",v -> startActivityForResult(new Intent(this,ScanActivity.class),SCAN));
        Button paste=button(body,"Colar código",v->{android.content.ClipboardManager cb=(android.content.ClipboardManager)getSystemService(CLIPBOARD_SERVICE);if(cb.hasPrimaryClip() && cb.getPrimaryClip().getItemCount()>0)computer.setText(cb.getPrimaryClip().getItemAt(0).coerceToText(this));});
        link(paste);
        text(body,"Leia o QR exibido na tela de ativação do VComanda.",16);
        body=pages[1];
        text(body,"Qual o período da licença?",18).setTypeface(null,1);
        String[] options={"1 mês","2 meses","3 meses","Vitalícia","Dias","Data"};
        for(int r=0;r<2;r++){LinearLayout row=new LinearLayout(this);body.addView(row);for(int c=0;c<3;c++){final int index=r*3+c;Button choice=button(row,options[index],v->{selectedPlan=index;refreshDates();});LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(0,dp(52),1);lp.setMargins(dp(c==0?0:6),dp(4),0,dp(4));choice.setLayoutParams(lp);planButtons[index]=choice;}}
        renewal=new CheckBox(this); renewal.setText("Renovação antecipada: acrescentar ao vencimento atual"); body.addView(renewal);
        renewal.setOnCheckedChangeListener((b,checked)->refreshDates());
        days=input(body,"Quantidade de dias (1 a 36.500)",false); days.setInputType(InputType.TYPE_CLASS_NUMBER); days.setText("30");
        days.setFilters(new android.text.InputFilter[]{new android.text.InputFilter.LengthFilter(5)});
        endDate=button(body,"",v -> new DatePickerDialog(this,(picker,y,m,d)->{selectedEnd=LocalDate.of(y,m+1,d);refreshDates();},selectedEnd.getYear(),selectedEnd.getMonthValue()-1,selectedEnd.getDayOfMonth()).show());
        date=button(body,"",v -> new DatePickerDialog(this,(picker,y,m,d)-> {start=LocalDate.of(y,m+1,d); refreshDates();},start.getYear(),start.getMonthValue()-1,start.getDayOfMonth()).show());
        divider(body);
        text(body,"Último dia de uso",14);lastDay=text(body,"",22);lastDay.setTypeface(null,1);
        summary=text(body,"",15);
        text(body,"Horário de Brasília (UTC−3). Na renovação, informe a data de vencimento indicada na licença atual.",13);
        body=pages[2];
        reviewRows=column();body.addView(reviewRows);
        authorized=new CheckBox(this); authorized.setText("Pagamento confirmado ou emissão autorizada"); authorized.setTextColor(graphite); body.addView(authorized);authorized.setButtonTintList(android.content.res.ColorStateList.valueOf(accent));
        text(body,"Confirme com o bloqueio do celular.",14);
        body=pages[3];

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
        divider(body);
        sendToPc=button(body,"Enviar ao PC",v->sendLicenseToPc());
        transferCode=text(body,"Use internet nos dois aparelhos para transferir por código curto.",16);transferCode.setTextIsSelectable(true);transferCode.setAccessibilityLiveRegion(View.ACCESSIBILITY_LIVE_REGION_POLITE);
        transferTimer.post(expireTransfer);
        text(body,"A chave é salva criptografada neste aparelho. Cada emissão exige a credencial de bloqueio do celular. Sem internet e sem incluir a chave privada nas licenças.",13);
        text(body,"Desenvolvido pela Varthex",12);
        LinearLayout footer=column();footer.setPadding(dp(20),dp(6),dp(20),dp(12));root.addView(footer);
        divider(footer);footerCount=text(footer,"",13);
        LinearLayout actions=new LinearLayout(this);footer.addView(actions);
        back=button(actions,"Voltar",v->showStep(step-1));
        back.setLayoutParams(new LinearLayout.LayoutParams(dp(90),dp(56)));
        next=button(actions,"Continuar",v->{
            if(step==0){if(validClient())showStep(1);}
            else if(step==1){try{expiration();showStep(2);}catch(Exception e){message.setText(e.getMessage());}}
            else if(step==2)generateLicense();
            else {clearResult();authorized.setChecked(false);client.setText("");computer.setText("");showStep(0);}
        });
        LinearLayout.LayoutParams nextSize=new LinearLayout.LayoutParams(0,dp(56),1);nextSize.setMargins(dp(8),0,0,0);next.setLayoutParams(nextSize);
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
        if(selectedPlan==4){
            try { count=Integer.parseInt(days.getText().toString()); }
            catch(NumberFormatException e){throw new IllegalArgumentException("Informe a quantidade de dias.");}
        }
        return LicensePeriod.end(selectedPlan,today(),renewal.isChecked()?start:null,count,selectedEnd);
    }
    private void refreshDates(){
        if(date==null || summary==null)return;
        int pos=selectedPlan;
        for(int i=0;i<6;i++)if(planButtons[i]!=null){planButtons[i].setBackground(background(i==pos?Color.rgb(255,240,235):Color.WHITE,i==pos?accent:Color.rgb(190,201,210)));planButtons[i].setTextColor(i==pos?accent:graphite);planButtons[i].setSelected(i==pos);}

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
            if(lastDay!=null)lastDay.setText(end==null?"Sem vencimento":longDate(end.minusDays(1)));
            summary.setText(end==null?"Sem vencimento (vitalícia)":"Uso até "+end.minusDays(1).format(DISPLAY)+". Vence em "+end.format(DISPLAY)+", às 00:00, horário de Brasília.");
        } catch(Exception e){summary.setText(e.getMessage());}
    }
    private void sendLicenseToPc(){
        if(sending||generated.isEmpty())return;
        final String token=generated;sending=true;sendToPc.setEnabled(false);transferExpires=0;
        transferCode.setText("Enviando licença…");
        new Thread(()->{try{final LicenseTransfer.Result result=LicenseTransfer.send(token);runOnUiThread(()->{if(!isDestroyed()&&generated.equals(token)){transferExpires=result.expiresAt;String code=result.code;transferCode.setText(code.substring(0,4)+" "+code.substring(4)+"\nNo computador: Receber licença → digite estes 8 dígitos.\nVálido até "+java.time.Instant.ofEpochSecond(result.expiresAt).atZone(java.time.ZoneId.systemDefault()).format(DateTimeFormatter.ofPattern("HH:mm"))+". Uso único. Reenviar invalida o código anterior.");}});}catch(Exception e){runOnUiThread(()->{if(!isDestroyed()&&generated.equals(token))transferCode.setText(e instanceof java.io.IOException?e.getMessage():"Não foi possível enviar. Confira a internet ou copie a licença.");});}finally{runOnUiThread(()->{sending=false;if(!isDestroyed())sendToPc.setEnabled(!generated.isEmpty());});}},"license-transfer").start();
    }
    private void clearResult(){
        generated="";transferExpires=0;
        if(sendToPc!=null)sendToPc.setEnabled(false);
        if(transferCode!=null)transferCode.setText("");
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
                    output.setText(generated);copy.setEnabled(true);share.setEnabled(true);sendToPc.setEnabled(true);
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
    @Override protected void onDestroy(){transferTimer.removeCallbacksAndMessages(null);resetPending();super.onDestroy();}
    private class StepMark extends View {
        int number,state;
        android.graphics.Paint paint=new android.graphics.Paint(3);
        StepMark(int n){super(MainActivity.this);number=n;setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);}
        protected void onDraw(android.graphics.Canvas canvas){super.onDraw(canvas);float cx=getWidth()/2f,cy=getHeight()/2f,r=Math.min(cx,cy);paint.setColor(state==2?Color.rgb(36,103,74):state==1?accent:Color.rgb(228,233,238));paint.setStyle(android.graphics.Paint.Style.FILL);canvas.drawCircle(cx,cy,r,paint);paint.setColor(state==0?Color.rgb(82,97,109):Color.WHITE);if(state==2){paint.setStyle(android.graphics.Paint.Style.STROKE);paint.setStrokeWidth(dp(2));android.graphics.Path p=new android.graphics.Path();p.moveTo(cx-r*.5f,cy);p.lineTo(cx-r*.12f,cy+r*.38f);p.lineTo(cx+r*.55f,cy-r*.45f);canvas.drawPath(p,paint);paint.setStyle(android.graphics.Paint.Style.FILL);}else{paint.setTextSize(dp(12));paint.setTextAlign(android.graphics.Paint.Align.CENTER);canvas.drawText(Integer.toString(number),cx,cy-(paint.ascent()+paint.descent())/2,paint);}}
    }
    private String longDate(LocalDate value){return value.format(DateTimeFormatter.ofPattern("d 'de' MMMM 'de' yyyy",new java.util.Locale("pt","BR")));}
    private void reviewRow(String label,String value){text(reviewRows,label,14).setTextColor(Color.rgb(82,97,109));TextView t=text(reviewRows,value,20);t.setTypeface(null,1);t.setTextIsSelectable(true);divider(reviewRows);}
    private void divider(LinearLayout parent){View line=new View(this);line.setBackgroundColor(Color.rgb(214,220,225));LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,dp(1));lp.setMargins(0,dp(12),0,dp(12));parent.addView(line,lp);}
    private android.graphics.drawable.GradientDrawable background(int fill,int stroke){android.graphics.drawable.GradientDrawable d=new android.graphics.drawable.GradientDrawable();d.setColor(fill);d.setCornerRadius(dp(10));if(stroke!=0)d.setStroke(dp(1),stroke);return d;}
    private void link(Button b){b.setBackground(new android.graphics.drawable.ColorDrawable(Color.TRANSPARENT));b.setTextColor(accent);b.setElevation(0);}
    private TextView required(LinearLayout body,String label){
        TextView view=text(body,label+" *",14);
        android.text.SpannableString value=new android.text.SpannableString(label+" *");
        value.setSpan(new android.text.style.ForegroundColorSpan(Color.rgb(178,53,22)),label.length()+1,value.length(),android.text.Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
        view.setText(value);view.setContentDescription(label+", obrigatório");return view;
    }
    private int dp(int v){return Math.round(v*getResources().getDisplayMetrics().density);}
    private TextView text(LinearLayout body,String value,int size){TextView t=new TextView(this);t.setText(value);t.setTextSize(size);t.setTextColor(graphite);t.setPadding(0,dp(10),0,dp(6));body.addView(t);return t;}
    private EditText input(LinearLayout body,String hint,boolean multi){EditText e=new EditText(this);e.setHint(hint);e.setTextSize(16);e.setTextColor(graphite);e.setHintTextColor(Color.rgb(82,97,109));e.setPadding(dp(12),dp(12),dp(12),dp(12));e.setBackground(background(Color.WHITE,Color.rgb(177,190,201)));e.setSingleLine(!multi);e.setMinHeight(dp(52));e.setSaveEnabled(false);e.setImportantForAutofill(View.IMPORTANT_FOR_AUTOFILL_NO);LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,-2);lp.setMargins(0,0,0,dp(16));body.addView(e,lp);return e;}
    private Button button(LinearLayout body,String title,View.OnClickListener action){Button b=new Button(this);b.setText(title);b.setAllCaps(false);b.setMinHeight(dp(52));b.setTextColor(graphite);b.setTextSize(15);b.setBackground(background(Color.WHITE,Color.rgb(177,190,201)));b.setElevation(0);b.setOnClickListener(action);body.addView(b,new LinearLayout.LayoutParams(-1,-2));return b;}
}
