CREATE TABLE bookings
(
	booking_id SERIAL PRIMARY KEY,
	basket_id VARCHAR(255),
	booking_date DATE
);

CREATE TABLE movies
(
	movie_id SERIAL PRIMARY KEY,
	title VARCHAR(255),
	price DECIMAL
);

CREATE TABLE booking_movies
(
	booking_movie_id SERIAL PRIMARY KEY,
	booking_id INT,
	movie_id INT,
	CONSTRAINT movie_id FOREIGN KEY(movie_id) REFERENCES movies(movie_id),
	CONSTRAINT booking_id FOREIGN KEY(booking_id) REFERENCES bookings(booking_id)
);

INSERT INTO movies(title, price) 
SELECT 'Mr Bean', 10.00;