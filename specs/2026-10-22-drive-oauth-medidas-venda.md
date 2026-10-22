# Google Drive OAuth e medidas de venda — 22/10/2026

Extensão da aplicação WPF existente, no modo Operate inspirado na organização
Consumer fornecida pelo usuário. O pedido autoriza convite após login sem Google
conectado, envio de backup pela conta Google e venda em UN, KG e L. O responsável
confirmou que ainda não possui credenciais e pediu a preparação da integração.
Não há projeto OAuth real, token ou conta conectada incluídos nesta entrega.
Não altera instaladores já publicados, estoque, cobrança externa ou a stack.

## Contratos e rastreabilidade

| Área | Requisitos e regras | Casos relacionados |
| --- | --- | --- |
| Produto, quantidade e histórico | RF03/RF04/RF09/RF10/RF12/RF19; RN04–RN08; RNF10/RNF15 | UC01/UC03/UC04/UC07; CT03–CT06/CT08/CT12/CT16 |
| Backup e restauração | RF21–RF24; RN19; RNF01/RNF11/RNF12/RNF14/RNF16 | UC08/UC09; CT13–CT16 |
| Convite, temas e teclado | RNF07/RNF18 | CT17 e testes específicos da extensão |

Esta especificação substitui, nestes pontos, a regra antiga de quantidade sempre
inteira e o fluxo Google por pasta sincronizada. Os documentos 02, 03 e 09 foram
ajustados; casos de uso e documentos históricos anteriores devem ser lidos com
este complemento. Domain e Application continuam sem dependências de APIs de
rede. Os fluxos locais operam offline; Google é opcional em Infrastructure/Desktop.

## Convite e conexão

Ao transitar para uma sessão de gestor com permissão de backup e sem conta Google
conectada, abrir o convite modal. Adiar não suprime o convite da próxima sessão.
O modal mede 450 × 560 DIP, mínimo declarado de 410 × 510, centralizado no dono,
sem redimensionamento; usa cabeçalho cinza compartilhado de 30 DIP sem marca,
Segoe UI de 12 DIP, cantos retos, neutros dinâmicos `Ajuda*`, ação verde e vetor
Drive de 64 DIP. Corpo rolável e ação inferior preservam o conteúdo nos dois temas.

Configurar depois libera somente após cinco segundos medidos por relógio
monotônico a partir da apresentação. X e Alt+F4 respeitam a mesma guarda; durante
a conexão, o fechamento fica bloqueado e a ação permite cancelar após esse prazo.
Falta de credenciais, cancelamento e falha têm mensagens explícitas. Não indicar
conta conectada nem envio concluído com base apenas na abertura do navegador.

O gestor importa o JSON de um cliente OAuth do tipo Aplicativo para computador;
JSON do tipo Web é recusado. O arquivo é copiado para `google-oauth-client.json`
na raiz local de dados, fora do Git. A autorização abre o navegador padrão e usa
PKCE S256, nonce `state`, retorno loopback em `127.0.0.1` com porta aleatória e
prazo de três minutos. O escopo é `https://www.googleapis.com/auth/drive.file`.
Tokens de acesso e renovação ficam protegidos com DPAPI `CurrentUser` em
`%LOCALAPPDATA%\VarthexComanda\google-drive-token.bin` (raiz própria na edição
de demonstração ou quando configurada). Não solicitar senha Google no Varthex.

Renovação com `invalid_grant` remove o token local e exige reconexão. Desconectar
revoga no Google e remove o token após sucesso; falha de revogação é informada.
Arquivos remotos são preservados. A proteção do token pelo Windows não representa
criptografia do conteúdo dos backups, nem exclusividade de acesso aos dados.
Preparação e publicação: [guia Google Drive](../docs/docs/22-configuracao-google-drive.md).

## Backup local e remoto

O serviço cria primeiro o banco consistente, o `.sha256` e o `.fotos.zip` conforme
a política existente de fotos. Se o conjunto de fotos não mudou, o arquivo
companheiro anterior pode ser reutilizado. Depois envia os arquivos disponíveis
pela API REST do Drive, com leitura em fluxo e sessão de upload `resumable`.
Cada execução cria um lote em `Varthex Backups`, inicialmente com sufixo
`(incompleto)`. O sufixo é removido e o lote marcado como completo somente após
todos os arquivos serem aceitos pelo serviço.

Não há retomada automática de uma sessão interrompida: fazer novo backup após
corrigir o problema. Não há exclusão ou retenção remota automática. Upload manual
com progresso tem limite de dez minutos; envio automático sem progresso tem
limite de trinta segundos para limitar a espera no encerramento. O backup de
inicialização executa fora da linha da interface. Falha remota gera aviso e
preserva banco, checksum e fotos locais disponíveis.

OneDrive permanece como cópia em pasta já sincronizada e não comprova upload.
A preferência legada de pasta Google é preservada, mas não recebe novas cópias
quando o serviço OAuth está instalado. O usuário deve conectar a conta para
novos envios Google; o núcleo local continua funcionando sem essa configuração.

Para restaurar um lote Google, baixar manualmente banco, checksum e zip de fotos
companheiros para a mesma pasta e usar Restaurar dados. O aplicativo não lista
nem baixa backups da nuvem. Validar a restauração em pasta de teste antes de
depender do fluxo de recuperação.

## UN, KG e L

