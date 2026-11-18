# Banco de ideias — sem compromisso de entrega

Registro das sugestões anteriores. Nomes de módulos e referências são conceitos, não produtos disponíveis nem compatibilidades verificadas. Prioridades oficiais: [plano de evolução](21-evolucao-atendimento.md). Estados de preparo pertencem ao pedido, não à comanda. Integrações precisam de validação própria.

## RAMO X — Restaurantes e Lanchonetes

O Varthex Orders foi pensado como um sistema **local-first**, modular e evolutivo para pequenos restaurantes, lanchonetes, cafeterias e operações similares.

A proposta é manter o núcleo do sistema simples e permitir que novas funcionalidades sejam adicionadas conforme as necessidades do estabelecimento, formando gradualmente o ecossistema Varthex.

| Evolução | Objetivo resumido | Referência / conceito |
|---|---|---|
| **Varthex KDS** | Enviar pedidos diretamente para telas da cozinha, organizados por estação, status e tempo de preparo. | Toast KDS / QSR |
| **Varthex Call** | Permitir que clientes chamem garçom, peçam atendimento ou solicitem a conta por botão, QR ou NFC. | Aceno / GPTRONICS / Nurse Call |
| **Varthex Ready** | Exibir pedidos prontos em TV ou monitor e avisar clientes para retirada. | McDonald's / sistemas QSR |
| **Varthex Queue** | Criar fila digital e controle de senhas para atendimento ou retirada. | Sistemas de filas / fast-food |
| **Varthex Table** | Usar QR Code ou NFC na mesa para cardápio, atendimento, conta e futuras interações. | Table Ordering / QR Ordering |
| **Varthex Self-Service** | Permitir que o próprio cliente monte e envie o pedido através de tablet ou totem. | McDonald's / Toast Kiosk |
| **Varthex Runner** | Avisar automaticamente garçons ou atendentes quando um pedido estiver pronto para entrega. | Food Runner / KDS |
| **Praças de Produção** | Direcionar automaticamente itens para chapa, fritura, bar, sobremesas ou outras estações. | KDS / Kitchen Routing |
| **Temporizador de Pedidos** | Monitorar tempo de espera e preparo, destacando pedidos atrasados. | Toast KDS |
| **Status da Comanda** | Separar situação da comanda dos estados de preparo e entrega dos pedidos; pagamentos dependem de escopo próprio. | POS de restaurantes |
| **Modificadores de Produto** | Permitir adicionais, remoções, observações, tamanho e ponto de preparo. | Toast / Oracle MICROS |
| **Combos** | Criar combinações de produtos com opções obrigatórias ou adicionais. | POS de fast-food |
| **Divisão de Conta** | Dividir pagamento por pessoa, item, quantidade ou valor. | Toast / Lightspeed |
| **Mapa de Mesas** | Mostrar visualmente mesas livres, ocupadas, aguardando atendimento ou pagamento. | Oracle MICROS / Lightspeed |
| **Transferência de Mesa** | Transferir uma comanda completa entre mesas. | POS de restaurantes |
| **Junção de Mesas** | Agrupar mesas e comandas em uma única conta. | Oracle MICROS |
| **Histórico de Operações** | Registrar alterações importantes realizadas em pedidos, comandas e pagamentos. | Audit Log |
| **Cancelamento Controlado** | Registrar motivo, horário e responsável por cancelamentos ou exclusões. | POS / controle gerencial |
| **Controle de Permissões** | Separar funções e acessos entre administrador, gerente, caixa, garçom e cozinha. | RBAC / sistemas POS |
| **Fechamento de Caixa** | Controlar abertura, entradas, retiradas, sangrias e fechamento diário. | Sistemas POS |
| **Dashboard Local** | Mostrar vendas, ticket médio, pedidos, produtos e horários de pico. | Toast Analytics |
| **Curva ABC** | Identificar produtos com maior participação nas vendas e faturamento. | ERP / gestão de estoque |
| **Varthex Stock** | Controlar estoque e realizar baixas conforme produtos são vendidos. | ERP / Inventory Management |
| **Ficha Técnica** | Relacionar produtos vendidos aos ingredientes realmente consumidos. | Food Cost / Recipe Management |
| **Estoque Mínimo** | Alertar automaticamente quando determinado produto ou ingrediente estiver acabando. | ERP / e-Kanban |
| **Varthex Refill** | Permitir solicitação rápida de reposição de insumos entre setores. | Andon / e-Kanban industrial |
| **Varthex Waste** | Registrar perdas, desperdícios, erros de produção e produtos vencidos. | Food Waste Management |
| **Previsão de Demanda** | Utilizar histórico para estimar quantidade de produtos necessária por período. | Demand Forecasting |
| **CMV / Food Cost** | Calcular custo dos produtos vendidos e estimar margem de cada item. | Gestão de custos |
| **Cardápio por Horário** | Alterar disponibilidade, preço ou produtos conforme período do dia. | Toast / QSR |
| **Promoções** | Criar descontos, combos, horários promocionais e regras comerciais. | POS / Pricing Engine |
| **Fidelidade** | Oferecer pontos, benefícios ou cashback para estimular recorrência. | Loyalty Systems |
| **Cadastro de Clientes** | Permitir identificação opcional de clientes para histórico e relacionamento. | CRM |
| **Histórico do Cliente** | Exibir frequência, compras anteriores e produtos mais consumidos. | CRM / POS |
| **Avaliação de Atendimento** | Coletar avaliação rápida após atendimento ou fechamento da conta. | Customer Experience |
| **Varthex Analytics** | Consolidar indicadores de vendas, operação, cozinha e atendimento. | BI / Analytics |
| **Multiunidade** | Permitir futuramente gerenciamento de várias lojas pelo mesmo proprietário. | Toast / Oracle MICROS |
| **Backup Automático** | Criar backups locais periódicos com possibilidade de cópia externa ou cloud opcional. | Local-first |
| **Modo Offline** | Manter vendas e operação principal funcionando mesmo sem internet. | Offline-first POS |
| **Sincronização Opcional** | Sincronizar backups, configurações e indicadores quando houver internet. | Local-first + cloud |
| **API de Integrações** | Criar camada para integração futura com delivery, fiscal, pagamentos e parceiros. | Toast API / Oracle API |

### Diretriz de arquitetura

As evoluções devem seguir os seguintes princípios:

- operação principal funcionando localmente;
- internet como recurso opcional, e não dependência;
- arquitetura modular;
- baixo custo de implantação;
- reaproveitamento da infraestrutura existente no estabelecimento;
- simplicidade para funcionários e clientes;
- integração entre os módulos Varthex;
- novas funcionalidades somente quando resolverem problemas reais;
- evitar transformar o Varthex Orders em um ERP monolítico;
- permitir que cada estabelecimento utilize apenas os módulos necessários.

### Ecossistema proposto

```text
VARTHEX LOCAL CORE
        │
        ├── Varthex Orders
        ├── Varthex KDS
        ├── Varthex Call
        ├── Varthex Ready
        ├── Varthex Queue
        ├── Varthex Table
        ├── Varthex Stock
        ├── Varthex Refill
        ├── Varthex Waste
        └── Varthex Analytics
```

O **Varthex Orders** permanece como núcleo operacional do ramo de restaurantes e lanchonetes, enquanto funcionalidades mais específicas podem evoluir para módulos independentes conectados ao mesmo ecossistema.
