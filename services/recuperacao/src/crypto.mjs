const enc = new TextEncoder();
export const b64url = bytes => btoa(String.fromCharCode(...new Uint8Array(bytes))).replaceAll('+','-').replaceAll('/','_').replaceAll('=','');
export const hex = bytes => [...new Uint8Array(bytes)].map(x=>x.toString(16).padStart(2,'0')).join('');
export const sha = async value => hex(await crypto.subtle.digest('SHA-256', enc.encode(value)));
export function code() {
  const a=new Uint32Array(1);
  do { crypto.getRandomValues(a); } while(a[0]>=4294000000);
  return (a[0]%1000000).toString().padStart(6,'0');
}
async function hmacKey(pepper) {
  return crypto.subtle.importKey('raw',enc.encode(pepper),{name:'HMAC',hash:'SHA-256'},false,['sign','verify']);
}
export async function mac(pepper,id,code) { return hex(await crypto.subtle.sign('HMAC',await hmacKey(pepper),enc.encode(`${id}:${code}`))); }
export async function verifyCode(pepper,id,code,digest) {
  return crypto.subtle.verify('HMAC',await hmacKey(pepper),Uint8Array.from(digest.match(/../g),x=>parseInt(x,16)),enc.encode(`${id}:${code}`));
}
export function mask(email) { const at=email.lastIndexOf('@');return email.slice(0,Math.min(2,at))+'***'+email.slice(at); }
export async function signGrant(pem,claims) {
  const der=Uint8Array.from(atob(pem.replace(/-----[^-]+-----|\s/g,'')),x=>x.charCodeAt(0));
  const key=await crypto.subtle.importKey('pkcs8',der,{name:'RSA-PSS',hash:'SHA-256'},false,['sign']);
  const payload=b64url(enc.encode(JSON.stringify(claims)));
  const signature=await crypto.subtle.sign({name:'RSA-PSS',saltLength:32},key,enc.encode(`v1.${payload}`));
  return `v1.${payload}.${b64url(signature)}`;
}
