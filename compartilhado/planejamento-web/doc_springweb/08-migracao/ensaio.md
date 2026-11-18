# Ensaio de migração e retorno
Origem: cópia SQLite autorizada, edição e versão anotadas. Destino: instância PostgreSQL de ensaio com chave de estabelecimento. Registrar hash da cópia, contagens e totais antes da carga. Ensaiar restauração primeiro. Separar demos de produção.
| Origem | Destino proposto | Verificação |
| --- | --- | --- |
| Comanda.Id, Numero, NumeroMesa, NomeCliente | comanda + mapa de origem | Manter IDs históricos rastreáveis, códigos, status, bloqueio e tipo; nome vazio permanece aceito. |
| ItemComanda | item e fotografia do produto | Preservar Quantidade decimal, UnidadeVenda, nome, preço e subtotal em centavos. |
| PagamentosJson de comanda e venda | recebimento e fotografia da venda | Preservar cancelamentos, aplicado e troco. Evitar contar duas vezes a mesma fotografia. |
| Venda | venda | Comparar por comanda, data e turno; preservar fuso e vendas sem forma antiga. |
| Catálogo, fotos, operadores e configuração | catálogo e configuração por local | Verificar arquivos ausentes, acesso e impressoras. Reconfigurar segredos por canal autorizado. |
 
Carregar em área temporária, validar integridade referencial e promover em transação. Chave única de origem por edição/local/tabela/ID. Isolar divergências e guardar relatório. Comparar contagem de contas por estado, itens por conta, subtotal por item, recebimentos ativos e cancelados, valor por forma e total por turno. Tolerância financeira proposta: zero centavos. Reexecutar a mesma carga e comparar resultados.
 
Retorno: suspender novas escritas, exportar o diário web desde a virada, identificar quem recebeu valores externamente e reconstruir apenas diferenças aprovadas no processo legado. Reconciliar por ID e forma, arquivar evidência, então reabrir um único caixa. Ensaio obrigatório com venda nova após o corte e com comanda iniciada antes dele. Bases ambíguas de grupo preservam seus lançamentos e recebem anotação de origem.
