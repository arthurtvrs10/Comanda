# ADR 0006 — Servidor local para atendimento pelo celular

Data: 23/10/2026. Status: aceito pelo pedido explícito do responsável nesta conversa. Ele solicitou iniciar o servidor no modelo do Food Flow POS e escolheu priorizar o garçom lançando pedidos pelo celular.

## Decisão

Preservar C#/.NET 10, WPF, SQLite e os casos de uso. Acrescentar biblioteca ASP.NET Core/Kestrel no processo do desktop, iniciada manualmente em Configurações → Servidor / acesso pelo celular. Porta padrão 3001, selecionável, rede privada IPv4 e QR Code. O desktop continua operando sem servidor e sem internet. Não copiar código, banco ou tecnologias Node/React do projeto de referência.

Antecipar somente o atendimento móvel da P3 proposta na ADR 0005: abrir/consultar comandas e adicionar produtos às comandas existentes. Não introduzir entidade Pedido, estados de preparo, entrega ou KDS remoto nesta etapa. A categoria COZINHA (KDS) preserva a consulta local de cozinha existente.

## Persistência e acesso

Somente o processo do desktop abre SQLite. A API usa AbrirComanda/AdicionarItem; nenhum dispositivo acessa o arquivo de banco. Link com chave aleatória por sessão, enviada em cabeçalho pelo navegador. Cada início invalida a chave anterior; chave no fragmento do QR, não na query nem em logs.

Expor somente catálogo ativo, comandas abertas e lançamentos; sem rotas de pagamentos, administração, histórico completo, cancelamento, exclusão ou encerramento. HTTP destina-se a rede confiável. Não abrir firewall, encaminhamento de porta, túnel ou acesso público automaticamente. Não equivale a criptografia de transporte ou autenticação individual de colaborador.

Escritas remotas serializadas exigem Idempotency-Key. Confirmações preservadas em memória durante a sessão, até 20.000, sem expulsar as anteriores; limite interrompe novos lançamentos. Mesma identificação e corpo devolvem a confirmação anterior. Não prometer reenvio após reinício ou armazenamento offline. O cliente conserva uma operação pendente até resultado conclusivo. Após conexão renovada, conferir os itens antes de lançar novamente.

## Continuidade e evidências

Parar servidor ou fechar desktop encerra acesso remoto. Licença inválida impede operações. Não restaurar dados/fotos com servidor ativo ou iniciando; não iniciar durante backup/recuperação. Atualização remota preserva campos locais em edição. Verificações de licença serializadas preservam o arquivo protegido.

RF31 → UC12 → CT26; RN29 e RN01/RN03–RN08/RN10/RN19/RN21/RN23; RNF01/RNF07/RNF18. Testes HTTP reais em loopback e SQLite descartável. Pendentes: Wi-Fi/firewall com celular físico, revisão visual em navegador e carga prolongada. Nenhuma publicação da release nesta etapa.
