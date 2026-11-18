# Primeira configuração — protótipo para validação

Modo Operate. Público: proprietários e atendentes com pouca familiaridade com tecnologia. Artefato independente em HTML/CSS/JS, sem acesso ao banco, licença, impressão ou segurança reais. Mantém o aplicativo WPF intacto. Não é implementação de produção.

## Direction contract

THESIS: preparar o primeiro atendimento com uma tarefa por seção e uma revisão explícita. Aproveitar a navegação guiada do URY com os termos e recursos da Comanda.

OWN-WORLD: identidade do site, azul profundo #17252C no cabeçalho, laranja #FF5934 na ação principal, branco e cinza claro no trabalho. Segoe UI, campos nativos, ícones SVG discretos e alinhamento consistente.

STORY: cadastrar estabelecimento, conferir comanda impressa, escolher atendimento, preparar produtos, pagamentos e equipe, revisar e simular conclusão.

FIRST VIEWPORT: cabeçalho horizontal com marca real e identificação do protótipo; sete etapas numeradas em barra horizontal no topo, conforme correção do usuário; formulário central; orientação contextual à direita. No cupom, a orientação é substituída por uma prévia em preto e branco com a faixa COMANDA 01. Rodapé com Voltar, adiar quando permitido e Continuar.

FORM: assistente sequencial, linguagem visual fixada pelo usuário ao site Comanda, sem seleção de outra direção. Interação distintiva: configurações do cupom atualizam a prévia em tempo real. Movimento mínimo, apenas feedback de estado; respeitar movimento reduzido.

FINISH: conferir capturas em lote, corrigir em no máximo uma rodada adicional, testar navegação/validação/rascunho sem credenciais, registrar limites e abrir para validação humana. Sem commit, push ou release.

## Correções do usuário

- Apenas o asterisco vermelho ao lado dos campos obrigatórios, sem legenda “Campos obrigatórios”.
- Incluir CNPJ e configurações que aparecem na comanda. Reaproveitar campos de estabelecimento e cupom identificados em JanelaConfiguracoesSistema.cs. Prévia ilustrativa, sem impressão física.
- Etapas numeradas 1–7 no topo no estilo checkout: barra de conclusão e percentual, indicação da etapa atual e conectores. O percentual depende de conclusão/adiamento explícito, não de simples visita. 100% apenas ao finalizar.

## Limites

Rascunho no armazenamento local deste navegador, sem senhas. A conclusão gera apenas uma simulação e pode exportar JSON. Todos os produtos, usuários e mesas inseridos são dados de avaliação; não altera cadastros reais. Regras financeiras e permissões operacionais existentes não são substituídas.
