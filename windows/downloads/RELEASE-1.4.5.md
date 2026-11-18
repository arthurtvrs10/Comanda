# Comanda · por Varthex 1.4.5 — KDS Kanban e primeiros passos

Pré-release Windows x64 de 29/10/2026, nas edições normal e demonstração.

## Mudanças

KDS com cinco raias (Chamadas, Pendente, Preparando, Pronto e Entregue), contadores, títulos editáveis, abertura maximizada e largura adaptável. Novos itens entram em cartão Pendente separado dos itens da mesma comanda em outras etapas. Movimentação por arraste ou seta, com validação de concorrência e estado; prévia completa, cursor e animação. Ícones para cancelar o pedido inteiro com confirmação e imprimir com prévia.

Cabeçalho compacto, indicador de última atualização no topo, rolagem independente, Chamadas estreita e limpeza visual de entregues com confirmação. A limpeza preserva status, itens, comandas e vendas. Sons distintos para novo item e pedido pronto; sem toque em Preparando ou Entregue. Volume abaixo de 80% ou mudo gera aviso dispensável, sem alterar o volume do computador.

Assistente inicial de sete etapas integrado ao programa: estabelecimento, cupom, comandas, produtos, pagamentos, equipe e revisão. Máscaras automáticas, rascunho e etapas sequenciais. Instalações existentes podem abrir em Configurações → Configurações do sistema → Geral → Primeiros passos. Segurança e preferências existentes são preservadas. Colaboradores usam seu cadastro completo e salvam quando confirmados. Formas aceitas selecionadas são respeitadas no fechamento, mantendo pagamentos parciais e mistos.

## Escopo e limites

Complementos, estoque, códigos automáticos novos, inclusão direta de produtos e suporte a complementos no navegador continuam em desenvolvimento e não acompanham esta release. As quatro migrações desse módulo foram excluídas; esta versão não adiciona migrações. Chamadas ainda não recebe solicitações diretamente das mesas. Protótipos de pagamento e configuração são fontes/ferramentas de desenvolvimento, não telas alternativas publicadas no menu.

Instalação por usuário em Windows x64, runtime .NET incluso, ativação offline obrigatória. Edições normal e demonstração preservam bases separadas. Feche o aplicativo e faça backup antes de atualizar. Nenhum dado local ou segredo acompanha os pacotes. Instaladores sem assinatura de código. A homologação de mouse físico, som no dispositivo, impressora física, Windows limpo e OAuth com conta Google real permanece pendente; por isso a publicação é pré-release.

## Validação

**914 testes .NET Release aprovados:** 437 Desktop, 313 Infrastructure, 146 Application, 14 Servidor e 4 Domain. Mais 7 testes JavaScript do atendimento e 9 do protótipo de configuração aprovados. Execução em fonte isolada, sem complementos, estoque e suas migrações. Os testes de UI não comprovam fluidez com mouse físico ou audibilidade no equipamento.

Instaladores normal e demonstração gerados com Inno Setup 7.1.0 e verificados localmente pelo Microsoft Defender (definições 1.463.28.0), sem detecção. A análise local não garante o resultado de outros equipamentos.

| Edição | SHA-256 |
| --- | --- |
| [Normal](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.5/VarthexComanda-1.4.5-Setup-win-x64.exe) | 3D3ACFD728C10592E43442201F6358FF1E675616AF0C27A96B79412B4E8FEB84 |
| [Demonstração](https://github.com/arthurtvrs10/Comanda/releases/download/v1.4.5/VarthexComanda-Demo-1.4.5-Setup-win-x64.exe) | 820C91ED5036FD7408D87FE2900210A1AEDCD0AD8418E52A1A2B8C7FAA0619CD |
