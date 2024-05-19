CREATE TABLE weather (
  city varchar(80),
  temperature_low int,
  temperature_high int,
  humidity smallint,
  uv smallint,
  date date
);

CREATE TABLE cities (
  name varchar(80),
  location point
);
