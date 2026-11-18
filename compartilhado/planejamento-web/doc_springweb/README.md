# VComanda Web · documentação de desenvolvimento
**Primeira versão para estimativa e validação | 17/11/2026.** O produto acompanha o consumo de cada pessoa desde o primeiro pedido. A mesa é uma localização opcional. O cenário de referência é receber Ana e continuar atendendo Bia na mesma mesa, com histórico e saldo independentes.

**Por onde começar.** Leia o inventário e o MVP. Leve o roteiro de campo à primeira visita. Para planejar o desenvolvimento, abra [o plano de sprints](06-entregas/sprints.md) e a [planilha](roteiro.xlsx). O material abaixo está ordenado por decisão, da descoberta à substituição do desktop.

| Entrega | Uso | Responsável sugerido | Estado |
| --- | --- | --- | --- |
| [inventario.md](01-descoberta/inventario.md) | Paridade da release Windows e conflitos históricos | Dev | Referência verificada |
| [campo.md](01-descoberta/campo.md) | Roteiro da primeira visita e ficha de observação | Fundador | Proposta para revisão |
| [concorrentes.md](01-descoberta/concorrentes.md) | Evidências públicas de onze concorrentes | Produto | Pesquisa pública |
| [mvp.md](02-produto/mvp.md) | Recorte individual e critérios do primeiro incremento | Produto e caixa | Proposta para revisão |
| [oportunidades.md](02-produto/oportunidades.md) | Quatro hipóteses e experimentos | Produto | Proposta para revisão |
| [matriz.md](03-requisitos/matriz.md) | RF, RN, RNF, permissões e casos de uso | Analista | Proposta para revisão |
| [prototipo.md](04-ux/prototipo.md) | Fluxos propostos para celular, teclado e caixa | UX e operadores | Proposta para revisão |
| [modelos.md](05-arquitetura/modelos.md) | Contexto, contêineres, estados, sequência e dados | Dev | Proposta para revisão |
| [openapi.yaml](05-arquitetura/openapi.yaml) | Contrato HTTP inicial de catálogo e comanda | Dev | Proposta para revisão |
| [ADR-001.md](05-arquitetura/ADR-001.md) | Decisão proposta de hospedagem e rede | Fundador | Proposta para revisão |
| [ADR-002.md](05-arquitetura/ADR-002.md) | Decisão proposta de transição tecnológica | Dev | Proposta para revisão |
| [ADR-003.md](05-arquitetura/ADR-003.md) | Decisão proposta de identificação e recebimento | Caixa e produto | Proposta para revisão |
| [backlog.md](06-entregas/backlog.md) | Doze histórias, estimativas e testes | Dev e produto | Proposta para revisão |
| [sprints.md](06-entregas/sprints.md) | Calendário, dependências, aceite e responsáveis | Fundador | Proposta para revisão |
| [comanda-por-pessoa.md](06-entregas/comanda-por-pessoa.md) | Jornada de abertura e consumo individual | Garçom | Proposta para revisão |
| [fechamento.md](06-entregas/fechamento.md) | Recebimento e encerramento independente | Caixa | Proposta para revisão |
| [testes.md](07-pilotos/testes.md) | Plano de teste e massa fictícia | Dev | Proposta para revisão |
| [P01.md](07-pilotos/P01.md) | Ensaio acompanhado | Garçom e caixa | A executar |
| [P02.md](07-pilotos/P02.md) | Turno real e ata de decisão | Dono | A executar |
| [acordo-do-piloto.md](07-pilotos/acordo-do-piloto.md) | Ficha de condições do piloto gratuito | Dono e fundador | Proposta para revisão |
| [custos.md](07-pilotos/custos.md) | Orçamento de um, três e cinco locais | Fundador | Proposta para revisão |
| [ensaio.md](08-migracao/ensaio.md) | Importação, conciliação, restauração e retorno | Administrador | Proposta para revisão |
| [ondas.md](09-transicao/ondas.md) | Paridade restante e riscos por onda | Produto | Proposta para revisão |
| [corte.md](09-transicao/corte.md) | Decisão de entrada ou retorno por local | Dono | Proposta para revisão |
| [roteiro.xlsx](roteiro.xlsx) | Calendário, backlog e simulação de custos | Fundador | Proposta para revisão |

**Pronto para estimar:** histórias H01–H12, critérios de aceite, dependências e contrato inicial da jornada principal. Oito sprints quinzenais propostas a partir de 23/11/2026. O fundador acumula funções; estimativas pressupõem seis dias de trabalho focado por ciclo e precisam ser revistas após S01.

**Decisões em aberto:** identificação física da pessoa, regra para porção compartilhada, capacidade do PC e impressora, implantação local ou externa, limites de tempo e dinheiro do fundador, autorização do estabelecimento, janela de corte e necessidade de continuidade offline. Nenhum resultado de campo ou aceite operacional foi preenchido.

**Primeira visita:** como se identifica quem pediu quando duas pessoas mudam de lugar? Quem confere o saldo quando uma delas sai? Qual foi a última divergência e quanto tempo levou para corrigir? Quem pode interromper o turno piloto? Registre casos reais antes de escolher o identificador ou financiar outro local.
