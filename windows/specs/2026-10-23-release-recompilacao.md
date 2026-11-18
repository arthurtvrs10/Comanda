# Recompilação 1.4.0 — 23/10/2026

Fontes da aplicação: commit 421c138 (inclui as implementações anteriores). Instaladores gerados em artifacts/release-20261023-final e artifacts/release-20261023-final-demo pelos scripts existentes, usando a mesma publicação normal limpa para as duas edições.

# Comanda · por Varthex 1.4.0 — Balcão, Cozinha e celular

Pré-release recompilada em 23/10/2026. Substitui os dois instaladores anteriores da 1.4.0: baixe novamente e confira os novos SHA-256. A versão estável anterior 1.2.2 permanece disponível.

## Novidades desta recompilação

- Principal reúne Mesas/comandas, Balcão, Produtos, históricos e Painel de senhas.
- Balcão permite pedidos sem mesa e reutiliza os fluxos de consumo e fechamento.
- Cozinha (KDS) desktop com Pendente, Em preparo, Pronto e Entregue, observações destacadas, filtros, ações por item/conjunto e aviso sonoro opcional.
- Atendimento pelo celular na mesma rede: abrir e consultar comandas, nome do atendente, catálogo com fotos, busca, categorias, quantidades UN/KG/L e observações. Pagamentos e fechamento permanecem no computador; sem KDS web.
- Movimento das janelas internas aprimorado. Mantidos pagamentos manuais parciais/mistos, divisão por itens/pessoas, renovação por WhatsApp e backup.
- Landing page em site/, separada do aplicativo e do servidor de atendimento; não acompanha o instalador desktop.

## Duas edições Windows x64

**Normal:** catálogo vazio em instalação nova; cadastre os produtos reais. **Demonstração:** 12 produtos fictícios com fotos, sem comandas ou vendas pré-abertas em base nova. Ambas incluem runtime .NET, exigem ativação offline e preservam dados existentes. A demonstração usa instalação/base separadas e atalho Comanda Demonstração. O catálogo fictício não substitui uma base já utilizada.

Feche o aplicativo antes de atualizar e faça backup. Migrações preservam os dados; instalação por usuário sem administrador. Instaladores sem assinatura de código.

## Iniciar o servidor e conectar o celular

Configurações → Servidor / acesso pelo celular → Iniciar servidor, porta padrão 3001. Selecione o IP da rede e leia o QR Code no celular conectado à mesma rede. O link contém a chave da sessão; após parar/reiniciar, leia o novo QR Code. Mantenha o desktop aberto.

O instalador não altera o firewall. Se necessário, permita o executável instalado no perfil em uso, restringindo entrada TCP à porta configurada e à sub-rede local. Bloqueios explícitos prevalecem sobre permissões. Não desative o firewall nem encaminhe portas no roteador. Use rede confiável: o acesso local é HTTP.

## Validação e limites

835 testes aprovados em Release: 4 Domain, 135 Application, 310 Infrastructure, 374 Desktop e 12 Servidor. Dois avisos xUnit2031 preexistentes no projeto de testes desktop. Publicação autocontida win-x64 e compilação Inno Setup das duas edições. Servidor respondeu HTTP 200 pelo endereço da rede neste computador; isso não comprova conexão de um celular físico.

Permanecem pendentes instalação em Windows limpo, homologação em celular físico e outras redes/firewalls, impressão física e OAuth com conta Google real. O Google Drive exige cliente OAuth próprio; credenciais, bancos e licença do emissor não acompanham os instaladores. Recebimentos são registros manuais, sem integração bancária, fiscal ou estoque completo.
