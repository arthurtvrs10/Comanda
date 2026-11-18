// Dependency-free bundle for Cloudflare's dashboard editor. Wrangler uses src/ directly.
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {pathToFileURL} from 'node:url';
export function bundleDashboard() {
  return ['crypto','store','worker'].map(name=>readFileSync(new URL(`src/${name}.mjs`,import.meta.url),'utf8')
    .replace(/^import .*\r?\n/gm,'').replace(/export (?=(async )?function|class|const)/g,'')).join('\n');
}
if(process.argv[1]&&import.meta.url===pathToFileURL(process.argv[1]).href) {
  mkdirSync(new URL('dist/',import.meta.url),{recursive:true});
  writeFileSync(new URL('dist/worker.mjs',import.meta.url),bundleDashboard());
  process.stdout.write('Bundle para o editor: dist/worker.mjs\n');
}
