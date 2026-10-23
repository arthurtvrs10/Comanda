---
name: comanda-mascote
description: Usa e adapta o mascote oficial de referência do VComanda, uma comanda laranja com rosto, luvas e tênis, consultando PNGs, paleta e limites de aprovação. Aplicável ao inserir o mascote em telas, materiais ou novas poses.
---

# Mascote Comanda — guia de implementação para IA

Este arquivo é a entrada do kit. Leia-o quando o usuário pedir para colocar, implementar, animar ou adaptar o mascote. É uma referência local; sua existência não instala automaticamente uma skill global nem treina permanentemente o modelo.

## Consulte antes de criar

1. Leia [a identidade](IDENTIDADE.md) e [o inventário](README.md).
2. Abra visualmente `referencias/mascote-comanda-master.png`. Não reconstrua o mascote apenas por descrição. Esse PNG é a principal referência da aparência atual.
3. Consulte `paleta/cores.json`. Para logo na roupa, use os arquivos oficiais em `referencias/logo-comanda-negativa.svg` e `.png`.
4. Verifique `manifesto.json` para distinguir conceito escolhido, nova variação e aprovação de produção. A preferência pelo conceito 05 não aprova automaticamente toda pose ou roupa futura.

## Identidade que deve permanecer

É uma comanda de papel antropomórfica, predominantemente laranja #FF5934, com bordas serrilhadas, proporções verticais arredondadas, grandes olhos escuros com brilho, sobrancelhas escuras, sorriso acolhedor e bochechas claras. Tem braços e pernas curtos escuros, luvas marfim e tênis escuros de sola clara. O acabamento é 3D suave, com volume e textura discreta. A pose de referência dá positivo com a mão à esquerda da imagem.

A variação mais recente solicitada usa avental escuro com a logo oficial Comanda no peito. Preserve a leitura do papel e da barra serrilhada. Não transforme o corpo em robô, animal, chama ou pessoa. Não acrescente nome próprio, personalidade narrativa ou acessórios permanentes sem pedido do usuário.

## Ao implementar

Para uso estático, prefira os PNGs transparentes existentes. Preserve a proporção, a área de segurança e as cores; não estique, recorte membros ou aplique filtros de cor. Escolha resolução suficiente para a dimensão física e a densidade da tela. Os PNGs de 32–64 px são referências técnicas: confira a leitura real da logo e do rosto antes de usar; não prometa que o texto será legível nesse tamanho.

Use texto alternativo conforme a função; se puramente decorativo, mantenha-o fora da navegação assistiva. Não cubra controles, valores ou mensagens importantes. Em animações, respeite redução de movimento e preserve silhueta e expressão.

O SVG com imagem embutida é um contêiner do PNG, não vetor editável. O SVG simplificado é estudo de formas. O GLB é um estudo esquemático com malhas reais, sem rig, animação ou reconstrução fiel do render. Nunca apresente esses estudos como equivalentes ao PNG principal ou substitua o mascote por eles sem validar a aparência.

## Ao pedir novas poses ou imagens

Use imagegen com o PNG mestre como referência visual. Declare os elementos invariáveis descritos acima e altere somente pose, expressão ou acessório autorizado. Para estampar a roupa, forneça também a logo oficial como referência; não redesenhe ou invente letras. O texto correto é `comanda`. Inspecione logo e transparência no resultado.

Prompt base: “Use o PNG mestre como referência de identidade. Preserve a comanda laranja #FF5934 com bordas serrilhadas, rosto, proporções, luvas marfim e tênis escuros. Preserve o avental e a logo oficial fornecida quando aplicáveis. Altere somente [mudança solicitada]. Render 3D suave, corpo inteiro, margens livres, fundo realmente transparente, sem novos textos ou marcas.”

Salve novas versões sem sobrescrever o mestre aprovado. Mostre PNG de prévia e peça aprovação para mudanças visuais antes de integração ou publicação, conforme o fluxo solicitado pelo usuário. Uma solicitação explícita para usar um arquivo já aprovado permite a implementação dentro desse escopo; não repita pedidos de autorização já atendidos.

## Entrega

Informe os arquivos usados, as adaptações realizadas e se houve apenas preparação local, integração ou publicação. Atualize o manifesto somente com aprovações que realmente ocorreram. Não marque uma geração ou um estudo como aprovado por inferência.
