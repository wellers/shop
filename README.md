# Shop microservices
.NET 8 containerised microservices for a Shopping application.

### Microservices

#### Basket
REST API to store customer baskets. A Redis server is utilised to store the purchased items. Baskets that are purchased are pushed onto the message queue.

#### Booking
Consumes basket purchase messages from the message queue and stores the bookings in a Postgres database.

#### Catalog
GraphQL API over a Mongo database. A cron job populates the database.

#### Kube-proxy
Nginx API gateway to provide access to the microservices from outside the Kubernetes cluster.

#### Shared
pgAdmin and RabbitMQ message queue.
