-- Drop table if it exists (for development purposes)
DROP TABLE IF EXISTS calendar_events CASCADE;

-- Create calendar_events table
CREATE TABLE calendar_events (
    event_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    title VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP NOT NULL,
    location VARCHAR(200),
    is_all_day BOOLEAN DEFAULT FALSE,
    type VARCHAR(50) DEFAULT 'written' CHECK (type IN ('written', 'oral', 'oral+written', 'project')),
    color VARCHAR(7) DEFAULT '#3788d8',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(uid) ON DELETE CASCADE
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_calendar_events_user_id ON calendar_events(user_id);
CREATE INDEX IF NOT EXISTS idx_calendar_events_start_time ON calendar_events(start_time);
