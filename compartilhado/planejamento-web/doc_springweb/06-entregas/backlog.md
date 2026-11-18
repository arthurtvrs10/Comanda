# Backlog de implementação
Épicos: descobrir o processo (H01); operar por pessoa (H02–H06); reproduzir a operação aprovada (H07–H10); adotar e sustentar (H11–H12). Prioridade pela sequência de dependências. Estimativas preliminares em dias de trabalho focado de uma pessoa. Ajustar após S01.
| História | Como usuário, preciso… | Aceite e demonstração | Estimativa | Referências |
| --- | --- | --- | --- | --- |
| H01 Descoberta do turno | Como Fundador, preciso ouvir caixa e garçom e contar correções em dez pedidos | Linha de base e método de identificação registrados | 5 dias | C01 / RF01 / T01 / S01 |
| H02 Acesso por estabelecimento | Como Gestor, preciso ativar local e atribuir perfil de atendente, caixa ou gestor | Atendente de A recebe recusa ao consultar conta de B | 3 dias | C02 / RF02 / T02 / S02 |
| H03 Catálogo de venda | Como Gestor, preciso cadastrar categoria, foto, preço, UN/KG e desativar produto | 0,250 kg a R$ 40/kg soma R$ 10 e preço histórico permanece | 3 dias | C03 / RF03 / T03 / S02 |
| H04 Comanda individual | Como Garçom, preciso abrir conta pelo código da pessoa, com mesa e nome opcionais | Duas pessoas na mesma mesa recebem contas distintas; código ocupado retorna conflito | 3 dias | C04 / RF04 / T04 / S03 |
| H05 Consumo por pessoa | Como Garçom, preciso lançar, corrigir quantidade, remover item e anotar observações | Reenvio após resposta perdida produz um item; outra conta permanece intacta | 3 dias | C05 / RF05 / T05 / S03 |
| H06 Recebimentos e fechamento | Como Caixa, preciso registrar pagamento externo, parcial ou misto, conferir troco e finalizar | Receber 18 de Ana encerra só Ana; Bia segue com 25; repetição mantém uma venda | 6 dias | C06 / RF06 / T06 / S04 |
| H07 Impressão e produção aprovada | Como Caixa, preciso emitir conta, cozinha e fichas; reimprimir com identificação | Falha no papel permite reimpressão sem outra venda nem novo envio à produção | 4 dias | C07 / RF07 / T07 / S05 |
| H08 Históricos e exportação | Como Gestor, preciso filtrar pedidos e vendas por turno e conferir recebimentos | Exportação e histórico reproduzem totais, formas e operador originais | 2 dias | C08 / RF08 / T08 / S05 |
| H09 Modalidades e operação local | Como Atendente, preciso usar Local (Mesa), Salão, Balcão, Para levar e Delivery, painel de senhas e acesso móvel | Cinco tipos persistem ao reabrir; dois terminais atendem com WAN desligada | 3 dias | C09 / RF09 / T09 / S06 |
| H10 Configuração e edições | Como Gestor, preciso configurar estabelecimento, limites, numeração, acesso local e edição | Base normal vazia e demos isoladas; reiniciar preserva configuração e catálogo | 3 dias | C10 / RF10 / T10 / S06 |
| H11 Importação e recuperação | Como Administrador, preciso importar cópia SQLite, restaurar backup e retomar com conciliação | Contagens, totais e recebimentos conciliam; segundo ensaio não duplica registros | 6 dias | C11 / RF11 / T11 / S07 |
| H12 Piloto e transição | Como Fundador e caixa, preciso acompanhar turno, treinar operador e decidir continuidade | Ata registra autonomia, saldos e decisão do local com evidência | 5 dias | C12 / RF12 / T12 / S08 |
