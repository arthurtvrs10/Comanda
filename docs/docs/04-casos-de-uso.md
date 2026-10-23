# Casos de uso

UC01–UC10 descrevem o MVP. Os fluxos futuros UCE01–UCE06 estão no [plano de evolução](21-evolucao-atendimento.md).

## UC01 Manter categorias e produtos

**Ator:** responsável pelo negócio.

**Pré-condição:** aplicativo iniciado e banco disponível.

**Fluxo principal:**

1. Abrir Produtos.
2. Cadastrar ou selecionar categoria.
3. Informar nome, preço, categoria e situação.
4. Validar campos.
5. Confirmar e atualizar o catálogo.

**Alternativas:** nome vazio ou preço inválido impedem gravação. Registro com histórico é desativado, não excluído.

## UC02 Abrir comanda

**Ator:** atendente.

**Pré-condição:** número sem comanda aberta.

1. Abrir Comandas.
2. Selecionar número livre.
3. Confirmar abertura.
4. Registrar data, hora e status `ABERTA`.
5. Exibir catálogo.

Se outro processo ocupar o número, recusar e atualizar a grade.

## UC03 Registrar item

**Ator:** atendente.

1. Selecionar categoria ou pesquisar produto.
2. Selecionar produto ativo.
3. Copiar nome e preço atuais para o item.
4. Adicionar uma unidade ou incrementar item equivalente.
5. Recalcular subtotal e total.

Produto desativado durante a operação não é incluído.

## UC04 Corrigir itens

**Ator:** atendente.

1. Selecionar item de comanda aberta.
2. Aumentar, diminuir ou informar quantidade.
3. Remover quando necessário.
4. Confirmar ação destrutiva.
5. Recalcular e persistir o total.

Quantidade fracionária ou negativa é recusada. Quantidade zero conduz à remoção confirmada.

## UC05 Exibir total e encerrar comanda

**Ator:** caixa.

**Pré-condição:** comanda aberta com itens e total positivo.

1. Localizar a comanda.
2. Revalidar itens, preços, subtotais e total.
3. Mostrar o detalhamento e destacar o total a pagar.
4. Orientar o operador a digitar o total manualmente na maquininha.
5. O operador realiza e aguarda a cobrança externa.
6. Se não aprovada, voltar sem alterar a comanda.
7. Se aprovada, o operador confirma o encerramento.
8. Em uma transação, criar venda e mudar a comanda para `FECHADA`.
9. Liberar o número.

**Pós-condição:** venda contém apenas referência da comanda, número, total, horário e status. Não contém dados do pagamento.

## UC06 Cancelar comanda

**Ator:** atendente ou caixa.

1. Abrir comanda.
2. Solicitar cancelamento.
3. Mostrar itens e efeito da ação.
4. Confirmar.
5. Marcar `CANCELADA` e liberar número.

Comanda fechada não pode ser cancelada no MVP.

## UC07 Consultar histórico e resumo

**Ator:** responsável pelo negócio.

1. Selecionar data ou período permitido.
2. Listar vendas concluídas.
3. Abrir detalhes de uma venda.
4. Exibir itens, total e horário.
5. Calcular quantidade, total e ticket médio.

Sem vendas, mostrar estado vazio e valores iguais a zero.

## UC08 Criar backup

**Ator:** responsável ou rotina automática.

1. Criar snapshot consistente.
2. Gravar arquivo com data, hora e versão.
3. Validar integridade.
4. Calcular checksum.
5. Registrar resultado e aplicar retenção.

Falha preserva backups anteriores e não bloqueia o atendimento.

## UC09 Restaurar backup

**Ator:** responsável ou suporte.

1. Impedir nova operação durante a restauração.
2. Selecionar arquivo.
3. Validar formato, versão e integridade.
4. Mostrar data e consequência.
5. Confirmar.
6. Criar cópia preventiva da base atual.
7. Substituir base e reiniciar.
8. Verificar abertura e integridade.

Arquivo incompatível ou corrompido é recusado sem alterar a base ativa.

## UC10 Recuperar atendimento interrompido

**Ator:** atendente ou caixa.

**Pré-condição:** o aplicativo foi encerrado com uma ou mais comandas abertas já persistidas.

1. Iniciar novamente o aplicativo.
2. Validar a estrutura e a integridade básica do banco.
3. Localizar comandas com status `ABERTA`.
4. Exibir os números como ocupados e restaurar itens e totais.
5. Permitir a continuidade normal do atendimento.

O aplicativo não cria venda, não fecha comanda e não repete a última alteração. Se a base falhar na validação, a aplicação bloqueia gravações e orienta a recuperação segura.

## UC11 Consultar histórico de pedidos — extensão em desenvolvimento de 22/10/2026

**Ator:** gestor ou atendente autenticado. **Requisito:** RF30; RN04/RN05/RN10/RN20/RN22/RN23 preservadas.

1. Abrir Principal → Histórico de pedidos. O Histórico de vendas também está em Principal.
2. Abrir a pesquisa e escolher status, abertura ou fechamento, período com horário, cliente, colaborador e códigos separados por vírgula. Cancelados são opcionais.
3. Pesquisar e conferir os registros e o total exibido. O total inclui os pedidos selecionados, inclusive abertos/cancelados quando presentes; não representa receita de vendas.
4. Usar Mais colunas para mostrar/ocultar dados existentes e agrupar por criador, origem, tipo ou pagamento. Código é obrigatório; preferências ficam no computador, separadas entre edição normal e demonstração.
5. Abrir os detalhes para consultar os itens e seus valores históricos. Somente uma comanda ainda aberta oferece continuidade no atendimento, com as permissões usuais.
6. Exportar as colunas visíveis para XLSX ou conferir a prévia antes de escolher a impressora.

Voltar ou fechar a pesquisa mantém os filtros anteriores. Erros não criam vendas ou alteram registros. O filtro usa Brasília e inclui todo o minuto final. Fiado, UTM, endereço e telefone não são inventados nem adicionados ao modelo. Esta extensão não altera os instaladores publicados da 1.4.0.

## UC12 — Lançar produtos pelo celular — 23/10/2026

RF31/RN29/CT26. Pré-condições: desktop aberto, licença válida e rede local confiável.
1. No computador, abrir Configurações → Servidor / acesso pelo celular e iniciar.
2. Selecionar o endereço da rede e ler QR Code no celular conectado ao mesmo Wi-Fi.
3. Informar atendente, selecionar comanda aberta ou abrir um número permitido.
4. Conferir os itens e total da comanda; tocar em + Produtos para abrir o catálogo com fotos. Pesquisar produto/categoria, tocar no cartão inteiro, informar quantidade e observação e confirmar.
5. Conferir a confirmação e os itens persistidos; o computador atualiza seus dados sem apagar rascunhos.
6. Se houver perda de resposta, confirmar a operação pendente com a mesma identificação.
7. Parar o servidor no computador para revogar acesso; cobrança e fechamento permanecem no Windows.

HTTP para rede confiável, sem mudança automática de firewall. Portas ocupadas, falta de licença, quantidade inválida, produto indisponível e comanda em fechamento não indicam sucesso. KDS continua como consulta local nesta entrega.
