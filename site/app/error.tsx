'use client';
import Link from 'next/link';
import {RotateCcw} from 'lucide-react';
export default function ErrorPage({reset}:{error:Error&{digest?:string};reset:()=>void}){return <main id="conteudo" className="not-found section"><h1>Não conseguimos<br/>abrir esta página.</h1><p>Tente novamente. Se o problema continuar, volte para a página inicial.</p><div className="recovery-actions"><button className="button" onClick={reset}><RotateCcw aria-hidden="true"/>Tentar novamente</button><Link className="text-link" href="/">Voltar ao início</Link></div></main>;}
