-- Drop table if it exists (for development purposes)
DROP TABLE IF EXISTS profiles;

-- Create profiles table
CREATE TABLE IF NOT EXISTS profiles (
    puid INTEGER PRIMARY KEY,
    navn VARCHAR(255) NOT NULL,
    adresse VARCHAR(500),
    mobil_nr VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    uid INTEGER UNIQUE NOT NULL
);

-- Create trigger to automatically update updated_at timestamp
CREATE OR REPLACE FUNCTION update_profiles_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_profiles_updated_at
    BEFORE UPDATE ON profiles
    FOR EACH ROW
    EXECUTE FUNCTION update_profiles_updated_at();

-- Insert a test profile for the test user
INSERT INTO profiles (uid, navn, adresse, mobil_nr, puid)
VALUES (1, 'Test Bruger', 'Testvej 123, 1234 Testby', '+45 12 34 56 78', 1)
ON CONFLICT (uid) DO NOTHING;

-- Display created table
SELECT 'Profiles table created successfully!' as status;