# Comanda · por Varthex 1.4.0 — pagamentos e avisos

Pré-release Windows x64 atualizada em 23/10/2026. Esta recompilação substitui os instaladores anteriores da 1.4.0: baixe novamente e confira o SHA-256. A 1.2.2 permanece como versão estável anterior.

## Pagamento conforme o fluxo de atendimento

- Dinheiro, Pix, débito e crédito abrem uma janela de lançamento com valor, atalhos, observação e confirmação explícita do recebimento. Em dinheiro, recebido e troco ficam separados.
- Pagamentos parciais e mistos persistem ao registrar e acompanham a venda. Resumo com lançamentos, saldo e progresso; remoção confirmada não executa estorno externo.
- Toggle à esquerda revela a conferência e a divisão igual por pessoas. Selecionar itens calcula o valor da pessoa e impede cobrar novamente linhas pagas. Cada seleção inclui toda a quantidade da linha.
- Finalizar pedido e Finalizar e imprimir ficam disponíveis após quitar o saldo. A venda é gravada antes da impressão; falha na impressão não duplica a venda.
- Alterações de itens e cancelamento ficam bloqueados com recebimentos ativos. A cobrança continua externa: sem integração bancária, fiado, taxas/descontos ou dados de maquininha nesta etapa.

## Drive e renovação

- Convite do Drive uma vez por abertura, após login, quando a conta não estiver conectada. Ao adiar, conecte manualmente em Backup → Google Drive. Bloquear/desbloquear não repete o convite.
- Aviso nos últimos sete dias da licença, com destaque vermelho e botão verde do WhatsApp. Contato também disponível na ativação. Renovação manual com o suporte e nova chave offline; sem checkout, site ou servidor novo.
- Configurações com ícones/toggles, acesso sem senha por padrão, proteção opcional e recuperação offline por senha permanecem disponíveis.

## Edições normal e demonstração

**Normal:** catálogo vazio em uma base nova. **Demonstração:** 12 produtos fictícios com fotos, sem comandas, consumo ou vendas pré-abertas em uma base nova. A demonstração usa pasta e atalhos separados e preserva o uso posterior ao reabrir/atualizar. Nenhum banco de dados ou credencial local é incluído nos pacotes.

Ambas incluem o .NET e exigem ativação offline. A licença e os dados existentes são preservados ao atualizar.

| Arquivo | Bytes | SHA-256 |
| --- | ---: | --- |
| VarthexComanda-1.4.0-Setup-win-x64.exe | 56405446 | `4FBF2E189F717A89FD9B1F5BCAD8A65BBCAA8A9A67A161DB5F5B1E3E5DDFC5D6` |
| VarthexComanda-Demo-1.4.0-Setup-win-x64.exe | 74616122 | `E5FE2488CD04CF00F83F669FDE3224895EA112A539E70794257308C7DFF335F4` |

## Atualização e validação

Feche o aplicativo e mantenha um backup antes de atualizar. Esta compilação adiciona a migração AddRecebimentosComandaVenda; a inicialização preserva uma cópia antes de migrar o banco.

**792 testes aprovados em Release:** 4 Domain, 134 Application, 303 Infrastructure e 351 Desktop. SQLite real nos testes, incluindo persistência, divisão por itens, prevenção de duplicidade e demonstração com catálogo sem comandas/vendas. Validação documental aprovada; capturas WPF com dados fictícios no README.

Instaladores sem assinatura de código. Windows limpo, impressão física, vários monitores/DPI, leitor de tela completo e OAuth/envio com conta Google real permanecem pendentes. Não há emissão fiscal nem atendimento em rede. O backup local continua independente da internet.
