package com.varthex.licencas;

/** Strict, versioned protocol shared with the Windows activation screen. */
public final class ActivationQr {
    private static final String PREFIX = "varthex://ativacao?v=1&maquina=";
    private ActivationQr() {}
    public static String parse(String value) {
        if (value == null || !value.startsWith(PREFIX))
            throw new IllegalArgumentException("Este QR Code não é uma ativação do VComanda compatível.");
        String machine = value.substring(PREFIX.length());
        if (!machine.matches("[0-9A-F]{64}"))
            throw new IllegalArgumentException("O QR Code contém um identificador inválido.");
        return machine;
    }
}
