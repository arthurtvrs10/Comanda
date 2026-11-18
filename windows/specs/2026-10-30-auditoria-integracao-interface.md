# Revisão de integração e interface — 30/10/2026

Revisão com Impeccable, leitura dos pontos de integração, testes automatizados e capturas WPF. Não representa certificação de todos os cenários de produção.

## Correções realizadas

- Pagamento: resumo recupera largura útil entre 700 e 899 DIP. Abaixo de 700 DIP fica em uma linha abaixo das formas. Antes a configuração de colunas deixava o resumo sem largura.
- Pagamento: rodapé recebeu duas linhas reais; botões deixam de sobrepor mensagens e atalhos.
- Ticket: Imprimir logo permanece desabilitado quando não há foto; informa o caminho Configurações do sistema > Geral. Seletor de logo removido conforme solicitação anterior.
- Testes de pagamento e Ribbon atualizados para os controles removidos por solicitação do usuário. Adicionadas verificações do resumo em 600, 800 e 1120 DIP e da logo ausente.

## Evidências

| Camada | Aprovados |
|---|---:|
| Domínio | 4 |
| Aplicação | 147 |
| Banco e infraestrutura | 333 |
| Servidor local | 17 |
| Desktop | 483 |
| API de recuperação (local) | 16 |

Total: 1.000 testes distintos aprovados considerando a execução inicial e a repetição dos testes afetados. Inicialmente o Desktop teve 481 aprovados e duas falhas por expectativas antigas; ambas passaram após atualização. A revisão dirigida final de integrações passou em 25 testes, e o último ajuste do rodapé passou no teste de pagamento. Não foi repetida toda a suíte após essas correções localizadas.

Resultados TRX nas subpastas Domain, Application, Infrastructure, Servidor, Desktop, correcoes, integracoes e pagamento-final. Capturas em capturas/. Detector Impeccable retornou []: a análise estática tem cobertura limitada de WPF e não comprova acessibilidade ou integração por si só.

## Conexões confirmadas

- Geral > imagem > armazenamento do layout > mesma imagem na conta: teste com imagem real, persistência e remoção.
- Ticket > opções > documento da conta: 58/80 mm, conteúdo e totais.
- Primeiros passos > rascunho > retomada > conclusão: banco real, preservação da segurança e ausência de duplicação de produtos.
- Dados da comanda > cliente/tipo/observação > persistência > atualização dos cartões KDS.
- KDS > transições de preparo, pedidos novos, limpeza somente visual e preservação dos registros: testes existentes aprovados.
- Pagamentos parciais/mistos > reabertura > saldo > fechamento de uma venda: banco SQLite descartável.
- Permissões, recuperação offline, backup e carregamento de telas: incluídos na execução Desktop inicial.

## Pendências verificadas e limites

1. [P1, integração] Chamadas das mesas: há modelo e armazenamento no KDS, mas o botão de registrar foi removido e não foi localizada rota no servidor local nem ação visível para a mesa enviar uma chamada. A coluna não constitui um fluxo completo de chamar atendente. Requer definir a origem da solicitação e conectá-la sem recolocar o botão removido.
2. [P2, configuração externa] Recuperação por e-mail: permanece deliberadamente desativada em produção até domínio e Resend serem configurados. Testes locais aprovados não significam entrega real de e-mail.
3. [P2, validação física] Impressoras, alto-falantes, firewall e acesso por um celular real não foram homologados nesta revisão. Prévia e regras de impressão não garantem saída física.
4. [P2, adaptividade] KDS mantém largura mínima de quadro de 1500 DIP e permite rolagem horizontal em telas menores. Cinco raias simultâneas dependem do tamanho disponível.
5. Complementos/estoque continuam no código de desenvolvimento. A aprovação dos testes não muda a orientação de mantê-los fora de publicação até validação do módulo.
6. DESIGN.md contém decisões históricas substituídas (cores, navegação e cozinha antiga). Foram usadas a implementação atual e as escolhas recentes como referência. Não houve reescrita automática da documentação de design.
7. A faixa compacta mantém a paleta clara aprovada inclusive no tema escuro, e somente a sessão atual é ocultada ao clicar em X. Progresso vem do rascunho concluído/validado; não é inferido automaticamente de todos os cadastros existentes.

Nenhum dado operacional do cliente foi alterado pelos testes. Bancos de teste são descartáveis. As correções desta revisão foram organizadas em commits separados de faixa, pagamento e ticket. Nenhum push ou release foi feito.

Próximas validações: conectar a origem das chamadas; homologar o percurso em celular e impressora reais; concluir configuração de e-mail quando houver domínio; executar revisão visual final com Impeccable polish após essas integrações.


## Complemento da revisão

A conexão das chamadas indicada como P1 foi implementada posteriormente nesta mesma data. Consulte [Chamadas das mesas e homologação](2026-10-30-chamadas-mesas-homologacao.md). O ensaio físico no celular continua aguardando confirmação do usuário; não há impressora disponível para homologação.
