# Comanda 1.4.8 — acesso pelo celular corrigido

Pré-release Windows x64 de 30/10/2026. Runtime .NET incluso. Ativação offline obrigatória.

## Alterações

- Corrigida a seleção automática do endereço para acesso pelo celular. Prioriza interfaces reais com gateway; entre equivalentes, Wi-Fi tem prioridade. Ignora adaptadores conhecidos de Hyper-V, WSL, VirtualBox, VMware, Docker e VPN.
- QR Code, campo Link de acesso e Copiar link de acesso usam o mesmo endereço/chave. O link completo está visível na tela do servidor.
- Sem rede local adequada, apenas 127.0.0.1 é oferecido, com orientação de que funciona somente no PC. Nenhum IP ou chave do desenvolvedor foi gravado no programa.
- Preservados OneDrive, Restaurante/Lanchonete, impressão e demais recursos publicados na 1.4.7. Sem migrações novas.

## Como conectar

Em Configurações → Servidor / acesso pelo celular, inicie o servidor, selecione o IP e copie o link completo ou leia o QR. Celular e computador precisam estar na mesma rede; o PC pode usar cabo. Ao reiniciar o servidor, leia o QR novamente. Rede de convidados, isolamento entre dispositivos e firewall podem impedir acesso mesmo com o IP correto.

O programa não desativa o Firewall nem altera regras automaticamente. Permita o executável instalado na porta TCP do servidor e no perfil adequado à rede do estabelecimento. Não publique o link/chave na internet.

## Validação e limites

23 testes do servidor aprovados, incluindo seleção de LAN/gateway, exclusão de redes virtuais, ausência de rede, deduplicação e acesso HTTP. 13 testes Desktop direcionados aprovados, cobrindo a janela do servidor, QR/link visível, início/parada, licença e metadados da versão. Na suíte Desktop completa, 461 passaram; o teste de versão detectou metadados ainda em 1.4.7, corrigidos e aprovados na execução direcionada. Não se afirma uma nova execução integral da suíte da 1.4.7.

Capturas WPF reais dos temas claro e escuro verificadas. Neste PC, o servidor anterior respondeu HTTP 200 em 192.168.15.115:3001 e foi identificado o IP virtual incorreto 172.22.64.1; os IPs são apenas evidência deste ambiente, não configuração fixa do programa. Celular físico e isolamento do roteador não foram homologados nesta execução.

Os três instaladores finais foram verificados pelo Microsoft Defender (definições 1.463.36.0), sem detecção nesta análise.

Base isolada da tag 1.4.7, sem chamadas/QR Codes das mesas, KDS recente, complementos/estoque, tipo de pedido, recuperação por e-mail e transferência de licença por código curto. O QR da equipe já existente permanece e foi corrigido. O emissor Android não foi alterado nesta publicação.

Instaladores sem assinatura de código. Homologação em Windows limpo, celular físico e impressoras de 58/80 mm permanece pendente; a publicação continua como pré-release. Faça backup e feche o aplicativo antes de atualizar.

## Downloads

- [VarthexComanda-1.4.8-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.8/VarthexComanda-1.4.8-Setup-win-x64.exe) — SHA-256 `D9C84BD03F822B311D50FD2E76D514EEE39C3BC56984FDE686267FB3F27B764F`
- [VarthexComanda-Demo-1.4.8-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.8/VarthexComanda-Demo-1.4.8-Setup-win-x64.exe) — SHA-256 `848F5A28F52E66989D15C0DE290E1F602D3D345BD4DDB1BAD9442015E193FC13`
- [VarthexComanda-Demo-Restaurante-1.4.8-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.8/VarthexComanda-Demo-Restaurante-1.4.8-Setup-win-x64.exe) — SHA-256 `5809CE829C25D1B217674F0CB8D8924F0DBE11DC819DD3C6F9F50E3938E81FB3`
