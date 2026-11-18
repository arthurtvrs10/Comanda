CREATE TABLE IF NOT EXISTS installations (
 id TEXT PRIMARY KEY,
 secret_hash TEXT NOT NULL,
 email TEXT,
 email_version INTEGER NOT NULL DEFAULT 0,
 created_at INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS challenges (
 id TEXT PRIMARY KEY,
 installation_id TEXT NOT NULL REFERENCES installations(id) ON DELETE CASCADE,
 purpose TEXT NOT NULL CHECK(purpose IN ('enroll','reset')),
 email TEXT NOT NULL,
 email_version INTEGER NOT NULL,
 mac TEXT NOT NULL,
 nonce TEXT NOT NULL,
 credential_hash TEXT NOT NULL,
 expires_at INTEGER NOT NULL,
 attempts INTEGER NOT NULL DEFAULT 0,
 used INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS challenges_installation ON challenges(installation_id, purpose);
CREATE INDEX IF NOT EXISTS challenges_expiry ON challenges(expires_at);
CREATE TABLE IF NOT EXISTS rate_limits (
 key TEXT PRIMARY KEY,
 count INTEGER NOT NULL,
 expires_at INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS rate_expiry ON rate_limits(expires_at);
