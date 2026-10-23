'use client';
import {useEffect,useRef} from 'react';

// Fragment-only effects use vGPU's generated fullscreen vertex stage.
const shader = `
struct Uniforms { resolution: vec2f, progress: f32, padding: f32 }
@group(0) @binding(0) var<uniform> uniforms: Uniforms;
fn capsule(p: vec2f) -> f32 {
  let q = vec2f(p.x, max(abs(p.y) - 0.27, 0.0));
  return length(q) - 0.19;
}
@fragment fn fs_main(@location(0) uv: vec2f) -> @location(0) vec4f {
  let aspect = uniforms.resolution.x / max(uniforms.resolution.y, 1.0);
  var p = vec2f((uv.x - 0.63) * aspect, uv.y - 0.48);
  let angle = -0.35;
  p = vec2f(cos(angle)*p.x - sin(angle)*p.y, sin(angle)*p.x + cos(angle)*p.y);
  p.x += (1.0 - uniforms.progress) * 0.035;
  var strength = 0.0;
  for (var i = 0; i < 3; i++) {
    let distance = abs(capsule(p - vec2f(f32(i) * 0.19, 0.0)));
    let edge = 1.0 - smoothstep(0.024, 0.029, distance);
    strength = max(strength, edge * (0.25 + f32(i) * 0.22));
  }
  let bg = vec3f(0.090196, 0.145098, 0.172549);
  let brand = vec3f(1.0, 0.34902, 0.203922);
  return vec4f(mix(bg, brand, strength), 1.0);
}`;

export function BrandFlow() {
  const hostRef = useRef<HTMLDivElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  useEffect(()=>{
    const host=hostRef.current;
    const canvas=canvasRef.current;
    if(!host||!canvas) return;
    const reduced=matchMedia('(prefers-reduced-motion: reduce)');
    let disposed=false;
    let started=false;
    let raf=0;
    let teardown:(()=>void)|undefined;
    const fallback=(reason:string)=>{
      cancelAnimationFrame(raf);
      host.dataset.render=reason;
      teardown?.();
      teardown=undefined;
    };
    async function render() {
      if(started||disposed||reduced.matches||!navigator.gpu) return;
      started=true;
      host!.dataset.render='loading';
      try {
        const [{init,surface,effect,frame},{prepareShader}]=await Promise.all([import('vgpu'),import('@vgpu/wgsl/prepare')]);
        if(disposed||reduced.matches) return;
        const gpu=await init({powerPreference:'low-power',label:'Comanda brand flow'});
        if(disposed||reduced.matches){gpu.dispose();return;}
        teardown=()=>gpu.dispose();
        gpu.onError(()=>fallback('fallback-error'));
        gpu.lost.then(()=>{if(!disposed) fallback('fallback-device-lost');});
        const output=surface(gpu,canvas!,{dpr:Math.min(devicePixelRatio,1.5),label:'Comanda CTA'});
        const visual=effect(gpu,prepareShader(shader,'comanda-brand-flow.wgsl'),{label:'Comanda capsule bands'});
        await visual.compile(output);
        if(disposed||gpu.disposed) return;
        const draw=(progress:number)=>{
          if(disposed||gpu.disposed) return;
          visual.set({uniforms:{resolution:output.size,progress,padding:0}});
          frame(gpu,f=>{f.pass(output,visual);});
        };
        draw(1);
        await gpu.settled();
        if(disposed||gpu.disposed) return;
        host!.dataset.render='webgpu';
        const start=performance.now();
        // A single 220 ms arrival, then a static frame. No perpetual GPU loop.
        const tick=(now:number)=>{
          if(disposed||gpu.disposed||document.hidden) return;
          const progress=Math.min((now-start)/220,1);
          try {draw(1-Math.pow(1-progress,4));} catch {fallback('fallback-error');return;}
          if(progress<1) raf=requestAnimationFrame(tick);
        };
        raf=requestAnimationFrame(tick);
        const onResize=()=>draw(1);
        const resize=new ResizeObserver(onResize);
        resize.observe(host!);
        const disposeGpu=teardown;
        teardown=()=>{resize.disconnect();disposeGpu();};
      } catch {if(!disposed) fallback('fallback-unavailable');}
    }
    const onPreference=()=>{
      if(reduced.matches) fallback('reduced-motion');
      else {started=false;void render();}
    };
    reduced.addEventListener('change',onPreference);
    host.dataset.render=reduced.matches?'reduced-motion':!navigator.gpu?'unsupported':'static';
    const observer=new IntersectionObserver(entries=>{if(entries.some(e=>e.isIntersecting))void render();},{rootMargin:'100px'});
    observer.observe(host);
    return ()=>{disposed=true;observer.disconnect();reduced.removeEventListener('change',onPreference);cancelAnimationFrame(raf);teardown?.();};
  },[]);
  return <div ref={hostRef} className="flow-host" aria-hidden="true" data-render="static"><div className="brand-flow"><i/><i/><i/></div><canvas ref={canvasRef}/></div>;
}
