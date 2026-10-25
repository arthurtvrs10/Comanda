import Link from 'next/link';
import Image from 'next/image';
import {ArrowLeft, MessageCircle} from 'lucide-react';
import {site} from '@/lib/site';
export default function NotFound(){return <main id="conteudo" className="not-found section"><Image className="recovery-mascot" src="/assets/mascote.webp" alt="" width={200} height={200}/><p className="error-code">404</p><h1>Essa página saiu<br/>do balcão.</h1><p>O endereço pode ter mudado ou não existir.<br/>Vamos voltar para um lugar conhecido?</p><div className="recovery-actions"><Link className="button" href="/"><ArrowLeft aria-hidden="true"/>Voltar ao início</Link><a className="text-link" href={site.whatsapp}><MessageCircle aria-hidden="true"/>Pedir ajuda</a></div></main>;}
