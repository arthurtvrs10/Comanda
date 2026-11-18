// Test vector only. Never generates or writes production credentials/private keys.
import {writeFileSync} from 'node:fs';
import {signGrant,hex} from '../src/crypto.mjs';
const pair=await crypto.subtle.generateKey({name:'RSA-PSS',modulusLength:2048,publicExponent:new Uint8Array([1,0,1]),hash:'SHA-256'},true,['sign','verify']);
const pem=(name,bytes)=>`-----BEGIN ${name}-----\n${Buffer.from(bytes).toString('base64')}\n-----END ${name}-----`;
const privatePem=pem('PRIVATE KEY',await crypto.subtle.exportKey('pkcs8',pair.privateKey));
const publicKey=pem('PUBLIC KEY',await crypto.subtle.exportKey('spki',pair.publicKey));
const credential=new Uint8Array(64).fill(42);
const claims={aud:'vcomanda-gestor-reset',installationId:'a'.repeat(32),challengeId:'b'.repeat(32),nonce:'c'.repeat(32),
  credentialHash:hex(await crypto.subtle.digest('SHA-256',credential)),iat:1800000000,exp:1800000600,jti:'test-only'};
const fixture={publicKey,credential:Buffer.from(credential).toString('base64'),claims,grant:await signGrant(privatePem,claims)};
writeFileSync(new URL('../../../backend/tests/VarthexComanda.Desktop.Tests/Fixtures/recuperacao-webcrypto.json',import.meta.url),JSON.stringify(fixture,null,2)+'\n');
