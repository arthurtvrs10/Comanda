# Recuperação de senha do gestor

API para Cloudflare Workers + D1 e envio pelo Resend. A integração WPF está em
`backend/src/VarthexComanda.Desktop/RecuperacaoEmail`. Nenhuma chave real acompanha o código.
Não há envio simulado na configuração de produção: os testes injetam um remetente em memória.

## Estado da entrega

- Cadastro/troca do e-mail, confirmação por código e recuperação do **gestor local**.
- Telas conectadas à abertura de sessão, bloqueio e Configurações > Segurança.
- Recuperação offline preservada. Contas individuais dos colaboradores continuam sob gestão do administrador.
- API publicada em 30/10/2026 no plano gratuito, com banco D1 vinculado e limpeza horária.
- [Saúde da API publicada](https://vcomanda-recuperacao.comandasoftwarebyvarthex.workers.dev/health): HTTP 200 verificado.
- Envio real ainda desativado: o endpoint de recuperação retorna HTTP 503 enquanto faltam os segredos.
- Resend, domínio verificado, chaves de assinatura e configuração pública do desktop permanecem pendentes.
- A API pode usar o endereço gratuito `workers.dev`; esse endereço não substitui um domínio próprio de envio no Resend.

## Modelo de confiança

### Escopo pendente — decisão de 30/10/2026

Manter a recuperação por e-mail no escopo do VComanda para uma próxima etapa.
A comparação de registradores não autoriza compra de domínio nem contratação de planos.
Preservar a API e o banco já publicados no plano gratuito e a recuperação offline.
O envio por e-mail permanece desativado até concluir:

- Escolha e registro do domínio, com aprovação do custo pelo responsável.
- Verificação dos registros DNS no Resend e definição do remetente.
- Configuração dos segredos de envio e assinatura na Cloudflare.
- Distribuição da configuração pública correspondente no desktop.
- Homologação real de cadastro, recebimento do código e redefinição da senha.

Retomar usando a integração Resend já implementada. A alternativa Gmail/Apps Script
foi apenas discutida e não foi implementada nem escolhida para substituir o Resend.
Não apresentar a recuperação por e-mail como disponível aos clientes antes da homologação.

O cadastro exige sessão do gestor local e confirmação da senha atual no desktop.
A instalação recebe ID aleatório e segredo de 256 bits; no Windows, ambos ficam
protegidos por DPAPI para o usuário atual, em `gestor.credencial.email`, ao lado da
credencial. Na nuvem, fica somente o hash do segredo. O ID é independente de CNPJ,
e-mail comercial ou licença: não enviamos esses dados nem o banco operacional.

A API não conhece a senha local. O segredo identifica uma instalação previamente
cadastrada; não concede sozinho uma redefinição. O endereço do destinatário de um
reset vem exclusivamente do cadastro verificado, nunca da tela de login.

Para redefinir, o servidor emite autorização RSA-PSS/SHA-256 contendo instalação,
desafio, nonce, impressão SHA-256 da credencial atual, emissão e expiração. O
desktop confere a assinatura com a chave pública distribuída e compara todos os
campos antes de trocar a senha. A senha nova é processada localmente com o
PBKDF2 existente; a senha offline é preservada e o gestor volta ao login.
A troca gera novo sal e invalida a autorização anterior em relação à credencial atual.

Os códigos usam gerador criptográfico sem viés, seis dígitos, validade de dez
minutos e até cinco tentativas, incluindo a quinta. Só o MAC HMAC-SHA256 é
armazenado; a chave de MAC fica fora do banco. Reenvio invalida o desafio anterior,
e confirmações usam condições atômicas no banco. A confirmação de e-mail invalida
desafios pendentes. A autorização assinada já emitida dura no máximo dez minutos.

O código precisa ser solicitado novamente se o aplicativo for fechado. Nonces
pendentes não são persistidos. Reiniciar não zera as tentativas do servidor.
O vínculo DPAPI não é portátil para outro computador/usuário Windows. Em
reinstalações que perderem esse arquivo, use a senha de acesso ou a recuperação
offline e cadastre/verifique novamente o e-mail. Se o arquivo foi copiado de outro
usuário e não pode ser aberto, o suporte deve remover **somente** o vínculo `.email`
depois da recuperação/autenticação do gestor, para permitir um novo cadastro.
Não remover a credencial nem o banco. Não há recuperação silenciosa por e-mail
de um vínculo perdido ou alteração de destinatário para quem está bloqueado.

Este recurso mantém o modelo de confiança local do aplicativo: não protege contra
um administrador do Windows capaz de modificar o executável e os arquivos de acesso.

## Limites gratuitos e proteção de envio

- Sessenta requisições/minuto por IP.
- Até cinco novos vínculos/hora por IP.
- Intervalo móvel de sessenta segundos entre envios por instalação.
- Cinco envios/hora por instalação e dez/hora por IP.
- Orçamento global: noventa tentativas de envio/dia UTC e 2.700/mês calendário UTC.
- Envios que falham também consomem a reserva, de modo conservador.
- Cron horário limpa desafios antigos, limites expirados e cadastros não verificados há sete dias.
- Nenhum plano pago é ativado pelo código. Cotas gratuitas da infraestrutura continuam aplicáveis.
- Use uma conta Resend dedicada ou considere outros envios da mesma conta na cota.
  Esta API não controla e-mails enviados por outros aplicativos com essa conta.

O Worker não habilita CORS nem escreve corpos, códigos, e-mails ou credenciais
nos logs da aplicação. A observabilidade padrão da Cloudflare foi preservada;
o manifesto foi alinhado ao painel, incluindo os registros de invocação da plataforma. O hash do IP
usado nos limites é temporário; não é uma anonimização forte.

## Preparar e publicar, quando as contas estiverem disponíveis

1. Criar/selecionar contas nos planos gratuitos Cloudflare e Resend.
2. Adicionar domínio próprio ao Resend e configurar os registros DNS solicitados.
   Sem domínio, usar somente os destinatários de teste permitidos pelo Resend;
   não prometer envio a clientes. Não existe remetente de produção gratuito
   compartilhado incluído nesta implementação.
3. Nesta pasta, instalar a CLI oficial: `npm install --save-dev wrangler`.
   Guardar o `package-lock.json` gerado e usar `npm ci` nos próximos ambientes.
4. Entrar na Cloudflare: `npx wrangler login`.
5. Nesta conta, o banco e o vínculo `DB` já foram criados e o ID foi atualizado
   no manifesto; não criar outro. Em outra conta, criar com
   `npx wrangler d1 create vcomanda-recuperacao` e atualizar o `database_id`.
6. Aplicar migração: `npx wrangler d1 migrations apply vcomanda-recuperacao --remote`.
7. Gerar uma chave RSA de pelo menos 2048 bits (sugerido: 3072 bits), exportada
   em PKCS8 PEM, e um segredo aleatório de pelo menos 32 bytes para o OTP_PEPPER.
   Guardar a chave privada e o pepper em gerenciador de segredos; nunca no Git.
   O script `./Preparar-Chaves.ps1 -ApiUrl https://SEU_WORKER.workers.dev/`
   gera esses arquivos em `.secrets`, ignorada pelo Git, sem exibir seu conteúdo.
   Não o executar novamente para substituir chaves de uma API já publicada.
8. Cadastrar os quatro segredos usando o prompt da CLI:

   ```powershell
   npx wrangler secret put RESEND_API_KEY
   npx wrangler secret put EMAIL_FROM
   npx wrangler secret put OTP_PEPPER
   npx wrangler secret put SIGNING_PRIVATE_KEY
   ```

   EMAIL_FROM: nome e endereço do domínio verificado, como
   `VComanda <acesso@seu-dominio.com.br>`. Usar chave Resend restrita a envio.
   SIGNING_PRIVATE_KEY: o conteúdo integral do PEM PKCS8, com quebras de linha.
9. Publicar: `npx wrangler deploy`. Usar somente o plano gratuito nesta etapa.
10. Criar `backend/src/VarthexComanda.Desktop/recuperacao-email.json`:

    ```json
    {
      "ApiUrl": "https://vcomanda-recuperacao.SEU_SUBDOMINIO.workers.dev/",
      "ChavePublicaPem": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----"
    }
    ```

    Esse arquivo contém apenas configuração pública. O build e o publish o copiam
    quando presente. A chave privada e a chave do Resend **nunca** entram nele.
    Sem configuração válida, a interface explica a indisponibilidade e oferece
    recuperação offline. Não usa endereço/chave de demonstração automaticamente.
11. Gerar versão de teste; em Configurações > Segurança > E-mail de recuperação,
    confirmar a senha do gestor, enviar o código e verificar o endereço.
12. Fazer teste real de envio, recebimento, código inválido/expirado, reenvio e
    redefinição em uma instalação de homologação antes da distribuição.

Não trocar a chave de assinatura sem distribuir a nova chave pública. Sem esse
alinhamento o desktop recusa todas as autorizações, preservando o acesso offline.
Para revogar imediatamente uma autorização já emitida, alterar a credencial local;
a exclusão do desafio no servidor sozinha não revoga um documento já assinado.

## Desenvolvimento e testes

Os testes da API usam SQLite real em memória por meio de `node:sqlite` (Node 24),
executam as consultas SQL e verificam a assinatura Web Crypto; nenhum e-mail real
é enviado e nenhuma conta externa é necessária:

```powershell
node --test test/worker.test.mjs
```

Para atualizar pelo editor da Cloudflare sem instalar dependências, executar
`node build-dashboard.mjs` e substituir o conteúdo de `worker.js` pelo arquivo
gerado `dist/worker.mjs`. O teste do bundle verifica saúde e bloqueio dos envios
sem configuração. Depois, publicar e verificar `/health`. Não copiar arquivos
de `.secrets` para o editor de código; segredos pertencem às variáveis Secret.

Na publicação inicial, as tabelas foram criadas pelo console do D1 e confirmadas
por consulta. A migração é idempotente para que a primeira execução via Wrangler
registre seu histórico sem tentar recriar tabelas existentes. Nenhum dado de
estabelecimento foi inserido. O cron `17 * * * *` foi salvo no painel.

Para executar com o emulador oficial da Cloudflare, configurar `.dev.vars` com
segredos de teste, aplicar `npx wrangler d1 migrations apply vcomanda-recuperacao --local`
e executar `npx wrangler dev`. O desktop de produção aceita somente HTTPS;
testes HTTP locais da API são independentes. Não habilitar HTTP no instalador.

Os testes .NET `RecuperacaoEmailTests` cobrem assinatura/contexto, replay,
preservação da recuperação offline, cadastro autenticado, DPAPI, integração de
cliente com transporte simulado e falhas de conexão. Há também testes de tela.

## API

Todas as rotas POST exigem JSON e `Authorization: Bearer <segredo da instalação>`.
Corpo máximo de 8 KiB. Sem senhas nos corpos.

| Rota | Dados além de installationId | Resultado |
|---|---|---|
| POST /v1/enroll/request | email, nonce, credentialHash | challengeId, maskedEmail, expiresIn, retryAfter |
| POST /v1/enroll/confirm | challengeId, code | verified, maskedEmail |
| POST /v1/reset/request | nonce, credentialHash | challengeId, maskedEmail, expiresIn, retryAfter |
| POST /v1/reset/confirm | challengeId, code | grant assinado |
| GET /health | — | Identificação do serviço; não garante entrega de e-mail |

## Referências oficiais

- [Resend: envio](https://resend.com/docs/api-reference/emails/send-email)
- [Resend: domínios](https://resend.com/docs/dashboard/domains/introduction)
- [Cloudflare D1: API e batches transacionais](https://developers.cloudflare.com/d1/worker-api/d1-database/)
- [Cloudflare Workers: Web Crypto](https://developers.cloudflare.com/workers/runtime-apis/web-crypto/)
