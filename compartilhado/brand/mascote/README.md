# Kit de referência — Mascote Comanda

Para a IA: comece em [SKILL.md](SKILL.md) e **abra o PNG mestre**, além de ler as instruções. Para pedir uma implementação: “Leia brand/mascote/SKILL.md, abra o PNG mestre e use esse mascote em [local], preservando sua identidade.”

## Arquivos

| Caminho | Conteúdo e limite |
| --- | --- |
| `referencias/mascote-comanda-master.png` | Referência visual atual, com avental e logo; proposta aguardando aprovação visual |
| `referencias/mascote-conceito-05-sem-avental.png` | PNG individual anterior, preservado como histórico |
| `referencias/prancha-laranja-04-07.png` | Prancha em que o conceito 05 foi escolhido |
| `referencias/logo-comanda-negativa.png` e `.svg` | Logo oficial para fundos escuros, copiada da identidade do sistema |
| `png/mascote-comanda-{tamanho}.png` | PNGs transparentes quadrados em 32, 48, 64, 96, 128, 256, 512 e 1024 px |
| `svg/mascote-referencia-embutida.svg` | PNG mestre embutido em SVG; preserva aparência, **não é vetorização** |
| `svg/mascote-estudo-vetorial.svg` | Desenho vetorial simplificado editável, **não equivalente ao render** |
| `3d/mascote-estudo-esquematico.glb` | Modelo 3D de estudo com materiais e malhas, sem rig ou animações; **não é a geometria original do PNG** |
| `paleta/` | Paleta em JSON, CSS e PNG |
| `IDENTIDADE.md` | Anatomia, cores e restrições |
| `manifesto.json` | Estado de aprovação e papéis dos arquivos |
| `gerar-referencias.py` | Recria tamanhos, paleta, SVG embutido e estudo 3D localmente a partir do mestre; requer Pillow e numpy |

Os estudos vetorial e tridimensional servem para explicar estrutura. O GLB tem avental esquemático sem decalque da logo. Para um modelo 3D fiel de produção será necessário trabalho de modelagem, materiais, rig e validação visual. Não existe arquivo Blender/FBX original desta imagem.

Esta pasta não altera o aplicativo nem publica assets. O kit registra referências; aprovação de novas imagens e aplicação segue o fluxo em SKILL.md. A skill local é utilizável por referência ao arquivo, sem alegar instalação global automática.
