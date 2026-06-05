CREATE TABLE IF NOT EXISTS genres (
                                      id   SERIAL PRIMARY KEY,
                                      name VARCHAR(50) NOT NULL
    );

CREATE TABLE IF NOT EXISTS albums (
                                      id         SERIAL PRIMARY KEY,
                                      title      VARCHAR(200) NOT NULL,
    artist     VARCHAR(200) NOT NULL,
    year       INT,
    genre_id   INT REFERENCES genres(id)
    );

CREATE TABLE IF NOT EXISTS tracks (
                                      id         SERIAL PRIMARY KEY,
                                      album_id   INT NOT NULL REFERENCES albums(id) ON DELETE CASCADE,
    title      VARCHAR(200) NOT NULL,
    duration   INT,          -- délka v sekundách
    track_no   INT
    );