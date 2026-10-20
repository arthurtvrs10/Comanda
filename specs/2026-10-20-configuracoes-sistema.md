# Configurações do sistema — 20/10/2026

A categoria Configurações reúne Configurações do sistema, Alterar senha e Backup e restauração. O modal é redimensionável, permite maximizar e confirma o descarte de alterações.

Seções: Geral (assinatura real e estabelecimento), Ajustes de cupom (comprovante não fiscal e prévia), Imagem de fundo (cor, imagem e posicionamento), Origem do Cliente (cadastro e seleção na comanda), Locais/cozinhas (cadastro, vínculo com produtos e filtro da cozinha), Código de Barras por Kg (configuração e conferência EAN-13) e Sobre (diagnóstico real copiável/exportável, sem senhas ou tokens).

Preferências, nome e limite de comandas são gravados em uma transação SQLite. Migração aditiva para origem do cliente; preservação do backup preventivo existente. Regras relacionadas: persistência local, valores em centavos, preservação histórica de itens e restrições de comandas abertas; nenhuma integração de pagamento adicionada.

Opções avançadas: backup, restauração, licença e acesso da equipe existentes. Não implementa resets destrutivos nem ações exclusivas do Consumer. Impressora por cozinha é informação de cadastro; roteamento automático de impressão permanece pendente. Etiquetas por quilo podem ser conferidas, mas não lançadas automaticamente: o domínio atual usa quantidades inteiras.

Validação: 638 testes aprovados (110 aplicação, 256 desktop, 268 infraestrutura, 4 domínio). Prévia das sete seções em claro/escuro e larguras 800/1180 em artifacts/configuracoes-sistema/previas. Testes cobrem persistência, validações, EAN-13, origem da comanda e navegação/salvamento do modal.

Ajuste visual de etiquetas: duas colunas conforme referência, diagrama de 13 caracteres, prefixo por lista, opções 4/5/6, preferência de ativação persistida e exemplo explicativo. Em janela estreita, as colunas são empilhadas. Conferência EAN-13 preservada. Teste visual de seções aprovado nos dois temas.

Locais/cozinhas: lista com colunas de nome e saída configurada, ações Produtos vinculados/Editar/Novo e editor modal com Somente monitor ou Monitor e impressora. Seleção usa diálogo nativo do Windows; preferências persistidas após Salvar alterações. A configuração não inicia envio automático de pedidos.

Origens: lista com separadores e ações Novo/Editar no topo; editor modal com validação de duplicidade. Fundo: oito cores predefinidas, cor personalizada hexadecimal, prévia reativa com Centralizar/Preencher/Zoom/Repetir, seleção de arquivo e restauração. Mantém dados existentes e aplica após Salvar alterações. Validação do modal aprovada em claro/escuro, larguras 800/1180.

Densidade compacta conforme referência: campos de 28 px, fonte 12 px, título de seção 14 px, navegação de 180 px e botões de 28 px. Estilos restritos ao modal e seus editores. Dados da assinatura em três colunas e quantidade de comandas com largura de 140 px. Validação visual das sete seções em dois temas e duas larguras aprovada.
