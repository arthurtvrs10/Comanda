import publicPem from './public.pem';
import {createHandler} from './transfer.mjs';
export default {fetch:createHandler(publicPem),async scheduled(_event,env){const now=Math.floor(Date.now()/1000);await env.DB.batch([env.DB.prepare('DELETE FROM transfers WHERE expires<=?').bind(now),env.DB.prepare('DELETE FROM limits WHERE expires<=?').bind(now)]);}};
