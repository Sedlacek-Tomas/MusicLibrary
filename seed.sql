INSERT INTO genres (name) VALUES
                              ('Rock'), ('Pop'), ('Classical'), ('Jazz'), ('Electronic'), ('Hip-Hop')
    ON CONFLICT DO NOTHING;