import Image from 'next/image';
import Link from 'next/link';
import {FileText, ShieldCheck, MessageCircle} from 'lucide-react';
import {site} from '@/lib/site';

export function Footer() {
  return <footer>
    <div className="footer-top">
      <Link href="/" aria-label="Comanda — início"><Image src="/assets/logo.webp" alt="Comanda por Varthex" width={185} height={62}/></Link>
      <p>Organização no balcão.<br/>Mais cuidado no atendimento.</p>
      <nav className="footer-links" aria-label="Informações e contato">
        <Link href="/termos"><FileText aria-hidden="true"/>Termos de uso</Link>
        <Link href="/privacidade"><ShieldCheck aria-hidden="true"/>Privacidade</Link>
        <a href={site.whatsapp}><MessageCircle aria-hidden="true"/>Fale com a gente</a>
      </nav>
    </div>
    <div className="footer-bottom"><span>© {new Date().getFullYear()} Comanda · por Varthex</span><span>Feito para a rotina do seu balcão.</span></div>
  </footer>;
}