Cadastro e edição mostram Unidade (UN), Quilo (KG) e Litro (L) na mesma linha,
com rótulo de preço de venda por medida. Novos produtos começam em UN. Item copia
nome, preço e unidade no lançamento; alterar o produto não reescreve itens antigos.
Listagem, localizador, comanda, histórico e impressão apresentam a medida.

UN aceita inteiro positivo; KG e L aceitam quantidade positiva com até três casas
decimais. Personalizar item mantém 880 × 620 DIP e mínimo de 700 × 440, com campo,
botões menos/mais, total, observação de até 300 caracteres e confirmação verde.
O limite desta janela é 999; o domínio valida até 1.000.000. Quantidade inicial
é 0,100 para KG e 1 para UN/L; os botões ajustam 1 UN ou 0,100 KG/L. Frações são
apresentadas com três casas em pt-BR; UN sem casas decimais. A regra não implementa
controle de estoque nem ficha técnica.

Dinheiro permanece inteiro em centavos. Subtotal usa multiplicação decimal e
arredondamento `AwayFromZero` para centavos, e total soma subtotais. R$ 45,00/KG
× 0,350 KG resulta em R$ 15,75; R$ 1,00/KG × 1,005 KG resulta em R$ 1,01.
Impressão e histórico preservam snapshot da medida; fichas UN são emitidas por
unidade e KG/L geram uma ficha com a quantidade exata.

No SQLite, quantidade e contadores de cozinha/fichas são inteiros em milésimos.
A migração `20261022104827_AddUnidadesVenda` adiciona medida UN aos registros
legados e multiplica quantidades/contadores por 1000, preservando IDs, totais,
vendas e histórico. Usa o backup preventivo anterior à migração já existente.
`Down` recusa conversão para evitar perda de frações: retorno à versão anterior
requer restaurar backup anterior à atualização.

## Fontes de implementação

- `backend/src/VarthexComanda.Domain/UnidadeVenda.cs`: validação, apresentação e subtotal.
- `backend/src/VarthexComanda.Application/Backup/IGoogleDriveBackup.cs`: contratos de conexão e envio.
- `backend/src/VarthexComanda.Desktop/Backup/GoogleDriveConnection.cs`: OAuth e token protegido.
- `backend/src/VarthexComanda.Desktop/Backup/ConectarGoogleDriveView.xaml` e `.xaml.cs`: convite e guarda de tempo.
- `backend/src/VarthexComanda.Infrastructure/Backup/GoogleDriveBackup.cs` e `EfBackupService.cs`: envio e preservação local.
- `backend/src/VarthexComanda.Desktop/Catalogo/ProdutosView.xaml`: seleção de medida e preço.
- `backend/src/VarthexComanda.Desktop/Atendimento/PersonalizarItemViewModel.cs`: quantidade e lançamento.
- `backend/src/VarthexComanda.Infrastructure/Persistence/Migrations/20261022104827_AddUnidadesVenda.cs`: evolução do banco.

## Evidências e limites

As dez capturas atuais em `.impeccable/review/drive-medidas/` cobrem os dois temas:
`drive-convite-{False|True}-{espera|adiar}.png`, `drive-backup-{False|True}.png`,
`medida-cadastro-{False|True}.png` e `medida-personalizar-{False|True}.png`.
Revisão independente retornou `ship` para a extensão visual, essas capturas e
código amostrado. Não constitui aprovação global nem validação de OAuth ou upload
com conta Google real. Documentação conferiu amostra de quatro capturas e fontes.

Testes registrados pela implementação: Domain 4/4, Application 133/133 e Desktop
316/316 aprovados (Desktop excluiu somente `ArquivoCommitadoDeRastreabilidade`,
que aguardava cópia do arquivo gerado). Infrastructure teve 294/294 aprovados
antes do teste extra de falha remota; depois, o grupo `GoogleDriveBackupTests`
passou 3/3 incluindo preservação local. Os dois testes atuais de capturas passaram.
Esses resultados são registros de execuções anteriores à finalização documental,
não uma contagem final da solução após todos os ajustes.

`DriveMedidasTests`, `UnidadesVendaTests`, `GoogleDriveConnectionTests` e
`GoogleDriveBackupTests` cobrem formulário, guarda monotônica, quantidade,
persistência, arredondamento, vetor RFC 7636, token DPAPI, renovação, revogação e
uploads HTTP simulados. Testes Desktop são serializados para evitar concorrência
de `ShowDialog` no Dispatcher compartilhado. Rastreabilidade foi copiada exatamente
do arquivo gerado para `docs/homologacao/rastreabilidade-testes.md`.

Pendentes: configuração real do Google Cloud, consentimento e publicação conforme
políticas Google, autorização no navegador com conta real, envio/consulta dos
companheiros e restauração dessa cópia. Também dependem de ambiente próprio os
ensaios no equipamento do estabelecimento, impressão física e operação sem rede.
Não foram executados testes nem abertos aplicativos durante esta passagem documental.

Conferência final pela implementação: Infrastructure 295/295, Domain 4/4 e
Application 133/133 aprovados. Desktop passou 316/316 sem o teste do arquivo de
rastreabilidade e, após a cópia documental, 6/6 de rastreabilidade incluindo esse
teste adicional (317 casos Desktop distintos verificados). Compilação Desktop
concluída com zero avisos e erros. `docs/scripts/validate.sh` passou; sua etapa
SQL foi ignorada por ausência do executável `sqlite3`, sem substituir os testes
de SQLite real registrados acima. OAuth e upload reais continuam pendentes.
