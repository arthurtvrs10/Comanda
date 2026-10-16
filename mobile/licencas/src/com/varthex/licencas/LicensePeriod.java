package com.varthex.licencas;

import java.time.LocalDate;

public final class LicensePeriod {
    private LicensePeriod() {}
    public static LocalDate end(int plan, LocalDate today, LocalDate renewal, int days, LocalDate selected) {
        LocalDate base = renewal != null && renewal.isAfter(today) ? renewal : today;
        if (plan >= 0 && plan <= 2) return base.plusMonths(plan + 1);
        if (plan == 3) return null;
        if (plan == 4) {
            if (days < 1 || days > 36500) throw new IllegalArgumentException("Informe entre 1 e 36.500 dias.");
            return base.plusDays(days);
        }
        if (plan == 5) {
            if (selected == null || selected.isBefore(today)) throw new IllegalArgumentException("Escolha hoje ou uma data futura.");
            return selected.plusDays(1); // The selected day is fully included.
        }
        throw new IllegalArgumentException("Plano inválido.");
    }
}
