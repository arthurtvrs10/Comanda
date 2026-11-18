# Comanda · por Varthex 1.4.1 — Servidor automático e atendimento

Pré-release Windows x64 de **24/10/2026**, nas edições normal e demonstração. Atualiza a 1.4.0; não substitui os arquivos históricos dessa versão. A 1.2.2 permanece como versão estável anterior.

> **Instaladores recompilados em 24/10/2026:** os pacotes anteriores foram bloqueados pelo Microsoft Defender como `Program:Win32/Contebrew.A!ml`. A recompilação com Inno Setup 7.1.0 oficial passou na verificação local do Defender, com proteção ativa e definições 1.461.184.0. Os arquivos do aplicativo permaneceram os mesmos. Baixe novamente e confira os novos SHA-256 abaixo. Não houve determinação da Microsoft de falso positivo; a análise local não garante o resultado em outros computadores.

## Novidades

- Servidor local inicia automaticamente com o desktop. Preferência e porta persistidas; painel com nome do computador, IPs, escuta, QR/link, teste local e acessos autenticados da sessão. Falha de inicialização não bloqueia o atendimento; recuperação mantém o servidor parado.
- Configurações → Logs e fila de eventos mostra registros locais e operações reais do servidor e desktop, com busca, filtros e detalhes. Fila limitada a 500 eventos da sessão e estados finais registrados no log mascarado. Não reenvia pedidos nem registra chaves, corpos HTTP ou observações dos clientes.
- Quantidade configurável de **1 a 500 comandas**. São posições na faixa: início 100 e quantidade 500 permite 100–599. Pedidos antigos fora da faixa e históricos são preservados.
- Catálogo com fotos por padrão: clicar no cartão ou em + adiciona uma unidade de UN; KG/L abrem Personalizar item. … sempre personaliza. As ações ficam dentro do cartão, logo abaixo do preço, com controles independentes para teclado e toque.
- Cozinha (KDS): descrição dos itens prontos em verde e riscada, mantendo observações legíveis e entrega separada da cobrança.

Mantém Balcão, atendimento móvel na mesma rede, janelas internas, recebimentos manuais parciais/mistos, divisão por pessoas/itens, backup, históricos, modo escuro e ativação offline.

## Duas edições

**Normal:** catálogo vazio em uma instalação nova, para cadastrar os produtos reais. **Demonstração:** 12 produtos fictícios com fotos, sem comandas ou vendas pré-abertas em uma base nova. A demonstração usa instalação, atalho e base separados; o catálogo de exemplo não substitui alterações existentes.

Ambas incluem o .NET e exigem ativação offline. Atualizar preserva dados e licença. Nenhum banco, credencial Google ou chave privada do emissor acompanha os pacotes.

## Instalação e acesso pelo celular

Feche o aplicativo e faça um backup antes de atualizar. Instalação por usuário em Windows x64, sem administrador. Os instaladores não têm assinatura de código.

O servidor inicia na porta 3001 por padrão. Em **Configurações → Servidor / acesso pelo celular**, confira o estado, selecione o IP da rede e leia o QR Code no celular conectado à mesma rede. Se o início automático estiver desativado, use Iniciar servidor. Reiniciar/parar revoga o link anterior; mantenha o desktop aberto. Cobrança e fechamento continuam no computador; não há KDS web.

O instalador não altera o firewall. Se houver bloqueio, permita o executável instalado no perfil da rede, somente entrada TCP na porta escolhida e sub-rede local. Não desative o firewall nem encaminhe portas no roteador. O acesso local usa HTTP e exige rede confiável.

## Verificação e limites

**862 testes aprovados em Release:** 4 Domain, 146 Application, 312 Infrastructure, 14 Servidor e 386 Desktop. Capturas WPF atuais com fixtures sintéticas no README. Dois avisos xUnit2031 preexistentes no projeto de testes desktop.

Publicação autocontida win-x64 e pacotes Inno Setup das duas edições. A validação usa SQLite real e HTTP local nos testes; isso não comprova operação de um celular físico ou instalação em Windows limpo. Permanecem pendentes ensaios de Windows limpo, impressão física, rede móvel física, DPI ampliado/leitor de tela e OAuth/envio com uma conta Google real.

O Google Drive exige cliente OAuth próprio configurado separadamente; o OneDrive usa pasta sincronizada. O backup local funciona sem internet. Recebimentos são registros manuais da cobrança externa; sem integração bancária, emissão fiscal ou estoque completo.

## Arquivos e SHA-256

| Instalador | Bytes | SHA-256 |
| --- | ---: | --- |
| [VarthexComanda-1.4.1-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.1/VarthexComanda-1.4.1-Setup-win-x64.exe) | 64569680 | `31AF8D092B81E4C1BBA8E9D1D67396A113B97A00257CC1A02DC84101FFDCC7E5` |
| [VarthexComanda-Demo-1.4.1-Setup-win-x64.exe](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.1/VarthexComanda-Demo-1.4.1-Setup-win-x64.exe) | 82780342 | `AE0EF5E8198A05727BC82B50C14164D3CE6C1DC16D39448176012CAAB48E9D46` |

Os arquivos `.sha256` também acompanham os instaladores nos anexos da release.
