import type {Metadata, Viewport} from 'next';
import localFont from 'next/font/local';
import {Header} from '@/components/header';
import {Footer} from '@/components/footer';
import {site,siteOrigin} from '@/lib/site';
import './globals.css';

const manrope = localFont({src:[
  {path:'../public/fonts/manrope-500.woff2',weight:'500',style:'normal'},
  {path:'../public/fonts/manrope-700.woff2',weight:'700',style:'normal'},
],display:'swap',variable:'--font-manrope'});

export const metadata: Metadata = {
  metadataBase: siteOrigin() ?? new URL('http://localhost:3000'),
  title: {default:'Comanda por Varthex | Organização no seu balcão', template:'%s | Comanda por Varthex'},
  description: site.description,
  openGraph: {type:'website',locale:'pt_BR',siteName:site.name,title:site.name,description:site.description,images:[{url:'/assets/social-preview.png',width:1200,height:630,alt:'Comanda por Varthex — organização no seu balcão'}]},
  twitter: {card:'summary_large_image',title:site.name,description:site.description,images:['/assets/social-preview.png']},
  icons:{icon:[{url:'/favicon.ico'},{url:'/icon.png',type:'image/png',sizes:'192x192'}],apple:[{url:'/apple-touch-icon.png',sizes:'180x180'}]},
  manifest:'/manifest.webmanifest',
};
export const viewport: Viewport = {themeColor:'#17252C',width:'device-width',initialScale:1,viewportFit:'cover'};

export default function RootLayout({children}:Readonly<{children:React.ReactNode}>) {
  return <html lang="pt-BR"><body className={manrope.variable}><div className="page">
    <a href="#conteudo" className="skip-link">Pular para o conteúdo</a>
    <Header/>{children}<Footer/>
  </div></body></html>;
}
