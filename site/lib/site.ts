export const site = {
  name: 'Comanda por Varthex',
  description: 'Organize mesas, comandas e consumo no seu computador Windows, mesmo sem internet. Conheça a Comanda, por Varthex.',
  whatsapp: 'https://wa.me/5561985532004',
  whatsappLabel: '(61) 98553-2004',
  repository: 'https://github.com/arthurtvrs10/Comanda',
  release: 'https://github.com/arthurtvrs10/Comanda/releases/tag/v1.4.0',
} as const;

export const questions = [
  {question:'Preciso de internet para usar?', answer:'O atendimento funciona localmente no Windows. Downloads, WhatsApp e serviços de nuvem precisam de conexão.'},
  {question:'A Comanda recebe o pagamento?', answer:'A cobrança acontece fora do sistema. Na Comanda você registra os recebimentos manualmente, incluindo pagamentos parciais e mistos.'},
  {question:'Posso conhecer antes de contratar?', answer:'Há uma edição de demonstração com produtos fictícios. Ela também exige ativação offline. Converse com o suporte para conhecer o sistema e obter sua licença.'},
] as const;

// Defina apenas um domínio real para habilitar canonical e sitemap de produção.
export function siteOrigin(): URL | undefined {
  const value = process.env.SITE_URL;
  if (!value) return undefined;
  try {
    const url = new URL(value);
    if (!['https:', 'http:'].includes(url.protocol)) return undefined;
    return url;
  } catch { return undefined; }
}
