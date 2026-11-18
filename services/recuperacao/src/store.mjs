export class Store {
  constructor(db) { this.db=db; }
  stmt(sql,...args) {return this.db.prepare(sql).bind(...args);}
  async limit(key,max,seconds,now) {
    const bucket=Math.floor(now/seconds);
    return this.reserve(`${key}:${bucket}`,max,(bucket+1)*seconds);
  }
  async reserve(key,max,expires) {
    const row=await this.stmt(`INSERT INTO rate_limits(key,count,expires_at) VALUES(?,1,?)
      ON CONFLICT(key) DO UPDATE SET count=count+1 WHERE count<? RETURNING count`,key,expires,max).first();
    return !!row;
  }
  async installation(id) {return this.stmt('SELECT * FROM installations WHERE id=?',id).first();}
  async register(id,secretHash,now) {
    await this.stmt('INSERT OR IGNORE INTO installations(id,secret_hash,created_at) VALUES(?,?,?)',id,secretHash,now).run();
    return this.installation(id);
  }
  async challenge(c) {
    await this.db.batch([
      this.stmt('UPDATE challenges SET used=1 WHERE installation_id=? AND purpose=? AND used=0',c.installation_id,c.purpose),
      this.stmt(`INSERT INTO challenges(id,installation_id,purpose,email,email_version,mac,nonce,credential_hash,expires_at)
        VALUES(?,?,?,?,?,?,?,?,?)`,c.id,c.installation_id,c.purpose,c.email,c.email_version,c.mac,c.nonce,c.credential_hash,c.expires_at)
    ]);
  }
  async attempt(id,installationId,purpose,now) {
    return this.stmt(`UPDATE challenges SET attempts=attempts+1 WHERE id=? AND installation_id=? AND purpose=?
      AND used=0 AND expires_at>? AND attempts<5 RETURNING *`,id,installationId,purpose,now).first();
  }
  async consume(id,now=0) {return !!await this.stmt(`UPDATE challenges SET used=1 WHERE id=? AND used=0 AND expires_at>?
    AND email_version=(SELECT email_version FROM installations WHERE id=installation_id) RETURNING id`,id,now).first();}
  async confirmEmail(c,now) {
    const results=await this.db.batch([
      this.stmt(`UPDATE installations SET email=?,email_version=email_version+1 WHERE id=? AND email_version=?
        AND EXISTS(SELECT 1 FROM challenges WHERE id=? AND used=0 AND expires_at>? AND attempts<=5)`,c.email,c.installation_id,c.email_version,c.id,now),
      this.stmt('UPDATE challenges SET used=1 WHERE installation_id=? AND email_version=? AND used=0',c.installation_id,c.email_version)
    ]);
    return results[0].meta.changes===1;
  }
  async cleanup(now) {
    await this.db.batch([
      this.stmt('DELETE FROM challenges WHERE expires_at<?',now-86400),
      this.stmt('DELETE FROM rate_limits WHERE expires_at<?',now),
      this.stmt('DELETE FROM installations WHERE email IS NULL AND created_at<?',now-604800)
    ]);
  }
}
