package com.varthex.licencas;

import java.net.*;
import java.io.*;
import java.nio.charset.StandardCharsets;
import org.json.JSONObject;

/** Sends the signed license only. No access to the private key or KeyVault. */
public final class LicenseTransfer {
    private static final String ENDPOINT="https://vcomanda-licencas-transferencia.comandasoftwarebyvarthex.workers.dev/v1/send";
    public static final class Result { public final String code;public final long expiresAt;Result(String c,long e){code=c;expiresAt=e;} }
    public static Result send(String license) throws Exception {
        if(license==null||license.isEmpty()||license.length()>12000)throw new IOException("Gere uma licença primeiro.");
        HttpURLConnection connection=(HttpURLConnection)new URL(ENDPOINT).openConnection();
        connection.setInstanceFollowRedirects(false);connection.setConnectTimeout(15000);connection.setReadTimeout(15000);
        connection.setRequestMethod("POST");connection.setRequestProperty("Content-Type","application/json");connection.setDoOutput(true);
        byte[] request=new JSONObject().put("license",license).toString().getBytes(StandardCharsets.UTF_8);connection.setFixedLengthStreamingMode(request.length);
        try {
            try(OutputStream out=connection.getOutputStream()){out.write(request);}
            int status=connection.getResponseCode();InputStream stream=status==200?connection.getInputStream():connection.getErrorStream();
            if(stream==null)throw new IOException("Sem resposta do serviço. Tente novamente ou copie a licença.");
            ByteArrayOutputStream bytes=new ByteArrayOutputStream();
            try(InputStream input=stream){byte[] buffer=new byte[1024];int n;while((n=input.read(buffer))!=-1){if(bytes.size()+n>4096)throw new IOException("Resposta inválida.");bytes.write(buffer,0,n);}}
            JSONObject body=new JSONObject(new String(bytes.toByteArray(),StandardCharsets.UTF_8));
            if(status!=200)throw new IOException(body.optString("error","Não foi possível enviar. Tente novamente."));
            String code=body.getString("code");long expiry=body.getLong("expiresAt");
            if(!code.matches("[0-9]{8}")||expiry<=System.currentTimeMillis()/1000||expiry>System.currentTimeMillis()/1000+660)throw new IOException("Resposta inválida. Confira a data do celular.");
            return new Result(code,expiry);
        }finally{connection.disconnect();}
    }
}
