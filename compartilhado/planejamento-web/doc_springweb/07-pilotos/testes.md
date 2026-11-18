# Plano de testes
Começar com fixtures isoladas por estabelecimento: dois operadores, duas contas na mesa 7, produto UN e produto KG, venda histórica e um pagamento cancelado. Executar regras unitárias, integração em PostgreSQL com transações concorrentes, contratos HTTP e jornada em navegador. Ensaio de hardware e rede requer equipamentos reais do piloto.
| ID | Procedimento ou resultado esperado | Responsável | Evidência a guardar |
| --- | --- | --- | --- |
| T01 | Linha de base e método de identificação registrados | Fundador | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T02 | Atendente de A recebe recusa ao consultar conta de B | Gestor | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T03 | 0,250 kg a R$ 40/kg soma R$ 10 e preço histórico permanece | Gestor | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T04 | Duas pessoas na mesma mesa recebem contas distintas; código ocupado retorna conflito | Garçom | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T05 | Reenvio após resposta perdida produz um item; outra conta permanece intacta | Garçom | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T06 | Receber 18 de Ana encerra só Ana; Bia segue com 25; repetição mantém uma venda | Caixa | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T07 | Falha no papel permite reimpressão sem outra venda nem novo envio à produção | Caixa | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T08 | Exportação e histórico reproduzem totais, formas e operador originais | Gestor | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T09 | Cinco tipos persistem ao reabrir; dois terminais atendem com WAN desligada | Atendente | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T10 | Base normal vazia e demos isoladas; reiniciar preserva configuração e catálogo | Gestor | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T11 | Contagens, totais e recebimentos conciliam; segundo ensaio não duplica registros | Administrador | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
| T12 | Ata registra autonomia, saldos e decisão do local com evidência | Fundador e caixa | Versão, ambiente, entrada, esperado/obtido e registro P01/P02 |
Adicionar aos testes de S04: saldo positivo impede fechamento, pagamento misto preservado, troco exato, reenvio após commit. S05: papel ausente e reimpressão identificada. S06: perda de Wi-Fi versus perda de WAN, reconexão, login expirado. S07: backup inválido, falha no meio da carga e repetição completa. Registrar falhas sem dados de clientes; homologação web ainda pendente.
