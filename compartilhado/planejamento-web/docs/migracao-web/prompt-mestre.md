# Prompt mestre — Analista de requisitos do VComanda Web

Atue como analista de requisitos sênior responsável pela documentação da migração integral do VComanda de C# para uma plataforma web com frontend Next.js, backend Spring Boot e PostgreSQL. Seu público é a equipe que irá estimar, desenvolver, testar e homologar o sistema com os estabelecimentos. Sua entrega nesta rodada é a documentação completa e um plano de implementação dividido em sprints. Reserve a programação, a execução de pilotos e a publicação para etapas posteriores.

O foco principal é a **comanda por pessoa**. Cada pessoa acompanha e encerra seu próprio consumo. A mesa, quando usada, indica a localização e pode reunir várias comandas individuais. Aplique essa prioridade ao MVP, aos diagramas, à planilha e aos testes de campo, inclusive ao revisar exemplos antigos do roteiro centrados na mesa.

## Método de trabalho

1. Leia AGENTS.md e o planejamento existente em docs/migracao-web. Consulte a branch oficial codex/release-atual e registre o SHA de referência. Cruze código, testes, notas de release, specs e documentos de homologação. Cite o caminho e a referência de cada evidência importante. Considere as edições normal e demonstrativas e identifique suas diferenças.
2. Produza uma primeira versão de **todos** os documentos abaixo. As etapas E01–E14 da planilha são marcos do planejamento. Desenvolva a documentação de todos os marcos nesta rodada. Quando uma decisão depender de entrevista, prepare a pergunta, marque o trecho como pendente e prossiga com os demais documentos.
3. Classifique as informações como confirmadas na fonte, propostas pela analista, pendentes de validação ou aprovadas pelo responsável. Preserve decisões e evidências anteriores. Registre conflitos entre código, documentação e relatos. Use exemplos fictícios identificados e mantenha dados reais e credenciais fora dos arquivos.

## Documentos a produzir

Use caminhos relativos a docs/migracao-web/. Aproveite os documentos existentes nos caminhos indicados em roteiro.xlsx. Crie um índice README.md com finalidade, caminho, estado e responsável sugerido de cada entrega.

| Pasta | Entregas e conteúdo mínimo |
|---|---|
| 01-descoberta/ | inventario.md com matriz de paridade por capacidade, perfil, edição, dependência, fonte e teste. campo.md com roteiro de entrevistas e observação, processo atual, problemas e evidências a coletar. |
| 02-produto/ | mvp.md com visão, usuários, problema, jornada futura, indicadores, prioridades e recorte do MVP. Justifique cada inclusão e adiamento. Mantenha no backlog as capacidades aprovadas que serão migradas depois do MVP. |
| 03-requisitos/ | matriz.md com requisitos funcionais (RF), não funcionais (RNF), regras de negócio (RN), glossário e rastreabilidade. Inclua casos de uso com ator, gatilho, permissões, pré-condições, fluxo principal, alternativas, exceções e pós-condições. |
| 04-ux/ | prototipo.md com fluxos por perfil, telas necessárias, navegação por teclado e celular, estados vazios, erros e roteiro de teste de usabilidade. Aplique impeccable nas tarefas de interface e experiência de uso. |
| 05-arquitetura/ | modelos.md com diagramas Mermaid de contexto, contêineres, estados da comanda, sequência de fechamento e modelo de dados. Descreva cardinalidades, chaves e invariantes. Especifique operações e erros em openapi.yaml. Documente opções e recomendações em ADRs numerados, com estado, justificativa, consequências e condição de revisão. |
| 06-entregas/ | backlog.md com épicos e histórias priorizadas. sprints.md com o plano integral da implementação. Use comanda-por-pessoa.md e fechamento.md para detalhar essas jornadas. Vincule cada história aos requisitos, aos testes e a uma sprint. |
| 07-pilotos/ | Plano de testes, P01.md para ensaio acompanhado e P02.md para piloto em turno autorizado. Descreva responsáveis, ambiente, massa de teste, referência de comparação, métrica, critério de interrupção e ficha de decisão. |
| 08-migracao/ | ensaio.md com origem e destino dos dados, conversões, precisão monetária, reconciliação de saldos e comandas abertas, cópia de segurança, restauração e tratamento das vendas registradas após o corte em caso de retorno. |
| 09-transicao/ | ondas.md com lacunas de paridade, riscos e responsáveis. corte.md com prontidão por local, treinamento, suporte, monitoramento, reversão e critérios de aceite para retirar o desktop. |

## Pesquisa dos concorrentes e oportunidades para o VComanda

Acesse os sites oficiais dos 11 concorrentes abaixo. Comece pela página indicada e siga os links públicos relevantes para produto, central de ajuda, demonstrações, integrações e preços. Registre a data de consulta, o mercado atendido e a URL específica que sustenta cada constatação. Quando a página estiver indisponível ou o recurso só puder ser visto em demonstração privada, registre a limitação e a pergunta que ficou em aberto. Diferencie recurso documentado, alegação comercial e informação não localizada.

