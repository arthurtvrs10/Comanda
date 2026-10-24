# Comanda · por Varthex 1.4.2 — Movimento das janelas internas

Pré-release Windows x64 de **24/10/2026**, nas edições normal e demonstração. Atualiza a 1.4.1, preservando as releases anteriores. A 1.2.2 permanece como versão estável anterior.

## Mudanças

- Mesas / comandas, Balcão, Produtos, históricos de pedidos e vendas, Cozinha, Configurações, Backup e Logs compartilham a correção do arraste rápido.
- O mouse é acompanhado em uma referência fixa. Somente a posição mais recente é aplicada a cada quadro, sem acumular posições intermediárias ou refazer a medição do conteúdo.
- A atualização visual dos cronômetros das comandas pausa durante o gesto e retoma no próximo segundo, calculando o tempo real. Soltar ou cancelar aplica o destino pendente; fechar, maximizar e descarregar liberam o estado do arraste.
- README e [galeria de capturas WPF](../docs/design/release-1.4.2/README.md) atualizados com dados fictícios.

Mantém servidor automático, atendimento móvel na mesma rede, catálogo, até 500 comandas, KDS, históricos, recebimentos manuais, backup e ativação offline. Não altera banco de dados nem regras de atendimento.

## Instalação

**Normal:** catálogo vazio em uma base nova. **Demonstração:** 12 produtos fictícios com fotos, sem pedidos ou vendas pré-abertos; instalação, atalho e base separados.

Feche o aplicativo e faça backup antes de atualizar. Instalação por usuário, sem administrador, com .NET incluído; preserva dados e licença existentes. Ativação offline obrigatória. Os instaladores não têm assinatura de código e não alteram o firewall. Nenhum banco, credencial Google ou chave privada acompanha os pacotes.

## Verificação e limites

**869 testes aprovados em Release:** 4 Domain, 146 Application, 312 Infrastructure, 14 Servidor e 393 Desktop. Entre eles, 17 testes de movimento/janelas, com rajadas de 2.002 movimentos nas nove telas, sem novas medições da área. Testes sintéticos não confirmam a fluidez no mouse físico, que permanece pendente de validação no uso real.

Instaladores gerados com Inno Setup 7.1.0 e verificados localmente com Microsoft Defender ativo, definições 1.461.184.0, sem detecção. Essa análise não garante o resultado de outros computadores. Instalação em Windows limpo, impressão física, rede móvel física, DPI/leitor de tela e OAuth com conta Google real continuam pendentes.

## Arquivos

Os instaladores e seus arquivos SHA-256 estão nos [anexos da release 1.4.2](https://github.com/arthurtvrs10/Comanda/releases/tag/v1.4.2). As capturas também acompanham a publicação em um ZIP separado.

| Edição | SHA-256 |
| --- | --- |
| [Normal](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.2/VarthexComanda-1.4.2-Setup-win-x64.exe) | `28AA9E8D70166DB98D0C131DB441B95444B1BBE263E6EB17A54C0322063F5CAA` |
| [Demonstração](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.2/VarthexComanda-Demo-1.4.2-Setup-win-x64.exe) | `B9E40C5E6C1CE22813E6BF2725747B8EC1C5B152A35C84EDB94571B26DF1C630` |
