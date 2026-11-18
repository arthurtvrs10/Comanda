# QR Codes por mesa — somente chamar garçom

Implementação de 30/10/2026. Cada mesa recebe um link próprio, independente de comandas e pagamentos. A página pública contém somente o número da mesa, uma solicitação opcional e Chamar garçom.

## Configuração

Em Configurações > Servidor / acesso pelo celular, inicie o servidor, selecione o IP da rede e abra **QR Codes das mesas**. A quantidade começa em 20 quando ainda não foi definida; se o assistente inicial configurar mesas, sua quantidade será usada. A quantidade é independente da quantidade de comandas.

Ao aplicar N mesas, a lista apresenta Mesa 1 até Mesa N. Confira o código selecionado e use **Imprimir mesas** para etiquetas identificadas ou **Salvar todos em PNG** para os arquivos individuais. Não é necessário abrir ou vincular nenhuma comanda. O cliente precisa estar no mesmo Wi-Fi e o computador precisa permanecer ligado com o servidor ativo.

Os códigos persistem após reiniciar. O segredo específico das mesas fica na configuração local e não é a chave de acesso da equipe. Se IP, porta ou computador/base mudar, confira e reimprima os links; recomenda-se manter o IP do servidor reservado no roteador. Reduzir a quantidade desabilita as mesas acima do novo limite.

## Funcionamento

A chamada entra na coluna Chamadas do KDS com mesa, solicitação e tempo. Uma chamada ativa por mesa. O botão Atendida encerra somente a solicitação. A página consulta a situação a cada quatro segundos e libera um novo chamado quando a equipe marcar atendimento. O KDS atualiza no seu ciclo de três segundos.

Não há login de cliente, catálogo, lançamento de produtos, pagamento ou leitura de comandas nessa página. Tokens de uma mesa não autorizam outra mesa nem as APIs da equipe. Reenvio após perda da resposta reutiliza a mesma identificação. O acesso permite até seis tentativas de chamada por minuto por mesa; um QR público é compartilhável e não prova presença física do cliente.

## Evidências

- Quatro testes HTTP: chamada sem nenhuma comanda; bloqueio do catálogo e pedidos; isolamento das mesas; quantidade; persistência da chave após reinício; origem externa; entrada inválida e limite de chamadas.
- Quatro testes JavaScript: envio sem comanda, bloqueio enquanto pendente, reenvio com mesma identificação, QR inválido e HTML separado da interface da equipe.
- Teste WPF: quantidade da lista, QR distintos, prévia nos dois temas e composição de três etiquetas sem abrir comanda.
- O assistente de primeira configuração mantém seus testes de preservação e grava a quantidade de mesas ao concluir.

A impressão foi validada como documento, sem impressora física. O navegador integrado estava desativado nas configurações do ambiente; a página móvel foi verificada por testes do JavaScript/DOM e HTTP, sem ensaio visual em aparelho físico. O celular real disponível ainda precisa ler uma etiqueta e confirmar a chamada no KDS.

Não há migração de banco adicional; utiliza as configurações persistidas e a fila de chamadas existente. Complementos e estoque continuam fora da publicação.
