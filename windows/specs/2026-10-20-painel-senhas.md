# Painel eletrônico de senhas — 20/10/2026

Apps → Painel de senhas abre um chamador independente da área de trabalho. Gestor e Garçom autenticados podem chamar; Cozinha permanece consulta. Bloqueio fecha o chamador; a tela pública continua exibindo a chamada. Fechar o aplicativo encerra as duas janelas.

## Fluxo validado

1. Selecionar monitor e preferências; configurações são persistidas em SQLite na chave apps.painel-senhas.v1.
2. Digitar 1 a 9 números ou usar o teclado virtual. Enter/Chamar abre ou reutiliza a tela pública no monitor configurado, mantendo zeros à esquerda.
3. Nova senha vira destaque central; a anterior passa à direita. Mantém as cinco últimas, em ordem da mais recente. Rechamar a atual repete o aviso, renova o prazo e não duplica o histórico. Chamar uma senha do histórico remove sua cópia da lista e destaca novamente.
4. Exibir restaura o painel; Ocultar mantém chamadas. Limpar apaga destaque e histórico. Encerrar fecha ambas as telas e limpa a sessão.
5. Se habilitado, após 60 segundos o destaque desaparece e a última chamada passa ao histórico. Sem monitor salvo disponível, usa um conectado com aviso.

Tela cheia configurável, Esc sai da tela cheia e F11 alterna no monitor atual. Cor inicial/final em degradê e números brancos/pretos com prévia. Salvar e Chamar persistem as preferências. Histórico não é restaurado entre reinícios, evitando exibir chamadas antigas como atuais. A numeração de pedidos não é alterada pelo chamador manual.

## Windows

EnumDisplayMonitors/GetMonitorInfo identificam os monitores; SetWindowPos usa coordenadas do monitor. SAPI anuncia com voz em português instalada. Caso o gênero escolhido não esteja disponível, usa a voz portuguesa disponível e informa; ausência de português/falha de áudio não impede a exibição. Hardware de áudio e TV devem ser conferidos no estabelecimento; os testes automáticos não emitem áudio.

Fontes primárias: [Microsoft: posicionamento em múltiplos monitores](https://learn.microsoft.com/en-us/windows/win32/gdi/positioning-objects-on-multiple-display-monitors), [SAPI: vozes instaladas](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ee125639(v=vs.85)), [SAPI: fala assíncrona](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ms723609(v=vs.85)).

## Evidências

Oito testes de estado/persistência e dois testes WPF exercitam chamadas, repetição, histórico, prazo com relógio controlado, limpeza, ocultação, reabertura após fechar, acesso bloqueado e posicionamento real da janela no monitor selecionado. Interfaces renderizadas nos dois temas e em 1280×720/800×450, com senhas de nove dígitos. Capturas em artifacts/painel-produtos/previas.

Ajuste à referência: chamador de660×660, teclado com Apagar em duas colunas, ícones nas ações, prévia vazia antes da primeira chamada e seletor sem botão extra. Monitores exibem Principal/Secundário e resolução. Save aparece na aba de cores; opções do chamador são salvas ao alterar, com teste de persistência. Revalidados os dois testes WPF e o posicionamento da tela pública.
