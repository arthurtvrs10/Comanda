# Comandos de configuração no Ribbon — 20/10/2026

Na categoria Configurações, a ordem é Configurações do sistema, Numeração de pedidos, Impressoras, Colaboradores, Alterar senha e Backup e restauração. Comandos administrativos preservam a exigência de acesso Gestor; um colaborador pode alterar sua própria senha.

## Numeração

Modal compacto com ativação, início, senha de atendimento, último código e reinício confirmado. Aplica-se ao módulo real Mesas/comandas; não expõe delivery/fiscal inexistentes. Código independente do número da comanda, atribuído ao criar na mesma transação do contador. Histórico e códigos já usados preservados. Migração aditiva de codigo_pedido, com backup preventivo existente na inicialização. Código visível no pedido e no comprovante não fiscal; senha no final quando configurada.

## Impressoras

Modal com caixa e cozinhas na lateral. Saídas desativada, instalada, compartilhada e mesma do caixa para cozinhas. Consulta filas reais do Windows; Salvar persiste e reconcilia Locais/cozinhas. Testar envia uma página de teste apenas sob clique. Impressão manual do comprovante usa a saída do caixa salva; configuração ausente mantém o diálogo de seleção anterior. Automação de impressão da cozinha e emissão fiscal continuam pendentes. Impressão física precisa ser validada com a impressora do estabelecimento.

## Colaboradores e senha

Pesquisa, cadastro e edição de nome, CPF opcional validado, função, telefone, login, perfil e ativo. Perfis existentes: Gestor, Garçom, Cozinha. Contas individuais entram no login e identificam o usuário no rodapé. Contas inativas não entram; gestor individual não altera a credencial principal; própria conta ativa não pode ser rebaixada/desativada pela própria sessão. Não há comissão ou folha de pagamento.

Senha atual, nova e confirmação em modal compacto. Preservada a política de no mínimo 8 caracteres e compatibilidade com senhas longas. PBKDF2 com sal individual; tentativas de login de colaboradores possuem espera após cinco falhas. Contas ficam em AppPaths.Root/colaboradores.json, junto das credenciais locais e fora do backup SQLite atual; copiar esse arquivo ao transferir a instalação. Nenhum dado dos prints foi importado.

## Validação

Suíte integrada: 659 testes aprovados (120 aplicação, 271 infraestrutura, 264 desktop, 4 domínio). Inclui concorrência e rollback de numeração, persistência de impressoras, autenticação por perfil/conta ativa, senha própria, testes de modal e Ribbon nos dois temas. Não foram enviadas impressões físicas. Prévia em artifacts/configuracao-ribbon/previas e pastas de testes de numeração, impressoras e colaboradores.

Minha assinatura: grupo Versão premium com ícone de chave. Consulta em modal compacto com cliente, identificação, licença por prazo/vitalícia, validade e recursos reais do Varthex. Copiar informações exporta somente o texto exibido; Trocar assinatura fecha a consulta e abre o fluxo existente de ativação offline, preservando suas regras. Backup e restauração foi incorporado ao grupo Configurações. Três testes de modal/licença/Ribbon aprovados.
