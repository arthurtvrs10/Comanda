# Modelo proposto para o piloto
## Contexto
```mermaid
flowchart TD
 G[Garçom] --> V[VComanda Web]
 C[Caixa] --> V
 A[Gestor] --> V
 V --> I[Impressora local]
 C --> M[Maquininha externa]
```
## Contêineres
```mermaid
flowchart TD
 N[Navegador] --> W[Next.js na LAN]
 W --> S[Spring Boot]
 S --> D[(PostgreSQL)]
 S --> F[Fila de impressão]
 F --> P[Adaptador local]
 D --> B[Cópia de recuperação]
```
## Estados
```mermaid
stateDiagram-v2
 [*] --> Aberta
 Aberta --> Cancelada: cancelar sem recebimento ativo
 Aberta --> EmRecebimento: registrar parcial
 EmRecebimento --> Aberta: cancelar todos os recebimentos
 EmRecebimento --> Fechada: quitar e confirmar
 Fechada --> [*]
 Cancelada --> [*]
```
EmRecebimento é condição derivada na proposta web. O enum Windows contém Aberta, Fechada e Cancelada. Preserve o campo Bloqueada separadamente ao importar.
## Fechamento após perda de resposta
```mermaid
sequenceDiagram
 participant C as Caixa
 participant A as API
 participant D as PostgreSQL
 C->>A: Fechar com chave K e versão
 A->>D: Validar saldo e gravar venda e resultado de K
 D-->>A: Commit
 Note over C,A: Resposta perdida
 C->>A: Repetir com K
 A->>D: Consultar resultado de K
 D-->>A: Venda original
 A-->>C: Mesma venda
```
## Dados
```mermaid
erDiagram
 ESTABELECIMENTO ||--o{ COMANDA : atende
 MESA o|--o{ COMANDA : localiza
 COMANDA ||--o{ ITEM : acumula
 PRODUTO ||--o{ ITEM : origina
 COMANDA ||--o{ RECEBIMENTO : registra
 COMANDA ||--o| VENDA : encerra
 OPERADOR ||--o{ ITEM : lanca
```
IDs internos imutáveis. Índice único para número de comanda ativa por estabelecimento e para venda por comanda. Pessoa é identificador operacional da visita; nome opcional. Preço histórico em centavos e quantidade decimal por unidade. Relacionamentos devem respeitar estabelecimento. Versão aumenta em cada mutação. Resultado de idempotência persistido na transação com identidade do local, rota, chave e hash do pedido; chave igual com dados diferentes gera conflito.