| Concorrente | Página inicial da pesquisa |
|---|---|
| Chef Hábil | https://chefhabil.com.br/ |
| Eclética | https://ecletica.com.br/ |
| Sischef | https://sischef.com/sistema-para-restaurante/ |
| KCMS | https://www.kcms.com.br/ |
| Epos Now | https://www.eposnow.com/us/systems/ |
| Aldelo | https://www.aldelo.com/restaurant-pos |
| Toast | https://pos.toasttab.com/ |
| Square for Restaurants | https://squareup.com/us/en/point-of-sale/restaurants |
| SpotOn | https://www.spoton.com/ |
| NCR Voyix | https://www.ncrvoyix.com/platform/restaurant-applications |
| Lavu | https://lavu.com/ |

Investigue a comanda individual desde o primeiro pedido. Para cada concorrente, verifique como o operador distingue duas pessoas na mesma mesa, atribui itens a cada uma, acompanha saldos e recebe de uma pessoa enquanto a outra continua consumindo. Classifique separadamente comanda individual desde a abertura, identificação por assento e divisão da conta apenas no fechamento. Registre a fonte de cada conclusão e indique quando o comportamento ficar desconhecido. Investigue como itens compartilhados e mudanças de mesa são tratados. Valide em campo qual identificador funciona melhor, sem presumir cadastro nominal obrigatório.

Compare também catálogo e lançamento no celular, comunicação com a cozinha, impressão, permissões, histórico, relatórios, conectividade e dependências de hardware. Para referências internacionais, indique quais práticas podem ser testadas num restaurante brasileiro e quais dependem de meios de pagamento, equipamentos ou serviços locais. Compare cada oportunidade com o inventário da release antes de classificá-la como novidade, melhoria de um fluxo existente ou capacidade já atendida.

Entregue dois documentos e vincule ambos ao índice:

- `01-descoberta/concorrentes.md` — matriz comparativa com os 11 nomes, fontes por constatação, lacunas e uma seção dedicada à jornada da comanda por pessoa.
- `02-produto/oportunidades.md` — ideias priorizadas com ID, problema observado ou presumido, perfil beneficiado, referência, situação atual do VComanda, melhoria proposta, impacto esperado, esforço preliminar, risco e experimento mais barato. Destaque quais ideias merecem a primeira visita ao restaurante e explique por quê.

Para cada ideia selecionada, escreva uma pergunta de campo, uma métrica e um critério de decisão. Por exemplo: para avaliar a abertura de comanda no primeiro lançamento, observar quantas ações o garçom realiza hoje e quantas vezes atribui o item à pessoa errada. Registre o resultado como hipótese até coletar evidência. Vincule oportunidades selecionadas ao backlog, aos requisitos e à sprint de investigação ou implementação. Preserve as restrições de publicação de cada módulo.

## Profundidade exigida

Para cada RF, informe quem usa, o comportamento observável e pelo menos um aceite em Dado/Quando/Então. Para cada RNF, indique cenário, unidade, alvo proposto, ambiente e método de medição. Para cada RN, inclua exemplo e exceção. Identifique permissões por perfil e use IDs estáveis para rastrear capacidade legada → requisito → história → sprint → teste → evidência de homologação.

Cubra os fluxos de instalação e ativação equivalentes na web, operadores, mesas, cinco tipos de pedido, catálogo UN/KG, pagamentos, fechamento, histórico, impressão, configurações, backup e restauração. Complete a lista a partir do inventário. Especifique concorrência entre dispositivos, reenvio de pedido, isolamento de estabelecimentos e comportamento nas quedas de internet, Wi-Fi ou servidor. Leve hospedagem local, nuvem, modelo híbrido e eventual ponte temporária com C# para decisão registrada. Preserve os limites do AGENTS.md quanto a chamadas das mesas, KDS recente, complementos/estoque, recuperação por e-mail e transferência de licença por código curto.

## Como montar o plano de implementação em sprints

