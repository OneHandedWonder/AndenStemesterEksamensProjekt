-- Drop tabeller hvis de allerede eksisterer
-- Lavet af Emil
DROP TABLE IF EXISTS teams CASCADE;
CREATE TABLE teams (
	id SERIAL PRIMARY KEY,
	name VARCHAR(100) NOT NULL,
	description TEXT,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Opret user_teams junction tabel til mange-til-mange relation
DROP TABLE IF EXISTS user_teams CASCADE;
CREATE TABLE user_teams (
	user_id INTEGER NOT NULL,
	team_id INTEGER NOT NULL,
	joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY (user_id, team_id),
	FOREIGN KEY (user_id) REFERENCES users(uid) ON DELETE CASCADE,
	FOREIGN KEY (team_id) REFERENCES teams(id) ON DELETE CASCADE
);

-- Opret indeks for hurtigere opslag
CREATE INDEX IF NOT EXISTS idx_user_teams_user_id ON user_teams(user_id);
CREATE INDEX IF NOT EXISTS idx_user_teams_team_id ON user_teams(team_id);