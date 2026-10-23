'use client';

import Image from 'next/image';
import {useRef, useState} from 'react';
import {motion, useReducedMotion} from 'motion/react';
import {LayoutGrid, WalletCards, ChartNoAxesCombined} from 'lucide-react';

const views = [
  {id:'atendimento', label:'Atendimento', title:'Cada comanda, no seu lugar.', description:'Visualize as comandas livres e ocupadas e acompanhe o consumo de cada atendimento.', src:'/assets/atendimento.webp', width:1180, height:350, alt:'Grade real de comandas livres e ocupadas da Comanda', icon:LayoutGrid},
  {id:'fechamento', label:'Fechamento', title:'Confira. Divida. Registre.', description:'Confira os itens, registre recebimentos parciais ou mistos e acompanhe o saldo da conta.', src:'/assets/pagamento.webp', width:1120, height:700, alt:'Tela real de fechamento com formas de recebimento e saldo da conta', icon:WalletCards},
  {id:'historico', label:'Histórico', title:'O atendimento termina. O registro fica.', description:'Consulte o histórico de vendas com filtros, detalhes e exportação para Excel.', src:'/assets/historico.webp', width:1180, height:600, alt:'Tela real do histórico de vendas da Comanda', icon:ChartNoAxesCombined},
] as const;

export function ProductShowcase() {
  const [selected, setSelected] = useState(0);
  const [keyboardChange, setKeyboardChange] = useState(false);
  const reduced = useReducedMotion();
  const tabs = useRef<(HTMLButtonElement|null)[]>([]);
  const view = views[selected];

  function select(index:number, keyboard:boolean) {
    setKeyboardChange(keyboard);
    setSelected(index);
    if (keyboard) tabs.current[index]?.focus();
  }

  return <div className="product-showcase">
    <div className="product-tabs" role="tablist" aria-label="Explore as telas da Comanda">
      {views.map(({id,label,icon:Icon}, index) => <button key={id} ref={element => {tabs.current[index] = element;}} type="button" role="tab" id={`tab-${id}`} aria-selected={index===selected} aria-controls={`panel-${id}`} tabIndex={index===selected?0:-1}
        onClick={event => select(index, event.detail===0)}
        onKeyDown={event => {
          let next:number|undefined;
          if(event.key==='ArrowRight') next=(selected+1)%views.length;
          if(event.key==='ArrowLeft') next=(selected+views.length-1)%views.length;
          if(event.key==='Home') next=0;
          if(event.key==='End') next=views.length-1;
          if(next!==undefined){event.preventDefault();select(next,true);}
        }}><Icon aria-hidden="true"/><span>{label}</span>{selected===index && <motion.span className="tab-indicator" layoutId="product-tab-active" transition={{duration:reduced||keyboardChange?0:.2,ease:[.16,1,.3,1]}}/>}</button>)}
    </div>
    {views.map((item,index) => <div key={item.id} role="tabpanel" id={`panel-${item.id}`} aria-labelledby={`tab-${item.id}`} hidden={index!==selected} tabIndex={0}>
      {index===selected && <>
        <motion.div className="showcase-screen" initial={false} animate={{opacity:1,transform:'translateY(0px)'}}>
          <motion.figure key={view.id} initial={reduced||keyboardChange?false:{opacity:.7,transform:'translateY(6px)'}} animate={{opacity:1,transform:'translateY(0px)'}} transition={{duration:.2,ease:[.16,1,.3,1]}}>
            <div className="showcase-top"><span>Comanda por Varthex</span><span>{view.label}</span></div>
            <div className="showcase-image"><Image src={view.src} alt={view.alt} width={view.width} height={view.height} sizes="(max-width:640px) calc(100vw - 48px), (max-width:1100px) 80vw, 760px"/></div>
          </motion.figure>
        </motion.div>
        <div className="showcase-description"><h3>{view.title}</h3><p>{view.description}</p></div>
      </>}
    </div>)}
    <p className="showcase-caption">Telas reais da versão 1.4.0 · dados fictícios de demonstração</p>
  </div>;
}
