import type {Metadata} from 'next';
import Image from 'next/image';
import {ArrowRight, MonitorCheck, PanelsTopLeft, Plus} from 'lucide-react';
import {BrandFlow} from '@/components/brand-flow';
import {site,siteOrigin,questions} from '@/lib/site';

export const metadata: Metadata = {alternates:siteOrigin()?{canonical:'/'}:undefined};

export default function Home() {
  return <main id="conteudo">
    <section className="hero" aria-labelledby="hero-title">
      <Image className="hero-picture" src="/assets/hero.webp" alt="" fill sizes="100vw" priority fetchPriority="high"/>
      <div className="hero-copy"><h1 id="hero-title">Seu atendimento<br/> flui. Seu negócio<br/> fica em ordem.</h1>
        <p>Mesas, comandas e consumo em um só lugar.<br className="desktop"/> No seu computador, mesmo sem internet.</p>
        <a className="button" href="#conhecer">Quero conhecer a Comanda <ArrowRight aria-hidden="true"/></a>
        <div className="hero-note"><PanelsTopLeft aria-hidden="true"/>Para Windows · operação local</div>
      </div>
      <p className="photo-caption">Mais atenção para quem está do outro lado do balcão.</p>
    </section>
    <div className="capabilities" aria-label="Capacidades"><span>Do primeiro pedido<br/> <strong>ao fechamento.</strong></span><span>Mesas e comandas</span><span>Produtos e consumo</span><span>Recebimentos</span><span>Histórico de vendas</span></div>
    <section className="product section" id="sistema" aria-labelledby="system-title">
      <div className="product-copy"><h2 id="system-title">Menos papel.<br/> Mais clareza<br/> no balcão.</h2><p>Acompanhe as comandas abertas, registre os itens e confira o consumo antes de fechar a conta.</p><p>Uma rotina organizada, com os dados salvos no próprio computador.</p><a className="text-link" href="#recursos">Veja o que a Comanda faz <ArrowRight aria-hidden="true"/></a></div>
      <figure><div className="window"><div className="window-top"><span>Comanda por Varthex</span><span>Mesas e comandas</span></div><Image src="/assets/atendimento.webp" alt="Grade de comandas livres e ocupadas na interface real da Comanda" width={1180} height={350} sizes="(max-width:640px) calc(100vw - 60px), (max-width:1024px) 80vw, 56vw"/></div><figcaption>Interface real da versão 1.4.0 · dados fictícios de demonstração</figcaption></figure>
    </section>
    <section className="features section" id="recursos" aria-labelledby="features-title">
      <div><h2 id="features-title">O essencial para<br/> cuidar da sua rotina.</h2><p>Da abertura da comanda à consulta das vendas,<br className="desktop"/> cada etapa tem seu lugar.</p><div className="local"><MonitorCheck aria-hidden="true"/><strong>Seu atendimento continua.<br/><span>Mesmo quando a internet não.</span></strong></div></div>
      <div className="feature-list"><article><h3>Organize mesas e comandas</h3><p>Itens, quantidades, observações e totais reunidos para acompanhar cada atendimento.</p></article><article><h3>Confira e divida a conta</h3><p>Registre recebimentos parciais ou mistos e faça a divisão por pessoas ou itens.</p></article><article><h3>Consulte o que já foi vendido</h3><p>Históricos com filtros, detalhes e exportação para Excel, além de backup local.</p></article></div>
    </section>
    <section className="faq section" id="perguntas" aria-labelledby="faq-title"><h2 id="faq-title">Antes de começar,<br/> tire suas dúvidas.</h2><div>{questions.map(({question,answer},index)=><details key={question} open={index===0}><summary>{question}<Plus aria-hidden="true"/></summary><p>{answer}</p></details>)}</div></section>
    <section className="cta section" id="conhecer" aria-labelledby="contact-title">
      <div className="cta-copy"><h2 id="contact-title">Uma rotina mais organizada<br/> começa com uma conversa.</h2><p>Conheça a Comanda e veja como ela se encaixa no seu balcão.</p><a className="button" href={site.whatsapp}>Conversar sobre a Comanda <ArrowRight aria-hidden="true"/></a><p className="fine">Versão 1.4.0 em testes · Windows x64 · requer ativação offline</p><a className="release-link" href={site.release}>Consultar demonstração e notas da versão <ArrowRight aria-hidden="true"/></a></div>
      <BrandFlow/>
    </section>
  </main>;
}