- Assuma inicialmente sprints de duas semanas e equipe pequena. Use a data inicial de roteiro.xlsx como proposta editável. Declare capacidade, disponibilidade dos pilotos e calendário como hipóteses. Dê datas previstas de início e fim, dependências e margem para retrabalho. Quando a capacidade for desconhecida, apresente estimativa preliminar e explique como recalibrar após a primeira sprint.
- Cada sprint deve ter ID, objetivo de negócio, histórias e requisitos associados, tarefas técnicas, tamanho estimado, perfil responsável, dependências, incremento demonstrável, testes, critério de aceite e participante sugerido para a revisão. Diferencie história pronta para iniciar de incremento pronto para homologar. Escreva as duas definições no plano.
- Distribua entregas por jornadas completas. Como proposta inicial, organize as primeiras sprints em torno da validação do problema, abertura da comanda individual e lançamento de produtos por pessoa, conferência no caixa e fechamento. Sequencie as demais pela prioridade e dependências descobertas, até cobrir a paridade, migração, piloto e transição. Ajuste a sequência se a pesquisa apontar outra prioridade.
- Para cada revisão, declare o que demonstrar, o que medir e qual resultado leva a avançar, corrigir ou retirar uma hipótese. Exemplo de teste de integridade: após o servidor gravar um lançamento, simule a perda da resposta e repita o envio; somente a comanda da pessoa selecionada deve receber o item, exatamente uma vez.
- Atualize roteiro.xlsx com uma aba Sprints e uma aba Backlog que referenciem os mesmos IDs, datas, caminhos e critérios de sprints.md. Relacione marcos E01–E14 às sprints. Preserve as abas existentes e os registros reais. Cada capacidade aprovada deve ter destino identificável no plano.

Inclua um cenário de aceite do MVP: Ana e Bia compartilham a mesa. Ana pede R$ 18 e Bia pede R$ 25. Quando Ana paga sua comanda, o histórico registra R$ 18 recebidos, a comanda de Bia continua aberta com R$ 25 e o garçom consegue lançar outro pedido para Bia.

## Validação com estabelecimentos

Antes de propor a primeira funcionalidade, oriente a equipe a investigar episódios recentes com dono, garçom e caixa. Registre frequência, impacto e solução atual. Proponha um teste pequeno para a hipótese de maior risco. Por exemplo, se o caixa redigita pedidos, conte as redigitações por dez pedidos antes e durante um ensaio. Identifique critérios e amostras sugeridos como propostas. Deixe resultados e aprovações em aberto até a coleta real.

## Piloto gratuito financiado pelo fundador

Planeje o piloto com cobrança de R$ 0 ao estabelecimento durante o período combinado. O fundador financia a fase inicial. Trate teto mensal em reais, horas disponíveis para suporte, duração, equipamentos e modelo comercial futuro como decisões em aberto. Proponha começar com um estabelecimento, um turno delimitado e um segundo local com rotina diferente para verificar se o aprendizado se repete.

- Em 07-pilotos/acordo-do-piloto.md, prepare uma ficha de uma página: problema a testar, datas, o que o local recebe, equipamentos e conexão necessários, treinamento, contato de suporte, responsável no turno e como desistir, exportar os registros e encerrar o teste. Combine previamente o uso de dados, o que será registrado e por quanto tempo. Qualquer cobrança futura depende de nova combinação expressa.
- Em 07-pilotos/custos.md e numa aba Custos da planilha, estime cenários de 1, 3 e 5 locais. Separe implantação de recorrência e desembolso de tempo do fundador. Inclua hospedagem, banco, backup, impressão/equipamentos, deslocamento, treinamento e suporte quando aplicáveis. Para cada opção gratuita, verifique condições de uso comercial, limites, suspensão por inatividade, recuperação, custo ao exceder a franquia e fonte com data. Compare infraestrutura existente no local e hospedagem externa, indicando quem absorve cada custo. Deixe preços desconhecidos pendentes. Calcule total mensal, custo médio por local ativo e folga em relação ao teto informado. Antes de propor despesas ou convidar outro local, mostre o impacto esperado.
- Prepare um roteiro curto para recrutamento e selecione locais onde várias pessoas consomem na mesma mesa e pagam separadamente. Observe um turno com o método atual, faça um ensaio com dados fictícios e combine quem autoriza o primeiro turno real. Registre versão testada, volume de atendimentos e condições do turno para interpretar a comparação.
- Escolha poucas medidas que permitam decidir: correções de atribuição por 100 itens, tempo de fechamento individual, divergências de caixa por turno e minutos de suporte do fundador por local. Registre numerador, denominador, linha de base e resultado. Com volume pequeno, mostre também a contagem bruta. Registre se o operador conseguiu concluir o fluxo sozinho e se o local voltou a usar o sistema no turno seguinte.
- Ao fim de cada piloto, registre a decisão e a evidência: repetir com ajustes, expandir para outro local ou interromper. Investigue separadamente valor para o operador, interesse do dono em continuar, eventual disposição a pagar e capacidade do fundador de sustentar a gratuidade. Antes de expandir, confira integridade dos saldos individuais, autonomia da equipe, esforço de suporte e orçamento disponível. Se houver perda de lançamento ou divergência financeira, preveja pausa, conciliação e retorno ao processo combinado.

## Critério de conclusão da análise

Entregue os arquivos preenchidos, a planilha e um índice navegável. Confira a cobertura de todas as capacidades identificadas, os vínculos entre IDs e a coerência de datas e dependências. Separe no resumo final o que já está pronto para estimativa, o que depende de decisão e as perguntas prioritárias para a primeira visita. Documentação concluída significa que a equipe consegue entender, estimar e validar cada incremento, inclusive reconhecer as lacunas que ainda impedem sua implementação.


