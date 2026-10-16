package com.varthex.licencas;

import android.content.Context;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.util.AtomicFile;
import java.io.*;
import java.security.*;
import java.security.spec.PKCS8EncodedKeySpec;
import java.util.Arrays;
import javax.crypto.*;
import javax.crypto.spec.GCMParameterSpec;

/** Private key bytes are only persisted as authenticated ciphertext in noBackupFilesDir. */
public final class KeyVault {
    private static final String ALIAS = "varthex.issuer.v1";
    private final AtomicFile file;
    public KeyVault(Context context) { file = new AtomicFile(new File(context.getNoBackupFilesDir(), "issuer.enc")); }
    public boolean exists() { return file.getBaseFile().exists(); }
    private KeyStore store() throws Exception { KeyStore s=KeyStore.getInstance("AndroidKeyStore"); s.load(null); return s; }
    private SecretKey encryptionKey() throws Exception {
        KeyStore s=store();
        if (!s.containsAlias(ALIAS)) {
            KeyGenerator g=KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore");
            g.init(new KeyGenParameterSpec.Builder(ALIAS,KeyProperties.PURPOSE_ENCRYPT|KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setUserAuthenticationRequired(true).setUserAuthenticationValidityDurationSeconds(30).build());
            return g.generateKey();
        }
        return (SecretKey)s.getKey(ALIAS,null);
    }
    public void save(PrivateKey key) throws Exception {
        byte[] raw=key.getEncoded();
        try {
            Cipher c=Cipher.getInstance("AES/GCM/NoPadding"); c.init(Cipher.ENCRYPT_MODE,encryptionKey());
            byte[] encrypted=c.doFinal(raw);
            FileOutputStream output=null;
            try { output=file.startWrite(); output.write(c.getIV()); output.write(encrypted); file.finishWrite(output); }
            catch(Exception e){ if(output!=null)file.failWrite(output); throw e; }
        } finally { Arrays.fill(raw,(byte)0); }
    }
    public PrivateKey unlock() throws Exception {
        byte[] data=file.readFully();
        if(data.length<29 || data.length>32768)throw new IOException("Chave salva inválida. Importe novamente.");
        Cipher c=Cipher.getInstance("AES/GCM/NoPadding");
        c.init(Cipher.DECRYPT_MODE,encryptionKey(),new GCMParameterSpec(128,Arrays.copyOf(data,12)));
        byte[] raw=c.doFinal(data,12,data.length-12);
        try { return KeyFactory.getInstance("RSA").generatePrivate(new PKCS8EncodedKeySpec(raw)); }
        finally { Arrays.fill(raw,(byte)0); }
    }
    public void remove() throws Exception { store().deleteEntry(ALIAS); file.delete(); }
}
