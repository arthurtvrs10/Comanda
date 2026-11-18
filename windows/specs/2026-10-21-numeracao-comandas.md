# Numeração inicial das comandas — 21/10/2026

O comando Numeração de pedidos agora abre Numeração das comandas. O campo Começa com define o primeiro número real da grade, em vez de atribuir apenas um código sequencial adicional a cada abertura.

- Com início100 e quantidade20, a grade oferece100–119. A quantidade permanece nas configurações do sistema.
- Abertura e transferência respeitam o início e o último número da faixa configurada.
- Comandas abertas fora da faixa continuam visíveis e podem ser retomadas até seu encerramento. Seus números e históricos não são reescritos.
- Desmarcar a personalização retorna o início a1.
- A preferência antiga preenchida na janela é lida como início da faixa. Ao salvar a nova configuração, o antigo contador separado é desativado para novas aberturas; códigos históricos e contador anterior são preservados.
- A opção de senha no comprovante usa o número da própria comanda.
- A faixa é validada antes de gravar e a grade é atualizada após salvar.

Testes verificam persistência, compatibilidade com a preferência anterior, limites100/119, rejeição99/120, transferência, preservação de comanda3 aberta fora da faixa e rejeição de transbordamento.
