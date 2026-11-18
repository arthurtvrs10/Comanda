CREATE TABLE transfers(hash TEXT PRIMARY KEY, machine TEXT NOT NULL UNIQUE, encrypted TEXT NOT NULL, expires INTEGER NOT NULL);
CREATE INDEX transfer_expiry ON transfers(expires);
CREATE TABLE limits(key TEXT PRIMARY KEY, count INTEGER NOT NULL, expires INTEGER NOT NULL);
