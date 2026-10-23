'use client';

import {useEffect, useRef, useState} from 'react';
import {NotebookPen, LayoutGrid, WalletCards, ReceiptText, ChartNoAxesCombined, DatabaseBackup} from 'lucide-react';

const features = [
  {label: 'Mesas e comandas', icon: LayoutGrid},
  {label: 'Produtos e consumo', icon: NotebookPen},
  {label: 'Recebimentos', icon: WalletCards},
  {label: 'Impressão térmica', icon: ReceiptText},
  {label: 'Histórico de vendas', icon: ChartNoAxesCombined},
  {label: 'Backup local', icon: DatabaseBackup},
];

/** Payer's continuous strip, reinterpreted as real capabilities, not customer logos. */
export function FeatureMarquee() {
  const root = useRef<HTMLElement>(null);
  const [inView, setInView] = useState(false);
  const [pageVisible, setPageVisible] = useState(true);

  useEffect(() => {
    const element = root.current;
    if (!element) return;
    const observer = new IntersectionObserver(([entry]) => setInView(entry.isIntersecting));
    observer.observe(element);
    const onVisibility = () => setPageVisible(!document.hidden);
    document.addEventListener('visibilitychange', onVisibility);
    onVisibility();
    return () => { observer.disconnect(); document.removeEventListener('visibilitychange', onVisibility); };
  }, []);

  return <section ref={root} className="feature-marquee" aria-labelledby="marquee-title" data-paused={!inView || !pageVisible}>
    <div className="marquee-heading"><h2 id="marquee-title">Do primeiro pedido ao fechamento.</h2>
    </div>
    <div className="marquee-viewport" id="capability-track">
      <div className="marquee-track">
        {[false, true].map(duplicate => <ul className="marquee-group" key={String(duplicate)} aria-hidden={duplicate || undefined}>
          {features.map(({label, icon: Icon}) => <li className="marquee-item" key={label}><Icon aria-hidden="true"/><span>{label}</span></li>)}
        </ul>)}
      </div>
    </div>
  </section>;
}
