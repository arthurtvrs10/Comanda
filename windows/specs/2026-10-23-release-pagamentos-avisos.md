# Atualização da pré-release 1.4.0 — 23/10/2026

Pedido: separar commits por assunto, manter a demonstração sem comandas abertas e atualizar README/release. Extensões aprovadas para pagamentos manuais e avisos estão nos specs de 22/10 e 23/10. Rastreabilidade RNF09/RNF14/RNF17, CT21 e aceitação de pagamentos descrita nos requisitos.

## Demonstração

O catálogo já cria 12 produtos sem comandas, consumo ou vendas; comportamento confirmado pelo teste DemonstracaoTests. Nenhuma rotina apaga pedidos automaticamente ao reabrir. Os instaladores recebem somente os arquivos publicados e, na demo, catálogo/fotos/marcador. Scripts recusam bancos e credenciais locais. A demo reutiliza a mesma publicação binária da edição normal.

Na manutenção solicitada da demonstração deste computador, foi feito backup SQLite íntegro antes de cancelar as 11 comandas fictícias abertas. Verificação posterior: zero abertas e integrity_check=ok. Históricos e catálogo mantidos; a edição normal não foi alterada por essa manutenção. O backup está na pasta backups da demonstração, com prefixo antes-limpar-comandas-demo.

## Validação e pacotes

792 testes distintos em Release: 4 Domain, 134 Application, 303 Infrastructure, 351 Desktop. Resultados em backend/artifacts/release-pagamento-resultados. Capturas WPF dos dois temas, com dados fictícios. ZIP normal verificado sem banco, credenciais ou marcador demo. Instaladores gerados com Inno Setup em artifacts/release-1.4.0-pagamentos; tamanhos/checksums em downloads/RELEASE-1.4.0.md.

Pré-release v1.4.0 mantida; instaladores sem assinatura. Windows limpo, impressora física, OAuth real, vários monitores/DPI e leitor de tela completo continuam pendentes. A atualização inclui nova migração de recebimentos, com backup prévio no fluxo de inicialização. Nenhum servidor novo.
