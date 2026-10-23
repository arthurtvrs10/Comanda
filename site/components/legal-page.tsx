import Link from 'next/link';
import {ArrowLeft, MessageCircle, Info} from 'lucide-react';
import {site} from '@/lib/site';

export function LegalPage({title,intro,children}:{title:string;intro:string;children:React.ReactNode}) {
  return <main id="conteudo" className="legal section">
    <Link href="/" className="text-link back-link"><ArrowLeft aria-hidden="true"/>Voltar ao início</Link>
    <h1>{title}</h1><p className="legal-intro">{intro}</p>
    <aside className="legal-notice" role="note"><Info aria-hidden="true"/><div><strong>Minuta para revisão antes da publicação</strong><p>A identificação legal do responsável e o e-mail de contato ainda precisam ser informados. Este texto deve ser confirmado pelo responsável antes de entrar em vigor.</p></div></aside>
    <div className="legal-body">{children}</div>
    <div className="legal-contact"><h2>Ficou com alguma dúvida?</h2><p>Converse com a equipe da Comanda pelo canal confirmado de atendimento.</p><a className="button" href={site.whatsapp}><MessageCircle aria-hidden="true"/>Falar pelo WhatsApp</a></div>
  </main>;
}
