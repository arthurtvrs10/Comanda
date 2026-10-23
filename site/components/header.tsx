import Image from 'next/image';
import Link from 'next/link';
import {ArrowUpRight} from 'lucide-react';

export function Header() {
  return <header className="nav">
    <Link href="/" className="brand-link" aria-label="Comanda por Varthex — início"><Image src="/assets/logo.webp" alt="Comanda por Varthex" width={210} height={70} priority /></Link>
    <nav aria-label="Principal">
      <Link href="/#sistema">O sistema</Link><Link href="/#recursos">Recursos</Link><Link href="/#perguntas">Dúvidas</Link>
      <Link className="button small" href="/#conhecer">Conhecer a Comanda <ArrowUpRight aria-hidden="true" size={18} /></Link>
    </nav>
    <Link className="menu" href="/#conhecer">Conhecer <ArrowUpRight aria-hidden="true" size={16}/></Link>
  </header>;
}
