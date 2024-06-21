CREATE TABLE cities (
    id SERIAL PRIMARY KEY,
    city_name VARCHAR(255),
    geom GEOMETRY(Point, 4326)
);

-- Voorbeeldgegevens
INSERT INTO cities (city_name, geom)
VALUES
    ('New York', ST_GeomFromText('POINT(-74.0060 40.7128)', 4326)),
    ('Los Angeles', ST_GeomFromText('POINT(-118.2500 34.0522)', 4326)),
    ('Chicago', ST_GeomFromText('POINT(-87.6298 41.8781)', 4326)),
    ('Houston', ST_GeomFromText('POINT(-95.3698 29.7604)', 4326)),
    ('Miami', ST_GeomFromText('POINT(-80.1918 25.7617)', 4326));