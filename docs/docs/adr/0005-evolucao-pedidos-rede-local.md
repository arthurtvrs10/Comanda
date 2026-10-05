# ADR 0005 — Evolução por pedidos e rede local
Data: 05/10/2026. Status: proposta documentada; implementação não iniciada.

## Contexto
A operação atual controla consumo em um Windows. A evolução deve acompanhar preparo e entrega e, posteriormente, conectar cozinha e atendimento.

## Direção
1. Preservar C#/.NET, camadas e dados atuais.
2. Introduzir pedido dentro da comanda antes de distribuir o sistema.
3. Validar preparo e entrega no Windows.
4. Introduzir API local somente em P3, com persistência central e reenvio sem duplicação.
5. Manter pagamentos e fiscal fora dessas entregas.

## Consequências
Sem reestruturação integral antecipada. Dispositivos dependem do servidor local; operar sem internet não implica confirmar pedidos sem servidor.
A solução de interface móvel, segurança, numeração e cancelamento ainda exige decisão.
ADRs anteriores continuam aplicáveis ao produto atual.

## Referência
[Plano, requisitos e decisões abertas](../21-evolucao-atendimento.md).
