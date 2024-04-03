FROM nginx:1.19.9

RUN apt-get update && \
	apt-get install -y curl && \
	apt-get install -y supervisor	

WORKDIR /app/lib

COPY lib/supervisor.conf /etc/supervisor/supervisor.conf
COPY lib/nginx.conf /etc/nginx/nginx.conf
COPY lib /app/lib

CMD ["bash", "./init.sh"]