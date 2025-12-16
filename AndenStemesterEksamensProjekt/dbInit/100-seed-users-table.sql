-- Seed 100 test users
-- Password for all test users: "password123" (bcrypt hash)
-- Hash generated with: $2a$11$X9p.QZ5YhZJZ5YhZJZ5YheuOJ5Zv5Y5Y5Y5Y5Y5Y5Y5Y5Y5Y5Y5Y5Y

DO $$
DECLARE
    i INT;
    user_email VARCHAR(255);
    user_role VARCHAR(50);
    roles VARCHAR[] := ARRAY['guest', 'student', 'lecturer', 'planner', 'censor', 'admin'];
BEGIN
    FOR i IN 1..100 LOOP
        user_email := 'testuser' || i || '@example.com';
        
        -- Distribute roles: mostly students (60%), some lecturers (15%), planners (10%), guests (10%), censors (3%), admins (2%)
        IF i <= 60 THEN
            user_role := 'student';
        ELSIF i <= 75 THEN
            user_role := 'lecturer';
        ELSIF i <= 85 THEN
            user_role := 'planner';
        ELSIF i <= 95 THEN
            user_role := 'guest';
        ELSIF i <= 98 THEN
            user_role := 'censor';
        ELSE
            user_role := 'admin';
        END IF;
        
        -- Insert user with bcrypt hashed password for "password123"
        INSERT INTO users (email, firstName, lastName, password_hash, role, is_active, created_at, updated_at)
        VALUES (
            user_email,
            'testuser',
            INITCAP(user_role),
            '$2a$12$SPDmr7PZip/M2r8KVZk/veE4GHkWUkJsho93T1K9n2ox4isAd2e1e',
            user_role,
            TRUE,
            NOW(),
            NOW()
        )
        ON CONFLICT (email) DO NOTHING;
        
        -- Create profile for each user
        INSERT INTO profiles (uid, navn, mobil_nr, adresse)
        SELECT 
            u.uid,
            'testuser ' || INITCAP(user_role),
            '+45 ' || LPAD((20000000 + i)::TEXT, 8, '0'),
            'Test Address ' || i || ', 1234 Test City'
        FROM users u
        WHERE u.email = user_email
        ON CONFLICT (uid) DO UPDATE SET 
            navn = EXCLUDED.navn,
            mobil_nr = EXCLUDED.mobil_nr,
            adresse = EXCLUDED.adresse;
    END LOOP;
END $$;
