# Primeiros passos integrado — teste local

O assistente WPF agora faz parte do aplicativo. Em uma instalação vazia, abre automaticamente para o gestor. Instalações existentes preservam o fluxo atual e podem abrir em **Configurações → Configurações do sistema → Geral → Primeiros passos**. Exige permissões de configurações e produtos; equipe respeita a permissão de colaboradores.

As sete etapas são Estabelecimento, Comanda impressa, Atendimento, Produtos, Pagamentos, Equipe e Revisão. Há barra de progresso, conclusão em verde, etapa atual disponível e futuras bloqueadas. Campos obrigatórios usam somente asterisco vermelho. CPF/CNPJ e telefone têm máscaras automáticas; a prévia utiliza o gerador de cupom existente. Avisos de renovação e conexão aguardam o fechamento do assistente, evitando modais concorrentes.

O rascunho fica na pasta de dados da instalação. A conclusão grava preferências do estabelecimento e cupom, quantidade de comandas, produtos iniciais UN/KG e formas aceitas em transação SQLite. Repetir a conclusão não duplica produtos idênticos. Conflitos e comandas abertas fora da faixa impedem a gravação. As preferências atuais de segurança, aparência e cozinha são preservadas. O cadastro completo de colaboradores é aberto pelo assistente e salva suas próprias alterações quando confirmado; não cria senhas automaticamente.

O atendimento usa as formas de pagamento selecionadas, mantendo pagamentos parciais e mistos. A etapa Atendimento configura a quantidade de comandas; mesas continuam vinculadas durante o atendimento, conforme o modelo atual do sistema.

## Execução para validação

- Executável: `backend/artifacts/primeiros-passos-integrado/VarthexComanda.exe`.
- Base separada: `backend/artifacts/dados-primeiros-passos-teste`, selecionada por `VARTHEX_COMANDA_DADOS` no processo de teste.
- Ativação normal mantida. A versão instalada e sua base não foram substituídas.
- Nenhum commit, push ou release. Complementos continuam fora da publicação.

## Verificações

- Compilação do aplicativo sem erros. Aviso NU1900: consulta de vulnerabilidades indisponível; não representa auditoria concluída.
- 30 testes direcionados aprovados: primeira configuração, pagamentos mistos, encerramento e configuração.
- Suíte desktop: 438 aprovados de 439 em uma execução; o teste restante de ordenação dos produtos passou na execução isolada. Não se declara execução integral sem falhas.
- Abertura automática observada no aplicativo real, em base isolada; formulário Estabelecimento aberto e campos conferidos. Validação humana das sete etapas permanece como objetivo desta versão de teste.

O projeto `backend/tools/VarthexComanda.ConfiguracaoPrototipo` reutiliza agora a janela do aplicativo em modo isolado para máscaras, fluxo e capturas. Não é necessário para usar o assistente integrado.

## Sincronização com Geral — 30/10/2026

Os campos compartilhados do estabelecimento usam os dados de Configurações do sistema → Geral: nome, razão social, CPF/CNPJ e tipo, inscrição estadual, país do telefone, telefone e endereço completo.

Ao continuar a etapa Estabelecimento, os dados válidos são salvos em Geral, mesmo que as demais etapas ainda estejam em andamento. Ao reabrir o assistente, alterações salvas em Geral atualizam os campos correspondentes do rascunho, inclusive campos apagados. Edições pendentes nos demais campos e etapas são preservadas. Antes da conclusão, os dados compartilhados são conferidos novamente para evitar restaurar valores antigos.

Rascunhos antigos sem referência de sincronização passam a usar o cadastro existente em Geral. Segurança, aparência, cozinha e imagem do estabelecimento são preservadas. Os demais ajustes do assistente continuam sendo aplicados na conclusão.

Validação: 14 testes direcionados aprovados, incluindo quatro cenários novos com banco real para salvamento parcial do assistente, atualização em Geral, rascunho legado e dados inválidos.
