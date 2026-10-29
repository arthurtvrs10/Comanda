# Primeira configuração — protótipo navegável

Abrir: http://localhost:8187

Protótipo local para validar fluxo, termos, campos e identidade visual. Não integra o aplicativo, não altera configurações reais, não imprime nem cria contas. Dados de avaliação ficam neste navegador; senhas não são salvas nem exportadas. Documento recebe somente uma checagem de formato (quantidade de dígitos), sem consulta ou validação cadastral/fiscal.

## Iniciar e parar

Na raiz do repositório: `node prototypes/primeira-configuracao/server.mjs`.

Servidor restrito a 127.0.0.1:8187. Para parar uma execução iniciada no terminal, usar Ctrl+C. A execução aberta nesta sessão teve PID registrado em server.pid; conferir o processo antes de interromper para não encerrar outro serviço.

## Etapas

1. Estabelecimento: nome, CNPJ/CPF, razão social, inscrição, telefone e endereço.
2. Comanda impressa: documento/endereço, agrupamento, observações, mensagens e prévia 58/80 mm com faixa preta no número.
3. Atendimento: balcão e/ou mesas, quantidades e lista de mesas numeradas com paginação.
4. Produtos: itens de exemplo adicionados por UN ou KG; valores em centavos e bloqueio de duplicação.
5. Pagamentos: Dinheiro, Pix, Débito e Crédito; não configura gateways.
6. Equipe: responsável, escolha de acesso simulada e colaboradores sem login. Recursos completos de múltiplas funções, comissão, permissões e senha mestra continuam no cadastro existente.
7. Revisão: edição por seção, falha simulada e conclusão com exportação JSON sem senhas.

Campos obrigatórios têm somente o asterisco vermelho, sem legenda. Dados de documento/endereço tornam-se obrigatórios quando escolhidos para impressão.

Correções do usuário: etapas 1–7 no topo; apresentação semelhante a checkout, com barra laranja, percentual, conexões entre etapas e identificação da tela atual. Progresso conta etapas validadas ou opcionais explicitamente adiadas; navegar por uma tela não soma progresso. Só chega a 100% depois de concluir a simulação. Retornar para revisar não significa apagar o progresso já feito.

## Verificação realizada

- 9 testes automatizados: obrigatoriedade, dados condicionais de cupom, limites de quantidades, dinheiro em centavos, duplicação, seleção de pagamento, restauração sem credenciais, revalidação e progresso sem contagem duplicada.
- Navegador: nome vazio bloqueou avanço e recebeu foco; prévia exibiu documento/endereço/mensagem; item KG de R$49,90 adicionado; geração de mesas; rascunho recuperado após recarga; revisão manteve dados; falha simulada preservou entradas e conclusão posterior funcionou.
- Aparência conferida no viewport normal e em 820×700, claro e escuro. Janela estreita permite rolagem para não cortar o formulário. A barra permanece na área superior, com navegação horizontal adaptável.
- Capturas em capturas/. Algumas capturas anteriores ao último ajuste visual podem mostrar a etapa anterior da apresentação; checkout-atual.jpg registra a barra solicitada.

## Limites e validação pendente

Validação humana ainda não realizada. Executar com usuários: identificar o estabelecimento, exibir CNPJ no cupom, selecionar mesas/balcão, adicionar produto, selecionar pagamentos, adiar equipe, revisar, simular falha e concluir. Registrar tempo, pedidos de ajuda e erros. Rascunhos de linhas ainda não adicionadas aos cadastros não são persistidos; para avaliar a retomada, adicionar o item antes de sair. Não substituir o aplicativo até implementar persistência de produção e testes de integração.

Protótipo isolado; complementos continuam fora de qualquer publicação. Nenhum commit, push ou release foi feito.
