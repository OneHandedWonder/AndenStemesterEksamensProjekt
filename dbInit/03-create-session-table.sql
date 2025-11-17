CREATE TABLE IF NOT EXISTS sessions (
    session_id SERIAL PRIMARY KEY,
    uid INT NOT NULL,
    session_token CHAR(32) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NULL,
    FOREIGN KEY (uid) REFERENCES users(uid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_session_token ON sessions(session_token);
CREATE INDEX IF NOT EXISTS idx_session_uid ON sessions(uid);
