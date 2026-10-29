# Primeira configuração — protótipo Windows do VComanda

Substitui o protótipo web como referência de validação. Usa WPF, recursos reais de App.xaml, TemaInterface e os controles atuais do VComanda. A prévia chama diretamente CupomTermico.Conta, incluindo marca, identificação, faixa preta, itens, totais e textos reais do modelo.

Executável atualizado: backend/artifacts/configuracao-primeiros-passos/VarthexComanda.ConfiguracaoPrototipo.exe.

O programa inicializa App(false), sem iniciar operação, licença, servidor ou banco real. A autorização InternalsVisibleTo permite apenas essa inicialização para o projeto de protótipo. Este executável é uma ferramenta isolada de desenvolvimento. O aplicativo operacional usa a integração descrita nas notas da 1.4.5.

## Fluxo

Fluxo de primeiros passos aplicado a partir da referência enviada: lista vertical com Estabelecimento, Comanda impressa, Atendimento, Produtos, Pagamentos, Equipe e Revisão. A tela apresenta uma barra e a quantidade de etapas concluídas, sem repetir círculo/percentual. Etapas concluídas têm fundo verde e ícone de confirmação, a disponível tem “Fazer agora” e as futuras ficam esmaecidas e bloqueadas. Clicar na etapa disponível abre o formulário na mesma janela. Concluir recolhe o formulário e libera a próxima ação na lista. “Voltar aos passos” retorna sem apagar valores; “Concluído · Revisar” permite conferir uma etapa anterior. Campos obrigatórios mantêm somente um asterisco vermelho. CNPJ/CPF e endereço tornam-se obrigatórios se sua exibição estiver marcada no cupom.

Etapas futuras ficam esmaecidas e desabilitadas. O acesso é liberado em sequência após Continuar com os campos válidos ou adiar explicitamente uma etapa opcional. Voltar é permitido. Editar uma etapa invalida a conclusão dela e das posteriores, preservando os valores. O bloqueio também ocorre no método de navegação, além do botão. O controle de tema foi removido desta tela.

Documento tem seletor CNPJ/CPF à esquerda e campo numérico com máscara automática à direita. A troca preserva o rascunho de cada tipo. Telefone formata fixo/celular com DDD. As máscaras aceitam digitação numérica e normalizam colagem de números formatados; não fazem consulta cadastral nem validam situação fiscal.

Inclui edição dos dados do estabelecimento, prévia 58/80 mm, modalidades de atendimento, quantidades, tabela de mesas com paginação, produtos UN/KG com valores monetários, formas aceitas, responsável e colaboradores simples, revisão e conclusão simulada. Campos de login, senha, comissão, múltiplas funções, administrador e senha mestra permanecem no cadastro completo; não foram recriados parcialmente nem ativados no protótipo.

O rascunho e a exportação ficam exclusivamente na pasta do executável: rascunho-configuracao.json e configuracao-exemplo.json. Não contém senhas. Recomeçar não é executado automaticamente; para nova avaliação pode-se usar uma pasta de saída nova. Não usar dados reais nesta avaliação.

## Verificação

- Compilação sem erros. NU1900 na consulta de vulnerabilidades de dependências por indisponibilidade do nuget.org; não equivale a auditoria de segurança concluída.
- `--verificar`: 29 verificações aprovadas, cobrindo obrigatoriedade, documento, dependências do cupom, limites, pagamentos, moeda, progresso, bloqueio sequencial e máscaras. `--verificar-interface`: digitação sequencial em TextBox WPF de CPF, CNPJ e telefone; lista inicial; abertura do formulário; conclusão liberando somente a próxima etapa; retorno à lista após revisão. Testes isolados sem gravar os dados de exemplo como rascunho.
- `--capturar`: 28 capturas WPF em 1000/1280 de largura e 800 de altura, sete etapas em claro/escuro, em capturas/ junto ao executável. Usa dados sintéticos sem gravá-los como rascunho.
- Duas rodadas visuais em lote: fundo de captura corrigido para usar o Canvas real; campos, tabelas, botões e prévia conferidos na rodada final.

O assistente foi integrado ao aplicativo para teste local. A janela, o estado e as máscaras ficam em `src/VarthexComanda.Desktop/Configuracao/PrimeiraConfiguracao`; este projeto reutiliza esses componentes em modo isolado. Consulte [a integração e seus limites](../../../specs/2026-10-29-primeiros-passos-integrado.md). A validação humana continua pendente. A prévia respeita o gerador existente; nenhuma impressora é acionada.

## Compilar

`dotnet build backend/tools/VarthexComanda.ConfiguracaoPrototipo/VarthexComanda.ConfiguracaoPrototipo.csproj -m:1 -p:UseSharedCompilation=false -p:OutDir=C:/Users/arthu/Desktop/VComanda/backend/artifacts/configuracao-nativa-build/`

Fechar a janela encerra o protótipo. O servidor web 8187 anterior é independente e não faz parte deste executável.
