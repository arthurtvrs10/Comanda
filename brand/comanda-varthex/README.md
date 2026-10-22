# COMANDA · por Varthex

Identidade selecionada na conversa de 22/10/2026. Abra **[catalogo.html](catalogo.html)** para visualizar e escolher os arquivos. A prévia geral está em [01-apresentacao/variacoes.png](01-apresentacao/variacoes.png).

- **01-apresentacao/**: painel original aprovado como direção visual.
- **exportacoes/**: arquivos para uso, organizados por composição → cor → formato.
- **fontes/vetores/**: matrizes coloridas em SVG.
- **02-logos/** e **03-icone/**: imagens de referência da primeira separação.
- **04-paleta/**: cores em JSON e CSS.
- **05-guia/**: orientações de aplicação.

## Exportações

**Background escuro:** `exportacoes/background/escuro/` contém o fundo central em SVG e PNGs de 1920, 2560 e 3840 px de largura (16:9). Azul profundo #17252C, símbolo laranja e nome branco. O aplicativo usa a versão vetorial nativa e alterna automaticamente com o tema; imagens e cores de fundo personalizadas são preservadas.

Quatro composições: **logo-horizontal** (com assinatura), **logo-sem-assinatura**, **logotipo** (apenas nome) e **simbolo**. Cada composição tem versões **colorida**, **preta**, **branca** e **negativa** (laranja e branco para fundo escuro), com subpastas **svg/** e **png/**.

PNGs dos logos: **256, 512, 1024, 2048 e 4096 px** de largura. PNGs do símbolo: **32, 64, 128, 256, 512 e 1024 px** de largura. Altura proporcional; não são telas quadradas.

Total: **16 SVGs e 84 PNGs**. Todos têm fundo transparente. SVGs contêm contornos vetoriais reais, sem bitmap incorporado ou dependência de fontes. PNGs renderizados diretamente dos vetores. Versões brancas e negativas devem ser aplicadas sobre fundos escuros.

Os vetores foram reconstruídos a partir da imagem selecionada, com simplificação dos contornos e cores sólidas. Pequenas diferenças em relação ao painel podem existir. O lettering não corresponde a uma fonte disponibilizada neste pacote.

**scripts/** contém a vetorização e exportação reproduzíveis (vetorizar.py e exportar.cjs, nessa ordem); **manifesto.json** registra as entregas. Os scripts usam as dependências locais do ambiente de criação.

A identidade anterior permanece em ../comanda-organizada/. A integração com o aplicativo está em `backend/src/VarthexComanda.Desktop/Assets/MarcaComanda.xaml`, a partir da raiz do projeto; `scripts/aplicar-desktop.py` sincroniza os recursos da marca.
