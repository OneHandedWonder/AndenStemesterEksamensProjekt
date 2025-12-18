-- Drop tabeller hvis de allerede eksisterer
-- CASCADE bruges for at fjerne afhængigheder
-- Lavet af Emil
DROP TABLE IF EXISTS calendar_events CASCADE;
DROP TABLE IF EXISTS sessions CASCADE;
DROP TABLE IF EXISTS profiles CASCADE;
DROP TABLE IF EXISTS user_teams CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS teams CASCADE;

-- Opret users tabel
CREATE TABLE users (
    uid SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    firstName VARCHAR(255) NOT NULL,
    lastName VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    role VARCHAR(50) DEFAULT 'guest' CHECK (role IN ('guest', 'student', 'lecturer', 'planner', 'censor', 'admin'))
);

-- Opret indeks på email for hurtigere opslag
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Opret indeks på is_active for filtrering af aktive brugere
CREATE INDEX IF NOT EXISTS idx_users_active ON users(is_active);

-- indset en testbruger
-- Email: test@example.com  
-- Password: password123
INSERT INTO users (email, firstName, lastName, password_hash, role) 
VALUES ('test@example.com', 'test', 'Admin', '$2a$12$SPDmr7PZip/M2r8KVZk/veE4GHkWUkJsho93T1K9n2ox4isAd2e1e', 'admin') -- Hash for pw.
ON CONFLICT (email) DO NOTHING;

-- Vis oprettet tabel
SELECT 'Users table created successfully!' as status;
