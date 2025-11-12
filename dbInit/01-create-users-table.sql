-- Drop table if it exists (for development purposes)
DROP TABLE IF EXISTS users;

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    uid SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- Create index on email for faster lookups
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Create index on is_active for filtering active users
CREATE INDEX IF NOT EXISTS idx_users_active ON users(is_active);

-- Insert a test user
-- Email: test@example.com  
-- Password: password123
INSERT INTO users (email, password_hash) 
VALUES ('test@example.com', '$2a$12$SPDmr7PZip/M2r8KVZk/veE4GHkWUkJsho93T1K9n2ox4isAd2e1e') -- Hash for pw.
ON CONFLICT (email) DO NOTHING;

-- Display created table
SELECT 'Users table created successfully!' as status;
