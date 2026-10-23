import Image from 'next/image';
import Link from 'next/link';
import {FileText, ShieldCheck, Instagram} from 'lucide-react';
import {site} from '@/lib/site';

export function Footer() {
  return <footer>
    <div className="footer-top">
      <Link href="/" aria-label="Comanda — início"><Image src="/assets/logo.webp" alt="Comanda por Varthex" width={185} height={62}/></Link>
      <p>Organização no balcão.<br/>Mais cuidado no atendimento.</p>
      <nav className="footer-links" aria-label="Informações e contato">
        <Link href="/termos"><FileText aria-hidden="true"/>Termos de uso</Link>
        <Link href="/privacidade"><ShieldCheck aria-hidden="true"/>Privacidade</Link>
        <div className="footer-socials">
          <a className="social-link social-instagram" href="https://www.instagram.com/varthexcompany/" aria-label="Instagram da Varthex"><Instagram aria-hidden="true"/><span className="social-tooltip" aria-hidden="true">Instagram</span></a>
          <a className="social-link social-whatsapp" href={site.whatsapp} aria-label="Conversar pelo WhatsApp"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="M20.5 11.6a8.5 8.5 0 0 1-12.6 7.5L3 20.5l1.4-4.8a8.5 8.5 0 1 1 16.1-4.1Z"/><path d="M8.4 7.5c-.4 0-.9.6-.9 1.4 0 2.5 3 5.5 5.5 6 .8.2 1.9-.3 2.2-1.1l-2.3-1.2-.9.9a8 8 0 0 1-2.8-2.8l.9-.9-1.1-2.3Z"/></svg><span className="social-tooltip" aria-hidden="true">WhatsApp</span></a>
        </div>
      </nav>
    </div>
    <div className="footer-bottom"><span>© {new Date().getFullYear()} Comanda · por Varthex</span><span>Feito para a rotina do seu balcão.</span></div>
  </footer>;
}
