# Backup padrão pelo OneDrive — implementação local

O aviso inicial passa a oferecer OneDrive quando não há destino de nuvem configurado. Contas Google e pastas já configuradas continuam preservadas, sem migração automática. Na página Backup, OneDrive aparece primeiro e Google Drive permanece como alternativa.

A detecção consulta as pastas pessoais/comerciais informadas pelo Windows e a pasta OneDrive do perfil, aceita somente diretórios existentes e elimina caminhos duplicados. A seleção detectada é uma sugestão: nenhuma preferência é gravada antes de clicar em Usar OneDrive. É possível escolher outra pasta sincronizada ou configurar depois, sem contagem regressiva.

O VComanda continua gerando banco, checksum e fotos localmente e usa a cópia já existente para Varthex\Backups dentro da pasta escolhida. O aplicativo OneDrive do Windows faz o envio. A interface não declara sucesso na nuvem a partir da existência de uma cópia local. Não é necessário cadastrar cliente OAuth Microsoft no VComanda para esse fluxo por pasta.

Validação: 19 testes desktop e 42 de infraestrutura de backup aprovados, mais repetição dos quatro novos cenários após ajuste de ajuda/asterisco. Abrange detecção e deduplicação, confirmação e preservação de destinos, adiamento sem gravação, temas, backup com fotos, integridade e falhas preservando a cópia local. A sincronização real de uma conta OneDrive permanece dependente de homologação no aplicativo da Microsoft.

Esta mudança acompanha a release 1.4.7. Complementos e estoque continuam fora do escopo.

Referência: https://support.microsoft.com/en-us/onedrive/what-do-the-onedrive-icons-mean
