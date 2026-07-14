# Static landing page served by nginx on Cloud Run (listens on 8080).
FROM nginx:1.27-alpine

# Replace default server config with our Cloud Run-friendly one (port 8080).
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy the site.
COPY index.html styles.css favicon.svg /usr/share/nginx/html/

EXPOSE 8080

# nginx runs in the foreground (base image default CMD already does this).
