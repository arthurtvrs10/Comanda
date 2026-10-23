import type {MetadataRoute} from 'next';
import {siteOrigin} from '@/lib/site';
export default function robots():MetadataRoute.Robots{const origin=siteOrigin();return {rules:{userAgent:'*',...(origin?{allow:'/',disallow:['/termos','/privacidade']}:{disallow:'/'})},...(origin?{sitemap:new URL('/sitemap.xml',origin).href}:{})};}
